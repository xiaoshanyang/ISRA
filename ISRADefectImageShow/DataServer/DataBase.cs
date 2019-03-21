using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ISRADefectImageShow.DataServer
{
    /// <summary>
    /// 通用数据库(SqlServer)类
    /// </summary>
    public class DataBase
    {
        #region 域成员

        private string m_ConnStr = null;

        #endregion

        #region 属性

        /// <summary>
        /// 数据库的连接字符串
        /// </summary>
        public string ConnStr
        {
            get
            {
                return m_ConnStr;
            }
            set
            {
                m_ConnStr = value;
            }
        }

        #endregion

        #region 构造器

        public DataBase()
        {
            //ConnStr = "Server=192.168.101.8;user id=sa;password=msi;database=GBDB20181224;";
            ConnStr = "Server=192.168.101.8;user id=sa;password=msi;database=GBDB;";
        }
        public DataBase(string str)
        {
            ConnStr = str;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 返回SqlConnection对象
        /// </summary>
        /// <returns>SqlConnection对象</returns>
        public SqlConnection ReturnConn()
        {
            try
            {
                SqlConnection Conn = new SqlConnection(ConnStr);
                Conn.Open();
                return Conn;
            }
            catch
            {
                throw new Exception("数据库连接字符串不正确，不能打开数据库！");
            }
        }
        /// <summary>
        /// 释放SqlConnection对象
        /// </summary>
        /// <param name="Conn"></param>
        public void Dispose(SqlConnection Conn)
        {
            if (Conn != null)
            {
                Conn.Close();
                Conn.Dispose();
            }
            GC.Collect();
        }
        /// <summary>
        /// 运行SQL语句
        /// </summary>
        /// <param name="SQL"></param>
        public void RunProc(string SQL)
        {
            SqlConnection Conn;
            Conn = new SqlConnection(ConnStr);
            Conn.Open();
            SqlCommand Cmd;
            Cmd = CreateCmd(SQL, Conn);
            try
            {
                Cmd.ExecuteNonQuery();
            }
            catch
            {
                throw new Exception(SQL);
            }
            finally
            {
                Dispose(Conn);
            }
        }
        /// <summary>
        /// 生成一个存储过程使用的sqlcommand.
        /// </summary>
        /// <param name="procName">存储过程名.</param>
        /// <param name="prams">存储过程入参数组.</param>
        /// <returns>sqlcommand对象.</returns>
        public SqlDataReader RunProcGetReader(string SQL)
        {
            SqlConnection Conn;
            Conn = new SqlConnection(ConnStr);
            Conn.Open();
            SqlCommand Cmd;
            Cmd = CreateCmd(SQL, Conn);
            SqlDataReader Dr;
            try
            {
                Dr = Cmd.ExecuteReader(CommandBehavior.Default);
                return Dr;
            }
            catch
            {
                throw new Exception(SQL);
            }
            //Dispose(Conn);
        }

        /*************新增函数**************/
        /// <summary>
        /// 关闭函数RunProcGetReader用过的SqlDataReader对象
        /// </summary>
        /// <param name="Dr">SqlDataReader对象</param>
        public void CloseSqlDataReader(SqlDataReader Dr)
        {
            if (Dr == null)
                return;
            try
            {
                if (!Dr.IsClosed)
                    Dr.Close();
            }
            catch
            {
                throw new Exception("无法关闭SqlDataReader对象");
            }
        }

        /// <summary>
        /// 生成Command对象
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="Conn"></param>
        /// <returns></returns>
        public SqlCommand CreateCmd(string SQL, SqlConnection Conn)
        {
            SqlCommand Cmd;
            Cmd = new SqlCommand(SQL, Conn);
            return Cmd;
        }

        /// <summary>
        /// 生成Command对象
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        public SqlCommand CreateCmd(string SQL)
        {
            SqlConnection Conn;
            Conn = new SqlConnection(ConnStr);
            Conn.Open();
            SqlCommand Cmd;
            Cmd = new SqlCommand(SQL, Conn);

            return Cmd;
        }
        /*************新增函数**************/
        /// <summary>
        /// 生成Command对象
        /// </summary>
        /// <returns><返回一个连接到当前数据库的Command对象/returns>
        public SqlCommand CreateCmd()
        {
            SqlConnection Conn;
            Conn = new SqlConnection(ConnStr);
            Conn.Open();
            SqlCommand Cmd;
            Cmd = new SqlCommand();
            Cmd.Connection = Conn;

            return Cmd;
        }
        /// <summary>
        /// 返回adapter对象
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="Conn"></param>
        /// <returns></returns>
        public SqlDataAdapter CreateDa(string SQL)
        {
            SqlConnection Conn;
            Conn = new SqlConnection(ConnStr);
            Conn.Open();
            SqlDataAdapter Da;
            Da = new SqlDataAdapter(SQL, Conn);
            return Da;
        }
        /**************************************************************************
         * 
         * 修改： 函数 返回类型由DataSet该为返回int类型
         *         返回值1代表成功 0代表失败
         * 
         * **********************************************************************/
        /// <summary>
        /// 运行SQL语句,返回DataSet对象 
        /// </summary>
        /// <param name="procName">SQL语句</param>
        /// <param name="prams">返回值1代表成功 0代表失败</param>
        public int RunProc(string SQL, DataSet Ds)
        {
            SqlConnection Conn;
            Conn = new SqlConnection(ConnStr);
            Conn.Open();
            SqlDataAdapter Da;
            //Da = CreateDa(SQL, Conn);
            Da = new SqlDataAdapter(SQL, Conn);
            try
            {
                Da.Fill(Ds);
                return 1;
            }
            catch (Exception ex)
            {
                return 0;
                //throw new Exception(SQL);
            }
            finally
            {
                Dispose(Conn);
            }
        }

        /**************************************************************************
       * 
       * 修改： 函数 返回类型由DataSet该为返回int类型
       *         返回值1代表成功 0代表失败
       * 以前版本：public DataSet RunProc(string SQL ,DataSet Ds,string tablename) 
       * 
       * **********************************************************************/
        /// <summary>
        /// 运行SQL语句,返回DataSet对象
        /// </summary>
        /// <param name="procName">SQL语句</param>
        /// <param name="prams">DataSet对象</param>
        /// <param name="dataReader">表名</param>
        public int RunProc(string SQL, DataSet Ds, string tablename)
        {
            SqlConnection Conn;
            Conn = new SqlConnection(ConnStr);
            Conn.Open();
            SqlDataAdapter Da;
            Da = CreateDa(SQL);
            try
            {
                Da.Fill(Ds, tablename);
                return 1;
            }
            catch
            {
                return 0;
            }
            finally
            {
                Dispose(Conn);
            }
            //return Ds;
        }

        /**************************************************************************
         * 
         * 修改： 函数 返回类型由DataSet该为返回int类型
         *         返回值1代表成功 0代表失败
         * 
         * **********************************************************************/
        /// <summary>
        /// 运行SQL语句,返回DataSet对象
        /// </summary>
        /// <param name="procName">SQL语句</param>
        /// <param name="prams">DataSet对象</param>
        /// <param name="dataReader">表名</param>
        public int RunProc(string SQL, DataSet Ds, int StartIndex, int PageSize, string tablename)
        {
            SqlConnection Conn;
            Conn = new SqlConnection(ConnStr);
            Conn.Open();
            SqlDataAdapter Da;
            Da = CreateDa(SQL);
            try
            {
                Da.Fill(Ds, StartIndex, PageSize, tablename);
                return 1;
            }
            catch//(Exception Ex)
            {
                // throw else;
                return 0;
            }
            finally
            {

                Dispose(Conn);
            }
            //return Ds;
        }

        /// <summary>
        /// 检验是否存在数据
        /// </summary>
        /// <returns></returns>
        public bool ExistDate(string SQL)
        {
            SqlConnection Conn;
            Conn = new SqlConnection(ConnStr);
            Conn.Open();
            SqlDataReader Dr;
            Dr = CreateCmd(SQL, Conn).ExecuteReader();

            if (Dr.HasRows)
            {
                Dr.Close();
                Dispose(Conn);
                return true;
            }
            else
            {
                Dr.Close();
                Dispose(Conn);
                return false;
            }
        }
        /// <summary>
        /// 返回SQL语句第一列,第一行
        /// </summary>
        /// <returns>字符串</returns>
        public string ReturnValue(string SQL)
        {
            SqlConnection Conn;
            Conn = new SqlConnection(ConnStr);
            Conn.Open();
            string result;
            SqlDataReader Dr;
            try
            {
                Dr = CreateCmd(SQL, Conn).ExecuteReader();
                if (Dr.Read())
                {
                    result = Dr[0].ToString();
                    Dr.Close();
                }
                else
                {
                    result = "";
                    Dr.Close();
                }
            }
            catch
            {
                throw new Exception(SQL);
            }
            finally
            {
                Dispose(Conn);
            }
            return result;
        }
        /// <summary>
        /// 返回SQL语句第Sit列,第一行,
        /// </summary>
        /// <returns>字符串</returns>
        public string ReturnValue(string SQL, int Sit)
        {
            SqlConnection Conn;
            Conn = new SqlConnection(ConnStr);
            Conn.Open();
            string result;
            SqlDataReader Dr;
            try
            {
                Dr = CreateCmd(SQL, Conn).ExecuteReader();
            }
            catch
            {
                throw new Exception(SQL);
            }
            if (Dr.Read())
            {
                result = Dr[Sit].ToString();
            }
            else
            {
                result = "";
            }

            Dr.Close();
            Dispose(Conn);
            return result;
        }

        /// <summary>
        /// 生成一个存储过程使用的sqlcommand.
        /// </summary>
        /// <param name="procName">存储过程名.</param>
        /// <param name="prams">存储过程入参数组.</param>
        /// <returns>sqlcommand对象.</returns>
        public SqlCommand CreateCmd(string procName, SqlParameter[] prams)
        {
            SqlConnection Conn;
            Conn = new SqlConnection(ConnStr);
            Conn.Open();
            SqlCommand Cmd = new SqlCommand(procName, Conn);
            Cmd.CommandType = CommandType.StoredProcedure;
            if (prams != null)
            {
                foreach (SqlParameter parameter in prams)
                {
                    if (parameter != null)
                    {
                        Cmd.Parameters.Add(parameter);
                    }
                }
            }
            return Cmd;
        }
        /// <summary>
        /// 为存储过程生成一个SqlCommand对象
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="prams">存储过程参数</param>
        /// <returns>SqlCommand对象</returns>
        private SqlCommand CreateCmd(string procName, SqlParameter[] prams, SqlDataReader Dr)
        {
            SqlConnection Conn;
            Conn = new SqlConnection(ConnStr);
            Conn.Open();
            SqlCommand Cmd = new SqlCommand(procName, Conn);
            Cmd.CommandType = CommandType.StoredProcedure;
            if (prams != null)
            {
                foreach (SqlParameter parameter in prams)
                    Cmd.Parameters.Add(parameter);
            }
            Cmd.Parameters.Add(
                new SqlParameter("ReturnValue", SqlDbType.Int, 4,
                ParameterDirection.ReturnValue, false, 0, 0,
                string.Empty, DataRowVersion.Default, null));

            return Cmd;
        }

        /// <summary>
        /// 运行存储过程,返回.
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="prams">存储过程参数</param>
        /// <param name="dataReader">SqlDataReader对象</param>
        public void RunProc(string procName, SqlParameter[] prams, SqlDataReader Dr)
        {

            SqlCommand Cmd = CreateCmd(procName, prams, Dr);
            Dr = Cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return;
        }
        /// <summary>
        /// 运行存储过程,返回.
        /// </summary>
        /// <param name="procName">存储过程名</param>
        /// <param name="prams">存储过程参数</param>
        public string RunProc(string procName, SqlParameter[] prams)
        {
            string result = "";
            SqlDataReader Dr;
            SqlCommand Cmd = CreateCmd(procName, prams);
            try
            {
                Dr = Cmd.ExecuteReader(CommandBehavior.CloseConnection);
                if (Dr.Read())
                {
                    result = Dr.GetValue(0).ToString();
                }
                Dr.Close();
                return result;
            }
            finally
            {
                Cmd.Connection.Close();
                Cmd.Connection.Dispose();
                Cmd.Dispose();
            }
        }
        /// <summary>
        /// 运行存储过程,返回dataset.
        /// </summary>
        /// <param name="procName">存储过程名.</param>
        /// <param name="prams">存储过程入参数组.</param>
        /// <returns>dataset对象.</returns>
        public DataSet RunProc(string procName, SqlParameter[] prams, DataSet Ds)
        {
            SqlCommand Cmd = CreateCmd(procName, prams);
            SqlDataAdapter Da = new SqlDataAdapter(Cmd);
            try
            {
                Da.Fill(Ds);
                return Ds;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                Da.Dispose();
                Cmd.Connection.Close();
                Cmd.Connection.Dispose();
                Cmd.Dispose();
            }
        }

        /// <summary>
        /// 运行存储过程,返回dataset.
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="prams"></param>
        /// <param name="Ds"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataSet RunProc(string procName, SqlParameter[] prams, DataSet Ds, string tableName)
        {
            SqlCommand Cmd = CreateCmd(procName, prams);
            SqlDataAdapter Da = new SqlDataAdapter(Cmd);
            try
            {
                Da.Fill(Ds, tableName);

                return Ds;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                Da.Dispose();
                Cmd.Connection.Close();
                Cmd.Connection.Dispose();
                Cmd.Dispose();
            }

        }


        /*新增函数*/
        /// <summary>
        /// 执行多条Sql操作并作为一个事务处理
        /// </summary>
        /// <param name="sql">传入的sql语句数组</param>
        public void RunSql(string[] sql)
        {
            if (sql == null)
                throw new Exception("没有要执行的Sql");
            SqlConnection conn = new SqlConnection(this.ConnStr);
            conn.Open();
            SqlCommand comm = new SqlCommand();
            comm.Connection = conn;
            SqlTransaction trans;
            trans = conn.BeginTransaction();

            try
            {
                comm.Transaction = trans;

                for (int i = 0; i < sql.Length; i++)
                {
                    comm.CommandText = sql[i];
                    comm.ExecuteNonQuery();
                }
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw new Exception("Sql执行失败");
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 批量插入 
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="prams"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public int Insert(string procName, SqlParameter[] prams, DataTable dt)
        {
            SqlCommand Cmd = CreateCmd(procName, prams);
            SqlDataAdapter Da = new SqlDataAdapter(Cmd);
            Da.InsertCommand = Cmd;

            DataTable dtNew = dt.GetChanges(DataRowState.Added);

            try
            {
                if (dtNew != null)
                {
                    return Da.Update(dtNew);
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
            finally
            {
                Da.Dispose();
                Cmd.Connection.Close();
                Cmd.Connection.Dispose();
                Cmd.Dispose();
            }
        }

        #endregion
    }
}