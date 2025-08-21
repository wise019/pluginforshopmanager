using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Text;

namespace ExcelToStore
{
    public static class ExcelParser
    {
        public static List<OrderRow> ReadExcelOrCsv(string path, string sheetName)
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

        public class OrderRow
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
