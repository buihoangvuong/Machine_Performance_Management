using Machine_Performance_Management.Common;
using Machine_Performance_Management.Extension;

using MySqlConnector;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Machine_Performance_Management.Performance
{
	public class PerformanceModel
	{
        protected LocalConfigTable config;
        
        public PerformanceModel()
        {
            IniService ini = new IniService();
            config = ini.GetLocalConfig();
        }
        public List<DevicePerformance> ReadExcelData(string filePath)
        {
            var result = new List<DevicePerformance>();

            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheets = package.Workbook.Worksheets;
                    foreach (var worksheet in worksheets)
                    {
                        if (worksheet.Dimension == null)
                            continue;

                        int rowCount = worksheet.Dimension.Rows;
                        int colCount = worksheet.Dimension.Columns;

                        int firstDayCol = 7; // Cột bắt đầu của ngày

                        for (int row = 5; row <= rowCount; row++)
                        {
                            if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text))
                                continue;

                            var perf = new DevicePerformance
                            {
                                NO = row - 4,
                                Factory = worksheet.Cells[row, 1].Text?.Trim().Substring(0, 2),
                                Machine_Name = worksheet.Cells[row, 1].Text?.Trim()
                            };

                            for (int col = firstDayCol; col <= colCount; col += 6)
                            {
                                string dateHeader = worksheet.Cells[3, col].Text?.Trim();
                                if (string.IsNullOrEmpty(dateHeader))
                                    continue;

                                if (double.TryParse(worksheet.Cells[row, col + 2].Text.Replace(",", "").Trim(),
                                                    NumberStyles.Any,
                                                    CultureInfo.InvariantCulture,
                                                    out var targetValue))
                                {
                                    perf.Performance_Target[dateHeader] = targetValue;
                                }

                                if (double.TryParse(worksheet.Cells[row, col + 3].Text.Replace(",", "").Trim(),
                                                    NumberStyles.Any,
                                                    CultureInfo.InvariantCulture,
                                                    out var completedValue))
                                {
                                    perf.Performance_Completed[dateHeader] = completedValue;
                                }

                                if (double.TryParse(worksheet.Cells[row, col + 4].Text.Replace("%", "").Trim(),
                                                    NumberStyles.Any,
                                                    CultureInfo.InvariantCulture,
                                                    out var perfValue))
                                {
                                    perf.DailyPerformance[dateHeader] = perfValue;
                                }

                                perf.Reason[dateHeader] = worksheet.Cells[row, col + 5].Text;
                            }

                            result.Add(perf); 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đọc file Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return result;
        }

        public void InsertToDatabase(List<DevicePerformance1> dataList, string fullname)
        {
            using (DbService db = new DbService(config))
            {
                foreach (var item in dataList)
                {
                    var queryInsert = @"
                INSERT INTO machine_performance 
                (date, factory, device_name, qty_taget, qty_completed, daily_performance, reason, event_user) 
                VALUES 
                (@date, @factory, @device_name, @qty_taget, @qty_completed, @daily_performance, @reason, @even_user )";

                    var parametersInsert = new List<MySqlParameter>
            {
                new MySqlParameter("@date", item.Date ?? (object)DBNull.Value),
                new MySqlParameter("@factory", item.Factory ?? (object)DBNull.Value),
                new MySqlParameter("@device_name", item.Machine_Name ?? (object)DBNull.Value),
                new MySqlParameter("@qty_taget", item.Performance_Target),
                new MySqlParameter("@qty_completed", item.Performance_Completed),
                new MySqlParameter("@daily_performance", item.DailyPerformance),
                new MySqlParameter("@reason", item.Reason ?? (object)DBNull.Value),
                new MySqlParameter("@even_user", fullname ?? (object)DBNull.Value),
            };

                    db.ExecuteQuery(queryInsert, parametersInsert);
                }
            }
        }

        public List<string> GetFactoryList()
        {
            var factoryList = new List<string>();

            try
            {
                using (DbService db = new DbService(config))
                {
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT DISTINCT factory FROM machine_performance");

                    var cmd = db.GetMySqlCommand(query.ToString());

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            factoryList.Add(reader["factory"]?.ToString());
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

        public List<string> LoadDeviceTypeList(string factory)
        {
            var deviceTypes = new List<string>();

            try
            {
                using (DbService db = new DbService(config))
                {
                    StringBuilder query = new StringBuilder();

                    if (factory == "All")
                        query.Append("SELECT DISTINCT device_type FROM history");
                    else
                    {
                        //query.Append("SELECT DISTINCT device_type FROM history WHERE factory = @factory");
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

        public List<DevicePerformance> LoadPerformanceMachineList(string factory)
        {
            var result = new List<DevicePerformance>();
            var deviceDict = new Dictionary<string, DevicePerformance>();
            var dateList = new List<string>();

            StringBuilder query = new StringBuilder("SELECT * FROM machine_performance");
            List<string> conditions = new List<string>();
            if (!string.IsNullOrWhiteSpace(factory) && factory != "All")
                conditions.Add("factory = @factory");

            if (conditions.Any())
                query.Append(" WHERE " + string.Join(" AND ", conditions));
            query.Append(" ORDER BY device_name, date");

            try
            {
                using (DbService db = new DbService(config))
                {
                    MySqlCommand cmd = db.GetMySqlCommand(query.ToString());
                    if (!string.IsNullOrWhiteSpace(factory) && factory != "All")
                        cmd.Parameters.AddWithValue("@factory", factory);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string deviceName = reader["device_name"]?.ToString();
                            string factoryName = reader["factory"]?.ToString();
                            string date = reader["date"]?.ToString();

                            double dailyPerformance = reader["daily_performance"] != DBNull.Value ? Convert.ToDouble(reader["daily_performance"]) : 0;
                            double performanceTarget = reader["qty_taget"] != DBNull.Value ? Convert.ToDouble(reader["qty_taget"]) : 0;
                            double performanceCompleted = reader["qty_completed"] != DBNull.Value ? Convert.ToDouble(reader["qty_completed"]) : 0;
                            string reason = reader["reason"]?.ToString();

                            if (!dateList.Contains(date))
                                dateList.Add(date);

                            string key = $"{factoryName}_{deviceName}";
                            if (!deviceDict.ContainsKey(key))
                            {
                                deviceDict[key] = new DevicePerformance
                                {
                                    Factory = factoryName,
                                    Machine_Name = deviceName,
                                    DailyPerformance = new Dictionary<string, double>(),
                                    Performance_Target = new Dictionary<string, double>(),
                                    Performance_Completed = new Dictionary<string, double>(),
                                    Reason = new Dictionary<string, string>()
                                };
                            }
                            deviceDict[key].DailyPerformance[date] = dailyPerformance;
                            deviceDict[key].Performance_Target[date] = performanceTarget;
                            deviceDict[key].Performance_Completed[date] = performanceCompleted;
                            deviceDict[key].Reason[date] = reason;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Load list machine performance failed: {ex.Message}");
            }

            int no = 1;
            foreach (var item in deviceDict.Values)
            {
                item.NO = no++;
                // Đảm bảo đủ các ngày, nếu thiếu thì để 0 hoặc null
                foreach (var date in dateList)
                {
                    if (!item.DailyPerformance.ContainsKey(date))
                        item.DailyPerformance[date] = 0;
                    if (!item.Performance_Target.ContainsKey(date))
                        item.Performance_Target[date] = 0;
                    if (!item.Performance_Completed.ContainsKey(date))
                        item.Performance_Completed[date] = 0;
                    if (!item.Reason.ContainsKey(date))
                        item.Reason[date] = string.Empty;
                }
                result.Add(item);
            }

            // Sắp xếp lại theo tên máy
            result = result.OrderBy(x => x.Machine_Name).ToList();

            return result;
        }


    }
}
