using MiMFa.General;
using MiMFa.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiMFa.UIL.Searcher.Model
{
    public class Configuration
    {
        public string Sources { get; set; } = MiMFa.Config.URL;
        public string Search { get; set; } = "";
        public string Replace { get; set; } = "";
        public List<string> Extensions { get; set; } = new List<string>();
        public int Sensitive { get; set; } = 1;
        public bool ExtensionFilter { get; set; } = false;
        public bool RecursiveSource { get; set; } = true;
        public bool AllowReplace { get; set; } = false;
        public bool InName { get; set; } = true;
        public bool InContext { get; set; } = true;

        public bool ShowByHotKey { get; set; } = true;
        public bool ClipBoardCheck { get; set; } = false;
        public bool Highlights { get; set; } = true;

        public string ID { get; set; } = "";


        public void Load()
        {
            if (!File.Exists(API.ConfigurationPath)) File.WriteAllText(API.ConfigurationPath,"");
            string[] configs = File.ReadAllLines(API.ConfigurationPath,Encoding.UTF8);
            foreach (var item in configs)
            {
                string[] kvp = item.Split(API.ConfigurationDelimited, StringSplitOptions.None);
                if (kvp.Length > 1)
                    switch (ConvertService.ToUnSigned(kvp.First().Trim().ToLower()))
                    {
                        case "id":
                            ID = kvp.Last().Trim();
                            break;
                        case "e":
                        case "ext":
                        case "extension":
                        case "extensions":
                        case "filter":
                        case "filters":
                            Extensions = kvp.Last().Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
                            break;
                        case "s":
                        case "search":
                            Search = kvp.Last().Trim().Replace("¶",Environment.NewLine);
                            break;
                        case "r":
                        case "replace":
                            Replace = kvp.Last().Trim().Replace("¶", Environment.NewLine);
                            break;
                        case "sour":
                        case "url":
                        case "urls":
                        case "uri":
                        case "uris":
                        case "path":
                        case "paths":
                        case "addr":
                        case "address":
                        case "addresses":
                        case "source":
                        case "sources":
                        case "sourcedirectory":
                        case "sourcedirectories":
                        case "dir":
                        case "dirs":
                        case "directory":
                        case "directories":
                            Sources = kvp.Last().Trim();
                            break;
                        case "hl":
                        case "hilight":
                        case "highlight":
                        case "highlights":
                            try { Highlights = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "case":
                        case "sensitive":
                        case "sens":
                        case "casesens":
                        case "casesensitive":
                            try { Sensitive = Convert.ToInt16(kvp.Last().Trim()); } catch { }
                            break;
                        case "a":
                        case "af":
                        case "ae":
                        case "allowext":
                        case "allowextension":
                        case "extensionfilter":
                        case "allowextensionfilter":
                            try { ExtensionFilter = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "rs":
                        case "recursour":
                        case "recursive":
                        case "recursivesource":
                            try { RecursiveSource = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "ar":
                        case "allowreplace":
                            try { AllowReplace = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "if":
                        case "inname":
                        case "inpath":
                        case "inurl":
                        case "inuri":
                        case "infilename":
                            try { InName = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "ic":
                        case "incontext":
                        case "incontent":
                        case "inpage":
                        case "incon":
                        case "indoc":
                        case "indocument":
                        case "indocuments":
                            try { InContext = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "hk":
                        case "sbhk":
                        case "hotkey":
                        case "shorcut":
                        case "sk":
                        case "shorcutkey":
                        case "showbyhotkey":
                            try { ShowByHotKey = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        case "cb":
                        case "cbc":
                        case "cc":
                        case "clipboard":
                        case "clipcheck":
                        case "clipboardcheck":
                            try { ClipBoardCheck = Convert.ToBoolean(kvp.Last().Trim()); } catch { }
                            break;
                        default:
                            break;
                    }
            }
        }
        public void Save()
        {
            List<string> configs = new List<string>()
            {
               "ID" + API.ConfigurationDelimited.First() + ID,
               "Sources" + API.ConfigurationDelimited.First() + Sources,
                "Search" + API.ConfigurationDelimited.First() + Search.Replace(Environment.NewLine,"¶").Replace("\n","¶").Replace("\r","¶") ,
                "Replace" + API.ConfigurationDelimited.First() + Replace.Replace(Environment.NewLine,"¶").Replace("\n","¶").Replace("\r","¶") ,
                "Extensions" + API.ConfigurationDelimited.First() + string.Join(";",Extensions),
               "Sensitive" + API.ConfigurationDelimited.First() + Sensitive,
               "ExtensionFilter" + API.ConfigurationDelimited.First() + ExtensionFilter,
               "RecursiveSource" + API.ConfigurationDelimited.First() + RecursiveSource,
               "AllowReplace" + API.ConfigurationDelimited.First() + AllowReplace,
               "InName" + API.ConfigurationDelimited.First() + InName,
               "InContext" + API.ConfigurationDelimited.First() + InContext,
               "ShowByHotKey" + API.ConfigurationDelimited.First() + ShowByHotKey,
               "ClipBoardCheck" + API.ConfigurationDelimited.First() + ClipBoardCheck,
               "Highlights" + API.ConfigurationDelimited.First() + Highlights,
            };
            File.WriteAllLines(API.ConfigurationPath, configs);
        }
    }
}
