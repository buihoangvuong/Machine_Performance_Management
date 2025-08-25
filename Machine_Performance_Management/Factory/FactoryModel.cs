using Machine_Performance_Management.Common;
using Machine_Performance_Management.Extension;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Machine_Performance_Management.Factory
{
    public class FactoryModel
    {
        protected LocalConfigTable config;

        public FactoryModel()
        {
            IniService ini = new IniService();
            config = ini.GetLocalConfig();
        }

      

        #region Factory
        public List<FactoryInfo> LoadFactoryList()
        {
            var list = new List<FactoryInfo>();
            string query = "SELECT * FROM factory";

            try
            {
                using (DbService db = new DbService(config))
                using (var cmd = db.GetMySqlCommand(query))
                using (var reader = cmd.ExecuteReader())
                {
                    int index = 1;
                    while (reader.Read())
                    {
                        list.Add(new FactoryInfo
                        {
                            No = index++,
                            Factory_id = reader.GetInt32("factory_id"),
                            FactoryName = reader.GetString("factory_name"),
                            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                            Fullname = reader.GetString("event_user"),
                            EventTime = reader.GetDateTime("event_time").ToString("yyy/MM/dd HH:mm:ss") // hoặc format khác nếu cần
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu nhà máy: " + ex.Message);
            }

            return list;
        }

        public bool AddFactory(string factoryName, string description, string userName, out int newFactoryId)
        {
            newFactoryId = -1;

            using (DbService db = new DbService(config))
            {
                MySqlConnection conn = db.GetConnection();
                using (MySqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string insertQuery = @"
                          INSERT INTO factory (factory_name, description, event_user)
                          VALUES (@factory_name, @description, @event_user);
                          SELECT LAST_INSERT_ID();";

                        using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@factory_name", string.IsNullOrEmpty(factoryName) ? (object)DBNull.Value : factoryName);
                            cmd.Parameters.AddWithValue("@description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                            cmd.Parameters.AddWithValue("@event_user", string.IsNullOrEmpty(userName) ? (object)DBNull.Value : userName);

                            object result = cmd.ExecuteScalar();
                            if (result != null && int.TryParse(result.ToString(), out int id))
                                newFactoryId = id;
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Error adding server\nLỗi khi thêm nhà máy: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
        }

        public bool UpdateFactory(int factoryId, string factoryName, string description, string userName)
        {
            string updateQuery = @"
              UPDATE factory
              SET factory_name = @factory_name,
                  description = @description,
                  event_user = @event_user,
                  event_time = NOW()
              WHERE factory_id = @factory_id;";

            try
            {
                using (DbService db = new DbService(config))
                using (var cmd = db.GetMySqlCommand(updateQuery))
                {
                    cmd.Parameters.AddWithValue("@factory_name", factoryName);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.Parameters.AddWithValue("@event_user", userName);
                    cmd.Parameters.AddWithValue("@factory_id", factoryId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating factory\nLỗi khi cập nhật nhà máy: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool DeleteFactory(int factoryId)
        {
            string deleteQuery = "DELETE FROM factory WHERE factory_id = @factory_id";

            try
            {
                using (DbService db = new DbService(config))
                using (var cmd = db.GetMySqlCommand(deleteQuery))
                {
                    cmd.Parameters.AddWithValue("@factory_id", factoryId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while deleting factory\nLỗi khi xóa nhà máy: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool IsFactoryNameExists(string factoryName, int? excludeId = null)
        {
            string query = "SELECT COUNT(*) FROM factory WHERE factory_name = @factory_name";

            // Nếu muốn kiểm tra trùng khi cập nhật, loại trừ bản ghi hiện tại
            if (excludeId.HasValue)
            {
                query += " AND factory_id <> @exclude_id";
            }

            try
            {
                using (DbService db = new DbService(config))
                using (var cmd = db.GetMySqlCommand(query))
                {
                    cmd.Parameters.AddWithValue("@factory_name", factoryName);
                    if (excludeId.HasValue)
                        cmd.Parameters.AddWithValue("@exclude_id", excludeId.Value);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking factory name\nLỗi khi kiểm tra tên nhà máy: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false; // Xem như không tồn tại để tránh chặn sai
            }
        }

       
        public List<string> GetFactoryList()
        {
            var factoryList = new List<string>();

            try
            {
                using (DbService db = new DbService(config))
                {
                    // Lấy các factory_name có liên kết với device_type
                    string query = @"
                SELECT DISTINCT f.factory_name
                FROM factory f
                LEFT JOIN device_type d ON f.factory_id = d.factory_id
            ";

                    var cmd = db.GetMySqlCommand(query);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            factoryList.Add(reader["factory_name"]?.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] : {ex.Message}");
            }

            return factoryList;
        }

        #endregion
    }


}
