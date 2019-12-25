using System;
using System.Collections.Generic;
using System.Text;

namespace BSECron.Domain
{
    public class BSEDailyTrade
    {
        public float? Sc_Code;

        public string Sc_Name;

        public string Sc_Group;

        public string Sc_Type;

        public float? Open;

        public float? High;

        public float? Low;

        public float? Close;

        public float? Last;

        public float? PrevClose;

        public float? No_Trades;

        public float? No_Of_Shares;

        public float? Net_Turnov;

        public string Tdcloindi;

        public string Isin_Code;

        public DateTime? Trading_Date;

        public string Filler2;

        public string Filler3;
        /*
SC_CODE float
SC_NAME nvarchar
SC_GROUP    nvarchar
SC_TYPE nvarchar
OPEN    float
HIGH    float
LOW float
CLOSE   float
LAST    float
PREVCLOSE   float
NO_TRADES   float
NO_OF_SHRS  float
NET_TURNOV  float
TDCLOINDI   nvarchar
ISIN_CODE   nvarchar
TRADING_DATE    datetime
FILLER2 nvarchar
FILLER3 nvarchar
        */

        public BSEDailyTrade()
        {

        }

    }





}
