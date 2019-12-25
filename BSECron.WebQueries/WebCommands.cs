using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Net;

namespace BSECron.WebQueries
{
    public class WebCommands
    {
        string GraphURL;


        public WebCommands()
        {
            GraphURL = ConfigurationManager.AppSettings["STOCKGRAPHURL"];
        }

        public string GetGraphDataForSymbol(string symbol,string range="max")
        {
            try
            {
                if (symbol != null)
                {
                    symbol = symbol.Replace(" ", "");
                }

                if (symbol.Length > 10)
                {
                    symbol = symbol.Remove(10);
                }

                symbol += ".BO";


                string _GraphURL = string.Format(GraphURL, symbol, range);

                _GraphURL = _GraphURL.Replace(" ", "%20");

                WebClient wc = new WebClient();

                var graphData = wc.DownloadString(_GraphURL);

                return graphData;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
