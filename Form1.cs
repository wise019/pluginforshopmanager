using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ExcelToStore
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        private const string ConnString =
            @"Server=127.0.0.1;Database=mp_Restaurant;User Id=sa;Password=YourStrong!Passw0rd;";

        private HashSet<string> missingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> missingGoods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public Form1()
        {
            InitializeComponent();

            // 事件绑定
            this.barButtonItem1.ItemClick += barButtonItem1_ItemClick;
            this.gvdata.RowStyle += gvdata_RowStyle;
        }

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel 或 CSV|*.xlsx;*.xls;*.csv";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                var rows = ExcelParser.ReadExcelOrCsv(ofd.FileName, "Sheet1$");

                // 显示到 Grid
                baseDataSet.dtExcelModel.Clear();
                int idx = 1;
                foreach (var r in rows)
                {
                    baseDataSet.dtExcelModel.AdddtExcelModelRow(
                        idx.ToString(),
                        r.OrderNo,
                        r.OrderDate,
                        r.OrderTime,
                        r.TableName,
                        r.GuestCount.ToString(),
                        r.ItemName,
                        r.UnitPrice.ToString(),
                        r.Qty.ToString(),
                        r.OrderAmount.ToString());
                    idx++;
                }

                // 预检字典
                missingTables.Clear();
                missingGoods.Clear();

                var allTables = rows.Select(r => r.TableName).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                var allGoods = rows.Select(r => r.ItemName).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                using (var conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    var tableMap = LoadDiningTableMap(conn, allTables);
                    var goodsMap = LoadGoodsMap(conn, allGoods);

                    foreach (var t in allTables)
                        if (!tableMap.ContainsKey(t)) missingTables.Add(t);

                    foreach (var gname in allGoods)
                        if (!goodsMap.ContainsKey(gname)) missingGoods.Add(gname);
                }

                txtMissingTableNo.Text = string.Join(Environment.NewLine, missingTables);
                txtMissingDishName.Text = string.Join(Environment.NewLine, missingGoods);

                gvdata.RefreshData();
            }
        }

        private void gvdata_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            if (e.RowHandle < 0) return;
            var row = gvdata.GetRow(e.RowHandle) as DataRowView;
            if (row == null) return;

            var table = row["tableno"]?.ToString();
            var good = row["fooddetail"]?.ToString();

            if ((table != null && missingTables.Contains(table)) ||
                (good != null && missingGoods.Contains(good)))
            {
                e.Appearance.BackColor = Color.Yellow;
            }
        }

        // ====== 查字典 ======
        private static Dictionary<string, string> LoadDiningTableMap(SqlConnection conn, List<string> names)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (names.Count == 0) return map;
            string inClause = string.Join(",", names.Select((_, i) => "@p" + i));
            using var cmd = new SqlCommand($"SELECT f_name, f_id FROM dbo.t_diningtable WHERE f_name IN ({inClause})", conn);
            for (int i = 0; i < names.Count; i++) cmd.Parameters.AddWithValue("@p" + i, names[i]);
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) map[rd.GetString(0)] = rd.GetString(1);
            return map;
        }

        private static Dictionary<string, string> LoadGoodsMap(SqlConnection conn, List<string> names)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (names.Count == 0) return map;
            string inClause = string.Join(",", names.Select((_, i) => "@p" + i));
            using var cmd = new SqlCommand($"SELECT f_name, f_id FROM dbo.t_Goods WHERE f_name IN ({inClause})", conn);
            for (int i = 0; i < names.Count; i++) cmd.Parameters.AddWithValue("@p" + i, names[i]);
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) map[rd.GetString(0)] = rd.GetString(1);
            return map;
        }
    }
}
