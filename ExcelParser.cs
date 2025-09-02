using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using OfficeOpenXml;
using NLog;
using System.ComponentModel;

namespace ExcelToStore
{
    /// <summary>
    /// Excel和CSV文件解析器（使用EPPlus，无需安装Office）
    /// </summary>
    public static class ExcelParser
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        // 列名常量
        private static class ColumnNames
        {
            public const string OrderNo = "订单编号";
            public const string OrderDate = "订单日期";
            public const string OrderTime = "订单时间点";
            public const string TableName = "桌台号";
            public const string GuestCount = "用餐人数";
            public const string ItemName = "菜品明细";
            public const string UnitPrice = "菜品单价";
            public const string Qty = "菜品数量";
            public const string OrderAmount = "订单金额";
        }

        static ExcelParser()
        {
            try
            {
                // 设置EPPlus许可证上下文（免费版本）
               // ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
               // Logger.Debug("EPPlus许可证上下文设置完成");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "初始化EPPlus时发生错误");
            }
        }

        /// <summary>
        /// 读取Excel或CSV文件并解析为订单行数据
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="sheetName">工作表名称（可选，默认使用第一个工作表）</param>
        /// <returns>订单行数据列表，如果发生错误返回空列表</returns>
        public static List<OrderRow> ReadExcelOrCsv(string path, string sheetName = null)
        {
            Logger.Info("开始解析文件: {0}, 工作表: {1}", path ?? "null", sheetName ?? "默认");

            if (!ValidateFilePath(path))
                return new List<OrderRow>();

            var ext = Path.GetExtension(path).ToLowerInvariant();
            Logger.Debug("检测到文件扩展名: {0}", ext);

            switch (ext)
            {
                case ".csv":
                    return ReadCsv(path);
                case ".xlsx":
                case ".xlsm":
                    return ReadExcel(path, sheetName);
                case ".xls":
                    Logger.Warn("不支持.xls格式文件: {0}，建议转换为.xlsx格式", path);
                    return new List<OrderRow>();
                default:
                    Logger.Error("不支持的文件格式: {0}，文件路径: {1}", ext, path);
                    return new List<OrderRow>();
            }
        }

        /// <summary>
        /// 获取Excel文件中的所有工作表名称
        /// </summary>
        /// <param name="path">Excel文件路径</param>
        /// <returns>工作表名称列表，如果发生错误返回空列表</returns>
        public static List<string> GetWorksheetNames(string path)
        {
            Logger.Info("获取Excel文件工作表列表: {0}", path);

            if (!ValidateFilePath(path))
                return new List<string>();

            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xlsm")
            {
                Logger.Error("获取工作表名称时文件格式不支持: {0}，仅支持.xlsx和.xlsm格式", ext);
                return new List<string>();
            }

            var worksheetNames = new List<string>();

            try
            {
                using (var package = new ExcelPackage(new FileInfo(path)))
                {
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {
                        worksheetNames.Add(worksheet.Name);
                        Logger.Debug("发现工作表: {0}", worksheet.Name);
                    }
                }

                Logger.Info("成功获取到 {0} 个工作表", worksheetNames.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "读取Excel文件工作表列表时发生错误: {0}", path);
                return new List<string>();
            }

            return worksheetNames;
        }

