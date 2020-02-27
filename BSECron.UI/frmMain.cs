using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BSECron.Application;
using BSECron.Common;


namespace BSECron.UI
{
    public partial class frmMain : Form
    {
        DataTable dtBseDataCache;

        DateTime dtFromDateCache;

        DateTime dtToDateCache;

        string graphScripName;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            dtBseDataCache = default(DataTable);

            dtFromDateCache = default(DateTime);

            dtToDateCache = default(DateTime);

            BSECommands cmd = new BSECommands();

            graphScripName = default(string);
        }

        private void btnDownloadBSEData_Click(object sender, EventArgs e)
        {
            DateTime fromDate = dateFromPicker.Value.Date;

            DateTime toDate = dateToPicker.Value.Date;

            BSECommands bSECommands = new BSECommands();

            lblNotify.Text = "Downloading BSE data from Server";

            bSECommands.FetchBseDataFromRange(fromDate, toDate);

            lblNotify.Text = "Completed Downloading BSE data from Server";

        }

        private void btnSaveToDB_Click(object sender, EventArgs e)
        {
            lblNotify.Text = "Saving BSE Data to Database";

            DateTime fromDate = dateFromPicker.Value.Date;

            DateTime toDate = dateToPicker.Value.Date;

            BSECommands bSECommands = new BSECommands();

            bSECommands.ParseBseDataFromRangeToDatabase(fromDate, toDate);

            lblNotify.Text = "Completed Saving BSE Data to Database";
        }

        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Title = "Save Analyzed Data";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Utility utility = new Utility();

                DataTable dtBseData = (DataTable)grdBseData.DataSource;

                if (dtBseData != null)
                {
                    utility.DumpDataToExcel(dtBseData, saveFileDialog.FileName);
                }
            }

        }

        private void btnAnalyzeLogic_Click(object sender, EventArgs e)
        {
            DateTime dtFromDate = dateFromPicker.Value.Date;

            DateTime dtToDate = dateToPicker.Value.Date;

            BSECommands cmd = new BSECommands();

            DataTable dtBseData = new DataTable();

            if (dtFromDate.Date != dtFromDateCache.Date || dtToDate.Date != dtToDateCache.Date)
            {
                dtBseData = cmd.GetDaywiseStatisticalData(dtFromDate, dtToDate);

                dtBseDataCache = dtBseData.Copy();

                dtFromDateCache = dtFromDate.Date;

                dtToDateCache = dtToDate.Date;
            }
            else
            {
                dtBseData = dtBseDataCache.Copy();
            }

            if (cmbBoxGainers.Text != "")
            {
                dtBseData = cmd.FilterTopGainers(dtBseData, int.Parse(cmbBoxGainers.Text));
            }

            if (cmbBoxLosers.Text != "")
            {
                dtBseData = cmd.FilterTopLosers(dtBseData, int.Parse(cmbBoxLosers.Text));
            }

            grdBseData.DataSource = dtBseData;

            ddlSortColumns.Items.Clear();

            if (dtBseData.Columns.Count > 0 && dtBseData.Rows.Count > 0)
            {
                foreach (DataColumn dc in dtBseData.Columns)
                {
                    ddlSortColumns.Items.Add(dc.ColumnName);
                }
            }
        }

        private void cmbBoxGainers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cmbBoxLosers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ddlSortColumns_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            BSECommands bSECommands = new BSECommands();

            DataTable dtBseData = (DataTable)grdBseData.DataSource;

            grdBseData.DataSource = bSECommands.FilterBseData(dtBseData, ddlSortColumns.Text, txtFilter.Text);
        }

        private void btnResetData_Click(object sender, EventArgs e)
        {
            cmbBoxGainers.SelectedIndex = -1;
            cmbBoxLosers.SelectedIndex = -1;
            grdBseData.DataSource = dtBseDataCache.Copy();
            grdBseData.Refresh();
        }

        private void grdBseData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            BSECommands bSECommands = new BSECommands();

            string scripName = string.Empty;

            var colIndex = e.ColumnIndex;

            var priceSpread = //bSECommands.GetPriceSpread(grdBseData.DataSource, e.RowIndex,ref scripName);

            bSECommands.GetPriceSpread(grdBseData.DataSource, e.RowIndex, ref scripName, true);

            chrtBseData.Series["Series1"].Points.Clear();

            chrtBseData.Series["Series1"].LegendText = scripName;

            graphScripName = scripName;


            foreach (var item in priceSpread)
            {
                chrtBseData.Series["Series1"].Points.AddXY(item.Key, item.Value);
            }

        }

        private void trckGraphDuration_Scroll(object sender, EventArgs e)
        {
            if (graphScripName!= null)
            {
                BSECommands cmd = new BSECommands();

                var range = "max";

                switch (trckGraphDuration.Value)
                {
                    case 1:
                        range = "1d";
                        break;
                    case 2:
                        range = "5d";
                        break;
                    case 3:
                        range = "1mo";
                        break;
                    case 4:
                        range = "3mo";
                        break;
                    case 5:
                        range = "6mo";
                        break;
                    case 6:
                        range = "1y";
                        break;
                    case 7:
                        range = "2y";
                        break;
                    case 8:
                        range = "5y";
                        break;
                    case 9:
                        range = "10y";
                        break;
                    case 10:
                        range = "ytd";
                        break;
                    default:
                        range = "max";
                        break;
                }

                var priceSpread =  cmd.GetPriceSpread(graphScripName, range, true);

                chrtBseData.Series["Series1"].Points.Clear();

                chrtBseData.Series["Series1"].LegendText = graphScripName;

                foreach (var item in priceSpread)
                {
                    chrtBseData.Series["Series1"].Points.AddXY(item.Key, item.Value);
                }
            }
        }

        private void btnMarginStocks_Click(object sender, EventArgs e)
        {
            string stockList = System.Configuration.ConfigurationManager.AppSettings["MAGINSTOCKNAMES"];

            BSECommands bSECommands = new BSECommands();

            grdBseData.DataSource = bSECommands.FilterMarginStocks((DataTable)grdBseData.DataSource, stockList);
        }
    }
}