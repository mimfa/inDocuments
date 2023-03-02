using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Xml;
using System.Linq;
using System.Net;
using System.Text;
using MiMFa.UIL.Searcher.Infra;
using MiMFa.General;
using System.Windows.Forms;
using MiMFa.Engine;
using MiMFa.Service;
using MiMFa.UIL.Searcher.Model;

namespace MiMFa.UIL.Searcher.Compute
{
    public class Kernel
    {
        public event LogHandler Logger = (t, m) => { };
        public MiMFa.Engine.Search Searcher { get; set; } = null;
        public void Log(int type, string message) => Logger( type,  message);

        public MiMFa.Exclusive.Technology.Text.Do Simple = new MiMFa.Exclusive.Technology.Text.Do();
        public MiMFa.Exclusive.Technology.PDF.Do PDF = new MiMFa.Exclusive.Technology.PDF.Do();
        public MiMFa.Exclusive.Technology.Excel.Do Excel = new MiMFa.Exclusive.Technology.Excel.Do();
        public MiMFa.Exclusive.Technology.Word.Do Word = new MiMFa.Exclusive.Technology.Word.Do();
        public MiMFa.Exclusive.Technology.PowerPoint.Do PowerPoint = new MiMFa.Exclusive.Technology.PowerPoint.Do();
        public MiMFa.Exclusive.Technology.XML.Do XML = new MiMFa.Exclusive.Technology.XML.Do();
        public MiMFa.Exclusive.Technology.Zip.Do Zip = new MiMFa.Exclusive.Technology.Zip.Do();
        public MiMFa.Exclusive.Technology.HTML.Do HTML = new MiMFa.Exclusive.Technology.HTML.Do();
        public MiMFa.Exclusive.Technology.Web.Convert Web = new MiMFa.Exclusive.Technology.Web.Convert();

        public Kernel(LogHandler connector_Logger)
        {
            Logger = connector_Logger;
        }
        public Kernel()
        {
        }


        #region Local
        public IEnumerable<string> GetAllFiles(string directoryAddress, bool reclcive)
        {
            if (Directory.Exists(directoryAddress))
                foreach (var file in Directory.GetFiles(directoryAddress))
                    yield return file;
            if (reclcive)
                foreach (var item in Directory.GetDirectories(directoryAddress))
                    foreach (var file in GetAllFiles(item, reclcive))
                        yield return file;
        }

