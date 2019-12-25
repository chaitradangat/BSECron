using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using BSECron.Domain;


namespace BSECron.DataAccess
{
    public class BseDataMappings
    {
        public BseDataMappings()
        {

        }

        public List<BSEDailyTrade> MapBSEDailyTrade(DataTable dtBseData)
        {
            List<BSEDailyTrade> bSEDailyTrades = new List<BSEDailyTrade>();

            foreach (DataRow dr in dtBseData.Rows)
            {
                BSEDailyTrade bSEDailyTrade = new BSEDailyTrade();

                bSEDailyTrade.Sc_Code = Convert.ToString(dr["SC_CODE"]) != "" ? (float?)float.Parse(Convert.ToString(dr["SC_CODE"])) : null;

                bSEDailyTrade.Sc_Name = Convert.ToString(dr["SC_NAME"]) != "" ? Convert.ToString(dr["SC_NAME"]) : null;

                bSEDailyTrade.Sc_Group = Convert.ToString(dr["SC_GROUP"]) != "" ? Convert.ToString(dr["SC_GROUP"]) : null;

                bSEDailyTrade.Sc_Type = Convert.ToString(dr["SC_TYPE"]) != "" ? Convert.ToString(dr["SC_TYPE"]) : null;

                bSEDailyTrade.Open = Convert.ToString(dr["OPEN"]) != "" ? (float?)float.Parse(Convert.ToString(dr["OPEN"])) : null;

                bSEDailyTrade.High = Convert.ToString(dr["HIGH"]) != "" ? (float?)float.Parse(Convert.ToString(dr["HIGH"])) : null;

                bSEDailyTrade.Low = Convert.ToString(dr["LOW"]) != "" ? (float?)float.Parse(Convert.ToString(dr["LOW"])) : null;

                bSEDailyTrade.Close = Convert.ToString(dr["CLOSE"]) != "" ? (float?)float.Parse(Convert.ToString(dr["CLOSE"])) : null;

                bSEDailyTrade.Last = Convert.ToString(dr["LAST"]) != "" ? (float?)float.Parse(Convert.ToString(dr["LAST"])) : null;

                bSEDailyTrade.PrevClose = Convert.ToString(dr["PREVCLOSE"]) != "" ? (float?)float.Parse(Convert.ToString(dr["PREVCLOSE"])) : null;

                bSEDailyTrade.No_Trades = Convert.ToString(dr["NO_TRADES"]) != "" ? (float?)float.Parse(Convert.ToString(dr["NO_TRADES"])) : null;

                bSEDailyTrade.No_Of_Shares = Convert.ToString(dr["NO_OF_SHRS"]) != "" ? (float?)float.Parse(Convert.ToString(dr["NO_OF_SHRS"])) : null;

                bSEDailyTrade.Net_Turnov = Convert.ToString(dr["NET_TURNOV"]) != "" ? (float?)float.Parse(Convert.ToString(dr["NET_TURNOV"])) : null;

                bSEDailyTrade.Tdcloindi = Convert.ToString(dr["TDCLOINDI"]) != "" ? Convert.ToString(dr["TDCLOINDI"]) : null;

                bSEDailyTrade.Isin_Code = Convert.ToString(dr["ISIN_CODE"]) != "" ? Convert.ToString(dr["ISIN_CODE"]) : null;

                bSEDailyTrade.Trading_Date = Convert.ToString(dr["TRADING_DATE"]) != "" ? (DateTime?)DateTime.Parse(Convert.ToString(dr["TRADING_DATE"])).Date : null;

                bSEDailyTrade.Filler2 = Convert.ToString(dr["FILLER2"]) != "" ? Convert.ToString(dr["FILLER2"]) : null;

                bSEDailyTrade.Filler3 = Convert.ToString(dr["FILLER3"]) != "" ? Convert.ToString(dr["FILLER3"]) : null;

                bSEDailyTrades.Add(bSEDailyTrade);
            }

            return bSEDailyTrades;
        }

    }
}
