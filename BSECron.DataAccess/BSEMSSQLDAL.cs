using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using BSECron.Common;
using System.Globalization;

namespace BSECron.DataAccess
{
    public class BSEMSSQLDAL
    {
        string constring;

        public BSEMSSQLDAL()
        {
            constring = ConfigurationManager.ConnectionStrings["SQLCONSTRING"].ConnectionString;
        }

        public void InsertBSEDataForDate(DateTime date)
        {
            Utility utility = new Utility();

            string fetchAddress = default(string);

            string fileName = default(string);

            string directoryName = default(string);

            string csvPath = default(string);

            utility.GenerateFetchAddress(date, ref fetchAddress, ref fileName, ref directoryName, ref csvPath);

            if (File.Exists(csvPath))
            {
                DataTable dtTemp = utility.ReadDataFromCsv(date);

                BseDataMappings bseDataMappings = new BseDataMappings();

                var bsedailyTrades = bseDataMappings.MapBSEDailyTrade(dtTemp);

                SqlConnection scon = new SqlConnection(constring);

                foreach (var bSEDailyTrade in bsedailyTrades)
                {
                    SqlCommand scmd = new SqlCommand("SP_BSE_INSERTDATA", scon);

                    scmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter[] sqlParameters = {
                    new SqlParameter("@SC_CODE",(object)bSEDailyTrade.Sc_Code??DBNull.Value),
                    new SqlParameter("@SC_NAME",(object)bSEDailyTrade.Sc_Name??DBNull.Value),
                    new SqlParameter("@SC_GROUP",(object)bSEDailyTrade.Sc_Group??DBNull.Value),
                    new SqlParameter("@SC_TYPE",(object)bSEDailyTrade.Sc_Type??DBNull.Value),
                    new SqlParameter("@OPEN",(object)bSEDailyTrade.Open??DBNull.Value),
                    new SqlParameter("@HIGH",(object)bSEDailyTrade.High??DBNull.Value),
                    new SqlParameter("@LOW",(object)bSEDailyTrade.Low??DBNull.Value),
                    new SqlParameter("@CLOSE",(object)bSEDailyTrade.Close??DBNull.Value),
                    new SqlParameter("@LAST",(object)bSEDailyTrade.Last??DBNull.Value),
                    new SqlParameter("@PREVCLOSE",(object)bSEDailyTrade.PrevClose??DBNull.Value),
                    new SqlParameter("@NO_TRADES",(object)bSEDailyTrade.No_Trades??DBNull.Value),
                    new SqlParameter("@NO_OF_SHRS",(object)bSEDailyTrade.No_Of_Shares??DBNull.Value),
                    new SqlParameter("@NET_TURNOV",(object)bSEDailyTrade.Net_Turnov??DBNull.Value),
                    new SqlParameter("@TDCLOINDI",(object)bSEDailyTrade.Tdcloindi??DBNull.Value),
                    new SqlParameter("@ISIN_CODE",(object)bSEDailyTrade.Isin_Code??DBNull.Value),
                    new SqlParameter("@TRADING_DATE",(object)bSEDailyTrade.Trading_Date??DBNull.Value),
                    new SqlParameter("@FILLER2",(object)bSEDailyTrade.Filler2??DBNull.Value),
                    new SqlParameter("@FILLER3",(object)bSEDailyTrade.Filler3??DBNull.Value)
                    };

                    scmd.Parameters.AddRange(sqlParameters);

                    SqlDataAdapter sda = new SqlDataAdapter(scmd);

                    DataSet ds = new DataSet();

                    sda.Fill(ds);
                }
            }
            else
            {
                //throw new Exception("Data not available from BSE Server");
            }

        }

