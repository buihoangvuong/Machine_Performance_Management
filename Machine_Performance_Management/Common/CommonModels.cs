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
}
