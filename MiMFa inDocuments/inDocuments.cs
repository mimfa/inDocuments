


using MiMFa.Engine;
using MiMFa.General;
using MiMFa.Service;
using MiMFa.UIL.Searcher.Compute;
using MiMFa.UIL.Searcher.Infra;
using MiMFa.UIL.Searcher.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiMFa.Exclusive.Animate;

namespace MiMFa.UIL.Searcher
{
    public partial class inDocuments : Form
    {
        public FormMove MFM = new FormMove();
        public Kernel MainKernel = null;
        private Thread MainThread = null;
        private bool Break = true;
        internal Func<int,bool> CheckActivation = i=>i>=0;

        public inDocuments(string data = null,bool importable = false, bool replace = false, bool start = false)
        {
            InitializeComponent();
            Default.Template(this);
            SuspendLayout();
            Initial();
            btn_Cancel.Visible = btn_Ok.Visible = importable;
            cb_Replace.Checked = replace;
            ResumeLayout(true);
            SetData(data, start);
        }

        public void Initial()
        {
            tsmi_CopyRoot.Text = tsmi_MoveRoot.Text = tb_Directory.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            API.Start();
            MFM.MainControl = this;
            MFM.AddToControl(this, l_Title, l_Subtitle, l_Message);
            tb_Search.Tag = tb_Replace.Tag = Environment.NewLine;
            MainKernel = new Kernel(Connector_Logger);
            LoadData();
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                object obj = key.GetValue(Application.ProductName);
                if (obj != null)
                    cb_ClipBoardCheck.Checked = Convert.ToBoolean(obj);
            }
            catch { }
            Log(System.Windows.Forms.Application.CompanyName + " " + System.Windows.Forms.Application.ProductName + " Version " + System.Windows.Forms.Application.ProductVersion, 0);
        }

        private void Connector_Logger(int type, string message)
        {
            message += Environment.NewLine;
            switch (type)
            {
                case -1:
                    ControlService.SetControlThreadSafe(RTB, (arg) => ControlService.RichTextBoxAppendWithStyle(ref RTB, "ERROR:\t" + message, Color.Red));
                    break;
                case 0:
                    ControlService.SetControlThreadSafe(RTB, (arg) => ControlService.RichTextBoxAppendWithStyle(ref RTB, "PROCESS:\t" + message, Color.DimGray));
                    break;
                case 1:
                    ControlService.SetControlThreadSafe(RTB, (arg) => ControlService.RichTextBoxAppendWithStyle(ref RTB, "SUCCESS:\t" + message, Color.Green));
                    break;
                default:
                    ControlService.SetControlThreadSafe(RTB, (arg) => ControlService.RichTextBoxAppendWithStyle(ref RTB, "MESSAGE:\t" + message, Color.Black));
                    break;
            }
        }
        public string GetParameters(string state)=>
                    "key=ks6s1m335tp5a6g&" +
                    "company=" + System.Windows.Forms.Application.CompanyName + "&" +
                    "product=" + System.Windows.Forms.Application.ProductName + "&" +
                    "version=" + System.Windows.Forms.Application.ProductVersion + "&" +
                    "userip=" + MiMFa.Service.NetService.GetInternalIPv4() + "&" +
                    "usermac=" + MiMFa.Service.NetService.GetMAC() + "&" +
                    "stdate=" + "*" + "&" +
                    "exdate=" + "*" + "&" +
                    "username=" + MiMFa.Service.NetService.GetHostName() + "&" +
                    "detail=" + API.Configuration.ID + "&" +
                    "state=" + state + "&" +
                    "useraddress=" + "*" + "&" +
                    "userphone=" + "*" + "&" +
                    "userstatus=" + 0 + "&" +
                    "licensetype=" + 0 + "&" +
                    "haslog=" + 0 + "&" +
                    "date=" + MiMFa.Default.Date + "&" +
                    "time=" + MiMFa.Default.Time;
        public IEnumerable<string> GetSelectedPaths()
        {
            foreach (DataGridViewRow item in DGV.SelectedRows)
                yield return item.Tag + "";
        }
        public IEnumerable<string> GetPaths()
        {
            foreach (DataGridViewRow item in DGV.Rows)
                yield return item.Tag + "";
        }

        public void SetData(string str, bool start = false)
        {
            if (str == null) return;
            string[] stra = str.Split(new string[] { Environment.NewLine, ";" }, StringSplitOptions.None);
            string dir = null;
            string search = null;
            string replace = null;
            string filter = null;
            if (stra.Length > 1)
            {
                List<string> ls = new List<string>();
                foreach (var item in stra)
                {
                    string val = item.Trim();
                    if (InfoService.IsAddress(val) || InfoService.IsAbsoluteURL(val))
                        dir = (dir == null ? "" : dir + tb_Directory.Tag + "") + item;
                    else if (val.StartsWith(".") || (val.Contains(".") && val.Contains(",")) || (val.Contains(".") && val.Contains(";")))
                        filter = (filter == null ? "" : filter + tb_Ext.Tag + "") + item;
                    else ls.Add(item.Replace("¶", Environment.NewLine));
                }
                if (ls.Count > 2)
                {
                    search = string.Join(Environment.NewLine, ls.GetRange(0, ls.Count - 1));
                    replace = ls.Last();
                }
                else if (ls.Count == 2)
                {
                    search = ls.First();
                    replace = ls.Last();
                }
                else if (ls.Count == 1)
                    search = ls.First();
            }
            else if (MiMFa.Service.InfoService.IsAddress(stra.First()) || MiMFa.Service.InfoService.IsAbsoluteURL(stra.First())) dir = stra.First();
            else search = str;
            if (dir != null) tb_Directory.Text = dir;
            if (search != null) tb_Search.Text = search;
            if (replace != null) tb_Replace.Text = replace;
            if (filter != null) tb_Ext.Text = filter;
            if (start
                && !p_ControlProcess.Visible
                //&& !string.IsNullOrEmpty(tb_Search.Text)
                && (
                tb_Directory.Text != API.Configuration.Sources
                || tb_Search.Text != API.Configuration.Search
                || tb_Replace.Text != API.Configuration.Replace
                || !IsHandleCreated
                )) ProcessStart(false);
        }
       
