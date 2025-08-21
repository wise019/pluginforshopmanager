using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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

                var rows = ReadExcelOrCsv(ofd.FileName, "Sheet1$");

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

        // ===== Excel/CSV 读取 =====
        private static List<OrderRow> ReadExcelOrCsv(string path, string sheetName)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".csv")
                return ReadCsv(path);

            if (!HasAceOleDb())
                throw new InvalidOperationException("缺少ACE OLEDB驱动");

            string connStr = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";";
            using var conn = new OleDbConnection(connStr);
            conn.Open();
            using var cmd = new OleDbCommand($"SELECT * FROM [{sheetName}]", conn);
            using var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);

            var result = new List<OrderRow>();
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["订单编号"] == DBNull.Value) continue;
                var row = new OrderRow
                {
                    OrderNo = dr["订单编号"].ToString().Trim(),
                    OrderDate = dr["订单日期"]?.ToString().Trim(),
                    OrderTime = dr["订单时间点"]?.ToString().Trim(),
                    TableName = dr["桌台号"]?.ToString().Trim(),
                    GuestCount = ToInt(dr["用餐人数"]),
                    ItemName = dr["菜品明细"]?.ToString().Trim(),
                    UnitPrice = ToDec(dr["菜品单价"]),
                    Qty = ToDec(dr["菜品数量"], 1m),
                    OrderAmount = ToDec(dr["订单金额"])
                };
                if (!string.IsNullOrEmpty(row.OrderNo) && !string.IsNullOrEmpty(row.ItemName))
                    result.Add(row);
            }
            return result;
        }

        private static List<OrderRow> ReadCsv(string path)
        {
            var list = new List<OrderRow>();
            using var sr = new StreamReader(path, Encoding.UTF8);
            string header = sr.ReadLine();
            if (header == null) return list;
            var cols = header.Split(',');
            int idx(string name) => Array.FindIndex(cols, c => c.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));

            int iOrderNo = idx("订单编号");
            int iOrderDate = idx("订单日期");
            int iOrderTime = idx("订单时间点");
            int iTable = idx("桌台号");
            int iGuest = idx("用餐人数");
            int iItem = idx("菜品明细");
            int iPrice = idx("菜品单价");
            int iQty = idx("菜品数量");
            int iAmt = idx("订单金额");

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                var cells = line.Split(',');
                var row = new OrderRow
                {
                    OrderNo = Get(cells, iOrderNo),
                    OrderDate = Get(cells, iOrderDate),
                    OrderTime = Get(cells, iOrderTime),
                    TableName = Get(cells, iTable),
                    GuestCount = ToInt(Get(cells, iGuest)),
                    ItemName = Get(cells, iItem),
                    UnitPrice = ToDec(Get(cells, iPrice)),
                    Qty = ToDec(Get(cells, iQty), 1m),
                    OrderAmount = ToDec(Get(cells, iAmt))
                };
                if (!string.IsNullOrEmpty(row.OrderNo) && !string.IsNullOrEmpty(row.ItemName))
                    list.Add(row);
            }
            return list;

            static string Get(string[] a, int i) => (i >= 0 && i < a.Length) ? a[i].Trim() : "";
        }

        private static bool HasAceOleDb()
        {
            try
            {
                var dummy = new OleDbConnectionStringBuilder();
                return true;
            }
            catch { return false; }
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

        private static int ToInt(object o, int def = 0)
        {
            if (o == null) return def;
            if (o is int i) return i;
            int.TryParse(o.ToString(), out i);
            return i == 0 ? def : i;
        }

        private static decimal ToDec(object o, decimal def = 0m)
        {
            if (o == null) return def;
            if (o is decimal d) return d;
            if (decimal.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out d)) return d;
            if (decimal.TryParse(o.ToString(), out d)) return d;
            return def;
        }

        private class OrderRow
        {
            public string OrderNo;
            public string OrderDate;
            public string OrderTime;
            public string TableName;
            public int GuestCount;
            public string ItemName;
            public decimal UnitPrice;
            public decimal Qty;
            public decimal OrderAmount;
        }
    }
}
