using MySqlConnector;
using System;
using System.Data;

namespace Machine_Performance_Management.Extension
{
    public class UserInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Fullname { get; set; }
    }

    public class DbService : IDisposable
    {
        private readonly MySqlConnection mysqlConnection;
        private MySqlTransaction transaction;
        private bool disposed = false;

        public DbService(LocalConfigTable localConfig)
        {
            string connStr = $"Server={localConfig.IP};" +
                             $"User ID={localConfig.UID};" +
                             $"Password={localConfig.PWD};" +
                             $"Database={localConfig.Db};" +
                             $"Port={localConfig.Port};" +
                             "SslMode=None;"; // Không dùng SSL (tuỳ chỉnh theo server)

            try
            {
                mysqlConnection = new MySqlConnection(connStr);
                mysqlConnection.Open();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi kết nối Database: {ex.Message}");
            }
        }

        public MySqlCommand GetMySqlCommand(string query)
        {
            if (mysqlConnection == null || mysqlConnection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Kết nối cơ sở dữ liệu chưa được mở.");
            }

            var command = mysqlConnection.CreateCommand();
            command.CommandText = query;
            if (transaction != null)
            {
                command.Transaction = transaction;
            }
            return command;
        }

        public void BeginTransaction()
        {
            if (mysqlConnection == null || mysqlConnection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Kết nối cơ sở dữ liệu chưa được mở.");
            }
            transaction = mysqlConnection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            transaction?.Commit();
            transaction = null;
        }

        public void RollbackTransaction()
        {
            transaction?.Rollback();
            transaction = null;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                transaction?.Dispose();
                if (mysqlConnection?.State == ConnectionState.Open)
                {
                    mysqlConnection.Close();
                    mysqlConnection.Dispose();
                }
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        public MySqlConnection GetConnection()
        {
            return mysqlConnection;
        }
    }
}
