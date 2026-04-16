using System;
using System.Data;
using Microsoft.Data.SqlClient;
using AssetPerformanceManager.Data;
using AssetPerformanceManager.Models;

namespace AssetPerformanceManager.Services
{
    public class AuthService
    {
        public bool Register(string username, string password, string fullname)
        {
            try
            {
                string sql = "INSERT INTO Users (Username, Password, FullName) VALUES (@u, @p, @f)";
                DbHelper.ExecuteNonQuery(sql, new SqlParameter[] {
                    new SqlParameter("@u", username),
                    new SqlParameter("@p", password), // В идеале хешировать
                    new SqlParameter("@f", fullname)
                });
                return true;
            }
            catch { return false; }
        }

        public User Login(string username, string password)
        {
            string sql = "SELECT UserID, Username, FullName FROM Users WHERE Username = @u AND Password = @p";
            DataTable dt = DbHelper.ExecuteQuery(sql, new SqlParameter[] {
                new SqlParameter("@u", username),
                new SqlParameter("@p", password)
            });

            if (dt.Rows.Count > 0)
            {
                return new User
                {
                    UserID = (int)dt.Rows[0]["UserID"],
                    Username = dt.Rows[0]["Username"].ToString(),
                    FullName = dt.Rows[0]["FullName"].ToString()
                };
            }
            return null;
        }
    }
}