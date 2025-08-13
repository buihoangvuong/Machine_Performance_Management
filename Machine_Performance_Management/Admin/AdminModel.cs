using CommunityToolkit.Mvvm.ComponentModel;
using Machine_Performance_Management.Extension;
using MySqlConnector;
using System;
using Machine_Performance_Management.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


    }
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

}
