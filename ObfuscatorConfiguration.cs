using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLDataObfuscator
{
    class ObfuscatorConfiguration
    {

        public static ObfuscatorConfiguration Current { get; private set; }

        static ObfuscatorConfiguration()
        {
            Current = LoadConfiguration();
        }

        private static ObfuscatorConfiguration LoadConfiguration()
        {
            ObfuscatorConfiguration config = null;
            var configPath = Path.Combine(Environment.CurrentDirectory, "SQLDataObfuscator.Configuration.json");
            Console.WriteLine("Config path " + configPath);
            using (var f = File.OpenText(configPath))
            {
                var configstring = f.ReadToEnd();
                config = JsonConvert.DeserializeObject<ObfuscatorConfiguration>(configstring);
                f.Close();
            }

            return config;
        }



        public static string ServiceDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private ObfuscatorConfiguration()
        {
            Queries = new List<Query>();
        }

        public int UploadTimeoutHours { get; set; }

        public string ApiUrl { get; set; }

        public string ServiceHostUrl { get; set; }

        public string TempPath { get; set; }

        public List<Query> Queries { get; set; }

        public string Source { get; set; }

        public string ServiceBusNamespace { get; set; }

        public string ContainerName { get; set; }

        public int Interval { get; set; }

        public DateTime StartTime { get; set; }

        public bool RunOnStart { get; set; }


        public string NullString { get; set; }
        public DateTime ExtractStartDate { get; set; }
        public DateTime ExtractEndDate { get; set; }


    }

}