        public void InsertBSEData(DataTable dtBseData)
        {
            if (dtBseData.Rows.Count > 0)
            {
                SqlConnection scon = new SqlConnection(constring);

                BseDataMappings bseDataMappings = new BseDataMappings();

                var bseDailyTrades = bseDataMappings.MapBSEDailyTrade(dtBseData);

                foreach (var bseDailyTrade in bseDailyTrades)
                {
                    SqlCommand scmd = new SqlCommand("SP_BSE_INSERTDATA", scon);

                    scmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter[] sqlParameters = {
                        new SqlParameter("@SC_CODE",(object)bseDailyTrade.Sc_Code??DBNull.Value),
                        new SqlParameter("@SC_NAME",(object)bseDailyTrade.Sc_Name??DBNull.Value),
                        new SqlParameter("@SC_GROUP",(object)bseDailyTrade.Sc_Group??DBNull.Value),
                        new SqlParameter("@SC_TYPE",(object)bseDailyTrade.Sc_Type??DBNull.Value),
                        new SqlParameter("@OPEN",(object)bseDailyTrade.Open??DBNull.Value),
                        new SqlParameter("@HIGH",(object)bseDailyTrade.High??DBNull.Value),
                        new SqlParameter("@LOW",(object)bseDailyTrade.Low??DBNull.Value),
                        new SqlParameter("@CLOSE",(object)bseDailyTrade.Close??DBNull.Value),
                        new SqlParameter("@LAST",(object)bseDailyTrade.Last??DBNull.Value),
                        new SqlParameter("@PREVCLOSE",(object)bseDailyTrade.PrevClose??DBNull.Value),
                        new SqlParameter("@NO_TRADES",(object)bseDailyTrade.No_Trades??DBNull.Value),
                        new SqlParameter("@NO_OF_SHRS",(object)bseDailyTrade.No_Of_Shares??DBNull.Value),
                        new SqlParameter("@NET_TURNOV",(object)bseDailyTrade.Net_Turnov??DBNull.Value),
                        new SqlParameter("@TDCLOINDI",(object)bseDailyTrade.Tdcloindi??DBNull.Value),
                        new SqlParameter("@ISIN_CODE",(object)bseDailyTrade.Isin_Code??DBNull.Value),
                        new SqlParameter("@TRADING_DATE",(object)bseDailyTrade.Trading_Date??DBNull.Value),
                        new SqlParameter("@FILLER2",(object)bseDailyTrade.Filler2??DBNull.Value),
                        new SqlParameter("@FILLER3",(object)bseDailyTrade.Filler3??DBNull.Value)
                        };

                    scmd.Parameters.AddRange(sqlParameters);

                    SqlDataAdapter sda = new SqlDataAdapter(scmd);

                    DataSet ds = new DataSet();

                    sda.Fill(ds);
                }

            }
        }

        public DataTable GetDaywiseStatisticalData(DateTime fromDate, DateTime toDate)
        {
            string dynamicQuery = GetDayWiseStatQuery(fromDate, toDate);

            using (SqlConnection scon = new SqlConnection(constring))
            {
                using (SqlCommand scmd = new SqlCommand(dynamicQuery, scon))
                {
                    scmd.CommandTimeout = 0;

                    using (SqlDataAdapter sda = new SqlDataAdapter(scmd))
                    {
                        DataSet ds = new DataSet();

                        sda.Fill(ds);

                        return ds.Tables[0];
                    }
                }
            }
        }

