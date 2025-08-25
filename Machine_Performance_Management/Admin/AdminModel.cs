using CommunityToolkit.Mvvm.ComponentModel;
using Machine_Performance_Management.Extension;
using System;
using Machine_Performance_Management.Common;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using System.Text;
using System.Windows;

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





        #region Machine Type

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

            using (var db = new DbService(config))
            using (var conn = db.GetConnection())
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    string sql = @"
                INSERT INTO device_type (
                    device_name, device_type, description, factory_id, event_user, event_time
                )
                VALUES (
                    @device_name, 
                    @device_type, 
                    @description, 
                    (SELECT factory_id FROM factory WHERE factory_name = @factory_name LIMIT 1),
                    @event_user, 
                    CURRENT_TIMESTAMP
                );
                SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(sql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@device_name", devicename);
                        cmd.Parameters.AddWithValue("@device_type", devicetype);
                        cmd.Parameters.AddWithValue("@description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                        cmd.Parameters.AddWithValue("@factory_name", factoryName);
                        cmd.Parameters.AddWithValue("@event_user", username);

                        var result = cmd.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out newProductId) && newProductId > 0)
                        {
                            transaction.Commit();
                            return true;
                        }
                        else
                        {
                            throw new Exception("Không thể insert device_type hoặc factory_name không tồn tại.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Lỗi khi insert: {ex.Message}");
                    return false;
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
        public bool UpdateDeviceType(int id, string devicename, string devicetype, string description, string factoryName, string username)
        {
            using (var db = new DbService(config))
            using (var conn = db.GetConnection())
            {
                try
                {
                    string sql = @"
                UPDATE device_type 
                SET device_name = @device_name,
                    device_type = @device_type,
                    description = @description,
                    factory_id = (SELECT factory_id FROM factory WHERE factory_name = @factory_name LIMIT 1),
                    event_user = @event_user,
                    event_time = CURRENT_TIMESTAMP
                WHERE id = @id";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@device_name", devicename);
                        cmd.Parameters.AddWithValue("@device_type", devicetype);
                        cmd.Parameters.AddWithValue("@description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                        cmd.Parameters.AddWithValue("@factory_name", factoryName);
                        cmd.Parameters.AddWithValue("@event_user", username);
                        cmd.Parameters.AddWithValue("@id", id);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating device_type: {ex.Message}");
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
