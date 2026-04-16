using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace AssetPerformanceManager.Data
{
    public static class DbHelper
    {
        // Метод для получения данных (SELECT)
        public static DataTable ExecuteQuery(string sql, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(DbConfig.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);

                    DataTable dt = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                    return dt;
                }
            }
        }

        // Метод для изменения данных (INSERT, UPDATE, DELETE)
        public static int ExecuteNonQuery(string sql, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(DbConfig.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery(); // Должно возвращать количество удаленных строк
                    return rowsAffected;
                }
            }
        }

        // Метод для получения одного значения (например, COUNT или SUM)
        public static object ExecuteScalar(string sql, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(DbConfig.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }
    }
}