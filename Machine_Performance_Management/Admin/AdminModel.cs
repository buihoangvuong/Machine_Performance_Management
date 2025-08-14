using CommunityToolkit.Mvvm.ComponentModel;
using Machine_Performance_Management.Extension;
using System;
using Machine_Performance_Management.Common;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Machine_Performance_Management.Admin
{
    public class AdminModel
    {
        protected LocalConfigTable config;

        public AdminModel()
        {
            IniService ini = new IniService();
            config = ini.GetLocalConfig();
        }
        #region User Profile
        public List<UserProfile> LoadUserProfiles()
        {
            StringBuilder query = new StringBuilder();
            query.Append("SELECT user_id, user_name, password, role, full_name FROM user_profile;");
            List<UserProfile> userList = new List<UserProfile>();

            try
            {
                using (DbService db = new DbService(config))
                {
                    MySqlCommand cmd = db.GetMySqlCommand(query.ToString());
                    using (var dataReader = cmd.ExecuteReader())
                    {
                        if (dataReader.HasRows)
                        {
                            int index = 1;
                            while (dataReader.Read())
                            {
                                UserProfile userProfile = new UserProfile
                                {
                                    No = index++,
                                    UserId = Convert.ToInt32(dataReader["user_id"]),
                                    UserName = dataReader.GetString("user_name"),
                                    PassWord = dataReader.GetString("password"),
                                    Role = dataReader.GetString("role"),
                                    FullName = dataReader.GetString("full_name"),
                                };

                                userList.Add(userProfile);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No data found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return userList;
        }

        public void AddUser(string userName, string password, string role, string fullName)
        {
            using (DbService db = new DbService(config))
            {
                using (var transaction = db.GetConnection().BeginTransaction())
                {
                    try
                    {
                        string insertQuery = @"INSERT INTO user_profile (user_name, password, role, full_name) VALUES (@userName, @password, @role, @fullName);";

                        using (var cmd = db.GetMySqlCommand(insertQuery))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@userName", userName);
                            cmd.Parameters.AddWithValue("@password", password);
                            cmd.Parameters.AddWithValue("@role", role);
                            cmd.Parameters.AddWithValue("@fullName", fullName);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show("User added successfully!\nNgười dùng đã được thêm thành công!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Error adding user: {ex.Message}");
                    }
                }
            }
        }
        public void UpdateUser(int userId, string userName, string password, string role, string fullName)
        {
            using (DbService db = new DbService(config))
            {
                using (var transaction = db.GetConnection().BeginTransaction())
                {
                    try
                    {
                        string updateQuery = @"UPDATE user_profile SET user_name = ?,
                                                                       password = ?,
                                                                       role = ?, 
                                                                       full_name = ?
                                                                      WHERE user_id = ?;";

                        using (var cmd = db.GetMySqlCommand(updateQuery))
                        {
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@userName", userName);
                            cmd.Parameters.AddWithValue("@password", password);
                            cmd.Parameters.AddWithValue("@role", role);
                            cmd.Parameters.AddWithValue("@fullName", fullName);
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Error updating user: {ex.Message}");
                    }
                }
            }
        }

        public void DeleteUser(string username)
        {
            using (DbService db = new DbService(config))
            {
                try
                {
                    string deleteQuery = "DELETE FROM user_profile WHERE user_name = ?";
                    using (var cmd = db.GetMySqlCommand(deleteQuery))


                    {
                        cmd.Parameters.AddWithValue("@user_name", username);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error deleting user: {ex.Message}");
                }
            }
        }

        public bool CheckUserExists(string userName)
        {
            using (DbService db = new DbService(config))
            {
                string query = "SELECT COUNT(*) FROM user_profile WHERE user_name = @userName;";
                using (var cmd = db.GetMySqlCommand(query))
                {
                    cmd.Parameters.AddWithValue("@userName", userName);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0; // Trả về true nếu tồn tại
                }
            }
        }
        #endregion
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
        #region Machine Type
        public List<Device_Type> LoadDatadeviceType()
        {
            List<Device_Type> deviceTypeItems = new List<Device_Type>();

            try
            {
                StringBuilder query = new StringBuilder();
                query.Append(@"
                    SELECT d.*, f.factory_name
                    FROM device_type d
                    INNER JOIN factory f ON d.factory_id = f.factory_id
                    WHERE 1=1
                ");

                using (DbService db = new DbService(config))
                using (MySqlCommand cmd = db.GetMySqlCommand(query.ToString()))
                {
                    using (MySqlDataReader dataReader = cmd.ExecuteReader())
                    {
                        int index = 1;
                        while (dataReader.Read())
                        {
                            Device_Type item = new Device_Type
                            {
                                NO = index,
                                id = Convert.ToInt32(dataReader["id"]),
                                Device_Name = dataReader["device_name"]?.ToString() ?? string.Empty,
                                Device_type = dataReader["device_type"]?.ToString() ?? string.Empty,
                                Description = dataReader["description"]?.ToString() ?? string.Empty,
                                // Lấy factory_name thay vì factory
                                Factory = dataReader["factory_name"]?.ToString() ?? string.Empty,
                                Event_User = dataReader["event_user"]?.ToString() ?? string.Empty,
                                Event_Time = dataReader.GetDateTime(dataReader.GetOrdinal("event_time")).ToString("yyyy/MM/dd HH:mm:ss")
                            };

                            index++;
                            deviceTypeItems.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] LoadDatadeviceType failed: {ex.Message}");
            }

            return deviceTypeItems;
        }

        public List<string> GetDeviceType(string factory)
        {
            var deviceTypes = new List<string>();

            try
            {
                using (DbService db = new DbService(config))
                {
                    StringBuilder query = new StringBuilder();

                    if (factory == "All")
                        query.Append("SELECT DISTINCT device_type FROM device_type");
                    else
                    {
                        query.Append("SELECT DISTINCT device_type FROM device_type WHERE factory = @factory");
                    }

                    var cmd = db.GetMySqlCommand(query.ToString());

                    if (factory != "All")
                        cmd.Parameters.AddWithValue("@factory", factory);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            deviceTypes.Add(reader["device_type"]?.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] LoadDeviceTypeList: {ex.Message}");
            }

            return deviceTypes;
        }


        public bool CheckDeviceNameExists(string devicename, string factory)
        {
            using (DbService db = new DbService(config))
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM device_type d
                    INNER JOIN factory f ON d.factory_id = f.factory_id
                    WHERE d.device_name = @devicename AND f.factory_name = @factory_name;
                ";
                // factoryId là kiểu int
                //string query = "SELECT COUNT(*) FROM device_type WHERE device_name = @devicename AND factory = @factory;";
                using (var cmd = db.GetMySqlCommand(query))
                {
                    //cmd.Parameters.AddWithValue("@devicename", devicename);
                    //cmd.Parameters.AddWithValue("@factory", factory);
                    cmd.Parameters.AddWithValue("@devicename", devicename);
                    cmd.Parameters.AddWithValue("@factory_name", factory);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0; // Trả về true nếu tồn tại
                }
            }
        }
        //public bool InsertDevicetype(string devicename, string devicetype, string description, string factory, string username, out int newProductId)
        //{
        //    newProductId = -1; // Mặc định nếu thất bại

        //    using (DbService db = new DbService(config))
        //    {
        //        MySqlConnection conn = db.GetConnection();
        //        using (MySqlTransaction transaction = conn.BeginTransaction())
        //        {
        //            try
        //            {
        //                string insertLogDB = @"
        //            INSERT INTO device_type 
        //            (device_name, device_type, description, factory, event_user, event_time)
        //            VALUES 
        //            (@device_name, @device_type, @description, @factory, @event_user, CURRENT_TIMESTAMP);
        //            SELECT LAST_INSERT_ID();";
        //                //SELECT LAST_INSERT_ID();"; // Lấy ID vừa insert

        //                using (MySqlCommand cmd = new MySqlCommand(insertLogDB, conn, transaction))
        //                {
        //                    cmd.Parameters.AddWithValue("@device_name", devicename);
        //                    cmd.Parameters.AddWithValue("@device_type", devicetype);
        //                    cmd.Parameters.AddWithValue("@description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
        //                    cmd.Parameters.AddWithValue("@factory", factory);
        //                    cmd.Parameters.AddWithValue("@event_user", username);

        //                    object result = cmd.ExecuteScalar();
        //                    if (result != null)
        //                    {
        //                        newProductId = Convert.ToInt32(result);
        //                    }
        //                }

        //                transaction.Commit();
        //                MessageBox.Show("Device Type successfully!\nĐã được thêm thành công!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
        //                return true;
        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                Console.WriteLine($"Lỗi khi insert: {ex.Message}");
        //                Console.WriteLine($"StackTrace: {ex.StackTrace}");
        //                newProductId = -1;
        //                return false;
        //            }
        //        }
        //    }
        //}
        public bool InsertDevicetype(string devicename, string devicetype, string description, string factoryName, string username, out int newProductId)
        {
            newProductId = -1;

            using (DbService db = new DbService(config))
            {
                // 1. Lấy factory_id từ factory_name
                int factoryId = -1;
                string getIdQuery = "SELECT factory_id FROM factory WHERE factory_name = @factory_name LIMIT 1";
                using (var getIdCmd = db.GetMySqlCommand(getIdQuery))
                {
                    getIdCmd.Parameters.AddWithValue("@factory_name", factoryName);
                    var result = getIdCmd.ExecuteScalar();
                    if (result == null || !int.TryParse(result.ToString(), out factoryId))
                    {
                        MessageBox.Show("Không tìm thấy nhà máy phù hợp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

                MySqlConnection conn = db.GetConnection();
                using (MySqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 2. Insert vào device_type với factory_id
                        string insertLogDB = @"
                            INSERT INTO device_type 
                            (device_name, device_type, description, factory_id, event_user, event_time)
                            VALUES 
                            (@device_name, @device_type, @description, @factory_id, @event_user, CURRENT_TIMESTAMP);
                            SELECT LAST_INSERT_ID();";

                        using (MySqlCommand cmd = new MySqlCommand(insertLogDB, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@device_name", devicename);
                            cmd.Parameters.AddWithValue("@device_type", devicetype);
                            cmd.Parameters.AddWithValue("@description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                            cmd.Parameters.AddWithValue("@factory_id", factoryId);
                            cmd.Parameters.AddWithValue("@event_user", username);

                            object result = cmd.ExecuteScalar();
                            if (result != null)
                            {
                                newProductId = Convert.ToInt32(result);
                            }
                        }

                        transaction.Commit();                    
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Lỗi khi insert: {ex.Message}");
                        newProductId = -1;
                        return false;
                    }
                }
            }
        }
        //public bool UpdateDeviceType(int Id, string devicename, string devicetype, string description, string factory, string UserName)
        //{
        //    string query = @"
        //    UPDATE device_type 
        //    SET id = @Id, 
        //    device_name = @devicename, 
        //    device_type = @devicetype, 
        //    description = @description, 
        //    factory = @factory,
        //    event_user = @UserName,
        //    event_time = CURRENT_TIMESTAMP
        //    WHERE id = @Id";

        //    try
        //    {
        //        using (DbService db = new DbService(config))
        //        {
        //            using (MySqlCommand cmd = db.GetMySqlCommand(query))
        //            {
        //                cmd.Parameters.AddWithValue("@devicename", devicename);
        //                cmd.Parameters.AddWithValue("@devicetype", devicetype);
        //                cmd.Parameters.AddWithValue("@description", description);
        //                cmd.Parameters.AddWithValue("@factory", factory);
        //                cmd.Parameters.AddWithValue("@UserName", UserName);
        //                cmd.Parameters.AddWithValue("@Id", Id);

        //                return cmd.ExecuteNonQuery() > 0;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error updating operation_spec: " + ex.Message);
        //        return false;
        //    }
        //}
        public bool UpdateDeviceType(int Id, string devicename, string devicetype, string description, string factoryName, string UserName)
        {
            using (DbService db = new DbService(config))
            {
                // 1. Lấy factory_id từ factory_name
                int factoryId = -1;
                string getIdQuery = "SELECT factory_id FROM factory WHERE factory_name = @factory_name LIMIT 1";
                using (var getIdCmd = db.GetMySqlCommand(getIdQuery))
                {
                    getIdCmd.Parameters.AddWithValue("@factory_name", factoryName);
                    var result = getIdCmd.ExecuteScalar();
                    if (result == null || !int.TryParse(result.ToString(), out factoryId))
                    {
                        MessageBox.Show("Không tìm thấy nhà máy phù hợp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

                string query = @"
                    UPDATE device_type 
                    SET device_name = @devicename, 
                        device_type = @devicetype, 
                        description = @description, 
                        factory_id = @factory_id,
                        event_user = @UserName,
                        event_time = CURRENT_TIMESTAMP
                    WHERE id = @Id";

                try
                {
                    using (MySqlCommand cmd = db.GetMySqlCommand(query))
                    {
                        cmd.Parameters.AddWithValue("@devicename", devicename);
                        cmd.Parameters.AddWithValue("@devicetype", devicetype);
                        cmd.Parameters.AddWithValue("@description", description);
                        cmd.Parameters.AddWithValue("@factory_id", factoryId);
                        cmd.Parameters.AddWithValue("@UserName", UserName);
                        cmd.Parameters.AddWithValue("@Id", Id);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error updating device_type: " + ex.Message);
                    return false;
                }
            }
        }
        public void DeleteDeviceType(int Id)
        {
            string deleteQuery = "DELETE FROM device_type WHERE id = ?";
            try
            {
                using (DbService db = new DbService(config))
                using (MySqlConnection conn = db.GetConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@operation_id", Id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Delete spec data success!\nXóa dữ liệu thành công!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("No matching record found to delete.\nKhông tìm thấy bản ghi nào phù hợp để xóa.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Data deletion failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

    }
    #region User Edit
    public class userPasswordModel : ObservableObject
    {
        public string UserEdit { get; set; }
        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
        public userPasswordModel(string name, string value)
        {
            UserEdit = name;
            Value = value;
        }
    }
    #endregion

    #region Factory Edit
    public class FactoryInforEdit : ObservableObject
    {
        public string FactoryEdit { get; set; }
        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

    }
    public enum FactoryName
    {
        efactory_name,
        edescription,
    }
    #endregion

    #region MachineTypeEdit
    public class DeviceTypeEdit : ObservableObject
    {
        public string DeviceTypeName { get; set; }
        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }

    public enum DeviceTypeName
    {
        edevice_type,
        edevice_name,
        edescription,
        efactory,
        eevent_user,
        eevent_time
    }

    #endregion

}
