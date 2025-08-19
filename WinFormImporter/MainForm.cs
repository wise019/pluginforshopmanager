using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ExcelToMpRestaurantImporter
{
    public class MainForm : Form
    {
        // === UI controls ===
        private readonly TextBox _txtExcel = new() { Left = 10, Top = 10, Width = 600 };
        private readonly Button _btnBrowse = new() { Left = 620, Top = 8, Width = 60, Text = "选择" };
        private readonly Button _btnValidate = new() { Left = 690, Top = 8, Width = 80, Text = "检验" };
        private readonly Button _btnImport = new() { Left = 690, Top = 40, Width = 80, Text = "导入", Enabled = false };
        private readonly TextBox _txtLog = new() { Left = 10, Top = 70, Width = 760, Height = 480, Multiline = true, ScrollBars = ScrollBars.Vertical };

        // === configuration ===
        private const string ConnString =
            @"Server=127.0.0.1;Database=mp_Restaurant;User Id=sa;Password=YourStrong!Passw0rd;";
        private const string ExcelSheet = "Sheet1$"; // 默认工作表

        // === runtime state ===
        private List<OrderRow>? _rows;
        private Dictionary<string,string>? _tableMap;
        private Dictionary<string,string>? _goodsMap;
        private bool _validated;

        public MainForm()
        {
            Text = "Excel 导入 mp_Restaurant";
            Width = 800;
            Height = 600;

            Controls.AddRange(new Control[] { _txtExcel, _btnBrowse, _btnValidate, _btnImport, _txtLog });

            _btnBrowse.Click += (_, __) => BrowseExcel();
            _btnValidate.Click += (_, __) => ValidateData();
            _btnImport.Click += (_, __) => ImportData();
        }

        private void BrowseExcel()
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Excel or CSV|*.xlsx;*.xls;*.csv|All files|*.*"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                _txtExcel.Text = dlg.FileName;
        }

        private void ValidateData()
        {
            _validated = false;
            _btnImport.Enabled = false;
            _rows = null;
            _tableMap = null;
            _goodsMap = null;
            _txtLog.Clear();

            string path = _txtExcel.Text.Trim();
            if (!File.Exists(path))
            {
                Log("❌ 文件不存在: " + path);
                return;
            }
            try
            {
                _rows = ReadExcelOrCsv(path, ExcelSheet);
                Log($"读取到 {_rows.Count} 行。");

                var allTables = _rows.Select(r => r.TableName).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                var allGoods  = _rows.Select(r => r.ItemName).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                using var conn = new SqlConnection(ConnString);
                conn.Open();
                _tableMap = LoadDiningTableMap(conn, allTables);
                _goodsMap = LoadGoodsMap(conn, allGoods);

                var missingTables = allTables.Where(n => !_tableMap.ContainsKey(n)).ToList();
                var missingGoods  = allGoods.Where(n => !_goodsMap.ContainsKey(n)).ToList();

                if (missingTables.Any() || missingGoods.Any())
                {
                    Log("❌ 字典缺失：");
                    if (missingTables.Any())
                        Log("  - 餐台缺失: " + string.Join("、", missingTables));
                    if (missingGoods.Any())
                        Log("  - 菜品缺失: " + string.Join("、", missingGoods));
                    Log("请先在字典表中补齐，检验未通过。");
                    return;
                }
                Log("✅ 检验通过，无缺失。");
                _validated = true;
                _btnImport.Enabled = true;
            }
            catch (Exception ex)
            {
                Log("❌ 检验失败: " + ex.Message);
            }
        }

        private void ImportData()
        {
            if (!_validated || _rows == null || _tableMap == null || _goodsMap == null)
            {
                Log("请先检验并确保无缺失再导入。");
                return;
            }
            try
            {
                using var conn = new SqlConnection(ConnString);
                conn.Open();

                var groups = _rows.GroupBy(r => r.OrderNo);
                int ok = 0, skip = 0, fail = 0;

                foreach (var g in groups)
                {
                    var orderNo = g.Key;
                    try
                    {
                        if (SellMainExists(conn, orderNo))
                        {
                            skip++;
                            continue; // 幂等：已存在则跳过
                        }

                        var first = g.First();
                        DateTime createTime = CombineDateTime(first.OrderDate, first.OrderTime);
                        string tableName = first.TableName;
                        string tableId = _tableMap[tableName];
                        int guestNum = first.GuestCount > 0 ? first.GuestCount : 0;

                        decimal sumDetail = g.Sum(x => x.UnitPrice * x.Qty);
                        decimal orderTotal = g.Max(x => x.OrderAmount);
                        if (orderTotal == 0) orderTotal = sumDetail;

                        if (Math.Abs((double)(sumDetail - orderTotal)) > 0.01)
                            Log($"⚠ 订单{orderNo} 明细之和({sumDetail}) 与总额({orderTotal})不一致，将以明细为准。");

                        using var tran = conn.BeginTransaction();
                        string sellMainId = Guid.NewGuid().ToString();
                        InsertSellMain(tran, sellMainId, orderNo, createTime, tableId, guestNum, sumDetail);
                        foreach (var r in g)
                        {
                            string gid = _goodsMap[r.ItemName];
                            decimal price = r.UnitPrice;
                            decimal qty = r.Qty;
                            decimal money = price * qty;
                            InsertSellDetail(tran, sellMainId, orderNo, gid, price, qty, money);
                        }
                        tran.Commit();
                        ok++;
                    }
                    catch (Exception ex)
                    {
                        Log($"❌ 导入失败：订单 {orderNo} - {ex.Message}");
                        fail++;
                    }
                }

                Log($"完成。成功 {ok}，跳过(已存在) {skip}，失败 {fail}。");
            }
            catch (Exception ex)
            {
                Log("致命错误：" + ex.Message);
            }
        }

        private void Log(string message)
        {
            _txtLog.AppendText(message + Environment.NewLine);
        }

        // ===== 以下为读取和数据库操作方法 =====

        private static List<OrderRow> ReadExcelOrCsv(string path, string sheetName)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".csv")
                return ReadCsv(path);

            if (!HasAceOleDb())
                throw new InvalidOperationException("缺少ACE OLEDB驱动");

            string connStr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";";
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
                    OrderNo     = dr["订单编号"].ToString()?.Trim() ?? string.Empty,
                    OrderDate   = dr["订单日期"]?.ToString()?.Trim(),
                    OrderTime   = dr["订单时间点"]?.ToString()?.Trim(),
                    TableName   = dr["桌台号"]?.ToString()?.Trim(),
                    GuestCount  = ToInt(dr["用餐人数"]),
                    ItemName    = dr["菜品明细"]?.ToString()?.Trim(),
                    UnitPrice   = ToDec(dr["菜品单价"]),
                    Qty         = ToDec(dr["菜品数量"], 1m),
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
            string? header = sr.ReadLine();
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

            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                var cells = SplitCsv(line);
                var row = new OrderRow
                {
                    OrderNo     = Get(cells, iOrderNo),
                    OrderDate   = Get(cells, iOrderDate),
                    OrderTime   = Get(cells, iOrderTime),
                    TableName   = Get(cells, iTable),
                    GuestCount  = ToInt(Get(cells, iGuest)),
                    ItemName    = Get(cells, iItem),
                    UnitPrice   = ToDec(Get(cells, iPrice)),
                    Qty         = ToDec(Get(cells, iQty), 1m),
                    OrderAmount = ToDec(Get(cells, iAmt))
                };
                if (!string.IsNullOrEmpty(row.OrderNo) && !string.IsNullOrEmpty(row.ItemName))
                    list.Add(row);
            }
            return list;

            static string Get(string[] a, int i) => (i >= 0 && i < a.Length) ? a[i].Trim() : string.Empty;
            static string[] SplitCsv(string l) => l.Split(',');
        }

        private static bool HasAceOleDb()
        {
            try
            {
                var _ = new OleDbConnectionStringBuilder();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private class OrderRow
        {
            public string OrderNo = string.Empty;
            public string? OrderDate;
            public string? OrderTime;
            public string? TableName;
            public int    GuestCount;
            public string ItemName = string.Empty;
            public decimal UnitPrice;
            public decimal Qty;
            public decimal OrderAmount;
        }

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

        private static bool SellMainExists(SqlConnection conn, string billno)
        {
            using var cmd = new SqlCommand("SELECT 1 FROM dbo.t_SellMain WHERE f_billno=@b", conn);
            cmd.Parameters.AddWithValue("@b", billno);
            var o = cmd.ExecuteScalar();
            return o != null;
        }

        private static void InsertSellMain(SqlTransaction tran, string id, string billno, DateTime createTime,
                                           string diningTableId, int guestNum, decimal sumDetail)
        {
            const string sql = @"
INSERT INTO dbo.t_SellMain
( f_id, f_billno, f_money, f_amount, f_ratios, f_rmoney, f_smoney, f_emoney, f_wipezero,
  f_payment, f_hangflag, f_hangtime, f_mealid, f_wmflag, f_wmmcid, f_wmempid, f_bakflag,
  f_bakbill, f_guestid, f_guestnum, f_guestloc, f_postip, f_postmac, f_shiftflag, f_shiftbill,
  f_aempid, f_freeCode, f_freenote, f_create_by, f_create_time, f_modify_by, f_modify_time,
  f_invoices, f_memflag, f_diningtableid, f_diningtableflag )
VALUES
( @id, @billno, @money, @amount, 100, @rmoney, @smoney, 0, 0,
  0, 0, @now, '', 0, '', '', 0,
  '', '', @guestnum, '', '', '', 0, '',
  '', '', '', '', @ctime, '', @ctime,
  '', 0, @table, 1 );";

            using var cmd = new SqlCommand(sql, tran.Connection, tran);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@billno", billno);
            cmd.Parameters.AddWithValue("@money", sumDetail);
            cmd.Parameters.AddWithValue("@amount", 0);
            cmd.Parameters.AddWithValue("@rmoney", sumDetail);
            cmd.Parameters.AddWithValue("@smoney", sumDetail);
            cmd.Parameters.AddWithValue("@now", DateTime.Now);
            cmd.Parameters.AddWithValue("@ctime", createTime);
            cmd.Parameters.AddWithValue("@guestnum", guestNum);
            cmd.Parameters.AddWithValue("@table", diningTableId);
            cmd.ExecuteNonQuery();
        }

        private static void InsertSellDetail(SqlTransaction tran, string billId, string billno, string gid,
                                             decimal price, decimal qty, decimal money)
        {
            const string sql = @"
INSERT INTO dbo.t_SellDetail
( f_id, f_billid, f_billno, f_gid, f_price, f_qty, f_money,
  f_disflag, f_disprice, f_dismoney, f_dismark, f_note,
  f_noteprice, f_notemoney, f_totalmoney, f_totalratios, f_ratimoney,
  f_package, f_bakflag, f_bakfid, f_bakqty, f_remark,
  f_create_by, f_create_time, f_modify_by, f_modify_time, f_mpflag, f_Immediate,
  f_baknote, f_GoodsCost, f_GoodsProfit, f_NoteCost, f_NoteProfit,
  f_TotalProfit, f_SgDisType, f_SgDisRatios, f_SgDisMoney,
  f_AuthUserId, f_UseMPrice, f_SecSpecId, f_SecSpecNum, f_IsDrink,
  f_SingleCP, f_TotalCP, f_PromotersName, f_PromotersId, f_PromotersOfCommission, f_WaiterOfCommission )
VALUES
( @id, @billid, @billno, @gid, @price, @qty, @money,
  0, 0, 0, '', '',
  0, 0, @money, 0, 0,
  0, 0, '', 0, '',
  '', @now, '', @now, 0, 0,
  '', 0, 0, 0, 0,
  @money, 0, 0, 0,
  '', 0, '', 0, 0,
  0, 0, '', '', 0, 0 );";

            using var cmd = new SqlCommand(sql, tran.Connection, tran);
            cmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("@billid", billId);
            cmd.Parameters.AddWithValue("@billno", billno);
            cmd.Parameters.AddWithValue("@gid", gid);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.Parameters.AddWithValue("@qty", qty);
            cmd.Parameters.AddWithValue("@money", money);
            cmd.Parameters.AddWithValue("@now", DateTime.Now);
            cmd.ExecuteNonQuery();
        }

        private static DateTime CombineDateTime(string? date, string? time)
        {
            if (DateTime.TryParse(((date ?? string.Empty) + " " + (time ?? string.Empty)).Trim(), out var dt)) return dt;
            if (DateTime.TryParse(date, out dt)) return dt;
            return DateTime.Now;
        }

        private static int ToInt(object? o, int def = 0)
        {
            if (o == null) return def;
            if (o is int i) return i;
            int.TryParse(o.ToString(), out i);
            return i == 0 ? def : i;
        }
        private static decimal ToDec(object? o, decimal def = 0m)
        {
            if (o == null) return def;
            if (o is decimal d) return d;
            if (decimal.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out d)) return d;
            if (decimal.TryParse(o.ToString(), out d)) return d;
            return def;
        }
    }
}