        public string GetDayWiseStatQuery(DateTime fromDate, DateTime toDate)
        {
            string daywiseStatQuery = string.Empty;

            string columnList = string.Empty;

            string columnList1 = string.Empty;

            string whereCondition = string.Empty;

            string totalReturns = default(string);

            Utility utility = new Utility();

            List<DateTime> datesToExclude = new List<DateTime>(utility.GetDatesToExclude());

            DateTime toDateTemp = toDate;

            #region -prev logic-
            /*
            while (fromDate <= toDate)
            {
                if (fromDate.DayOfWeek == DayOfWeek.Saturday || fromDate.DayOfWeek == DayOfWeek.Sunday || datesToExclude.Contains(fromDate))
                {
                    fromDate = fromDate.AddDays(1);
                    continue;
                }

                columnList += string.Format(",[{0}]",fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),prependInt);

                whereCondition += string.Format("AND T1.[{0}] IS NOT NULL ", fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),prependInt);

                fromDate = fromDate.AddDays(1);

                ++prependInt;
            }

            if (columnList.StartsWith(",") && whereCondition.StartsWith("AND"))
            {
                columnList = columnList.Remove(0, 1);

                whereCondition = whereCondition.Remove(0, 3);
            }
            else
            {
                throw new Exception("Error in date range");
            }
            

            daywiseStatQuery = string.Format("SELECT BseData.SC_NAME,BseData.[CLOSE],BseData.NO_OF_SHRS,BseData.NO_TRADES,BseData.NET_TURNOV, {0} FROM({1}",columnList," ");
            daywiseStatQuery += string.Format("SELECT * FROM({0}", " ");
            daywiseStatQuery += string.Format("SELECT * FROM({0}", " ");
            daywiseStatQuery += string.Format("SELECT SC_NAME,TRADING_DATE AS [DAY],MAX(ROUND(((([CLOSE] - PREVCLOSE) / PREVCLOSE) * 100),3)) AS CHANGE FROM BseData WHERE PREVCLOSE <> 0 GROUP BY SC_NAME,TRADING_DATE{0}", " ");
            daywiseStatQuery += string.Format(")T{0}", " ");
            daywiseStatQuery += string.Format("PIVOT(MAX(CHANGE) FOR [DAY] IN ({1})) AS TOT{0}", " ",columnList);
            daywiseStatQuery += string.Format(")T1{0}", " ");
            daywiseStatQuery += string.Format("WHERE {1}{0}", " ",whereCondition);
            daywiseStatQuery += string.Format(")T2{0}", " ");
            daywiseStatQuery += string.Format("JOIN BseData ON BseData.SC_NAME = T2.SC_NAME{0}", " ");
            daywiseStatQuery += string.Format("WHERE BseData.TRADING_DATE = '{1}'{0}", " ", toDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            daywiseStatQuery += string.Format("ORDER BY BseData.NO_TRADES DESC{0}", " "); */
            #endregion

            while (toDateTemp >= fromDate)
            {
                if (toDateTemp.DayOfWeek == DayOfWeek.Saturday || toDateTemp.DayOfWeek == DayOfWeek.Sunday || datesToExclude.Contains(toDateTemp))
                {
                    toDateTemp = toDateTemp.AddDays(-1);
                    continue;
                }

                columnList += string.Format(",ISNULL([{0}],0)[{0}]", toDateTemp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

                columnList1 += string.Format(",[{0}]", toDateTemp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

                totalReturns += string.Format("+ISNULL([{0}],0)", toDateTemp.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

                toDateTemp = toDateTemp.AddDays(-1);
            }

            if (columnList.StartsWith(",") && totalReturns.StartsWith("+"))
            {
                columnList = columnList.Remove(0, 1);

                columnList1 = columnList1.Remove(0, 1);

                totalReturns = totalReturns.Remove(0, 1);

                totalReturns = string.Format("{0} AS TOTAL_PERC,", totalReturns);
            }
            else
            {
                throw new Exception("Error in date range");
            }

            Random tblRandom = new Random(11);

            string tblRandomName = "#TEMP";

            while (tblRandomName.Length <= 30)
            {
                tblRandomName += tblRandom.Next();
            }


            daywiseStatQuery = string.Format("SELECT * INTO {0} FROM BseData WHERE BseData.TRADING_DATE = '{1}'; ",tblRandomName,toDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            daywiseStatQuery += string.Format("SELECT {2} T2.SC_NAME,{3}.[CLOSE],{3}.NO_OF_SHRS,{3}.NO_TRADES,{3}.NET_TURNOV, {0} FROM({1}", columnList, " ", totalReturns,tblRandomName);
            daywiseStatQuery += string.Format("SELECT * FROM({0}", " ");
            daywiseStatQuery += string.Format("SELECT * FROM({0}", " ");
            //daywiseStatQuery += string.Format("SELECT SC_NAME,TRADING_DATE AS [DAY],MAX(ROUND(((([CLOSE] - [LAST]) / [LAST]) * 100),3)) AS CHANGE FROM BseData WHERE PREVCLOSE <> 0 AND PREVCLOSE IS NOT NULL GROUP BY SC_NAME,TRADING_DATE{0}", " ");
            //daywiseStatQuery += string.Format("SELECT SC_NAME,TRADING_DATE AS [DAY],ROUND(((([CLOSE] - [LAST]) / [LAST]) * 100),3) AS CHANGE FROM BseData WHERE PREVCLOSE <> 0 AND PREVCLOSE IS NOT NULL AND TRADING_DATE BETWEEN '{0}' AND '{1}'{2}", fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), toDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), " ");
            daywiseStatQuery += string.Format("SELECT SC_NAME,TRADING_DATE AS [DAY],MAX(ROUND(((([CLOSE] - [PREVCLOSE]) / [PREVCLOSE]) * 100),3)) AS CHANGE FROM BseData WHERE PREVCLOSE <> 0 GROUP BY SC_NAME,TRADING_DATE {0}", " ");
            daywiseStatQuery += string.Format(")T{0}", " ");
            daywiseStatQuery += string.Format("PIVOT(MAX(CHANGE) FOR [DAY] IN ({1})) AS TOT{0}", " ", columnList1);
            daywiseStatQuery += string.Format(")T1{0}", " ");
            daywiseStatQuery += string.Format(")T2{0}", " ");
            daywiseStatQuery += string.Format("FULL JOIN {0} ON {0}.SC_NAME = T2.SC_NAME WHERE T2.SC_NAME IS NOT NULL{1}",tblRandomName," ");
            //daywiseStatQuery += string.Format("WHERE BseData.TRADING_DATE = '{1}'{0}", " ", toDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            //daywiseStatQuery += string.Format("ORDER BY ({0}) * BseData.NET_TURNOV DESC", totalReturns.Replace(" AS TOTAL_PERC,", ""));
            daywiseStatQuery += string.Format("ORDER BY {0}.NET_TURNOV DESC;",tblRandomName);
            daywiseStatQuery += string.Format("DROP TABLE {0}",tblRandomName);
            return daywiseStatQuery;
        }

        public void InsertBSEData(DataTable dtBseData, bool quick)
        {
            if (dtBseData.Rows.Count > 0)
            {
                using (var scon = new SqlConnection(constring))
                {
                    DataSet ds = new DataSet();

                    SqlCommand selectCmd = new SqlCommand("SELECT * FROM BSEDATA WHERE 0 = 1", scon);

                    SqlDataAdapter sda = new SqlDataAdapter(selectCmd);

                    //sda.MissingSchemaAction = MissingSchemaAction.Ignore;

                    SqlCommandBuilder sbld = new SqlCommandBuilder(sda);

                    sda.Fill(ds);

                    if (ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in dtBseData.Rows)
                        {
                            ds.Tables[0].Rows.Add(dr.ItemArray);
                        }

                        //ds.Tables[0].AcceptChanges();

                        sda.Update(ds);
                    }
                }
            }
        }

        public bool DateExistsInDatabase(DateTime date)
        {
            var ds = new DataSet();

            using (var scon = new SqlConnection(constring))
            using (var scmd = new SqlCommand("SELECT DISTINCT 1 FROM BseData WHERE TRADING_DATE = '" + date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'", scon))
            using (var sda = new SqlDataAdapter(scmd))
            {
                sda.Fill(ds);
                return ds.Tables[0].Rows.Count > 0;
            }
        }
    }
}