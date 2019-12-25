using System;
using System.Collections.Generic;
using System.Text;

using System.Configuration;
using System.Data;
using System.IO;
using System.Globalization;
using System.Data.OleDb;
using ClosedXML.Excel;

using System.Net;

using System.IO.Compression;

namespace BSECron.Common
{
    public class Utility
    {
        public void GenerateFetchAddress(DateTime dt, ref string fetchAddress, ref string fileName, ref string directoryName, ref string csvPath)
        {
            string day = dt.Date.Day < 10 ? "0" + Convert.ToString(dt.Date.Day) : Convert.ToString(dt.Date.Day);

            string month = dt.Date.Month < 10 ? "0" + Convert.ToString(dt.Date.Month) : Convert.ToString(dt.Date.Month);

            string year = Convert.ToString(dt.Date.Year).Substring(2);

            string BSEUrl = Convert.ToString(ConfigurationManager.AppSettings["BSEURL"]);

            fetchAddress = string.Format("{0}EQ_ISINCODE_{1}{2}{3}.zip", BSEUrl, day, month, year);

            fileName = string.Format("EQ_ISINCODE_{0}{1}{2}.zip", day, month, year);

            directoryName = string.Format("EQ_ISINCODE_{0}{1}{2}", day, month, year);

            csvPath = string.Format("{0}\\EQ_ISINCODE_{1}{2}{3}.csv", directoryName, day, month, year);
        }

        public DataTable ReadDataFromCsv(DateTime date)
        {
            string fileName = default(string);

            string directoryName = default(string);

            string fetchAddress = default(string);

            string csvPath = default(string);

            GenerateFetchAddress(date, ref fetchAddress, ref fileName, ref directoryName, ref csvPath);

            DataTable dt = new DataTable();

            if (File.Exists(csvPath))
            {
                string header = "Yes";

                string pathOnly = Path.GetDirectoryName(csvPath);

                fileName = Path.GetFileName(csvPath);

                string sql = @"SELECT * FROM [" + fileName + "]";

                using (OleDbConnection connection = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly + ";Extended Properties=\"Text;HDR=" + header + "\""))
                using (OleDbCommand command = new OleDbCommand(sql, connection))
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                {
                    DataTable dtTemp = new DataTable();

                    dtTemp.Locale = CultureInfo.CurrentCulture;

                    adapter.Fill(dtTemp);

                    dt = dtTemp.Clone().Copy();

                    if (dtTemp.Rows.Count > 0)
                    {
                        dt.Merge(dtTemp, true, MissingSchemaAction.Ignore);
                    }
                }
            }

            return dt;
        }

        public void ReadBseDataFromCsv(DateTime date, ref DataTable dtBseData)
        {
            string fileName = default(string);

            string directoryName = default(string);

            string fetchAddress = default(string);

            string csvPath = default(string);

            GenerateFetchAddress(date, ref fetchAddress, ref fileName, ref directoryName, ref csvPath);

            if (File.Exists(csvPath))
            {
                string header = "Yes";

                string pathOnly = Path.GetDirectoryName(csvPath);

                fileName = Path.GetFileName(csvPath);

                string sql = @"SELECT * FROM [" + fileName + "]";

                using (OleDbConnection connection = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly + ";Extended Properties=\"Text;HDR=" + header + "\""))
                using (OleDbCommand command = new OleDbCommand(sql, connection))
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                {
                    DataTable dtTemp = new DataTable();

                    dtTemp.Locale = CultureInfo.CurrentCulture;

                    adapter.Fill(dtTemp);

                    if (dtBseData.Rows.Count == 0)
                    {
                        dtBseData = dtTemp.Clone().Copy();
                    }

                    if (dtTemp.Rows.Count > 0)
                    {
                        dtBseData.Merge(dtTemp, true, MissingSchemaAction.Ignore);
                    }
                }
            }
        }

        public void DumpDataToExcel(DataTable dtData, string filename = null)
        {
            if (dtData.Rows.Count > 0)
            {
                if (filename == null)
                {
                    if (File.Exists("BseData.xlsx"))
                    {
                        File.Delete("BseData.xlsx");
                    }
                    filename = "BseData.xlsx";
                }
                else if (File.Exists(filename))
                {
                    throw new Exception("Unable to Overwrite File at path " + filename);
                }

                dtData.TableName = "BseData";

                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.AddWorksheet(dtData);

                    wb.SaveAs(filename);

                }
            }
        }

        public IEnumerable<DateTime>  GetDatesToExclude()
        {
            var arrDatesToExclude = ConfigurationManager.AppSettings["EXCLUDEDATES"].Split(',');

            var dateFormat = ConfigurationManager.AppSettings["EXCLUDEDATESFORMAT"];

            foreach (var dateStr in arrDatesToExclude)
            {
                DateTime dateToExclude = DateTime.MinValue;

                if (DateTime.TryParseExact(dateStr, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateToExclude))
                {
                    yield return dateToExclude;
                }
            }
        }

        public bool DownloadFile(string fetchAddress,string fileName)
        {
            if (!File.Exists(fileName))
            {
                int cntRetry = 0;

                while (cntRetry <= 10)
                {
                    try

                    {
                        using (var wb = new WebClient())
                        {
                            wb.DownloadFile(fetchAddress, fileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }

                        File.AppendAllLines("Error.log", new[] { ex.Message });
                    }

                    ++cntRetry;
                }
            }

            return File.Exists(fileName);
        }

        public bool UnzipFileToDirectory(string fileName,string directoryName)
        {
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            if (Directory.GetFiles(directoryName).Length == 0)
            {
                try
                {
                    ZipFile.ExtractToDirectory(fileName, directoryName);
                }
                catch (Exception ex)
                {
                    File.AppendAllLines("Error.log", new[] { ex.Message });
                }
            }

            return Directory.GetFiles(directoryName).Length > 0;
        }
    }
}
