using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

using System.IO;
using System.IO.Compression;

using System.Net;

using System.Data;
using System.Data.OleDb;

using System.Globalization;

using ClosedXML;
using ClosedXML.Excel;

using BSECron.DataAccess;
using BSECron.Common;
using BSECron.WebQueries;

using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace BSECron.Application
{
    public class BSECommands
    {
        public BSECommands()
        {

        }

        /// <summary>
        /// Method to download data for a specific date from BSE Server
        /// </summary>
        /// <param name="date"></param>
        /// <param name="success"></param>
        private void FetchBSEDataFromServer(DateTime date, ref bool success)
        {
            success = false;

            if (date.Date == DateTime.Today.Date)
            {
                throw new Exception("Data not available for Date " + date.Date.ToShortDateString());
            }

            if (date.Date.DayOfWeek == DayOfWeek.Saturday || date.Date.DayOfWeek == DayOfWeek.Sunday)
            {
                return;
            }

            string fetchAddress = default(string);

            string fileName = default(string);

            string directoryName = default(string);

            string csvPath = default(string);

            Utility utility = new Utility();

            utility.GenerateFetchAddress(date, ref fetchAddress, ref fileName, ref directoryName, ref csvPath);

            if (utility.DownloadFile(fetchAddress, fileName))
            {
                if (utility.UnzipFileToDirectory(fileName, directoryName))
                {
                    success = true;
                }
            }
        }//working fine

        /// <summary>
        /// Method to download data within date range from BSE Server
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        public void FetchBseDataFromRange(DateTime fromDate, DateTime toDate)
        {
            DateTime dateTemp = fromDate;

            bool success = default(bool);

            while (dateTemp.Date <= toDate.Date)
            {
                FetchBSEDataFromServer(dateTemp, ref success);

                dateTemp = dateTemp.AddDays(1);
            }
        }//working fine

        /// <summary>
        /// Method to parse BSE data within a range in memory/datatable
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="dtBseData"></param>
        public void ParseBseDataFromRange(DateTime fromDate, DateTime toDate, ref DataTable dtBseData)
        {
            DateTime dateTemp = fromDate;

            dtBseData = new DataTable();

            Utility utility = new Utility();

            while (dateTemp.Date <= toDate.Date)
            {
                utility.ReadBseDataFromCsv(dateTemp, ref dtBseData);

                dateTemp = dateTemp.AddDays(1);
            }
        }

        public void ParseBseDataFromRangeToDatabase(DateTime fromDate, DateTime toDate)
        {
            DataTable dtBseData = new DataTable();

            DateTime dateTemp = fromDate;

            BSEMSSQLDAL bSEMSSQLDAL = new BSEMSSQLDAL();

            Utility utility = new Utility();

            while (dateTemp.Date <= toDate.Date)
            {
                if (!DateExistsInDatabase(dateTemp))
                {
                    DataTable dt = utility.ReadDataFromCsv(dateTemp);

                    bSEMSSQLDAL.InsertBSEData(dt, true);
                }
                dateTemp = dateTemp.AddDays(1);
            }
        }

        public DataTable GetDaywiseStatisticalData(DateTime fromDate, DateTime toDate)
        {
            int firstChar = default(int);

            BSEMSSQLDAL bSEMSSQLDAL = new BSEMSSQLDAL();

            DataTable dtBseStatData = bSEMSSQLDAL.GetDaywiseStatisticalData(fromDate, toDate);

            int idx = GetDateColumnIndex(dtBseStatData);

            dtBseStatData = AddStreakInfoToTable(dtBseStatData, idx);

            idx = GetDateColumnIndex(dtBseStatData);

            for (int i = idx; i <= dtBseStatData.Columns.Count - 1; ++i)
            {
                dtBseStatData.Columns[i].ColumnName = string.Format("({0}){1}", i - idx + 1, dtBseStatData.Columns[i].ColumnName);
            }

            for (int i = dtBseStatData.Rows.Count - 1; i >= 0; i--)
            {
                DataRow dr = dtBseStatData.Rows[i];

                string SC_NAME = Convert.ToString(dr["SC_NAME"]);

                if (int.TryParse(SC_NAME[0].ToString(), out firstChar))
                {
                    dtBseStatData.Rows.Remove(dr);
                    dtBseStatData.AcceptChanges();
                }
            }

            return dtBseStatData;
        }

        public DataTable FilterTopGainers(object dtBseData, int spread = 0)
        {
            DataTable dtBseData_ = (DataTable)dtBseData;

            if (dtBseData_ != null && dtBseData_.Rows.Count > 0)
            {
                string filterString = string.Empty;

                int i = GetDateColumnIndex(dtBseData_);

                while (i <= dtBseData_.Columns.Count - 1 && spread > 0)
                {
                    filterString += string.Format(" [{0}] >= 0 AND", dtBseData_.Columns[i].ColumnName);
                    ++i;
                    --spread;
                }

                if (filterString.EndsWith("AND"))
                {
                    filterString = filterString.Remove(filterString.LastIndexOf("AND"));
                }

                if (filterString.Length > 0)
                {
                    DataView dv = dtBseData_.DefaultView;

                    dv.RowFilter = filterString;

                    dv.Sort = "NO_TRADES DESC";

                    dtBseData_ = dv.ToTable();
                }
            }
            return dtBseData_;
        }

        public DataTable FilterTopLosers(object dtBseData, int spread = 0)
        {
            DataTable dtBseData_ = (DataTable)dtBseData;

            if (dtBseData_ != null && dtBseData_.Rows.Count > 0)
            {
                string filterString = string.Empty;

                int idx = GetDateColumnIndex(dtBseData_);

                while (idx <= dtBseData_.Columns.Count - 1 && spread > 0)
                {
                    filterString += string.Format(" [{0}] <= 0 AND", dtBseData_.Columns[idx].ColumnName);
                    ++idx;
                    --spread;
                }

                if (filterString.EndsWith("AND"))
                {
                    filterString = filterString.Remove(filterString.LastIndexOf("AND"));
                }

                if (filterString.Length > 0)
                {
                    DataView dv = dtBseData_.DefaultView;

                    dv.RowFilter = filterString;

                    dv.Sort = "NO_TRADES DESC";

                    dtBseData_ = dv.ToTable();
                }
            }
            return dtBseData_;
        }

        public bool DateExistsInDatabase(DateTime date)
        {
            BSEMSSQLDAL bSEMSSQLDAL = new BSEMSSQLDAL();

            return bSEMSSQLDAL.DateExistsInDatabase(date);
        }

        public DataTable SortBseData(DataTable dtBseData, string columnName, bool SortDescending)
        {
            if (dtBseData != null && dtBseData.Rows.Count > 0 && !string.IsNullOrEmpty(columnName))
            {
                DataView dv = dtBseData.DefaultView;

                dv.Sort = string.Format("{0} {1}", columnName, SortDescending ? "DESC" : "");

                dtBseData = dv.ToTable();
            }

            return dtBseData;
        }

        public DataTable FilterBseData(DataTable dtBseData, string columnName, string filterString)
        {
            if (dtBseData != null && dtBseData.Rows.Count > 0 && !string.IsNullOrEmpty(filterString))
            {
                DataView dv = dtBseData.DefaultView;

                dv.RowFilter = string.Format("[{0}] {1}", columnName, filterString);

                dtBseData = dv.ToTable();
            }

            return dtBseData;
        }

        public int GetDateColumnIndex(DataTable dtBseData)
        {
            if (dtBseData != null)
            {
                DateTime dateTemp = default(DateTime);

                int idx = 0;

                foreach (DataColumn dc in dtBseData.Columns)
                {
                    var columnName = dc.ColumnName.ToString();

                    columnName = Regex.Replace(columnName, "\\(\\d+\\)", "");

                    if (!DateTime.TryParse(columnName, out dateTemp))
                    {
                        ++idx;
                    }
                    else
                    {
                        break;
                    }
                }

                return idx;
            }
            else
            {
                return int.MaxValue;
            }
        }

        public Dictionary<string, double> GetPriceSpread(object DtBseData, int index, ref string scripName)
        {
            Dictionary<string, double> priceSpread = new Dictionary<string, double>();

            DataTable dtBseData = (DataTable)DtBseData;

            if (dtBseData != null && dtBseData.Rows.Count > 0 && index < dtBseData.Rows.Count && index != -1)
            {
                DataRow dr = dtBseData.DefaultView[index].Row;//dtBseData.Rows[index];

                int cIdx = GetDateColumnIndex(dtBseData);

                var columnName = string.Empty;

                scripName = dr.Field<string>("SC_NAME");

                var closePrice = dr.Field<double>("CLOSE");

                while (cIdx < dr.Table.Columns.Count)
                {
                    columnName = dr.Table.Columns[cIdx].ColumnName;

                    //priceSpread.Add(Regex.Replace(columnName, "\\(\\d+\\)", ""), dr.Field<double>(columnName));

                    priceSpread.Add(Regex.Replace(columnName, "\\(\\d+\\)", ""), closePrice);


                    closePrice = closePrice - (closePrice * (dr.Field<double>(columnName) / 100));

                    ++cIdx;
                }
            }

            return priceSpread;
        }

        public Dictionary<string, double> GetPriceSpread(object DtBseData, int index, ref string scripName, bool useLive)
        {
            DataTable dtBseData = (DataTable)DtBseData;

            Dictionary<string, double> priceSpread = new Dictionary<string, double>();

            if (dtBseData != null && dtBseData.Rows.Count > 0 && index < dtBseData.Rows.Count && index != -1)
            {
                DataRow dr = dtBseData.DefaultView[index].Row;

                int cIdx = GetDateColumnIndex(dtBseData);

                var columnName = string.Empty;

                scripName = dr.Field<string>("SC_NAME");

                WebCommands wcmd = new WebCommands();

                var graphData = wcmd.GetGraphDataForSymbol(scripName);

                if (graphData != null)
                {
                    dynamic json = JsonConvert.DeserializeObject(graphData);

                    int idx = 0;

                    if (json != null && json.chart != null && json.chart.result != null && json.chart.result[0] != null)
                    {
                        if (json.chart.result[0].timestamp != null && json.chart.result[0].indicators.adjclose[0] != null)
                        {
                            foreach (var item in json.chart.result[0].timestamp)
                            {
                                if (json.chart.result[0].indicators.adjclose[0].adjclose[idx] != null)
                                {
                                    priceSpread.Add(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToInt64(item)).ToString("dd-MMM-yyyy"), Convert.ToDouble(json.chart.result[0].indicators.adjclose[0].adjclose[idx]));
                                }
                                idx++;
                            }
                        }
                    }
                }
            }
            return priceSpread;
        }


        public Dictionary<string, double> GetPriceSpread(string scripName, string range, bool useLive)
        {
            Dictionary<string, double> priceSpread = new Dictionary<string, double>();

            WebCommands wcmd = new WebCommands();

            var graphData = wcmd.GetGraphDataForSymbol(scripName,range);

            if (graphData != null)
            {
                dynamic json = JsonConvert.DeserializeObject(graphData);

                int idx = 0;

                if (json != null && json.chart != null && json.chart.result != null && json.chart.result[0] != null)
                {
                    if (json.chart.result[0].timestamp != null && json.chart.result[0].indicators.adjclose[0] != null)
                    {
                        foreach (var item in json.chart.result[0].timestamp)
                        {
                            if (json.chart.result[0].indicators.adjclose[0].adjclose[idx] != null)
                            {
                                priceSpread.Add(new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Convert.ToInt64(item)).ToString("dd-MMM-yyyy"), Convert.ToDouble(json.chart.result[0].indicators.adjclose[0].adjclose[idx]));
                            }
                            idx++;
                        }
                    }
                }
            }
            return priceSpread;
        }

        public DataTable AddStreakInfoToTable(DataTable dtBseData, int dateColumnIndex)
        {
            if (dtBseData != null && dtBseData.Columns.Count > 0)
            {
                dtBseData.Columns.Add("POS_STREAK", typeof(Int32)).SetOrdinal(dateColumnIndex);
                dtBseData.Columns.Add("NEG_STREAK", typeof(Int32)).SetOrdinal(dateColumnIndex + 1);
                dtBseData.Columns.Add("RECENT_STREAK", typeof(Int32)).SetOrdinal(dateColumnIndex + 2);
            }

            dateColumnIndex += 3;

            for (int i = 0; i < dtBseData.Rows.Count; i++)
            {
                int pos_streak = 0;

                int neg_streak = 0;

                int pos_streak_temp = 0;

                int neg_streak_temp = 0;

                DataRow dr = dtBseData.Rows[i];

                for (int j = dateColumnIndex; j < dr.ItemArray.Length; j++)
                {
                    if (Convert.ToDouble(Convert.ToString(dr[j])) > 0)
                    {
                        ++pos_streak_temp;

                        if (neg_streak_temp > 0)
                        {
                            if (neg_streak < neg_streak_temp)
                            {
                                neg_streak = neg_streak_temp;
                            }
                            neg_streak_temp = 0;
                        }
                        if (neg_streak == 0)
                        {
                            dr["RECENT_STREAK"] = pos_streak_temp;
                        }
                    }
                    if (Convert.ToDouble(Convert.ToString(dr[j])) < 0)
                    {
                        ++neg_streak_temp;

                        if (pos_streak_temp > 0)
                        {
                            if (pos_streak < pos_streak_temp)
                            {
                                pos_streak = pos_streak_temp;
                            }
                            pos_streak_temp = 0;
                        }
                        if (pos_streak == 0)
                        {
                            dr["RECENT_STREAK"] = -1 * neg_streak_temp;
                        }
                    }
                    if (Convert.ToDouble(Convert.ToString(dr[j])) == 0)
                    {
                        continue;
                    }
                }

                if (pos_streak < pos_streak_temp)
                {
                    pos_streak = pos_streak_temp;
                }

                if (neg_streak < neg_streak_temp)
                {
                    neg_streak = neg_streak_temp;
                }

                dr["POS_STREAK"] = pos_streak;

                dr["NEG_STREAK"] = neg_streak;

                if (dr["RECENT_STREAK"] == null)
                {
                    dr["RECENT_STREAK"] = 0;
                }

                dtBseData.AcceptChanges();
            }

            return dtBseData;
        }
    }
}