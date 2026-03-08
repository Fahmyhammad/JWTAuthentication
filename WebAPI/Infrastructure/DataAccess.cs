using Dapper;
using System.Data.SqlClient;
using WebAPI.Models;

namespace WebAPI.Infrastructure
{
    public class DataAccess : IDisposable
    {
        private SqlConnection connection;

        public DataAccess(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("TestApiDB");
            connection = new SqlConnection(connectionString);
            connection.Open();
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }

        public bool RegisterUser(string email, string password, string role)
        {
            var accountCount = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM [UserAccount] WHERE [Email] = @Email", new { Email = email });

            if (accountCount > 0)
            {
                return false; // Email already exists
            }

            var sql = "INSERT INTO [UserAccount] (Email, Password, Role) VALUES (@Email, @Password, @Role)";
            var result = connection.Execute(sql, new { Email = email, Password = password, Role = role });

            return result > 0; // Return true if the insert was successful
        }

        public UserAccount? FindUserByEmail(string email)
        {
            var sql = "SELECT * FROM [UserAccount] WHERE [Email] = @Email";
            return connection.QueryFirstOrDefault<UserAccount>(sql, new { Email = email });
        }

        public bool InsertRefreshToken(RefreshToken refreshToken, string email)
        {
            var sql = "INSERT INTO [RefreshToken] (Token , CreatedDate , Expires , Enabled , Email) VALUES (@token , @createddate , @expires , @enabled , @email)";
            var result = connection.Execute(sql, new
            {
                token = refreshToken.Token,
                createddate = refreshToken.CreatedDate,
                expires = refreshToken.Expires,
                enabled = refreshToken.Enabled,
                email
            });
            return result > 0; // Return true if the insert was successful
        }

        public bool DisableUserTokenByEmail(string email)
        {
            var sql = "UPDATE [RefreshToken] SET Enabled = 0 WHERE Email = @Email";
            var result = connection.Execute(sql, new { Email = email });
            return result > 0; // Return true if the update was successful
        }
        //public RefreshToken? GetRefreshTokenByEmail(string email)
        //{
        //    var sql = "SELECT * FROM [RefreshToken] WHERE [Email] = @Email AND Enabled = 1";
        //    return connection.QueryFirstOrDefault<RefreshToken>(sql, new { Email = email });
        //}
        public bool DisableUserToken(string token)
        {
            var sql = "UPDATE [RefreshToken] SET Enabled = 0 WHERE Token = @Token";
            var result = connection.Execute(sql, new { Token = token });
            return result > 0; // Return true if the update was successful
        }

        public bool IsRefreshTokenValid(string token)
        {
            var sql = "SELECT COUNT(1) FROM [RefreshToken] WHERE [Token] = @token AND [Enabled] = 1 AND [Expires] >= CAST(GETDATE() AS DATE)";
            var count = connection.ExecuteScalar<int>(sql, new { Token = token });
            return count > 0; // Return true if the token is valid
        }

        public UserAccount? FindUserByToken(string token)
        {
            var sql = @"SELECT [UserAccount].*
                        FROM [RefreshToken] JOIN [UserAccount] ON [RefreshToken].Email = [UserAccount].Email
                        WHERE [Token = @token]";
            return connection.QueryFirstOrDefault<UserAccount>(sql, new { Token = token });

        }
    }
}
