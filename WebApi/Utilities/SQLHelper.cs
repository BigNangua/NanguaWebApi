using System.Data;
using System.Data.SqlClient;
using System.Text;
namespace WebApi.Utilities
{
    /// <summary> 
    /// 用于SQL Server数据库访问的帮助类 
    /// </summary> 
    public class SQLHelper : IDisposable
    {
        private static string? _connectionString;

        private SqlConnection _connection4Tran;
        private SqlTransaction _transaction;
        private readonly Queue<SqlTask> _transactionTaskList;
        private int timeOutSeconds = 600;
        private bool _disposed = false;     // 定义是否已释放资源的标志

        /// <summary> 
        /// 创建一个新的SQLHelper并指定连接字符串使用默认的数据库连接
        /// </summary> 
        public SQLHelper()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            _connectionString = configuration.GetConnectionString("TESTconn");

            _connectionString = Encoding.Default.GetString(Convert.FromBase64String(_connectionString));
            // 初始化 _transactionTaskList
            _transactionTaskList = new Queue<SqlTask>();

        }

        /// <summary> 
        /// 创建一个新的SQLHelper并指定连接字符串并指定数据库连接
        /// </summary> 
        public SQLHelper(string connectionString)
        {
            _connectionString = Encoding.Default.GetString(Convert.FromBase64String(connectionString));
            // 初始化 _transactionTaskList
            _transactionTaskList = new Queue<SqlTask>();
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        /// <returns></returns>
        public bool TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    connection.Close();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        /// <summary> 
        /// ExecuteNonQuery操作，对数据库进行 增、删、改 操作(1） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <returns> </returns> 
        public int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, CommandType.Text, null);
        }

        /// <summary> 
        /// ExecuteNonQuery操作，对数据库进行 增、删、改 操作(1） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <param name="joinTransaction">是否加入事务处理；如果为true，前提是必须使用BeginTransaction开始了一个事务，如果为false,不使用事务。</param>
        /// <returns> </returns> 
        public int ExecuteNonQuery(string sql, bool joinTransaction)
        {
            return ExecuteNonQuery(sql, CommandType.Text, null, joinTransaction);
        }
        /// <summary> 
        /// ExecuteNonQuery操作，对数据库进行 增、删、改 操作（2） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <param name="commandType">要执行的查询类型（存储过程、SQL文本） </param> 
        /// <returns> </returns> 
        public int ExecuteNonQuery(string sql, CommandType commandType)
        {
            return ExecuteNonQuery(sql, commandType, null);
        }
        /// <summary> 
        /// ExecuteNonQuery操作，对数据库进行 增、删、改 操作（3） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <param name="commandType">要执行的查询类型（存储过程、SQL文本） </param> 
        /// <param name="parameters">参数数组 </param> 
        /// <returns> </returns> 
        public int ExecuteNonQuery(string sql, CommandType commandType, SqlParameter[] parameters)
        {
            return ExecuteNonQuery(sql, commandType, parameters, false);
        }

        /// <summary>
        /// ExecuteNonQuery操作，对数据库进行 增、删、改 操作(4） 
        /// </summary>
        /// <param name="sql">要执行的SQL语句</param>
        /// <param name="commandType">要执行的查询类型（存储过程、SQL文本）</param>
        /// <param name="parameters">参数数组</param>
        /// <param name="joinTransaction">是否加入事务处理；如果为true，前提是必须使用BeginTransaction开始了一个事务，如果为false,不使用事务。</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, CommandType commandType, SqlParameter[] parameters, bool joinTransaction)
        {
            int count = 0;
            if (joinTransaction)
            {
                if (_transaction == null || _connection4Tran == null)
                {
                    throw new Exception("事务未初始化！");
                }
                //using (var command = new SqlCommand(sql, _connection4Tran))
                //{
                //    command.CommandType = commandType;
                //    if (parameters != null)
                //    {
                //        foreach (SqlParameter parameter in parameters)
                //        {
                //            command.Parameters.Add(parameter);
                //        }
                //    }
                //    command.Transaction = _transaction;
                //    count = command.ExecuteNonQuery();
                //}
                _transactionTaskList.Enqueue(new SqlTask(sql, commandType, parameters));
            }
            else
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = commandType;
                        command.CommandTimeout = timeOutSeconds;
                        if (parameters != null)
                        {
                            foreach (SqlParameter parameter in parameters)
                            {
                                command.Parameters.Add(parameter);
                            }
                        }
                        connection.Open();
                        count = command.ExecuteNonQuery();
                    }
                }
            }
            return count;
        }

        /// <summary> 
        /// SqlDataAdapter的Fill方法执行一个查询，并返回一个DataSet类型结果（1） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <returns> </returns> 
        public DataSet ExecuteDataSet(string sql)
        {
            return ExecuteDataSet(sql, CommandType.Text, null);
        }
        /// <summary> 
        /// SqlDataAdapter的Fill方法执行一个查询，并返回一个DataSet类型结果（2） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <param name="commandType">要执行的查询类型（存储过程、SQL文本） </param> 
        /// <returns> </returns> 
        public DataSet ExecuteDataSet(string sql, CommandType commandType)
        {
            return ExecuteDataSet(sql, commandType, null);
        }
        /// <summary> 
        /// SqlDataAdapter的Fill方法执行一个查询，并返回一个DataSet类型结果（3） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <param name="commandType">要执行的查询类型（存储过程、SQL文本） </param> 
        /// <param name="parameters">参数数组 </param> 
        /// <returns> </returns> 
        public DataSet ExecuteDataSet(string sql, CommandType commandType, SqlParameter[] parameters)
        {
            var ds = new DataSet();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = commandType;
                    command.CommandTimeout = timeOutSeconds;
                    if (parameters != null)
                    {
                        foreach (SqlParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }
                    var adapter = new SqlDataAdapter(command);
                    adapter.Fill(ds);
                    command.Parameters.Clear();//将使用完之后的Command命令的Parameters集合清空
                }
            }
            return ds;
        }
        /// <summary> 
        /// SqlDataAdapter的Fill方法执行一个查询，并返回一个DataTable类型结果（1） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <returns> </returns> 
        public DataTable ExecuteDataTable(string sql)
        {
            return ExecuteDataTable(sql, CommandType.Text, null);
        }
        /// <summary> 
        /// SqlDataAdapter的Fill方法执行一个查询，并返回一个DataTable类型结果（2） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <param name="commandType">要执行的查询类型（存储过程、SQL文本） </param> 
        /// <returns> </returns> 
        public DataTable ExecuteDataTable(string sql, CommandType commandType)
        {
            return ExecuteDataTable(sql, commandType, null);
        }
        /// <summary> 
        /// SqlDataAdapter的Fill方法执行一个查询，并返回一个DataTable类型结果（3） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <param name="commandType">要执行的查询类型（存储过程、SQL文本） </param> 
        /// <param name="parameters">参数数组 </param> 
        /// <returns> </returns> 
        public DataTable ExecuteDataTable(string sql, CommandType commandType, SqlParameter[] parameters)
        {
            var data = new DataTable();
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = commandType;
                    command.CommandTimeout = timeOutSeconds;
                    if (parameters != null)
                    {
                        foreach (SqlParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }
                    var adapter = new SqlDataAdapter(command);
                    adapter.Fill(data);
                    command.Parameters.Clear();//将使用完之后的Command命令的Parameters集合清空
                }
            }
            return data;
        }
        /// <summary> 
        /// ExecuteReader执行一查询，返回一SqlDataReader对象实例（1） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <returns> </returns> 
        public SqlDataReader ExecuteReader(string sql)
        {
            return ExecuteReader(sql, CommandType.Text, null);
        }
        /// <summary> 
        /// ExecuteReader执行一查询，返回一SqlDataReader对象实例（2） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <param name="commandType">要执行的查询类型（存储过程、SQL文本） </param> 
        /// <returns> </returns> 
        public SqlDataReader ExecuteReader(string sql, CommandType commandType)
        {
            return ExecuteReader(sql, commandType, null);
        }
        /// <summary> 
        /// ExecuteReader执行一查询，返回一SqlDataReader对象实例（3） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <param name="commandType">要执行的查询类型（存储过程、SQL文本） </param> 
        /// <param name="parameters">参数数组 </param> 
        /// <returns> </returns> 
        public SqlDataReader ExecuteReader(string sql, CommandType commandType, SqlParameter[] parameters)
        {
            var connection = new SqlConnection(_connectionString);
            var command = new SqlCommand(sql, connection) { CommandType = commandType, CommandTimeout = timeOutSeconds };
            if (parameters != null)
            {
                foreach (SqlParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }
            connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }
        /// <summary> 
        /// ExecuteScalar执行一查询，返回查询结果的第一行第一列（1） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <returns> </returns> 
        public Object ExecuteScalar(string sql)
        {
            return ExecuteScalar(sql, CommandType.Text, null);
        }
        /// <summary> 
        /// ExecuteScalar执行一查询，返回查询结果的第一行第一列（2） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <param name="commandType">要执行的查询类型（存储过程、SQL文本） </param> 
        /// <returns> </returns> 
        public Object ExecuteScalar(string sql, CommandType commandType)
        {
            return ExecuteScalar(sql, commandType, null);
        }

        /// <summary> 
        /// ExecuteScalar执行一查询，返回查询结果的第一行第一列（3） 
        /// </summary> 
        /// <param name="sql">要执行的SQL语句 </param> 
        /// <param name="commandType">要执行的查询类型（存储过程、SQL文本） </param>
        /// <param name="parameters">参数数组</param>
        /// <returns> </returns> 
        public Object ExecuteScalar(string sql, CommandType commandType, SqlParameter[] parameters)
        {
            object result;
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sql, connection))
                {
                    command.CommandType = commandType;
                    command.CommandTimeout = timeOutSeconds;
                    if (parameters != null)
                    {
                        foreach (SqlParameter parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }
                    connection.Open();
                    result = command.ExecuteScalar();
                    command.Parameters.Clear();//将使用完之后的Command命令的Parameters集合清空
                }
            }
            return result;
        }
        /// <summary> 
        /// 返回当前连接的数据库中所有由用户创建的数据表 
        /// </summary> 
        /// <returns> </returns> 
        public DataTable GetTables()
        {
            DataTable data;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                data = connection.GetSchema("Tables");
            }
            return data;
        }

        /// <summary> 
        /// 返回当前连接的数据库列表 
        /// </summary> 
        /// <returns> </returns> 
        public DataTable GetDatabases()
        {
            DataTable data;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                data = connection.GetSchema("Databases");
            }
            return data;
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (_transaction != null || _connection4Tran != null)
            {
                throw new InvalidOperationException("要开始一个新的事务，请先完成当前事务!");
            }

            try
            {
                _connection4Tran = new SqlConnection(_connectionString);
                _connection4Tran.Open();
                _transaction = _connection4Tran.BeginTransaction(isolationLevel);
                _transactionTaskList.Clear(); // 开始事务时清空队列
            }
            catch (Exception ex)
            {
                // 日志记录或异常处理
                throw new Exception("启动事务失败", ex);
            }
        }

        /// <summary>
        /// 提交事务 
        /// </summary>
        public void CommitTransaction()
        {
            try
            {
                if (_transactionTaskList.Count > 0)
                {
                    foreach (SqlTask sqlTask in _transactionTaskList)
                    {
                        using (var command = new SqlCommand(sqlTask.Text, _connection4Tran))
                        {
                            command.CommandType = sqlTask.CommandType;
                            command.CommandTimeout = timeOutSeconds;
                            if (sqlTask.Parameters != null)
                            {
                                foreach (SqlParameter parameter in sqlTask.Parameters)
                                {
                                    command.Parameters.Add(parameter);
                                }
                            }
                            command.Transaction = _transaction;
                            command.ExecuteNonQuery();
                        }
                    }
                }
                _transaction.Commit();
            }
            catch (Exception)
            {
                _transaction.Rollback();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                }
                if (_connection4Tran != null)
                {
                    _connection4Tran.Close();
                }
                if (_connection4Tran != null)
                {
                    _connection4Tran.Dispose();
                }
                _transaction = null;
                _connection4Tran = null;
                _transactionTaskList.Clear();
            }
        }

        /// <summary>
        /// 通过存储过程返回结果集方法
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="param">参数组</param>
        /// <returns>返回SqlDataReader数据结果集</returns>
        public SqlDataReader GetReaderByProcedure(string procedureName, SqlParameter[] param)
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand();
            try
            {
                conn.Open();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = timeOutSeconds;
                cmd.CommandText = procedureName;
                cmd.Parameters.AddRange(param);
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                WritterError("执行 GetReaderByProcedure(string procedureName,SqlParameter[] param)出现异常:", ex.Message);
                conn.Close();
                throw;
            }
        }

        /// <summary>
        /// 通过存储过程返回一个DataSet数据集
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="param">存储过程所需参数</param>
        /// <returns>DataSet数据集</returns>
        public DataSet GetDataSetByProcedure(string procedureName, SqlParameter[] param)
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            SqlCommand cmd = new SqlCommand(procedureName, conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            try
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = timeOutSeconds;
                cmd.Parameters.AddRange(param);
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds;

            }
            catch (Exception ex)
            {
                //WritterError("执行GetTableByProcedure(string procedureName,SqlParameter[] param)出现异常:", ex.Message);
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }

        #region 错误信息写入日志
        public void WritterError(string errorObjName, string exMessage)
        {
            FileStream fs = new FileStream("libraryError.log", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine(string.Format("{0}{1}", errorObjName, exMessage + GetServerTime()));
            sw.Close();
            fs.Close();
        }
        #endregion

        #region 获取服务器时间
        public DateTime GetServerTime()
        {
            return Convert.ToDateTime(ExecuteScalar("select getdate()"));
        }

        #endregion

        /// <summary>
        /// 取消事务
        /// </summary>
        public void CancelTransaction()
        {
            if (_transaction != null)
            {
                try
                {
                    // 回滚事务
                    _transaction.Rollback();
                }
                catch (Exception ex)
                {
                    // 回滚失败时，记录日志或者做其他处理
                    throw new Exception("回滚事务失败", ex);
                }
                finally
                {
                    // 清理资源
                    _transaction.Dispose();
                    _transaction = null;
                }
            }

            if (_connection4Tran != null)
            {
                _connection4Tran.Close();
                _connection4Tran.Dispose();
                _connection4Tran = null;
            }
        }

        #region ICloneable 成员

        //public  object Clone()
        //{
        //    return (new SQLHelper(_connectionString));
        //}

        #endregion

        #region IDisposable 成员

        //public void Dispose()
        //{
        //    _connection4Tran = null;
        //    _transaction = null;
        //}

        #endregion

        public class SqlTask
        {
            public string Text { get; private set; }
            public CommandType CommandType { get; private set; }
            public SqlParameter[] Parameters { get; private set; }

            public SqlTask(string text, CommandType commandType, SqlParameter[] paras)
            {
                Text = text;
                CommandType = commandType;
                Parameters = paras;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt">数据表，必须和同步的表结构一致</param>
        /// <param name="tbName">同步的表名</param>
        /// <param name="message">返回消息</param>
        /// <returns></returns>
        public bool DataTableToSQLServer(DataTable dt, string tbName)//, ref string message
        {
            if (dt == null || dt.Rows.Count == 0)
                throw new Exception("表数据为空,请先填充有效数据");
            dt.TableName = tbName;
            using (SqlConnection destinationConnection = new SqlConnection(_connectionString))
            {
                destinationConnection.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection))
                {
                    bulkCopy.BulkCopyTimeout = timeOutSeconds;
                    bulkCopy.DestinationTableName = tbName;//要插入的表的表名
                    bulkCopy.BatchSize = dt.Rows.Count;

                    foreach (DataColumn col in dt.Columns)
                    {
                        string colName = col.ColumnName;
                        bulkCopy.ColumnMappings.Add(colName, colName);//映射字段名 DataTable列名 ,数据库 对应的列名  
                    }
                    bulkCopy.WriteToServer(dt);
                    return true;
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _connection4Tran?.Dispose();
                _disposed = true;
            }
        }
    }
}