        public string FileNameAndContextWithExtensionMatchAny(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return FileNameAndContextMatchAny(addr);
            return null;
        }
        public string FileNameAndContextWithExtensionMatchLike(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return FileNameAndContextMatchLike(addr);
            return null;
        }
        public string FileNameAndContextWithExtensionMatchSame(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return FileNameAndContextMatchSame(addr);
            return null;
        }
        public string FileNameAndContextWithExtensionMatchPattern(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return FileNameAndContextMatchPattern(addr);
            return null;
        }
        public string ContextWithExtensionMatchAny(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return ContextMatchAny(addr);
            return null;
        }
        public string ContextWithExtensionMatchLike(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return ContextMatchLike(addr);
            return null;
        }
        public string ContextWithExtensionMatchSame(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return ContextMatchSame(addr);
            return null;
        }
        public string ContextWithExtensionMatchPattern(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return ContextMatchPattern(addr);
            return null;
        }
        public string FileNameWithExtensionMatchAny(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return FileNameMatchAny(addr);
            return null;
        }
        public string FileNameWithExtensionMatchLike(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return FileNameMatchLike(addr);
            return null;
        }
        public string FileNameWithExtensionMatchSame(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return FileNameMatchSame(addr);
            return null;
        }
        public string FileNameWithExtensionMatchPattern(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return FileNameMatchPattern(addr);
            return null;
        }
        public string FileNameAndContextMatchAny(string addr)
        {
            string res = null;
            res = FileNameMatchAny(addr);
            if (string.IsNullOrEmpty(res))res = ContextMatchAny(addr);
            return res;
        }
        public string FileNameAndContextMatchLike(string addr)
        {
            string res = null;
            res = FileNameMatchLike(addr);
            if (string.IsNullOrEmpty(res)) res = ContextMatchLike(addr);
            return res;
        }
        public string FileNameAndContextMatchSame(string addr)
        {
            string res = null;
            res = FileNameMatchSame(addr);
            if (string.IsNullOrEmpty(res)) res = ContextMatchSame(addr);
            return res;
        }
        public string FileNameAndContextMatchPattern(string addr)
        {
            string res = null;
            res = FileNameMatchPattern(addr);
            if (string.IsNullOrEmpty(res)) res = ContextMatchPattern(addr);
            return res;
        }
        public  string FileNameMatchAny(string addr)
        {
            return Simple.FindAny(Searcher, Path.GetFileName(addr));
        }
        public string FileNameMatchLike(string addr)
        {
            return Simple.FindLike(Searcher, Path.GetFileName(addr));
        }
        public string FileNameMatchSame(string addr)
        {
            return Simple.FindSame(Searcher, Path.GetFileName(addr));
        }
        public string FileNameMatchPattern(string addr)
        {
            return Simple.FindPattern(Searcher, Path.GetFileName(addr));
        }
        public string ContextMatchAny(string addr)
        {
            switch (InfoService.GetMimeFile(addr))
            {
                case "doc":
                case "application/msword":
                case "docx":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    return Word.FindAny(addr, Searcher);
                case "xls":
                case "application/vnd.ms-excel":
                case "xlsx":
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    return Excel.FindAny(addr, Searcher);
                case "ppt":
                case "application/vnd.ms-powerpoint":
                case "pptx":
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                    return PowerPoint.FindAny(addr, Searcher);
                case "application/octet-stream":
                case "application/x-zip-compressed":
                    string ext = System.IO.Path.GetExtension(addr).ToLower();
                    switch (ext)
                    {
                        case ".ppt":
                        case ".pptx":
                            return PowerPoint.FindAny(addr, Searcher);
                        case ".xls":
                        case ".xlsx":
                            return Excel.FindAny(addr, Searcher);
                        case ".doc":
                        case ".docx":
                            return Word.FindAny(addr, Searcher);
                        case ".zip":
                            string res = null;
                            foreach (var item in ((MiMFa.Exclusive.Technology.Zip.Convert)Zip.Converter).ToFiles(addr))
                                if (!string.IsNullOrEmpty(res = ContextMatchAny(item))) return res;
                            return null;
                        case "text/xml":
                            return XML.FindAnyInAll(addr, Searcher);
                        case "text/html":
                            return HTML.FindAnyInAll(addr, Searcher);
                        default:
                            return Simple.FindAny(addr, Searcher);
                    }
                case "pdf":
                case "application/pdf":
                    return PDF.FindAny(addr, Searcher);
                case "text/xml":
                    return XML.FindAnyInAll(addr, Searcher);
                case "text/html":
                    return HTML.FindAnyInAll(addr, Searcher);
                default:
                    return Simple.FindAny(addr, Searcher);
            }
        }
        public string ContextMatchLike(string addr)
        {
            switch (InfoService.GetMimeFile(addr))
                {
                    case "doc":
                    case "application/msword":
                    case "docx":
                    case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                        return Word.FindLike(addr,Searcher);
                    case "xls":
                    case "application/vnd.ms-excel":
                    case "xlsx":
                    case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                        return Excel.FindLike(addr, Searcher);
                    case "ppt":
                    case "application/vnd.ms-powerpoint":
                    case "pptx":
                    case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                        return PowerPoint.FindLike(addr, Searcher);
                    case "application/octet-stream":
                    case "application/x-zip-compressed":
                        string ext = System.IO.Path.GetExtension(addr).ToLower();
                        switch (ext)
                        {
                            case ".ppt":
                            case ".pptx":
                                return PowerPoint.FindLike(addr, Searcher);
                            case ".xls":
                            case ".xlsx":
                                return Excel.FindLike(addr, Searcher);
                            case ".doc":
                            case ".docx":
                                return Word.FindLike(addr, Searcher);
                        case ".zip":
                            string res = null;
                            foreach (var item in ((MiMFa.Exclusive.Technology.Zip.Convert)Zip.Converter).ToFiles(addr))
                                if (!string.IsNullOrEmpty(res = ContextMatchLike(item))) return res;
                            return null;
                        case "text/xml":
                            return XML.FindLikeInAll(addr, Searcher);
                        case "text/html":
                            return HTML.FindLikeInAll(addr, Searcher);
                        default:
                                return Simple.FindLike(addr, Searcher);
                        }
                    case "pdf":
                    case "application/pdf":
                        return PDF.FindLike(addr, Searcher);
                case "text/xml":
                    return XML.FindLikeInAll(addr, Searcher);
                case "text/html":
                    return HTML.FindLikeInAll(addr, Searcher);
                default:
                        return Simple.FindLike(addr, Searcher);
                }
        }
        public string ContextMatchSame(string addr)
        {
            switch (InfoService.GetMimeFile(addr))
            {
                case "doc":
                case "application/msword":
                case "docx":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    return Word.FindSame(addr, Searcher);
                case "xls":
                case "application/vnd.ms-excel":
                case "xlsx":
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    return Excel.FindSame(addr, Searcher);
                case "ppt":
                case "application/vnd.ms-powerpoint":
                case "pptx":
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                    return PowerPoint.FindSame(addr, Searcher);
                case "application/octet-stream":
                case "application/x-zip-compressed":
                case "application/x-gzip-compressed":
                    string ext = System.IO.Path.GetExtension(addr).ToLower();
                    switch (ext)
                    {
                        case ".ppt":
                        case ".pptx":
                            return PowerPoint.FindSame(addr, Searcher);
                        case ".xls":
                        case ".xlsx":
                            return Excel.FindSame(addr, Searcher);
                        case ".doc":
                        case ".docx":
                            return Word.FindSame(addr, Searcher);
                        case ".zip":
                        case ".cab":
                        case ".rar":
                        case ".htm":
                        case ".html":
                            string res = null;
                            foreach (var item in ((MiMFa.Exclusive.Technology.Zip.Convert)Zip.Converter).ToFiles(addr))
                                if (!string.IsNullOrEmpty(res = ContextMatchSame(item)))
                                    return res;
                            return null;
                        default:
                            return Simple.FindSame(addr, Searcher);
                    }
                case "pdf":
                case "application/pdf":
                    return PDF.FindSame(addr, Searcher);
                case "text/xml":
                    return XML.FindSameInAll(addr, Searcher);
                case "text/html":
                    return HTML.FindSameInAll(addr, Searcher);
                default:
                    return Simple.FindSame(addr, Searcher);
            }
        }
        public string ContextMatchPattern(string addr)
        {
            switch (InfoService.GetMimeFile(addr))
            {
                case "doc":
                case "application/msword":
                case "docx":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    return Word.FindPattern(addr, Searcher);
                case "xls":
                case "application/vnd.ms-excel":
                case "xlsx":
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    return Excel.FindPattern(addr, Searcher);
                case "ppt":
                case "application/vnd.ms-powerpoint":
                case "pptx":
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                    return PowerPoint.FindPattern(addr, Searcher);
                case "application/octet-stream":
                case "application/x-zip-compressed":
                    string ext = System.IO.Path.GetExtension(addr).ToLower();
                    switch (ext)
                    {
                        case ".ppt":
                        case ".pptx":
                            return PowerPoint.FindPattern(addr, Searcher);
                        case ".xls":
                        case ".xlsx":
                            return Excel.FindPattern(addr, Searcher);
                        case ".doc":
                        case ".docx":
                            return Word.FindPattern(addr, Searcher);
                        case ".zip":
                            string res = null;
                            foreach (var item in ((MiMFa.Exclusive.Technology.Zip.Convert)Zip.Converter).ToFiles(addr))
                                if (!string.IsNullOrEmpty(res = ContextMatchPattern(item))) return res;
                            return null;
                        case "text/xml":
                            return XML.FindPatternInAll(addr, Searcher);
                        case "text/html":
                            return HTML.FindPatternInAll(addr, Searcher);
                        default:
                            return Simple.FindPattern(addr, Searcher);
                    }
                case "pdf":
                case "application/pdf":
                    return PDF.FindPattern(addr, Searcher);
                case "text/xml":
                    return XML.FindPatternInAll(addr, Searcher);
                case "text/html":
                    return HTML.FindPatternInAll(addr, Searcher);
                default:
                    return Simple.FindPattern(addr, Searcher);
            }
        }


