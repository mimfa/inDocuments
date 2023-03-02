using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MiMFa.UIL.Searcher.Model;
using MiMFa.Exclusive.Animate;

namespace MiMFa.UIL.Searcher
{
    public static class API
    {
        public static Configuration Configuration { get; set; } = new Configuration();
        public static string ConfigurationPath { get; set; } = MiMFa.Config.ConfigurationDirectory + "Search" + MiMFa.Config.ConfigurationExtension;
        public static string[] ConfigurationDelimited { get; set; } = new string[] { "->" };

        public static void Start()
        {
            MiMFa.Config config = new MiMFa.Config();
            MiMFa.Config.ProductSignature = "fg/d"; 
            Configuration = new Configuration();
            Configuration.Load();
        }
        public static void End()
        {
        }
    }
}
