using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

/// <summary>
/// 数据库操作类
/// </summary>
public sealed class SQLiteHelper
{
    private string connectionString = string.Empty;

    public SQLiteHelper()
    {
    }

    public SQLiteHelper(string datasource, string password)
    {
        SetConnectionString(datasource, password);
    }

    /// <summary>
    /// 根据数据源、密码、版本号设置连接字符串。
    /// </summary>
    /// <param name="datasource">数据源。</param>
    /// <param name="password">密码。</param>
    /// <param name="version">版本号（缺省为3）。</param>
    public void SetConnectionString(string datasource, string password, int version = 3)
    {
        connectionString = string.Format("Data Source={0};Version={1};password={2}", datasource, version, password);
    }

    /// <summary>
    /// 使用 connectionString 配置信息创建一个数据库文件
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void CreateDB()
    {
        try { SQLiteConnection.CreateFile(connectionString); }
        catch (Exception e1)
        {
            System.Diagnostics.Debug.WriteLine("SQLiteHelper CreateDB Exception: " + e1.Message);
        }
    }

    /// <summary>
    /// 判断表是否存在
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <returns>true为存在，false为不存在</returns>
    public bool TableIsExist(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            return false;

        string sql = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='" + tableName + "'";
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                try
                {
                    connection.Open();
                    command.CommandText = sql;
                    if (0 == System.Convert.ToInt32(command.ExecuteScalar()))
                        return false;
                    else
                        return true;
                }
                catch (Exception e1)
                {
                    System.Diagnostics.Debug.WriteLine("SQLiteHelper TableIsExist [" + tableName + "]  Exception: " + e1.Message);
                    return false;
                }
            }
        }
    }

    /// <summary>
    /// 判断表内记录是否为空
    /// </summary>
    /// <param name="tableName">表名</param>
    /// <returns>true为空，false非空</returns>
    public bool TableIsNull(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            return false;

        string sql = "SELECT count(*) FROM " + tableName;
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                try
                {
                    connection.Open();
                    command.CommandText = sql;
                    if (0 == System.Convert.ToInt32(command.ExecuteScalar()))
                        return true;
                    else
                        return false;
                }
                catch (Exception e1)
                {
                    System.Diagnostics.Debug.WriteLine("SQLiteHelper TableIsNull [" + tableName + "]  Exception: " + e1.Message);
                    return false;
                }
            }
        }
    }

    /// <summary> 
    /// 对SQLite数据库执行增删改操作，返回受影响的行数。 
    /// </summary> 
    /// <param name="sql">要执行的增删改的SQL语句。</param> 
    /// <param name="parameters">执行增删改语句所需要的参数，参数必须以它们在SQL语句中的顺序为准。</param> 
    /// <returns></returns> 
    /// <exception cref="Exception"></exception>
    public int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
    {
        int affectedRows = 0;
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                try
                {
                    connection.Open();
                    command.CommandText = sql;
                    if (parameters.Length != 0)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    affectedRows = command.ExecuteNonQuery();
                }
                catch (Exception e1)
                {
                    System.Diagnostics.Debug.WriteLine("SQLiteHelper ExecuteNonQuery [" + sql + "] Exception: " + e1.Message);
                }
            }
        }
        return affectedRows;
    }

    /// <summary>
    /// 批量处理数据操作语句。
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="par">SQL参数列表的List</param>
    /// <exception cref="Exception"></exception>
    public void ExecuteNonQueryBatch(string sql, List<SQLiteParameter[]> par)
    {
        using (SQLiteConnection conn = new SQLiteConnection(connectionString))
        {
            try { conn.Open(); }
            catch { throw; }
            using (SQLiteTransaction tran = conn.BeginTransaction())
            {
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    try
                    {
                        cmd.CommandText = sql;
                        foreach (var item in par)
                        {
                            if (item != null)
                            {
                                cmd.Parameters.AddRange(item);
                            }
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        tran.Commit();
                    }
                    catch (Exception e1)
                    {
                        tran.Rollback();
                        System.Diagnostics.Debug.WriteLine("SQLiteHelper ExecuteNonQueryBatch [" + sql + "] Exception: " + e1.Message);
                    }
                }
            }
        }
    }

    public void ExecuteNonQueryBatch(List<KeyValuePair<string, SQLiteParameter[]>> list)
    {
        using (SQLiteConnection conn = new SQLiteConnection(connectionString))
        {
            try { conn.Open(); }
            catch { throw; }
            using (SQLiteTransaction tran = conn.BeginTransaction())
            {
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    try
                    {
                        foreach (var item in list)
                        {
                            cmd.CommandText = item.Key;
                            if (item.Value != null)
                            {
                                cmd.Parameters.AddRange(item.Value);
                            }
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                    catch (Exception e1)
                    {
                        tran.Rollback();
                        System.Diagnostics.Debug.WriteLine("SQLiteHelper ExecuteNonQueryBatch Exception: " + e1.Message);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 执行查询语句，并返回第一个结果。
    /// </summary>
    /// <param name="sql">查询语句。</param>
    /// <returns>查询结果。</returns>
    /// <exception cref="Exception"></exception>
    public object ExecuteScalar(string sql, params SQLiteParameter[] parameters)
    {
        using (SQLiteConnection conn = new SQLiteConnection(connectionString))
        {
            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                try
                {
                    conn.Open();
                    cmd.CommandText = sql;
                    if (parameters.Length != 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteScalar();
                }
                catch (Exception e1)
                {
                    System.Diagnostics.Debug.WriteLine("SQLiteHelper ExecuteScalar [" + sql + "] Exception: " + e1.Message);
                }
            }
        }
        return null;
    }

    /// <summary> 
    /// 执行一个查询语句，返回一个包含查询结果的DataTable。 
    /// </summary> 
    /// <param name="sql">要执行的查询语句。</param> 
    /// <param name="parameters">执行SQL查询语句所需要的参数，参数必须以它们在SQL语句中的顺序为准。</param> 
    /// <returns></returns> 
    /// <exception cref="Exception"></exception>
    public DataTable ExecuteQuery(string sql, params SQLiteParameter[] parameters)
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                if (parameters.Length != 0)
                {
                    command.Parameters.AddRange(parameters);
                }
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                DataTable data = new DataTable();
                try { adapter.Fill(data); }
                catch (Exception e1)
                {
                    System.Diagnostics.Debug.WriteLine("SQLiteHelper ExecuteQuery [" + sql + "] Exception: " + e1.Message);
                }
                return data;
            }
        }
    }

    /// <summary> 
    /// 执行一个查询语句，返回一个关联的SQLiteDataReader实例。 
    /// </summary> 
    /// <param name="sql">要执行的查询语句。</param> 
    /// <param name="parameters">执行SQL查询语句所需要的参数，参数必须以它们在SQL语句中的顺序为准。</param> 
    /// <returns></returns> 
    /// <exception cref="Exception"></exception>
    public SQLiteDataReader ExecuteReader(string sql, params SQLiteParameter[] parameters)
    {
        SQLiteConnection connection = new SQLiteConnection(connectionString);
        SQLiteCommand command = new SQLiteCommand(sql, connection);
        try
        {
            if (parameters.Length != 0)
            {
                command.Parameters.AddRange(parameters);
            }
            connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }
        catch (Exception e1)
        {
            System.Diagnostics.Debug.WriteLine("SQLiteHelper ExecuteReader [" + sql + "] Exception: " + e1.Message);
        }
        return null;
    }

    /// <summary> 
    /// 查询数据库中的所有数据类型信息。
    /// </summary> 
    /// <returns></returns> 
    /// <exception cref="Exception"></exception>
    public DataTable GetSchema()
    {
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            try
            {
                connection.Open();
                return connection.GetSchema("TABLES");
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine("SQLiteHelper GetSchema Exception: " + e1.Message);
            }
        }
        return null;
    }
}