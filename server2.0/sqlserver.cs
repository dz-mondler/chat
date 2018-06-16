using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace server2._0
{
    static class sqlserver
    {
        private static string sqlAdd = "server=.;database=chatData;uid=root;pwd=";
        //修改 sql server 地址
        public static void changeAddress(string address,string uid,string pwd)
        {
            sqlAdd = "server=" + address + ";database=chatData;uid=" + uid + ";pwd=" + pwd ;
        }
        //数据库查询操作，返回datatable，表名为：account
        public static DataTable SQLselect(string sql)
        {
            SqlConnection coon = new SqlConnection(sqlAdd);
            try
            {
                coon.Open();
            }
            catch (SqlException)
            {
                DataTable d = null;
                return d;
            }
            
            SqlCommand cmd = new SqlCommand(sql, coon);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            coon.Dispose();
            cmd.Dispose();
            da.Dispose();
            return dt;
        }

        //数据库插入更新操作,返回受影响行数
        public static int SQLupdate(string sql)
        {
            SqlConnection coon = new SqlConnection(sqlAdd);
            try
            {
                coon.Open();
            }
            catch (SqlException)
            {
                return -1;
            }
            SqlCommand cmd = new SqlCommand(sql, coon);
            int effPeople = (int)cmd.ExecuteNonQuery();
            coon.Close();
            coon.Dispose();
            cmd.Dispose();
            return effPeople;
        }
        
    }
}
