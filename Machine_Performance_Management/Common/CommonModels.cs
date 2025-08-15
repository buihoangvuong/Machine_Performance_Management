using CommunityToolkit.Mvvm.ComponentModel;
using Machine_Performance_Management.Extension;
using MySqlConnector;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Machine_Performance_Management.Common
{
    public class UserProfile
    {
        public int No { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
    }

    public class DevicePerformance
    {
        public int NO { get; set; }
        public string Factory { get; set; }
        public string Machine_Name { get; set; }

        // Hiệu suất (%)
        public Dictionary<string, double> DailyPerformance { get; set; }
            = new Dictionary<string, double>();

        // Chỉ tiêu (capa/일)
        public Dictionary<string, double> Performance_Target { get; set; }
            = new Dictionary<string, double>();

        // Thực tế (생산량)
        public Dictionary<string, double> Performance_Completed { get; set; }
            = new Dictionary<string, double>();

        public Dictionary<string, string> Reason { get; set; }
    = new Dictionary<string, string>();
        public double AveragePerformance
        {
            get
            {
                if (DailyPerformance == null || DailyPerformance.Count == 0)
                    return 0;
                return DailyPerformance.Values.Average();
            }
        }

        //public string Performance_Taget { get; set; }
        //public string Performance_Completed { get; set; }
    }

    public class FactoryInfo
    {
        public int No { get; set; }

        public int Factory_id { get; set; }

        public string FactoryName { get; set; }

        public string Description { get; set; }

        public string Fullname { get; set; }

        public string EventTime { get; set; }
    }

    public class Device_Type
    {
        public int NO { get; set; }
        public int id { get; set; }
        public string Device_Name { get; set; }
        public string Device_type { get; set; }
        public string Description { get; set; }
        public string Factory { get; set; }
        public string Event_User { get; set; }
        public string Event_Time { get; set; }

    }
}