        public string PathReplaceAny(string addr, string replace)
        {
            if (!API.Configuration.InName) return addr;
            string name = Path.GetFileName(addr);
            if (Searcher.FindAnyIn(name))
                try
                {
                    string naddr = Path.GetDirectoryName(addr) + Path.DirectorySeparatorChar + Searcher.ReplaceAnyIn(name, replace);
                    File.Move(addr, naddr);
                    return naddr;
                }
                catch (Exception ex) { Log(-1, ex.Message); }
            return addr;
        }
        public string PathReplaceLike(string addr, string replace)
        {
            if (!API.Configuration.InName) return addr;
            string name = Path.GetFileName(addr);
            if (Searcher.FindLikeIn(name))
                try
                {
                    string naddr = Path.GetDirectoryName(addr) + Path.DirectorySeparatorChar + Searcher.ReplaceLikeIn(name, replace);
                    File.Move(addr, naddr);
                    return naddr;
                }
                catch (Exception ex) { Log(-1, ex.Message); }
            return addr;
        }
        public string PathReplaceSame(string addr,  string replace)
        {
            if (!API.Configuration.InName) return addr;
            string name = Path.GetFileName(addr);
            if (Searcher.FindSameIn(name))
                try
                {
                    string naddr = Path.GetDirectoryName(addr) + Path.DirectorySeparatorChar + Searcher.ReplaceSameIn(name, replace);
                    File.Move(addr, naddr);
                    return naddr;
                }
                catch (Exception ex) { Log(-1, ex.Message); }
            return addr;
        }
        public string PathReplacePattern(string addr,  string replace)
        {
            if (!API.Configuration.InName) return addr;
            string name = Path.GetFileName(addr);
            if (Searcher.FindPatternIn(name))
                try
                {
                    string naddr = Path.GetDirectoryName(addr) + Path.DirectorySeparatorChar + Searcher.ReplacePatternIn(name, replace);
                    File.Move(addr, naddr);
                    return naddr;
                }
                catch (Exception ex) { Log(-1, ex.Message); }
            return addr;
        }
        public string ContextReplaceAny(string addr, string replace)
        {
            switch (InfoService.GetMimeFile(addr))
            {
                case "doc":
                case "application/msword":
                case "docx":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    return Word.ReplaceAny(addr, Searcher, replace);
                    break;
                case "xls":
                case "application/vnd.ms-excel":
                case "xlsx":
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    return Excel.ReplaceAny(addr, Searcher, replace);
                    break;
                case "ppt":
                case "application/vnd.ms-powerpoint":
                case "pptx":
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                    return PowerPoint.ReplaceAny(addr, Searcher, replace);
                    break;
                case "application/octet-stream":
                case "application/x-zip-compressed":
                    string ext = System.IO.Path.GetExtension(addr).ToLower();
                    switch (ext)
                    {
                        case ".ppt":
                        case ".pptx":
                            return PowerPoint.ReplaceAny(addr, Searcher, replace);
                            break;
                        case ".xls":
                        case ".xlsx":
                            return Excel.ReplaceAny(addr, Searcher, replace);
                            break;
                        case ".doc":
                        case ".docx":
                            return Word.ReplaceAny(addr, Searcher, replace);
                            break;
                        case ".xml":
                            return XML.ReplaceAny(addr, Searcher, replace);
                            break;
                        case ".htm":
                        case ".htmx":
                        case ".html":
                        case ".htmlx":
                            return HTML.ReplaceAny(addr, Searcher, replace);
                            break;
                        default:
                            return Simple.ReplaceAny(addr, Searcher, replace);
                            break;
                    }
                    break;
                case "pdf":
                case "application/pdf":
                    return PDF.ReplaceAny(addr, Searcher, replace);
                    break;
                case "text/xml":
                    return XML.ReplaceAny(addr, Searcher, replace);
                    break;
                case "text/htm":
                case "text/html":
                case "text/htmlx":
                    return HTML.ReplaceAny(addr, Searcher, replace);
                    break;
                default:
                    return Simple.ReplaceAny(addr, Searcher, replace);
                    break;
            }
        }
        public string ContextReplaceLike(string addr,  string replace)
        {
            switch (InfoService.GetMimeFile(addr))
            {
                case "doc":
                case "application/msword":
                case "docx":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    return Word.ReplaceLike(addr, Searcher,replace);
                    break;
                case "xls":
                case "application/vnd.ms-excel":
                case "xlsx":
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    return Excel.ReplaceLike(addr, Searcher, replace);
                    break;
                case "ppt":
                case "application/vnd.ms-powerpoint":
                case "pptx":
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                    return PowerPoint.ReplaceLike(addr, Searcher, replace);
                    break;
                case "application/octet-stream":
                case "application/x-zip-compressed":
                    string ext = System.IO.Path.GetExtension(addr).ToLower();
                    switch (ext)
                    {
                        case ".ppt":
                        case ".pptx":
                            return PowerPoint.ReplaceLike(addr, Searcher, replace);
                            break;
                        case ".xls":
                        case ".xlsx":
                            return Excel.ReplaceLike(addr, Searcher, replace);
                            break;
                        case ".doc":
                        case ".docx":
                            return Word.ReplaceLike(addr, Searcher, replace);
                            break;
                        case ".xml":
                            return XML.ReplaceLike(addr, Searcher, replace);
                            break;
                        case ".htm":
                        case ".htmx":
                        case ".html":
                        case ".htmlx":
                            return HTML.ReplaceLike(addr, Searcher, replace);
                            break;
                        default:
                            return Simple.ReplaceLike(addr, Searcher, replace);
                            break;
                    }
                    break;
                case "pdf":
                case "application/pdf":
                    return PDF.ReplaceLike(addr, Searcher, replace);
                    break;
                case "text/xml":
                    return XML.ReplaceLike(addr, Searcher, replace);
                    break;
                case "text/htm":
                case "text/html":
                case "text/htmlx":
                    return HTML.ReplaceLike(addr, Searcher, replace);
                    break;
                default:
                    return Simple.ReplaceLike(addr, Searcher, replace);
                    break;
            }
        }
        public string ContextReplaceSame(string addr, string replace)
        {
            switch (InfoService.GetMimeFile(addr))
            {
                case "doc":
                case "application/msword":
                case "docx":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    return Word.ReplaceSame(addr, Searcher, replace);
                    break;
                case "xls":
                case "application/vnd.ms-excel":
                case "xlsx":
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    return Excel.ReplaceSame(addr, Searcher, replace);
                    break;
                case "ppt":
                case "application/vnd.ms-powerpoint":
                case "pptx":
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                    return PowerPoint.ReplaceSame(addr, Searcher, replace);
                    break;
                case "application/octet-stream":
                case "application/x-zip-compressed":
                    string ext = System.IO.Path.GetExtension(addr).ToLower();
                    switch (ext)
                    {
                        case ".ppt":
                        case ".pptx":
                            return PowerPoint.ReplaceSame(addr, Searcher, replace);
                            break;
                        case ".xls":
                        case ".xlsx":
                            return Excel.ReplaceSame(addr, Searcher, replace);
                            break;
                        case ".doc":
                        case ".docx":
                            return Word.ReplaceSame(addr, Searcher, replace);
                            break;
                        case ".xml":
                            return XML.ReplaceSame(addr, Searcher, replace);
                            break;
                        case ".htm":
                        case ".htmx":
                        case ".html":
                        case ".htmlx":
                            return HTML.ReplaceSame(addr, Searcher, replace);
                            break;
                        default:
                            return Simple.ReplaceSame(addr, Searcher, replace);
                            break;
                    }
                    break;
                case "pdf":
                case "application/pdf":
                    return PDF.ReplaceSame(addr, Searcher, replace);
                    break;
                case "text/xml":
                    return XML.ReplaceSame(addr, Searcher, replace);
                    break;
                case "text/htm":
                case "text/html":
                case "text/htmlx":
                    return HTML.ReplaceSame(addr, Searcher, replace);
                    break;
                default:
                    return Simple.ReplaceSame(addr, Searcher, replace);
                    break;
            }
        }
        public string ContextReplacePattern(string addr, string replace)
        {
            switch (InfoService.GetMimeFile(addr))
            {
                case "doc":
                case "application/msword":
                case "docx":
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    return Word.ReplacePattern(addr, Searcher, replace);
                    break;
                case "xls":
                case "application/vnd.ms-excel":
                case "xlsx":
                case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
                    return Excel.ReplacePattern(addr, Searcher, replace);
                    break;
                case "ppt":
                case "application/vnd.ms-powerpoint":
                case "pptx":
                case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
                    return PowerPoint.ReplacePattern(addr, Searcher, replace);
                    break;
                case "application/octet-stream":
                case "application/x-zip-compressed":
                    string ext = System.IO.Path.GetExtension(addr).ToLower();
                    switch (ext)
                    {
                        case ".ppt":
                        case ".pptx":
                            return PowerPoint.ReplacePattern(addr, Searcher, replace);
                            break;
                        case ".xls":
                        case ".xlsx":
                            return Excel.ReplacePattern(addr, Searcher, replace);
                            break;
                        case ".doc":
                        case ".docx":
                            return Word.ReplacePattern(addr, Searcher, replace);
                            break;
                        case ".xml":
                            return XML.ReplacePattern(addr, Searcher, replace);
                            break;
                        case ".htm":
                        case ".htmx":
                        case ".html":
                        case ".htmlx":
                            return HTML.ReplacePattern(addr, Searcher, replace);
                            break;
                        default:
                            return Simple.ReplacePattern(addr, Searcher, replace);
                            break;
                    }
                    break;
                case "pdf":
                case "application/pdf":
                    return PDF.ReplacePattern(addr, Searcher, replace);
                    break;
                case "text/xml":
                    return XML.ReplacePattern(addr, Searcher, replace);
                    break;
                case "text/htm":
                case "text/html":
                case "text/htmlx":
                    return HTML.ReplacePattern(addr, Searcher, replace);
                    break;
                default:
                    return Simple.ReplacePattern(addr, Searcher, replace);
                    break;
            }
        }
        #endregion


