using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace SQLDataObfuscator
{
    class Obfuscator
    {

        private static Thread _thread;
            
        public static void Start()
        {
            var Queries = ObfuscatorConfiguration.Current.Queries.Where(x => x.Enabled == true);

            var dateStart = ObfuscatorConfiguration.Current.ExtractStartDate;
            var dateEnd = ObfuscatorConfiguration.Current.ExtractEndDate;
            Console.WriteLine("-------------------------started at  [{0}] ", DateTime.Now);
            Console.WriteLine("extract from  [{0}] to [{1}]", dateStart.ToShortDateString(), dateEnd.ToShortDateString());
            

            ExtractAndObfuscateDataToFile(Queries, dateStart, dateEnd);
            

            Console.WriteLine("completed at  [{0}] ", DateTime.Now);
            Console.ReadLine();
        }

        private static void ExtractAndObfuscateDataToFile(IEnumerable<Query> queries, DateTime dateStart, DateTime dateEnd)
        {

            foreach (var allowedQuery in queries)
            {
                if (allowedQuery.Enabled)
                {

                    var sqlBlobService = new UploaderService();

                    sqlBlobService.UploadCsv(allowedQuery, dateStart, dateEnd);

                }
            }
        }
    }
}
