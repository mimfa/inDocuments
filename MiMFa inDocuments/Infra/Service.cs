using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace MiMFa.UIL.Searcher.Infra
{
    class Service
    {
        #region RichTextBox
        public static RichTextBox RichTextBoxAppendWithStyle(ref RichTextBox rtb, string text, Color color, HorizontalAlignment ha)
        {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionColor = color;
            rtb.SelectionAlignment = ha;
            rtb.SelectedText = text;
            rtb.ScrollToCaret();
            return rtb;
        }
        public static RichTextBox RichTextBoxAppendWithStyle(ref RichTextBox rtb, string text, Color color)
        {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionColor = color;
            rtb.SelectedText = text;
            rtb.ScrollToCaret();
            return rtb;
        }

        #endregion

    }
}