        #region Web
        public string URLAndPageWithExtensionMatchAny(string addr, out string path)
        {
            path = null;
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return URLAndPageMatchAny(addr,out path);
            return null;
        }
        public string URLAndPageWithExtensionMatchLike(string addr, out string path)
        {
            path = null;
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return URLAndPageMatchLike(addr,out path);
            return null;
        }
        public string URLAndPageWithExtensionMatchSame(string addr, out string path)
        {
            path = null;
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return URLAndPageMatchSame(addr,out path);
            return null;
        }
        public string URLAndPageWithExtensionMatchPattern(string addr, out string path)
        {
            path = null;
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return URLAndPageMatchPattern(addr,out path);
            return null;
        }
        public string PageWithExtensionMatchAny(string addr , out string path)
        {
            path = null;
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return PageMatchAny(addr,out path);
            return null;
        }
        public string PageWithExtensionMatchLike(string addr, out string path)
        {
            path = null;
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return PageMatchLike(addr,out path);
            return null;
        }
        public string PageWithExtensionMatchSame(string addr, out string path)
        {
            path = null;
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return PageMatchSame(addr,out path);
            return null;
        }
        public string PageWithExtensionMatchPattern(string addr, out string path)
        {
            path = null;
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return PageMatchPattern(addr,out path);
            return null;
        }
        public string URLWithExtensionMatchAny(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return URLMatchAny(addr);
            return null;
        }
        public string URLWithExtensionMatchLike(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return URLMatchLike(addr);
            return null;
        }
        public string URLWithExtensionMatchSame(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return URLMatchSame(addr);
            return null;
        }
        public string URLWithExtensionMatchPattern(string addr)
        {
            if (API.Configuration.Extensions.Exists((ext) => addr.EndsWith(ext))) return URLMatchPattern(addr);
            return null;
        }
        public string URLAndPageMatchAny(string addr, out string path)
        {
            path = null;
            string res = null;
            res = URLMatchAny(addr);
            if (string.IsNullOrEmpty(res)) res = PageMatchAny(addr,out path);
            return res;
        }
        public string URLAndPageMatchLike(string addr, out string path)
        {
            path = null;
            string res = null;
            res = URLMatchLike(addr);
            if (string.IsNullOrEmpty(res)) res = PageMatchLike(addr, out path);
            return res;
        }
        public string URLAndPageMatchSame(string addr, out string path)
        {
            path = null;
            string res = null;
            res = URLMatchSame(addr);
            if (string.IsNullOrEmpty(res)) res = PageMatchSame(addr, out path);
            return res;
        }
        public string URLAndPageMatchPattern(string addr, out string path)
        {
            path = null;
            string res = null;
            res = URLMatchPattern(addr);
            if (string.IsNullOrEmpty(res)) res = PageMatchPattern(addr, out path);
            return res;
        }
        public string URLMatchAny(string addr)
        {
            return Simple.FindAny(Searcher, addr);
        }
        public string URLMatchLike(string addr)
        {
            return Simple.FindLike(Searcher, addr);
        }
        public string URLMatchSame(string addr)
        {
            return Simple.FindSame(Searcher, addr);
        }
        public string URLMatchPattern(string addr)
        {
            return Simple.FindPattern(Searcher, addr);
        }
        public string PageMatchAny(string addr, out string path)
        {
            path = Web.ToPath(addr);
            return  ContextMatchAny(path);
        }
        public string PageMatchLike(string addr, out string path)
        {
             path = Web.ToPath(addr);
            return ContextMatchLike(path) ;
        }
        public string PageMatchSame(string addr, out string path)
        {
            path = Web.ToPath(addr);
            return  ContextMatchSame(path);
        }
        public string PageMatchPattern(string addr, out string path)
        {
             path = Web.ToPath(addr);
            return ContextMatchPattern(path);
        }

        public IEnumerable<string> FetchURLs(Uri uri, string path)
        {
            HtmlDocument html = null;
            try { html = ((MiMFa.Exclusive.Technology.HTML.Convert)HTML.Converter).ToHtmlDocument(path); } catch (Exception ex) { yield break; }
            string attr = null;
            string root = "about:";
            foreach (HtmlElement item in html.Links)
                if ((attr = item.GetAttribute("HREF").Trim()) != null)
                    yield return ConvertService.ToAbsoluteURL(uri,
                        attr.Length > root.Length && attr.StartsWith(root)? attr.Substring(root.Length) : attr);
        }
        #endregion

    }
}
