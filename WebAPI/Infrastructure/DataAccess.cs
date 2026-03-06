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

        public UserAccount FindUserByEmail(string email)
        {
            var sql = "SELECT * FROM [UserAccount] WHERE [Email] = @Email";
            return connection.QueryFirstOrDefault<UserAccount>(sql, new { Email = email });
        }
    }
}
