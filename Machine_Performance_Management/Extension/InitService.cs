using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Machine_Performance_Management.Extension
{
    public struct LocalConfigTable
    {
        // [DB]
        public string IP;
        public string Port;
        public string UID;
        public string PWD;
        public string Db;

    }

    public class IniService
    {
        private const int MAX_BUFFER_SIZE = 255;

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        // Lấy đường dẫn thư mục chứa file exe
        readonly string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CONFIG/LocalConfig.ini");

        public IniService()
        {
            try
            {
                // Kiểm tra xem file config có tồn tại không
                if (!File.Exists(iniPath))
                {
                    MessageBox.Show("LocalConfig.ini not found\n" + iniPath, "Notification", MessageBoxButton.OK, MessageBoxImage.Error);
                    string msg = "LocalConfig.ini not found\n" + iniPath;
                    throw new CoreException(msg); // Ném ngoại lệ nếu không tìm thấy file

                }

                // Nếu file config tồn tại, lấy giá trị từ nó
                GetLocalConfig();
            }

            catch (CoreException ex)
            {
                // Xử lý lỗi cụ thể
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(0);
            }

            catch (Exception ex)
            {
                // Xử lý lỗi chung
                Console.WriteLine($"Error not defined: {ex.Message}");
                Environment.Exit(0);
            }
        }

        private void CreateDirectoryWithParents(string path)
        {
            DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
            string parentPath = directoryInfo.Parent.FullName;

            if (!Directory.Exists(parentPath))
            {
                CreateDirectoryWithParents(parentPath);
            }
        }

        public LocalConfigTable GetLocalConfig()
        {
            StringBuilder sb = new StringBuilder(MAX_BUFFER_SIZE);
            LocalConfigTable localConfig = new LocalConfigTable();

            try
            {
                // [DB]
                _ = GetPrivateProfileString("DB", "IP", "", sb, sb.Capacity, iniPath);
                localConfig.IP = sb.ToString();

                _ = GetPrivateProfileString("DB", "Port", "", sb, sb.Capacity, iniPath);
                localConfig.Port = sb.ToString();

                _ = GetPrivateProfileString("DB", "UID", "", sb, sb.Capacity, iniPath);
                localConfig.UID = sb.ToString();

                _ = GetPrivateProfileString("DB", "PWD", "", sb, sb.Capacity, iniPath);
                localConfig.PWD = sb.ToString();

                _ = GetPrivateProfileString("DB", "Db", "", sb, sb.Capacity, iniPath);
                localConfig.Db = sb.ToString();
                
            }
            catch (Exception ex)
            {
                throw new CoreException(ex.Message);
            }

            return localConfig;
        }
        public string ReadIniValue(string section, string key, string defaultValue = "0")
        {
            StringBuilder result = new StringBuilder(255);
            GetPrivateProfileString(section, key, defaultValue, result, 255, iniPath);
            return result.ToString();
        }

        //public void WriteIniValue(string section, string key, string value)
        //{
        //    var iniFile = new IniFile(iniPath);
        //    iniFile.Write(section, key, value);
        //}
    }
}