        private void SaveData()
        {
            //SelectedPaths = GetSelectedPaths().ToArray();
            try
            {
                API.Configuration.Sources = tb_Directory.Text;
                API.Configuration.Sensitive = rb_Pattern.Checked ? 0 : rb_Like.Checked ? 2 : rb_Same.Checked ? 3 : 1;
                API.Configuration.Replace = tb_Replace.Text;
                API.Configuration.Search = tb_Search.Text;
                MainKernel.Searcher = new MiMFa.Engine.Search(API.Configuration.Search);
                if (API.Configuration.Sensitive > 1) API.Configuration.Extensions = tb_Ext.Text.Split(new string[] { ";", ",", Environment.NewLine }, StringSplitOptions.None).Distinct().ToList();
                else API.Configuration.Extensions = tb_Ext.Text.ToLower().Split(new string[] { ";", ",", Environment.NewLine }, StringSplitOptions.None).Distinct().ToList();
                API.Configuration.ExtensionFilter = cb_Ext.Checked;
                API.Configuration.RecursiveSource = cb_Recursive.Checked;
                API.Configuration.InContext = cb_InContext.Checked;
                API.Configuration.InName = cb_InFileName.Checked;
                API.Configuration.AllowReplace = cb_Replace.Checked;
                API.Configuration.Highlights = cb_resources.Checked;
                API.Configuration.Save();
            }
            catch (Exception ex) { DialogService.ShowMessage(ex); }
        }
        private void LoadData()
        {
            try
            {
                API.Configuration.Load();
                tb_Ext.Text = string.Join(tb_Ext.Tag+"", API.Configuration.Extensions);
                tb_Search.Text = API.Configuration.Search;
                tb_Replace.Text = API.Configuration.Replace;
                switch (API.Configuration.Sensitive)
                {
                    case 3:
                        rb_Same.Checked = true;
                        break;
                    case 2:
                        rb_Like.Checked = true;
                        break;
                    case 0:
                        rb_Pattern.Checked = true;
                        break;
                    default:
                        rb_Any.Checked = true;
                        break;
                }
                cb_Ext.Checked = API.Configuration.ExtensionFilter;
                cb_Recursive.Checked = API.Configuration.RecursiveSource;
                cb_Replace.Checked = API.Configuration.AllowReplace;
                cb_InContext.Checked = API.Configuration.InContext;
                cb_InFileName.Checked = API.Configuration.InName;
                cb_resources.Checked =  API.Configuration.Highlights;
                tb_Directory.Text = API.Configuration.Sources;
            }
            catch (Exception ex) {/* DialogService.ShowMessage(ex);*/ }
        }

        private void btn_Search_Click(object sender, EventArgs e)
        {
            ProcessStart(false);
        }
        private void btn_Replace_Click(object sender, EventArgs e)
        {
            ProcessStart(true);
        }
        private void btn_Stop_Click(object sender, EventArgs e)
        {
            if (MainThread == null) End("PROCESS");
            else if (MainThread.IsAlive)
            {
                try { MainThread.Resume(); } catch { }
                if (!Break)
                {
                    Break = true;
                    p_ControlProcess.Visible = false;
                }
                else
                    try
                    {
                        MainThread.Abort(); 
                        p_ControlProcess.Visible = false;
                    }
                    catch (Exception ex) {/* DialogService.ShowMessage(ex);*/ }
            }
            Log("Process has been stopped!", 0);
        }
        private void btn_Pause_Click(object sender, EventArgs e)
        {
            if (CheckActivation(1))
                try
                {
                    if (MainThread != null)
                    {
                        if (btn_Pause.Tag + "" == "0")
                        {
                            MainThread.Resume();
                            btn_Pause.Image = MiMFa.Properties.Resources.pause_color;
                            btn_Pause.Tag = "1";
                        }
                        else
                        {
                            MainThread.Suspend();
                            btn_Pause.Image = MiMFa.Properties.Resources.right_color;
                            btn_Pause.Tag = "0";
                        }
                    }
                }
                catch { }
        }
        private void ProcessStart(bool replace)
        {
            if (!Break) return;
            SaveData();
            if (API.Configuration.Sensitive < 1 && !CheckActivation(1)) return;
            if (rb_Pattern.Checked && !InfoService.IsValidRegexPattern(MainKernel.Searcher.SearchPattern, "test", 20))
            {
                DialogService.ShowMessage(MessageMode.Error,true,"The pattern is not valid!");
                return;
            }
            Start(replace ? "REPLACE" : "SEARCH");
            MainThread = new Thread(() =>
            {
                try
                {
                    List<Sources> allSources = GetSources(API.Configuration.Sources);
                    if (replace)
                    {
                        if (CheckActivation(1) && DialogService.ShowMessage(MessageMode.Warning, false, "Your files will have change!\r\nAre you sure about that?") == DialogResult.Yes)
                            foreach (var source in allSources)
                                if (Break) break;
                                else ReplaceProcessStart(source);
                    }
                    else
                        foreach (var source in allSources)
                            if (Break) break;
                            else SearchProcessStart(source);
                }
                catch (Exception ex) { if (!(ex is ThreadAbortException)) DialogService.ShowMessage(ex);}
                finally { End("PROCESS"); }
            });
            MainThread.IsBackground = true;
            MainThread.SetApartmentState(ApartmentState.STA);
            MainThread.Start();
        }

