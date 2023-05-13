using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using Dapper;

namespace GenCL.Utilities
{
    public class DataService
    {
        private static string _connectionString = string.Empty;
        public static string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }
        private static void Init()
        {
            string provider = ConfigurationManager.AppSettings["DefaultProvider"] + "";
            if (string.IsNullOrEmpty(ConnectionString))
                ConnectionString = ConfigurationManager.ConnectionStrings[provider].ConnectionString;
        }
        public static DataTable GetDataTable(string query, params SqlParameter[] parameters)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                Init();

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                string commandtext = query;

                SqlCommand cmd = new SqlCommand(commandtext, conn);

                if (parameters != null)
                {
                    foreach (var p in parameters)
                        cmd.Parameters.Add(p);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                    reader.Close();
                }
            }

            return dt;
        }

        public static List<T> FindList<T>(string cmd, object parameter = null)
        {

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                return conn.Query<T>(cmd, parameter).ToList();
            }

        }

        public static T Find<T>(string cmd, object param = null)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                return conn.QuerySingleOrDefault<T>(cmd, param);
            }
        }

        public static int ExecuteNonQuery(SqlCommand cmd)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                Init();

            int result = 0;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                cmd.Connection = conn;
                result = cmd.ExecuteNonQuery();
            }
            return result;
        }
        public static int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                Init();

            int result = 0;

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                string commandtext = query;

                SqlCommand cmd = new SqlCommand(commandtext, conn);
                if (parameters != null)
                {
                    foreach (var p in parameters)
                        cmd.Parameters.Add(p);
                }

                result = cmd.ExecuteNonQuery();
            }

            return result;
        }
    }
}