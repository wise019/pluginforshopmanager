using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.Grid;
using DatabaseHelper;

namespace ExcelToStore
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        private readonly SqlServerDatabaseHelper _dbHelper = new SqlServerDatabaseHelper();
        private readonly HashSet<int> _rowsWithMissing = new HashSet<int>();

        public Form1()
        {
            InitializeComponent();
            this.btnexcelimport.ItemClick += Btnexcelimport_ItemClick;
            this.gvdata.RowStyle += Gvdata_RowStyle;
        }

        private void Btnexcelimport_ItemClick(object sender, ItemClickEventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel or CSV|*.xlsx;*.xlsm;*.csv";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                var rows = ExcelParser.ReadExcelOrCsv(ofd.FileName);
                baseDataSet.dtExcelModel.Clear();
                _rowsWithMissing.Clear();
                txtMissingTableNo.Clear();
                txtMissingDishName.Clear();

                var missingTables = new HashSet<string>();
                var missingDishes = new HashSet<string>();
                int index = 1;
                foreach (var row in rows)
                {
                    var dr = baseDataSet.dtExcelModel.NewdtExcelModelRow();
                    dr.no = index.ToString();
                    dr.orderno = row.OrderNo;
                    dr.orderdate = row.OrderDate;
                    dr.ordertime = row.OrderTime;
                    dr.tableno = row.TableName;
                    dr.customcount = row.GuestCount.ToString();
                    dr.fooddetail = row.ItemName;
                    dr.foodprice = row.UnitPrice.ToString(CultureInfo.InvariantCulture);
                    dr.foodnumber = row.Qty.ToString(CultureInfo.InvariantCulture);
                    dr.orderprice = row.OrderAmount.ToString(CultureInfo.InvariantCulture);
                    baseDataSet.dtExcelModel.AdddtExcelModelRow(dr);

                    bool tableExists = _dbHelper.TableNameExists(row.TableName);
                    bool dishExists = _dbHelper.DishNameExists(row.ItemName);
                    if (!tableExists)
                        missingTables.Add(row.TableName);
                    if (!dishExists)
                        missingDishes.Add(row.ItemName);
                    if (!tableExists || !dishExists)
                        _rowsWithMissing.Add(baseDataSet.dtExcelModel.Rows.Count - 1);

                    index++;
                }

                txtMissingTableNo.Text = string.Join(Environment.NewLine, missingTables);
                txtMissingDishName.Text = string.Join(Environment.NewLine, missingDishes);
                gvdata.RefreshData();
            }
        }

        private void Gvdata_RowStyle(object sender, RowStyleEventArgs e)
        {
            if (_rowsWithMissing.Contains(e.RowHandle))
            {
                e.Appearance.BackColor = Color.Yellow;
            }
        }
    }
}