        /// <summary>
        /// 验证文件路径
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>验证是否通过</returns>
        private static bool ValidateFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Logger.Error("文件路径为空或null");
                return false;
            }

            if (!File.Exists(path))
            {
                Logger.Error("文件不存在: {0}", path);
                return false;
            }

            Logger.Debug("文件路径验证通过: {0}", path);
            return true;
        }

        /// <summary>
        /// 读取Excel文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="sheetName">工作表名称</param>
        /// <returns>订单行数据列表</returns>
        private static List<OrderRow> ReadExcel(string path, string sheetName)
        {
            Logger.Debug("开始读取Excel文件: {0}", path);

            try
            {
                using (var package = new ExcelPackage(new FileInfo(path)))
                {
                    ExcelWorksheet worksheet;

                    if (string.IsNullOrWhiteSpace(sheetName))
                    {
                        // 使用第一个工作表
                        if (package.Workbook.Worksheets.Count == 0)
                        {
                            Logger.Warn("Excel文件中没有找到任何工作表: {0}", path);
                            return new List<OrderRow>();
                        }

                        worksheet = package.Workbook.Worksheets[1];
                        Logger.Debug("使用第一个工作表: {0}", worksheet.Name);
                    }
                    else
                    {
                        // 使用指定的工作表
                        worksheet = package.Workbook.Worksheets[sheetName];
                        if (worksheet == null)
                        {
                            Logger.Error("找不到名为'{0}'的工作表，文件: {1}", sheetName, path);
                            return new List<OrderRow>();
                        }
                        Logger.Debug("使用指定工作表: {0}", sheetName);
                    }

                    var result = ParseExcelWorksheet(worksheet);
                    Logger.Info("Excel文件解析完成，共获得 {0} 条有效记录", result.Count);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "读取Excel文件时发生错误: {0}", path);
                return new List<OrderRow>();
            }
        }

        /// <summary>
        /// 解析Excel工作表
        /// </summary>
        /// <param name="worksheet">工作表对象</param>
        /// <returns>订单行数据列表</returns>
        private static List<OrderRow> ParseExcelWorksheet(ExcelWorksheet worksheet)
        {
            var result = new List<OrderRow>();

            if (worksheet.Dimension == null)
            {
                Logger.Warn("工作表 '{0}' 为空", worksheet.Name);
                return result;
            }

            int startRow = worksheet.Dimension.Start.Row;
            int endRow = worksheet.Dimension.End.Row;
            int startCol = worksheet.Dimension.Start.Column;
            int endCol = worksheet.Dimension.End.Column;

            Logger.Debug("工作表 '{0}' 数据范围: 行 {1}-{2}, 列 {3}-{4}",
                worksheet.Name, startRow, endRow, startCol, endCol);

            // 构建列映射（假设第一行是标题行）
            var columnMapping = BuildExcelColumnMapping(worksheet, startRow, startCol, endCol);

            if (columnMapping.Count == 0)
            {
                Logger.Warn("工作表 '{0}' 中没有找到有效的列标题", worksheet.Name);
                return result;
            }

            Logger.Debug("找到 {0} 个有效列标题", columnMapping.Count);

            // 从第二行开始读取数据
            int validRows = 0;
            int invalidRows = 0;

            for (int row = startRow + 1; row <= endRow; row++)
            {
                try
                {
                    var orderRow = ParseExcelRow(worksheet, row, columnMapping);
                    if (IsValidOrderRow(orderRow))
                    {
                        result.Add(orderRow);
                        validRows++;
                    }
                    else
                    {
                        invalidRows++;
                        Logger.Debug("第 {0} 行数据无效，订单编号: '{1}', 菜品: '{2}'",
                            row, orderRow.OrderNo ?? "空", orderRow.ItemName ?? "空");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "解析第 {0} 行数据时发生错误", row);
                    invalidRows++;
                }
            }

            Logger.Info("工作表 '{0}' 解析完成: 有效行 {1}, 无效行 {2}",
                worksheet.Name, validRows, invalidRows);

            return result;
        }

        /// <summary>
        /// 构建Excel列映射
        /// </summary>
        /// <param name="worksheet">工作表对象</param>
        /// <param name="headerRow">标题行</param>
        /// <param name="startCol">开始列</param>
        /// <param name="endCol">结束列</param>
        /// <returns>列映射字典</returns>
        private static Dictionary<string, int> BuildExcelColumnMapping(ExcelWorksheet worksheet, int headerRow, int startCol, int endCol)
        {
            var mapping = new Dictionary<string, int>();

            for (int col = startCol; col <= endCol; col++)
            {
                try
                {
                    var cellValue = worksheet.Cells[headerRow, col].Text?.Trim();
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        mapping[cellValue] = col;
                        Logger.Debug("映射列 '{0}' -> 第 {1} 列", cellValue, col);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "读取标题行第 {0} 列时发生错误", col);
                }
            }

            return mapping;
        }

        /// <summary>
        /// 解析Excel行数据
        /// </summary>
        /// <param name="worksheet">工作表对象</param>
        /// <param name="row">行号</param>
        /// <param name="columnMapping">列映射</param>
        /// <returns>订单行数据</returns>
        private static OrderRow ParseExcelRow(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMapping)
        {
            return new OrderRow
            {
                OrderNo = GetExcelCellValue(worksheet, row, columnMapping, ColumnNames.OrderNo),
                OrderDate = GetExcelCellValue(worksheet, row, columnMapping, ColumnNames.OrderDate),
                OrderTime = GetExcelCellValue(worksheet, row, columnMapping, ColumnNames.OrderTime),
                TableName = GetExcelCellValue(worksheet, row, columnMapping, ColumnNames.TableName),
                GuestCount = ParseInt(GetExcelCellValue(worksheet, row, columnMapping, ColumnNames.GuestCount)),
                ItemName = GetExcelCellValue(worksheet, row, columnMapping, ColumnNames.ItemName),
                UnitPrice = ParseDecimal(GetExcelCellValue(worksheet, row, columnMapping, ColumnNames.UnitPrice)),
                Qty = ParseDecimal(GetExcelCellValue(worksheet, row, columnMapping, ColumnNames.Qty), 1m),
                OrderAmount = ParseDecimal(GetExcelCellValue(worksheet, row, columnMapping, ColumnNames.OrderAmount))
            };
        }

        /// <summary>
        /// 获取Excel单元格值
        /// </summary>
        /// <param name="worksheet">工作表对象</param>
        /// <param name="row">行号</param>
        /// <param name="columnMapping">列映射</param>
        /// <param name="columnName">列名</param>
        /// <returns>单元格值</returns>
        private static string GetExcelCellValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMapping, string columnName)
        {
            try
            {
                if (columnMapping.TryGetValue(columnName, out int col))
                {
                    var cell = worksheet.Cells[row, col];
                    return cell.Text?.Trim() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "读取单元格值时发生错误: 行 {0}, 列 '{1}'", row, columnName);
            }
            return string.Empty;
        }

        /// <summary>
        /// 读取CSV文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>订单行数据列表</returns>
        private static List<OrderRow> ReadCsv(string path)
        {
            Logger.Debug("开始读取CSV文件: {0}", path);
            var list = new List<OrderRow>();

            try
            {
                using (var sr = new StreamReader(path, Encoding.UTF8))
                {
                    string header = sr.ReadLine();
                    if (string.IsNullOrEmpty(header))
                    {
                        Logger.Warn("CSV文件为空或没有标题行: {0}", path);
                        return list;
                    }

                    var columnMapping = BuildColumnMapping(header);
                    if (columnMapping.Count == 0)
                    {
                        Logger.Warn("CSV文件中没有找到有效的列标题: {0}", path);
                        return list;
                    }

                    Logger.Debug("CSV文件找到 {0} 个有效列", columnMapping.Count);

                    int lineNumber = 1;
                    int validRows = 0;
                    int invalidRows = 0;

                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lineNumber++;

                        try
                        {
                            var row = ParseCsvLine(line, columnMapping);
                            if (IsValidOrderRow(row))
                            {
                                list.Add(row);
                                validRows++;
                            }
                            else
                            {
                                invalidRows++;
                                Logger.Debug("CSV第 {0} 行数据无效，订单编号: '{1}', 菜品: '{2}'",
                                    lineNumber, row.OrderNo ?? "空", row.ItemName ?? "空");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Warn(ex, "解析CSV第 {0} 行数据时发生错误: {1}", lineNumber, line);
                            invalidRows++;
                        }
                    }

                    Logger.Info("CSV文件解析完成: 有效行 {0}, 无效行 {1}", validRows, invalidRows);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "读取CSV文件时发生错误: {0}", path);
                return new List<OrderRow>();
            }

            return list;
        }

        /// <summary>
        /// 构建CSV列映射
        /// </summary>
        /// <param name="header">标题行</param>
        /// <returns>列映射字典</returns>
        private static Dictionary<string, int> BuildColumnMapping(string header)
        {
            var cols = ParseCsvLine(header);
            var mapping = new Dictionary<string, int>();

            for (int i = 0; i < cols.Length; i++)
            {
                var colName = cols[i].Trim();
                if (!string.IsNullOrEmpty(colName))
                {
                    mapping[colName] = i;
                    Logger.Debug("CSV映射列 '{0}' -> 第 {1} 列", colName, i);
                }
            }

            return mapping;
        }

        /// <summary>
        /// 解析CSV行数据
        /// </summary>
        /// <param name="line">CSV行</param>
        /// <param name="columnMapping">列映射</param>
        /// <returns>订单行数据</returns>
        private static OrderRow ParseCsvLine(string line, Dictionary<string, int> columnMapping)
        {
            var cells = ParseCsvLine(line);

            return new OrderRow
            {
                OrderNo = GetCsvCellValue(cells, columnMapping, ColumnNames.OrderNo),
                OrderDate = GetCsvCellValue(cells, columnMapping, ColumnNames.OrderDate),
                OrderTime = GetCsvCellValue(cells, columnMapping, ColumnNames.OrderTime),
                TableName = GetCsvCellValue(cells, columnMapping, ColumnNames.TableName),
                GuestCount = ParseInt(GetCsvCellValue(cells, columnMapping, ColumnNames.GuestCount)),
                ItemName = GetCsvCellValue(cells, columnMapping, ColumnNames.ItemName),
                UnitPrice = ParseDecimal(GetCsvCellValue(cells, columnMapping, ColumnNames.UnitPrice)),
                Qty = ParseDecimal(GetCsvCellValue(cells, columnMapping, ColumnNames.Qty), 1m),
                OrderAmount = ParseDecimal(GetCsvCellValue(cells, columnMapping, ColumnNames.OrderAmount))
            };
        }

        /// <summary>
        /// 解析CSV行（处理引号和逗号）
        /// </summary>
        /// <param name="line">CSV行字符串</param>
        /// <returns>字段数组</returns>
        private static string[] ParseCsvLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return new string[0];

            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 转义的引号
                        current.Append('"');
                        i++; // 跳过下一个引号
                    }
                    else
                    {
                        // 切换引号状态
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // 字段分隔符
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            // 添加最后一个字段
            result.Add(current.ToString());

            return result.ToArray();
        }

        /// <summary>
        /// 获取CSV单元格值
        /// </summary>
        /// <param name="cells">单元格数组</param>
        /// <param name="columnMapping">列映射</param>
        /// <param name="columnName">列名</param>
        /// <returns>单元格值</returns>
        private static string GetCsvCellValue(string[] cells, Dictionary<string, int> columnMapping, string columnName)
        {
            try
            {
                if (columnMapping.TryGetValue(columnName, out int index) &&
                    index >= 0 && index < cells.Length)
                {
                    return cells[index]?.Trim() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "获取CSV单元格值时发生错误: 列 '{0}'", columnName);
            }
            return string.Empty;
        }

        /// <summary>
        /// 验证订单行是否有效
        /// </summary>
        /// <param name="row">订单行数据</param>
        /// <returns>是否有效</returns>
        private static bool IsValidOrderRow(OrderRow row)
        {
            return row != null &&
                   !string.IsNullOrEmpty(row.OrderNo) &&
                   !string.IsNullOrEmpty(row.ItemName);
        }

        /// <summary>
        /// 解析整数
        /// </summary>
        /// <param name="value">字符串值</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>解析结果</returns>
        private static int ParseInt(string value, int defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (int.TryParse(value, out int result))
                return result;

            Logger.Debug("无法解析整数值: '{0}', 使用默认值: {1}", value, defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// 解析小数
        /// </summary>
        /// <param name="value">字符串值</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>解析结果</returns>
        private static decimal ParseDecimal(string value, decimal defaultValue = 0m)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            // 首先尝试使用不变区域性解析
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;

            // 然后尝试使用当前区域性解析
            if (decimal.TryParse(value, out result))
                return result;

            Logger.Debug("无法解析小数值: '{0}', 使用默认值: {1}", value, defaultValue);
            return defaultValue;
        }
    }

    /// <summary>
    /// 订单行数据类
    /// </summary>
    public class OrderRow
    {
        /// <summary>订单编号</summary>
        public string OrderNo { get; set; }

        /// <summary>订单日期</summary>
        public string OrderDate { get; set; }

        /// <summary>订单时间点</summary>
        public string OrderTime { get; set; }

        /// <summary>桌台号</summary>
        public string TableName { get; set; }

        /// <summary>用餐人数</summary>
        public int GuestCount { get; set; }

        /// <summary>菜品明细</summary>
        public string ItemName { get; set; }

        /// <summary>菜品单价</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>菜品数量</summary>
        public decimal Qty { get; set; }

        /// <summary>订单金额</summary>
        public decimal OrderAmount { get; set; }

        public override string ToString()
        {
            return $"订单号: {OrderNo}, 菜品: {ItemName}, 数量: {Qty}";
        }
    }
}