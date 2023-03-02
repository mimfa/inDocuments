using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiMFa.UIL.Searcher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Support.Kernel.Initialize();
            inDocuments app = new inDocuments();
            foreach (var item in args)
                app.SetData(item, false);
            Application.Run(app);
        }
    }
}
