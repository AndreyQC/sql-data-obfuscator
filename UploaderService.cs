using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Configuration;
using System.Threading.Tasks;


namespace SQLDataObfuscator
{
    class UploaderService
    {
        public void UploadCsv(Query query, DateTime startDate, DateTime endDate)
        {

            Upload(query, startDate, endDate);

        }

        private void Upload(Query query, DateTime startDate, DateTime endDate)
        {

            var fileName = query.Name;
            var fileExtension = ".csv";

            var blobName = $"{fileName}{fileExtension}";
            blobName = blobName.ToLower();

            var path = Path.Combine(ObfuscatorConfiguration.Current.TempPath, blobName);
            string startDateName = null;
            string endDateName = null;

            if (query.Parameters.Count() == 2)
            {
                var startDateParam = query.Parameters.First();
                var endDateParam = query.Parameters.Skip(1).First();
                startDateName = startDateParam.Name;
                endDateName = endDateParam.Name;
            }

            //try to delete the file if already exists
            //TryDeleteFile(path);

            //TODO: Remove from here
            Console.WriteLine($"Start: Processing {query.Name} From {startDate.ToString("yyyy-MM-dd")} to {endDate.ToString("yyyy-MM-dd")} ");

            try
            {
                long rowCount = 0;

                using (var stream = File.OpenWrite(path))
                {
                    var writer = new StreamWriter(stream) { AutoFlush = true };
                    using (var readerToCsvFormatter = new ReaderToCsvFormatter(query, startDate, endDate, startDateName, endDateName))
                    {
                        var headerWrite = false;
                        while (readerToCsvFormatter.ReadLine())
                        {
                            if (!headerWrite)
                            {
                                writer.WriteLine(readerToCsvFormatter.MapHeader());
                                headerWrite = true;
                            }
                            else
                            {
                                rowCount++;
                            }
                            writer.WriteLine(readerToCsvFormatter.MapRecordLine());
                            //Console.WriteLine($"rowcount: {rowCount} From  ");
                        }
                        if (rowCount == 0)
                        {//we just write the header
                            writer.WriteLine(readerToCsvFormatter.MapHeader());
                        }
                        stream.Close();
                    }
                }

                Console.WriteLine($"Processed: {query.Name} From {startDate.ToString("yyyy-MM-dd")} to {endDate.ToString("yyyy-MM-dd")} ");

            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }
                //TODO: Remove from here
                Console.WriteLine("Error " + ex.Message);
                Console.ReadLine();
                throw;
            }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch { }
        }


        private static string FormatDayMonth(int value)
        {
            return value < 10 ? $"0{value}" : value.ToString();
        }
    }
}
