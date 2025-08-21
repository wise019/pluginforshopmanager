using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DatabaseHelper
{
    /// <summary>
    /// SQL Server数据库读写助手类
    /// 支持SQL Server 2000及以上版本，不包含导出功能
    /// </summary>
    public class SqlServerDatabaseHelper
    {
        #region 私有字段

        // 写死的连接字符串配置
        private const string CONNECTION_STRING = @"Server=127.0.0.1,1433;Trusted_Connection=yes;Connection Timeout=10;";
        private const string DATABASE_NAME = "mp_Restaurant";

        #endregion

        #region 数据模型类

        /// <summary>
        /// 表信息
        /// </summary>
        public class TableInfo
        {
            public string SchemaName { get; set; }
            public string TableName { get; set; }
            public string FullName => $"[{SchemaName}].[{TableName}]";
        }

        /// <summary>
        /// 列信息
        /// </summary>
        public class ColumnInfo
        {
            public string Name { get; set; }
            public string DataType { get; set; }
            public int Length { get; set; }
            public int Precision { get; set; }
            public int Scale { get; set; }
            public bool IsNullable { get; set; }
            public bool IsIdentity { get; set; }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        /// <returns>数据库连接对象</returns>
        public SqlConnection GetConnection()
        {
            try
            {
                var conn = new SqlConnection(CONNECTION_STRING);
                conn.Open();

                // 切换到指定数据库
                using (var cmd = new SqlCommand($"USE [{DATABASE_NAME}]", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine($"✅ 数据库连接成功: {DATABASE_NAME}");
                return conn;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 连接失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 获取所有用户表列表
        /// </summary>
        /// <returns>表信息列表</returns>
        public List<TableInfo> GetTableList()
        {
            var tables = new List<TableInfo>();

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT user_name(o.uid) AS schema_name, o.name AS table_name
                FROM sysobjects o
                WHERE o.xtype = 'U'
                ORDER BY o.name", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    tables.Add(new TableInfo
                    {
                        SchemaName = reader["schema_name"].ToString(),
                        TableName = reader["table_name"].ToString()
                    });
                }
            }

            return tables;
        }

        /// <summary>
        /// 获取表的列信息
        /// </summary>
        /// <param name="schemaName">架构名</param>
        /// <param name="tableName">表名</param>
        /// <returns>列信息列表</returns>
        public List<ColumnInfo> GetTableColumns(string schemaName, string tableName)
        {
            var columns = new List<ColumnInfo>();
            var tableRef = schemaName != "dbo" ? $"{schemaName}.{tableName}" : tableName;

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT 
                    c.colid,
                    c.name AS column_name,
                    t.name AS data_type,
                    c.length,
                    c.xprec AS col_precision,
                    c.xscale AS col_scale,
                    CASE WHEN c.isnullable = 1 THEN 1 ELSE 0 END AS is_nullable,
                    COLUMNPROPERTY(c.id, c.name, 'IsIdentity') AS is_identity
                FROM syscolumns c
                JOIN systypes t ON c.xtype = t.xtype AND c.usertype = t.usertype
                WHERE c.id = OBJECT_ID(@tableRef)
                ORDER BY c.colid", conn))
            {
                cmd.Parameters.AddWithValue("@tableRef", tableRef);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columns.Add(new ColumnInfo
                        {
                            Name = reader["column_name"].ToString(),
                            DataType = reader["data_type"].ToString(),
                            Length = Convert.ToInt32(reader["length"]),
                            Precision = Convert.ToInt32(reader["col_precision"]),
                            Scale = Convert.ToInt32(reader["col_scale"]),
                            IsNullable = Convert.ToInt32(reader["is_nullable"]) == 1,
                            IsIdentity = Convert.ToInt32(reader["is_identity"]) == 1
                        });
                    }
                }
            }

            return columns;
        }

        /// <summary>
        /// 获取表的主键列
        /// </summary>
        /// <param name="schemaName">架构名</param>
        /// <param name="tableName">表名</param>
        /// <returns>主键列名列表</returns>
        public List<string> GetPrimaryKeyColumns(string schemaName, string tableName)
        {
            var primaryKeys = new List<string>();

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT c.name
                FROM syscolumns c
                JOIN sysobjects o ON c.id = o.id
                JOIN sysindexkeys k ON c.id = k.id AND c.colid = k.colid
                JOIN sysindexes i ON k.id = i.id AND k.indid = i.indid
                WHERE o.name = @tableName 
                  AND user_name(o.uid) = @schemaName
                  AND (i.status & 2048) = 2048
                ORDER BY k.keyno", conn))
            {
                cmd.Parameters.AddWithValue("@tableName", tableName);
                cmd.Parameters.AddWithValue("@schemaName", schemaName);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        primaryKeys.Add(reader["name"].ToString());
                    }
                }
            }

            return primaryKeys;
        }

        /// <summary>
        /// 获取表的行数
        /// </summary>
        /// <param name="schemaName">架构名</param>
        /// <param name="tableName">表名</param>
        /// <returns>行数</returns>
        public int GetTableRowCount(string schemaName, string tableName)
        {
            var fullTableName = $"[{schemaName}].[{tableName}]";

            using (var conn = GetConnection())
            using (var cmd = new SqlCommand($"SELECT COUNT(*) FROM {fullTableName}", conn))
            {
                return (int)cmd.ExecuteScalar();
            }
        }



        /// <summary>
        /// 执行自定义SQL查询
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>查询结果</returns>
        public DataTable ExecuteQuery(string sql)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                var dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        /// <summary>
        /// 执行非查询SQL命令（INSERT, UPDATE, DELETE等）
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>受影响的行数</returns>
        public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                return cmd.ExecuteNonQuery();
            }
        }

        #endregion


    }

    
}