        private void SearchProcessStart(Sources sources)
        {
            if (sources.Maximum < 1) return;
            try
            {
                ControlService.SetControlThreadSafe(PB, (aa) =>
                {
                    PB.Maximum = sources.Maximum;
                    PB.Value = sources.Value;
                });
                if (API.Configuration.ExtensionFilter && API.Configuration.Extensions.Count > 0)
                    switch (sources.State)
                    {
                        case SourceState.Net:
                            if (!CheckActivation(1)) return;
                            break;
                        case SourceState.Web:
                            if (!CheckActivation(1)) return;
                            switch (API.Configuration.Sensitive)
                            {
                                case 3:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.URLAndPageWithExtensionMatchSame);
                                    else if (API.Configuration.InName) SaerchInAllPages(sources, MainKernel.URLWithExtensionMatchSame);
                                    else if (API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.PageWithExtensionMatchSame);
                                    break;
                                case 2:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.URLAndPageWithExtensionMatchLike);
                                    else if (API.Configuration.InName) SaerchInAllPages(sources, MainKernel.URLWithExtensionMatchLike);
                                    else if (API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.PageWithExtensionMatchLike);
                                    break;
                                case 0:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.URLAndPageWithExtensionMatchPattern);
                                    else if (API.Configuration.InName) SaerchInAllPages(sources, MainKernel.URLWithExtensionMatchPattern);
                                    else if (API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.PageWithExtensionMatchPattern);
                                    break;
                                default:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.URLAndPageWithExtensionMatchAny);
                                    else if (API.Configuration.InName) SaerchInAllPages(sources, MainKernel.URLWithExtensionMatchAny);
                                    else if (API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.PageWithExtensionMatchAny);
                                    break;
                            }
                            break;
                        default:
                            switch (API.Configuration.Sensitive)
                            {
                                case 3:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.FileNameAndContextWithExtensionMatchSame);
                                    else if (API.Configuration.InName) SaerchInAllFiles(sources, MainKernel.FileNameWithExtensionMatchSame);
                                    else if (API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.ContextWithExtensionMatchSame);
                                    break;
                                case 2:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.FileNameAndContextWithExtensionMatchLike);
                                    else if (API.Configuration.InName) SaerchInAllFiles(sources, MainKernel.FileNameWithExtensionMatchLike);
                                    else if (API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.ContextWithExtensionMatchLike);
                                    break;
                                case 0:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.FileNameAndContextWithExtensionMatchPattern);
                                    else if (API.Configuration.InName) SaerchInAllFiles(sources, MainKernel.FileNameWithExtensionMatchPattern);
                                    else if (API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.ContextWithExtensionMatchPattern);
                                    break;
                                default:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.FileNameAndContextWithExtensionMatchAny);
                                    else if (API.Configuration.InName) SaerchInAllFiles(sources, MainKernel.FileNameWithExtensionMatchAny);
                                    else if (API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.ContextWithExtensionMatchAny);
                                    break;
                            }
                            break;
                    }
                else
                    switch (sources.State)
                    {
                        case SourceState.Net:
                            if (!CheckActivation(1)) return;
                            break;
                        case SourceState.Web:
                            if (!CheckActivation(1)) return;
                            switch (API.Configuration.Sensitive)
                            {
                                case 3:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.URLAndPageMatchSame);
                                    else if (API.Configuration.InName) SaerchInAllPages(sources, MainKernel.URLMatchSame);
                                    else if (API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.PageMatchSame);
                                    break;
                                case 2:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.URLAndPageMatchLike);
                                    else if (API.Configuration.InName) SaerchInAllPages(sources, MainKernel.URLMatchLike);
                                    else if (API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.PageMatchLike);
                                    break;
                                case 0:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.URLAndPageMatchPattern);
                                    else if (API.Configuration.InName) SaerchInAllPages(sources, MainKernel.URLMatchPattern);
                                    else if (API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.PageMatchPattern);
                                    break;
                                default:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.URLAndPageMatchAny);
                                    else if (API.Configuration.InName) SaerchInAllPages(sources, MainKernel.URLMatchAny);
                                    else if (API.Configuration.InContext) SaerchInAllPages(sources, MainKernel.PageMatchAny);
                                    break;
                            }
                            break;
                        default:
                            switch (API.Configuration.Sensitive)
                            {
                                case 3:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.FileNameAndContextMatchSame);
                                    else if (API.Configuration.InName) SaerchInAllFiles(sources, MainKernel.FileNameMatchSame);
                                    else if (API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.ContextMatchSame);
                                    break;
                                case 2:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.FileNameAndContextMatchLike);
                                    else if (API.Configuration.InName) SaerchInAllFiles(sources, MainKernel.FileNameMatchLike);
                                    else if (API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.ContextMatchLike);
                                    break;
                                case 0:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.FileNameAndContextMatchPattern);
                                    else if (API.Configuration.InName) SaerchInAllFiles(sources, MainKernel.FileNameMatchPattern);
                                    else if (API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.ContextMatchPattern);
                                    break;
                                default:
                                    if (API.Configuration.InName && API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.FileNameAndContextMatchAny);
                                    else if (API.Configuration.InName) SaerchInAllFiles(sources, MainKernel.FileNameMatchAny);
                                    else if (API.Configuration.InContext) SaerchInAllFiles(sources, MainKernel.ContextMatchAny);
                                    break;
                            }
                            break;
                    }
                Log("Search doned successfully!", 0);
            }
            catch (Exception ex) { Log(ex.Message, -1); }
        }
        private void ReplaceProcessStart(Sources sources)
        {
            try
            {
                if (sources.Maximum < 1) return;
                ControlService.SetControlThreadSafe(PB, (aa) =>
                {
                    PB.Maximum = sources.Maximum;
                    PB.Value = sources.Value;
                });
                if (API.Configuration.ExtensionFilter && API.Configuration.Extensions.Count > 0)
                    switch (sources.State)
                    {
                        case SourceState.Net:
                            break;
                        case SourceState.Web:
                            switch (API.Configuration.Sensitive)
                            {
                                case 3:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.URLAndPageWithExtensionMatchSame);
                                    else if (API.Configuration.InName) ReplaceInAllPages(sources, MainKernel.URLWithExtensionMatchSame);
                                    else if (API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.PageWithExtensionMatchSame);
                                    break;
                                case 2:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.URLAndPageWithExtensionMatchLike);
                                    else if (API.Configuration.InName) ReplaceInAllPages(sources, MainKernel.URLWithExtensionMatchLike);
                                    else if (API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.PageWithExtensionMatchLike);
                                    break;
                                case 0:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.URLAndPageWithExtensionMatchPattern);
                                    else if (API.Configuration.InName) ReplaceInAllPages(sources, MainKernel.URLWithExtensionMatchPattern);
                                    else if (API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.PageWithExtensionMatchPattern);
                                    break;
                                default:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.URLAndPageWithExtensionMatchAny);
                                    else if (API.Configuration.InName) ReplaceInAllPages(sources, MainKernel.URLWithExtensionMatchAny);
                                    else if (API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.PageWithExtensionMatchAny);
                                    break;
                            }
                            break;
                        default:
                            switch (API.Configuration.Sensitive)
                            {
                                case 3:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.FileNameAndContextWithExtensionMatchSame);
                                    else if (API.Configuration.InName) ReplaceInAllFiles(sources, MainKernel.FileNameWithExtensionMatchSame);
                                    else if (API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.ContextWithExtensionMatchSame);
                                    break;
                                case 2:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.FileNameAndContextWithExtensionMatchLike);
                                    else if (API.Configuration.InName) ReplaceInAllFiles(sources, MainKernel.FileNameWithExtensionMatchLike);
                                    else if (API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.ContextWithExtensionMatchLike);
                                    break;
                                case 0:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.FileNameAndContextWithExtensionMatchPattern);
                                    else if (API.Configuration.InName) ReplaceInAllFiles(sources, MainKernel.FileNameWithExtensionMatchPattern);
                                    else if (API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.ContextWithExtensionMatchPattern);
                                    break;
                                default:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.FileNameAndContextWithExtensionMatchAny);
                                    else if (API.Configuration.InName) ReplaceInAllFiles(sources, MainKernel.FileNameWithExtensionMatchAny);
                                    else if (API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.ContextWithExtensionMatchAny);
                                    break;
                            }
                            break;
                    }
                else
                    switch (sources.State)
                    {
                        case SourceState.Net:
                            break;
                        case SourceState.Web:
                            switch (API.Configuration.Sensitive)
                            {
                                case 3:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.URLAndPageMatchSame);
                                    else if (API.Configuration.InName) ReplaceInAllPages(sources, MainKernel.URLMatchSame);
                                    else if (API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.PageMatchSame);
                                    break;
                                case 2:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.URLAndPageMatchLike);
                                    else if (API.Configuration.InName) ReplaceInAllPages(sources, MainKernel.URLMatchLike);
                                    else if (API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.PageMatchLike);
                                    break;
                                case 0:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.URLAndPageMatchPattern);
                                    else if (API.Configuration.InName) ReplaceInAllPages(sources, MainKernel.URLMatchPattern);
                                    else if (API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.PageMatchPattern);
                                    break;
                                default:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.URLAndPageMatchAny);
                                    else if (API.Configuration.InName) ReplaceInAllPages(sources, MainKernel.URLMatchAny);
                                    else if (API.Configuration.InContext) ReplaceInAllPages(sources, MainKernel.PageMatchAny);
                                    break;
                            }
                            break;
                        default:
                            switch (API.Configuration.Sensitive)
                            {
                                case 3:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.FileNameAndContextMatchSame);
                                    else if (API.Configuration.InName) ReplaceInAllFiles(sources, MainKernel.FileNameMatchSame);
                                    else if (API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.ContextMatchSame);
                                    break;
                                case 2:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.FileNameAndContextMatchLike);
                                    else if (API.Configuration.InName) ReplaceInAllFiles(sources, MainKernel.FileNameMatchLike);
                                    else if (API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.ContextMatchLike);
                                    break;
                                case 0:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.FileNameAndContextMatchPattern);
                                    else if (API.Configuration.InName) ReplaceInAllFiles(sources, MainKernel.FileNameMatchPattern);
                                    else if (API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.ContextMatchPattern);
                                    break;
                                default:
                                    if (API.Configuration.InName && API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.FileNameAndContextMatchAny);
                                    else if (API.Configuration.InName) ReplaceInAllFiles(sources, MainKernel.FileNameMatchAny);
                                    else if (API.Configuration.InContext) ReplaceInAllFiles(sources, MainKernel.ContextMatchAny);
                                    break;
                            }
                            break;
                    }
                Log("Replace doned successfully!", 0);
            }
            catch (Exception ex) { Log(ex.Message, -1); }

        }

        private bool SaerchInAllFiles(Sources files, MachHandler matchs)
        {
            Log("Search in Files...", 0);
            string summary = "";
            return ForEach(files, (addr) =>
            {
                try { if (!string.IsNullOrEmpty(summary = matchs(addr)))
                        AddResult(System.IO.Path.GetFileName(addr), addr, addr, summary); }
                catch (Exception ex) { Log(ex, addr); }
            });
        }
        private bool ReplaceInAllFiles(Sources files, MachHandler matchs)
        {
            Log("Replace in Files...", 0);
            string summary = "";
            Color cfn = l_rfn.ForeColor;
            Color cct = l_rct.ForeColor;
            Color cfnct = l_rfnct.ForeColor;
            string replace = tb_Replace.Text;
            switch (API.Configuration.Sensitive)
            {
                case 3:
                    return ForEach(files, (addr) =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(summary = matchs(addr)))
                            {
                                bool ct = !string.IsNullOrEmpty( MainKernel.ContextReplaceSame(addr, replace));
                                string naddr = MainKernel.PathReplaceSame(addr, replace);
                                int r = AddResult(System.IO.Path.GetFileName(addr), naddr, naddr,summary);
                                bool fn = naddr != addr;
                                if (ct && fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfnct;
                                else if (ct) DGV.Rows[r].DefaultCellStyle.BackColor = cct;
                                else if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                            }
                        }
                        catch (Exception ex) { Log(ex, addr); }
                    });
                case 2:
                    return ForEach(files, (addr) =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(summary = matchs(addr)))
                            {
                                bool ct = !string.IsNullOrEmpty(MainKernel.ContextReplaceLike(addr, replace));
                                string naddr = MainKernel.PathReplaceLike(addr, replace);
                                int r = AddResult(System.IO.Path.GetFileName(addr), naddr, naddr,summary);
                                bool fn = naddr != addr;
                                if (ct && fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfnct;
                                else if (ct) DGV.Rows[r].DefaultCellStyle.BackColor = cct;
                                else if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                            }
                        }
                        catch (Exception ex) { Log(ex, addr); }
                    });
                case 0:
                    return ForEach(files, (addr) =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(summary = matchs(addr)))
                            {
                                bool ct = !string.IsNullOrEmpty(MainKernel.ContextReplacePattern(addr, replace));
                                string naddr = MainKernel.PathReplacePattern(addr, replace);
                                int r = AddResult(System.IO.Path.GetFileName(addr), naddr, naddr, summary);
                                bool fn = naddr != addr;
                                if (ct && fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfnct;
                                else if (ct) DGV.Rows[r].DefaultCellStyle.BackColor = cct;
                                else if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                            }
                        }
                        catch (Exception ex) { Log(ex, addr); }
                    });
                default:
                    return ForEach(files, (addr) =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(summary = matchs(addr)))
                            {
                                bool ct = !string.IsNullOrEmpty(MainKernel.ContextReplaceAny(addr, replace));
                                string naddr = MainKernel.PathReplaceAny(addr, replace);
                                int r = AddResult(System.IO.Path.GetFileName(addr), naddr, naddr,summary);
                                bool fn = naddr != addr;
                                if (ct && fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfnct;
                                else if (ct) DGV.Rows[r].DefaultCellStyle.BackColor = cct;
                                else if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                            }
                        }
                        catch (Exception ex) { Log(ex, addr); }
                    });
            }
        }

        private bool SaerchInAllPages(Sources urls, MachHandler matchs)
        {
            Log("Search in Web...", 0);
            string summary = "";
            return ForEach(urls, (addr) =>
            {
                try { if (!string.IsNullOrEmpty(summary = matchs(addr)))
                        AddResult(GetPageName(addr), addr, addr,summary); }
                catch (Exception ex) { Log(ex, addr); }
            });
        }
        private bool SaerchInAllPages(Sources urls, MachHandlerByPOut matchs)
        {
            Log("Search in Web...", 0);
            string summary = "";
            string path = null;
            return ForEach(urls, (addr) =>
            {
                string name = GetPageName(addr);
                path = null;
                try { if (!string.IsNullOrEmpty(summary = matchs(addr, out path))) AddResult(name, addr, path,summary); }
                catch (Exception ex) { Log(ex, addr); }
                finally
                {
                    Uri uri = new Uri(addr);
                    string path2 = null;
                    if (path != null)
                        ForEach(MainKernel.FetchURLs(uri,path), item => {
                           try { if (!string.IsNullOrEmpty(summary = matchs(item, out path2))) AddResult(name, item, path2, summary); }
                           catch (Exception ex) { Log(ex, item); }
                       });
                }
            });
        }
        private bool ReplaceInAllPages(Sources urls, MachHandler matchs)
        {
            Log("Replace in Web Result...", 0);
            string summary = "";
            Color cfn = l_rfn.ForeColor;
            Color cct = l_rct.ForeColor;
            Color cfnct = l_rfnct.ForeColor;
            string replace = tb_Replace.Text;
            switch (API.Configuration.Sensitive)
            {
                case 3:
                    return ForEach(urls, (addr) =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(summary = matchs(addr)))
                            {
                                string naddr = MainKernel.PathReplaceSame(addr, replace);
                                int r = AddResult(System.IO.Path.GetFileName(addr), naddr, naddr,summary);
                                bool fn = naddr != addr;
                                if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                            }
                        }
                        catch (Exception ex) { Log(ex, addr); }
                    });
                case 2:
                    return ForEach(urls, (addr) =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(summary = matchs(addr)))
                            {
                                string naddr = MainKernel.PathReplaceLike(addr, replace);
                                int r = AddResult(System.IO.Path.GetFileName(addr), naddr, naddr, summary);
                                bool fn = naddr != addr;
                                if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                            }
                        }
                        catch (Exception ex) { Log(ex, addr); }
                    });
                case 0:
                    return ForEach(urls, (addr) =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(summary = matchs(addr)))
                            {
                                string naddr = MainKernel.PathReplacePattern(addr, replace);
                                int r = AddResult(System.IO.Path.GetFileName(addr), naddr, naddr, summary);
                                bool fn = naddr != addr;
                                if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                            }
                        }
                        catch (Exception ex) { Log(ex, addr); }
                    });
                default:
                    return ForEach(urls, (addr) =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(summary = matchs(addr)))
                            {
                                string naddr = MainKernel.PathReplaceAny(addr, replace);
                                int r = AddResult(System.IO.Path.GetFileName(addr), naddr, naddr,summary);
                                bool fn = naddr != addr;
                                if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                            }
                        }
                        catch (Exception ex) { Log(ex, addr); }
                    });
            }
        }
        private bool ReplaceInAllPages(Sources urls, MachHandlerByPOut matchs)
        {
            Log("Replace in Web Result...", 0);
            string summary = "";
            Color cfn = l_rfn.ForeColor;
            Color cct = l_rct.ForeColor;
            Color cfnct = l_rfnct.ForeColor;
            string replace = tb_Replace.Text;
            string path = null;
            switch (API.Configuration.Sensitive)
            {
                case 3:
                    return ForEach(urls, (addr) =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(summary = matchs(addr, out path)))
                            {
                                bool ct = !string.IsNullOrEmpty(MainKernel.ContextReplaceSame(path, replace));
                                string naddr = MainKernel.PathReplaceSame(path, replace);
                                int r = AddResult(GetPageName(addr), addr, naddr, summary);
                                bool fn = naddr != path;
                                if (ct && fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfnct;
                                else if (ct) DGV.Rows[r].DefaultCellStyle.BackColor = cct;
                                else if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                            }
                        }
                        catch (Exception ex) { Log(ex, addr); }
                        finally
                        {
                            Uri uri = new Uri(addr);
                            string path2 = null;
                            if (path != null)
                                ForEach(MainKernel.FetchURLs(uri,path), (item) =>
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(summary = matchs(item, out path2)))
                                        {
                                            bool ct = !string.IsNullOrEmpty(MainKernel.ContextReplaceSame(path2, replace));
                                            string naddr = MainKernel.PathReplaceSame(path2, replace);
                                            int r = AddResult(GetPageName(item), item, naddr, summary);
                                            bool fn = naddr != path2;
                                            if (ct && fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfnct;
                                            else if (ct) DGV.Rows[r].DefaultCellStyle.BackColor = cct;
                                            else if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                                        }
                                    }
                                    catch (Exception ex) { Log(ex, item); }
                                });
                        }
                    });
                case 2:
                    return ForEach(urls, (addr) =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(summary = matchs(addr, out path)))
                            {
                                bool ct = !string.IsNullOrEmpty(MainKernel.ContextReplaceLike(path, replace));
                                string naddr = MainKernel.PathReplaceLike(path, replace);
                                int r = AddResult(GetPageName(addr), addr, naddr, summary);
                                bool fn = naddr != path;
                                if (ct && fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfnct;
                                else if (ct) DGV.Rows[r].DefaultCellStyle.BackColor = cct;
                                else if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                            }
                        }
                        catch (Exception ex) { Log(ex, addr); }
                        finally
                        {
                            Uri uri = new Uri(addr);
                            string path2 = null;
                            if (path != null)
                                ForEach(MainKernel.FetchURLs(uri,path), (item) =>
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(summary = matchs(item, out path2)))
                                        {
                                            bool ct = !string.IsNullOrEmpty(MainKernel.ContextReplaceLike(path2, replace));
                                            string naddr = MainKernel.PathReplaceLike(path2, replace);
                                            int r = AddResult(GetPageName(item), item, naddr, summary);
                                            bool fn = naddr != path2;
                                            if (ct && fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfnct;
                                            else if (ct) DGV.Rows[r].DefaultCellStyle.BackColor = cct;
                                            else if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                                        }
                                    }
                                    catch (Exception ex) { Log(ex, item); }
                                });
                        }
                    });
                case 0:
                    return ForEach(urls, (addr) =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(summary = matchs(addr, out path)))
                            {
                                bool ct = !string.IsNullOrEmpty(MainKernel.ContextReplacePattern(path, replace));
                                string naddr = MainKernel.PathReplacePattern(path, replace);
                                int r = AddResult(GetPageName(addr), addr, naddr, summary);
                                bool fn = naddr != path;
                                if (ct && fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfnct;
                                else if (ct) DGV.Rows[r].DefaultCellStyle.BackColor = cct;
                                else if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                            }
                        }
                        catch (Exception ex) { Log(ex, addr); }
                        finally
                        {
                            Uri uri = new Uri(addr);
                            string path2 = null;
                            if (path != null)
                                ForEach(MainKernel.FetchURLs(uri,path), (item) =>
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(summary = matchs(item, out path2)))
                                        {
                                            bool ct = !string.IsNullOrEmpty(MainKernel.ContextReplacePattern(path2, replace));
                                            string naddr = MainKernel.PathReplacePattern(path2, replace);
                                            int r = AddResult(GetPageName(item), item, naddr, summary);
                                            bool fn = naddr != path2;
                                            if (ct && fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfnct;
                                            else if (ct) DGV.Rows[r].DefaultCellStyle.BackColor = cct;
                                            else if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                                        }
                                    }
                                    catch (Exception ex) { Log(ex, item); }
                                });
                        }
                    });
                default:
                    return ForEach(urls, (addr) =>
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(summary = matchs(addr, out path)))
                            {
                                bool ct = !string.IsNullOrEmpty(MainKernel.ContextReplaceAny(path, replace));
                                string naddr = MainKernel.PathReplaceAny(path, replace);
                                int r = AddResult(GetPageName(addr), addr, naddr, summary);
                                bool fn = naddr != path;
                                if (ct && fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfnct;
                                else if (ct) DGV.Rows[r].DefaultCellStyle.BackColor = cct;
                                else if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                            }
                        }
                        catch (Exception ex) { Log(ex, addr); }
                        finally
                        {
                            Uri uri = new Uri(addr);
                            string path2 = null;
                            if (path != null)
                                ForEach(MainKernel.FetchURLs(uri,path), (item) =>
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(summary = matchs(item, out path2)))
                                        {
                                            bool ct = !string.IsNullOrEmpty(MainKernel.ContextReplaceAny(path2, replace));
                                            string naddr = MainKernel.PathReplaceAny(path2, replace);
                                            int r = AddResult(GetPageName(item), item, naddr, summary);
                                            bool fn = naddr != path2;
                                            if (ct && fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfnct;
                                            else if (ct) DGV.Rows[r].DefaultCellStyle.BackColor = cct;
                                            else if (fn) DGV.Rows[r].DefaultCellStyle.BackColor = cfn;
                                        }
                                    }
                                    catch (Exception ex) { Log(ex, item); }
                                });
                        }
                    });
            }
        }

        private bool ForEach(Sources items, ForEachHandler func)
        {
            List<string> ls = new List<string>();
            foreach (var item in items.GetEnumerable())
            {
                if (Break) return false;
                if (ls.Exists((it => it == item)))
                {
                    PB.Value = items.Value;
                    continue;
                }
                ls.Add(item);
                func(item);
                ControlService.SetControlThreadSafe(PB, a=> PB.Value = items.Value);
                ControlService.SetControlThreadSafe(l_Message, a => l_Message.Text = ls.Count + " items,,,");
            }
            return true;
        }
        private bool ForEach(IEnumerable<string> items, ForEachHandler func)
        {
            List<string> ls = new List<string>();
            foreach (var item in items)
            {
                if (Break) return false;
                if (ls.Exists((it => it == item))) continue;
                ls.Add(item);
                func(item);
                ControlService.SetControlThreadSafe(l_Message, a=> l_Message.Text = ls.Count + " items,,,");
            }
            return true;
        }
        public List<Sources> GetSources(string address)
        {
            Sources local = new Sources(SourceState.Local);
            Sources net = new Sources(SourceState.Net);
            Sources Web = new Sources(SourceState.Web);
            foreach (var addr in address.Split(new string[] { ";", ",", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Distinct())
                if (string.IsNullOrWhiteSpace(addr)) continue;
                else if (addr.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                {
                    local.AddRange(Directory.GetFiles(addr));
                    if (API.Configuration.RecursiveSource)
                        foreach (var item in Directory.GetDirectories(addr))
                            local.EnumerableItems.Add(MainKernel.GetAllFiles(item, API.Configuration.RecursiveSource));
                }
                else if (addr.Contains("/") || addr.ToLower().StartsWith("www.") || InfoService.IsAbsoluteURL(addr)) Web.Add(ConvertService.ToAbsoluteURL(addr));
                else try
                    {
                        if (File.Exists(addr)) local.Add(addr);
                        else if (Directory.Exists(addr))
                        {
                            local.AddRange(Directory.GetFiles(addr));
                            if (API.Configuration.RecursiveSource)
                                foreach (var item in Directory.GetDirectories(addr))
                                    local.EnumerableItems.Add(MainKernel.GetAllFiles(item, API.Configuration.RecursiveSource));
                        }
                        else Web.Add(addr);
                    }
                    catch
                    {
                        if (Directory.Exists(addr))
                        {
                            local.AddRange(Directory.GetFiles(addr));
                            if (API.Configuration.RecursiveSource)
                                foreach (var item in Directory.GetDirectories(addr))
                                    local.EnumerableItems.Add(MainKernel.GetAllFiles(item, API.Configuration.RecursiveSource));
                        }
                        else Web.Add(addr);
                    }
            return new List<Sources>() { local, net, Web };
        }
        public string GetPageName(string url)
        {
            string[] ada = url.Split(new string[] { "//", "/" }, StringSplitOptions.RemoveEmptyEntries);
            return ada.Length > 1 ? ada[1] : url;
        }
        private int AddResult(string name, string addr, string path, string summary)
        {
            int num = -1;
            ControlService.SetControlThreadSafe(DGV, (aa) =>
            DGV.Rows[num = DGV.Rows.Add(DGV.RowCount + 1, name, addr, API.Configuration.Highlights ? summary : "")].Tag = path);
            Log(DGV.RowCount + "th Found in " + name, 1);
            return num;
        }
        private void ShowItem()
        {
            btn_Ok.Text = "Import all "+ DGV.SelectedRows.Count + " Selected";
            if (!rtb_Result.Visible) return;
            rtb_Result.Clear();
            if (DGV.CurrentRow == null) return;
            try
            {
                ControlService.RichTextBoxAppendWithStyle(ref rtb_Result, DGV.CurrentRow.Cells[FileName.Name].Value + Environment.NewLine + Environment.NewLine, Color.Black, HorizontalAlignment.Center);
                ControlService.RichTextBoxAppendWithStyle(ref rtb_Result, StringService.Compress(DGV.CurrentRow.Cells[Summary.Name].Value + "", 1000, "...") + Environment.NewLine + Environment.NewLine, Color.Black, HorizontalAlignment.Left);
                ControlService.RichTextBoxAppendWithStyle(ref rtb_Result, DGV.CurrentRow.Tag + "", Color.DarkGray, HorizontalAlignment.Left);
                foreach (var v in MainKernel.Searcher.SearchLowerWords) ControlService.RichTextBoxChangeWordColor(ref rtb_Result, v, Color.Red, Color.Yellow);
            }
            catch (Exception ex) { DialogService.ShowMessage(ex); }
        }

        private void Start(string processName)
        {
            ControlService.SetControlThreadSafe(this, (aa) =>
            {
                Log(processName + " STARTED!", 0);
                p_ControlProcess.Visible = true;
                btn_Ok.Enabled =
                btn_Cancel.Enabled =
                p_StartProcess.Visible = tlp_Options.Enabled = false;
                btn_Pause.Image = MiMFa.Properties.Resources.pause_color;
                btn_Pause.Tag = "1";
                Break = false;
                PB.Value = 0;
                PB.Maximum = 0;
                DGV.Rows.Clear();
            });
        }
        private void Log(string message, int status = 0)
        {
            MainKernel.Log(status, message);
            ControlService.SetControlThreadSafe(l_Message, (aa) => l_Message.Text = message);
        }
        private void Log(Exception ex, string addr = "")
        {
            Log("[" + System.IO.Path.GetFileName(addr) + "]\t" + ex.Message,-1);
        }
        private void End(string processName)
        {
            ControlService.SetControlThreadSafe(this, (aa) =>
            {
                Break = true;
                p_ControlProcess.Visible = false;
                btn_Ok.Enabled =
                btn_Cancel.Enabled =
                p_StartProcess.Visible = tlp_Options.Enabled = true;
                btn_Pause.Image = MiMFa.Properties.Resources.pause_color;
                btn_Pause.Tag = "1";
                PB.Value = 0;
                l_Message.Text += " (" + DGV.RowCount + ")";
                Log(processName + " FINISHED!", 0);
                DGV.Refresh();
            });
        }


        private void btn_browse_Click(object sender, EventArgs e)
        {
            bool file = false;
            string dir = "";
            try
            {
                string[] stra = tb_Directory.Text.Split(new string[] { tb_Directory.Tag + "" }, StringSplitOptions.RemoveEmptyEntries);
                dir = stra.First();
                if (dir.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString())) file = false;
                else if (file = File.Exists(dir)) dir = System.IO.Path.GetDirectoryName(dir) + System.IO.Path.DirectorySeparatorChar.ToString();
            }
            catch { }
            if (file)
            {
                OFD.InitialDirectory = dir;
                if (OFD.ShowDialog() == DialogResult.OK)
                    tb_Directory.Text = string.Join(tb_Directory.Tag + "", OFD.FileNames);
            }
            else
            {
                FBD.SelectedPath = dir;
                if (FBD.ShowDialog() == DialogResult.OK)
                    tb_Directory.Text = FBD.SelectedPath + System.IO.Path.DirectorySeparatorChar.ToString();
            }
        }
        private void DGV_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (rtb_Result.Visible)
                ShowItem();
            else try
                {
                    if (DGV.CurrentRow != null) Process.Start((DGV.CurrentRow.Tag ?? DGV.CurrentRow.Cells[Source.Name].Value) + "");
                }
                catch (Exception ex) { DialogService.ShowMessage(ex); }
        }
        private void btn_View_Click(object sender, EventArgs e)
        {
            try
            {
                string[] stra = tb_Directory.Text.Split(new string[] { tb_Directory.Tag + "" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < stra.Length; i++)
                    Process.Start(stra[i]);
            }
            catch { }
        }
        private void btn_ExtDef_Click(object sender, EventArgs e)
        {
            tb_Ext.Text = ".docx,.pptx,.xlsx,.pdf,.ppt,.xls,.zip,.img,.txt,.ini,.cnf,.xml,.htm,.html,.xhtml,.php,.js,.css,.lss,.less,.c,.cs,.class,.h,.cpp,.java,.sh,.swift,.vb,.res,.resx,.rtf,.mrc,.tex,.wpd,.wks,.mrp";
        }
        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rtb_Result.Visible = true;
            ShowItem();
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PB.Maximum = DGV.SelectedRows.Count;
            PB.Value = 0;
            for (int i = 0; i < DGV.SelectedRows.Count; i++)
                try
                {
                    Process.Start((DGV.SelectedRows[i].Tag ?? DGV.CurrentRow.Cells[Source.Name].Value) + "");
                    PB.Value = i;
                }
                catch { }
            PB.Value = 0;
        }
        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(() =>
            {
                try
                {
                    if (SFD.ShowDialog() == DialogResult.OK)
                    {
                        PB.Maximum = DGV.SelectedRows.Count;
                        PB.Value = 0;
                        List<string> ls = new List<string>();
                        for (int i = 0; i < DGV.SelectedRows.Count; i++)
                        {
                            ls.Add((DGV.SelectedRows[i].Tag ?? DGV.CurrentRow.Cells[Source.Name].Value) + "");
                            PB.Value = i;
                        }
                        File.WriteAllLines(SFD.FileName, ls);
                        PB.Value = 0;
                        Log("Result Stored successful!", 1);
                    }
                }
                catch (Exception ex) { Log(ex.Message, -1); }
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }
        private void tsmi_CopyRoot_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(() =>
               {
                   try
                   {
                       PB.Maximum = DGV.SelectedRows.Count;
                       PB.Value = 0;
                       string dir = tsmi_CopyRoot + System.IO.Path.DirectorySeparatorChar.ToString();
                       for (int i = 0; i < DGV.SelectedRows.Count; i++)
                       {
                           string addr = (DGV.SelectedRows[i].Tag ?? DGV.CurrentRow.Cells[Source.Name].Value) + "";
                           File.Copy(addr, dir + System.IO.Path.GetFileName(addr), true);
                           PB.Value = i;
                       }
                       PB.Value = 0;
                       Log("Selected items copied successful!", 0);
                   }
                   catch (Exception ex) { Log(ex.Message, -1); }
               });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }
        private void tsmi_MoveRoot_Click(object sender, EventArgs e)
        {
            if (DialogService.ShowMessage(MessageMode.Warning, true, "Move " + DGV.SelectedRows.Count + (DGV.SelectedRows.Count > 1 ? " Files" : " File"), "Are you sure to move " + DGV.SelectedRows.Count + (DGV.SelectedRows.Count > 1 ? " files" : " file") + " from" + (DGV.SelectedRows.Count > 1 ? " their" : " that") + " main directories?\r\nThe moved file will not be returned!") == DialogResult.Yes)
            {
                Thread th = new Thread(() =>
            {
                try
                {
                    PB.Maximum = DGV.SelectedRows.Count;
                    PB.Value = 0;
                    string dir = tsmi_CopyRoot + System.IO.Path.DirectorySeparatorChar.ToString();
                    for (int i = 0; i < DGV.SelectedRows.Count; i++)
                    {
                        string addr = (DGV.SelectedRows[i].Tag ?? DGV.CurrentRow.Cells[Source.Name].Value) + "";
                        File.Move(addr, dir + System.IO.Path.GetFileName(addr));
                        PB.Value = i;
                    }
                    PB.Value = 0;
                    Log("Selected items moved successful!", 1);
                }
                catch (Exception ex) { Log(ex.Message, -1); }
            });
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
            }
        }
        private void browseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckActivation(1)) return;
            if (FBD.ShowDialog() == DialogResult.OK)
            {
                tsmi_CopyRoot.Text = FBD.SelectedPath;
                tsmi_CopyRoot_Click(tsmi_CopyRoot, EventArgs.Empty);
            }
        }
        private void browseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!CheckActivation(1)) return;
            if (FBD.ShowDialog() == DialogResult.OK)
            {
                tsmi_MoveRoot.Text = FBD.SelectedPath;
                tsmi_MoveRoot_Click(tsmi_MoveRoot, EventArgs.Empty);
            }
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckActivation(1) || DGV.SelectedRows.Count< 1) return;
            if (DialogService.ShowMessage(MessageMode.Warning,true, "Delete " + DGV.SelectedRows.Count + (DGV.SelectedRows.Count > 1 ? " Files" : " File"), "Are you sure to delete " + DGV.SelectedRows.Count + (DGV.SelectedRows.Count > 1 ? " files" : " file") + " from"+ (DGV.SelectedRows.Count > 1 ? " their" : " that") + " main directories?\r\nThe deleted file will not be returned!") == DialogResult.Yes)
            {
                Thread th = new Thread(() =>
              {
                  try
                  {
                      PB.Maximum = DGV.SelectedRows.Count;
                      PB.Value = 0;
                      for (int i = DGV.SelectedRows.Count - 1; i >= 0; i--)
                      {
                          File.Delete((DGV.SelectedRows[i].Tag ?? DGV.CurrentRow.Cells[Source.Name].Value) + "");
                          PB.Value++;
                      }
                      PB.Value = 0;
                      Log("Selected items deleted successful!", 1);
                  }
                  catch (Exception ex) { Log(ex.Message, -1); }
              });
                th.SetApartmentState(ApartmentState.STA);
                th.Start();
            }
        }
        private void tb_Ext_TextChanged(object sender, EventArgs e)
        {
            cb_Ext.Checked = (tb_Ext.Text != "");
        }
        private void InDocument_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.StringFormat)) e.Effect = DragDropEffects.Copy;
        }
        private void InDocument_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files.Length > 0) tb_Directory.Text = string.Join(tb_Directory.Tag + "", files);
                    if (!string.IsNullOrEmpty(tb_Search.Text)) ProcessStart(false);
                }
                else if (e.Data.GetDataPresent(DataFormats.StringFormat))
                    SetData((string)e.Data.GetData(DataFormats.StringFormat), true);
            }
            catch { }
        }
        private void tb_Replace_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.StringFormat)) e.Effect = DragDropEffects.Copy;
        }
        private void tb_Replace_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string tag = ((TextBox)sender).Tag + "";
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (((TextBox)sender).Multiline) ((TextBox)sender).Text = string.Join(Environment.NewLine, files);
                    else ((TextBox)sender).Text = string.Join(tag, files);
                }
                else if (e.Data.GetDataPresent(DataFormats.StringFormat))
                    if (((TextBox)sender).Multiline) ((TextBox)sender).Text = ((string)e.Data.GetData(DataFormats.StringFormat)).Trim();
                    else ((TextBox)sender).Text = ((string)e.Data.GetData(DataFormats.StringFormat)).Trim().Replace(Environment.NewLine, tag).Replace("\n", tag).Replace("\r", tag);
            }
            catch { }
        }
        private void l_title_DoubleClick(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            else WindowState = FormWindowState.Minimized;
        }
        private void InDocument_SizeChanged(object sender, EventArgs e)
        {
        }
        private void l_FullScreen_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
            else WindowState = FormWindowState.Maximized;
        }
        private void cb_ClipBoardCheck_MouseEnter(object sender, EventArgs e)
        {
            cb_ClipBoardCheck.Text = "ClipBoard Check";

        }
        private void cb_ClipBoardCheck_MouseLeave(object sender, EventArgs e)
        {
            cb_ClipBoardCheck.Text = "";
        }
        private void cb_ClipBoardCheck_CheckedChanged(object sender, EventArgs e)
        {
        }
        private void tb_ChangeHeight(object sender, EventArgs e)
        {
            TextBox tb = GetOwnerTextBox(sender);
            if (tb.Height > 40)
            {
                tb.Height = tb.Height / 5;
                tb.Text = string.Join(tb.Tag + "", tb.Text.Split(new string[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries));
            }
            else
            {
                tb.Text = string.Join(Environment.NewLine, tb.Text.Split(new string[] { tb.Tag + "" }, StringSplitOptions.RemoveEmptyEntries));
                tb.Height = tb.Height * 5;
            }
        }
        private void MHK_KeyPressed(object sender, KeyEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            else WindowState = FormWindowState.Minimized;
        }
        private void InDocument_Load(object sender, EventArgs e)
        {
            if (!CheckActivation(0)) Close();
        }
        private void InDocument_Activated(object sender, EventArgs e)
        {
            if (!cb_ClipBoardCheck.Checked) return;
            string str = Clipboard.GetText();
            if (!string.IsNullOrWhiteSpace(str))
                SetData(str, false);
        }
        private void tb_Search_MouseEnter(object sender, EventArgs e)
        {
            if (((TextBox)sender).Lines.Length > 4) ((TextBox)sender).ScrollBars = ScrollBars.Vertical;
        }
        private void tb_Search_MouseLeave(object sender, EventArgs e)
        {
            ((TextBox)sender).ScrollBars = ScrollBars.None;
        }
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetOwnerTextBox(sender).SelectAll();
        }
        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TextBox tb = GetOwnerTextBox(sender);
            if (string.IsNullOrWhiteSpace(tb.SelectedText)) return;
            try { Clipboard.SetText(tb.SelectedText); } catch { }
        }
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextBox tb = GetOwnerTextBox(sender);
            if (string.IsNullOrWhiteSpace(tb.SelectedText)) return;
            try { Clipboard.SetText(tb.SelectedText); } catch { }
            tb.SelectedText = "";
        }
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetOwnerTextBox(sender).SelectedText = Clipboard.GetText();
        }
        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GetOwnerTextBox(sender).SelectedText = "";
        }
        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetOwnerTextBox(sender).Clear();
        }
        private TextBox GetOwnerTextBox(object sender)
        {
            if (this.ActiveControl is TextBox)
                return (TextBox)this.ActiveControl;
            else return tb_Search;
        }
        private void InDocument_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (MainThread != null) try { MainThread.Abort(); } catch { }
            SaveData();
            //if (!Force && DialogService.ShowMessage(MiMFa_MessageType.Question, true, "Are you sure about closing this FocusedGadget?") != DialogResult.Yes) e.Cancel = true;
            //else
            API.End();
        }
        private void tlp_Options_EnabledChanged(object sender, EventArgs e)
        {
            p_SearchIn.Enabled = tlp_Options.Enabled;
        }
        private void cb_InFileName_CheckedChanged(object sender, EventArgs e)
        {
            if (!cb_InContext.Checked && !cb_InFileName.Checked) { ((CheckBox)sender).Checked = true; }
        }
        private void cb_Replace_CheckedChanged(object sender, EventArgs e)
        {
            p_ReplaceGuide.Visible = tb_Replace.Visible = l_Replace.Visible = btn_Replace.Visible = cb_Replace.Checked;
        }
        private void cb_Ext_CheckedChanged(object sender, EventArgs e)
        {
            p_Ext.Visible = l_Ext.Visible = cb_Ext.Checked;
        }
        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            rb.ForeColor = rb.Checked ? Color.FromArgb(7, 83, 184) : Color.White;
        }
        private void tb_Search_Leave(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Height > 40)
            {
                tb.Height = tb.Height / 5;
                tb.Text = string.Join(tb.Tag + "", tb.Text.Split(new string[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        private void DGV_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }
        private void l_Drower_Click(object sender, EventArgs e)
        {
            rtb_Result.Visible = !rtb_Result.Visible;
        }
        private void DGV_SelectionChanged(object sender, EventArgs e)
        {
            ShowItem();
        }
        private void cb_resources_CheckedChanged(object sender, EventArgs e)
        {
            p_result.Visible = cb_resources.Checked;
        }
        private void label3_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void btn_Ok_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        private void tb_Directory_TextChanged(object sender, EventArgs e)
        {
            if (MainKernel == null) return;
            var sou = GetSources(tb_Directory.Text);
            if (sou[2].Count > 0) btn_browse.Visible = false;
            else btn_browse.Visible = true;
        }

    }
}
