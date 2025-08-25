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
        public List<DevicePerformance> ReadExcelData1(string filePath)
        {
            var result = new List<DevicePerformance>();
            int index = 1;
            var allDates = new List<HashSet<string>>(); // Danh sách các tập hợp ngày từ từng sheet

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
                        var datesInSheet = new HashSet<string>(); // Tập hợp ngày cho sheet hiện tại

                        for (int row = 5; row <= rowCount; row++)
                        {
                            if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text))
                                continue;

                            var perf = new DevicePerformance
                            {
                                NO = index,
                                Factory = worksheet.Cells[row, 1].Text?.Trim().Substring(0, 2),
                                Item = worksheet.Cells[row, 1].Text?.Trim().Substring(3, 3),
                                Machine_Name = worksheet.Cells[row, 1].Text?.Trim()
                            };

                            for (int col = firstDayCol; col <= colCount; col += 6)
                            {
                                var cell = worksheet.Cells[3, col];
                                DateTime? parsedDate = cell.GetValue<DateTime?>();

                                if (!parsedDate.HasValue)
                                    continue;

                                string dateKey = parsedDate.Value.ToString("dd.MM.yyyy");
                                datesInSheet.Add(dateKey);

                                perf.Date[dateKey] = parsedDate.Value.Day;

                                if (double.TryParse(worksheet.Cells[row, col + 1].Text.Replace(",", "").Trim(),
                                                    NumberStyles.Any, CultureInfo.InvariantCulture, out var STtValue))
                                {
                                    perf.Performance_ST[dateKey] = STtValue;
                                }

                                if (double.TryParse(worksheet.Cells[row, col + 2].Text.Replace(",", "").Trim(),
                                                    NumberStyles.Any, CultureInfo.InvariantCulture, out var targetValue))
                                {
                                    perf.Performance_Target[dateKey] = targetValue;
                                }

                                if (double.TryParse(worksheet.Cells[row, col + 3].Text.Replace(",", "").Trim(),
                                                    NumberStyles.Any, CultureInfo.InvariantCulture, out var completedValue))
                                {
                                    perf.Performance_Completed[dateKey] = completedValue;
                                }

                                if (double.TryParse(worksheet.Cells[row, col + 4].Text.Replace("%", "").Trim(),
                                                    NumberStyles.Any, CultureInfo.InvariantCulture, out var perfValue))
                                {
                                    perf.DailyPerformance[dateKey] = perfValue;
                                }

                                perf.Reason[dateKey] = worksheet.Cells[row, col + 5].Text;
                            }

                            // Kiểm tra ngày đầu tiên
                            if (datesInSheet.Count > 0)
                            {
                                // Parse và sắp xếp ngày
                                var sortedDates = datesInSheet
                                    .Select(d => DateTime.ParseExact(d, "dd.MM.yyyy", CultureInfo.InvariantCulture))
                                    .OrderBy(d => d)
                                    .ToList();

                                //string s = "01.08.2025";
                                //DateTime d1 = DateTime.Parse(s);

                                // Kiểm tra ngày đầu tiên
                                if (sortedDates.First().Day != 1)
                                {
                                    MessageBox.Show($"Ngày đầu tiên trong sheet '{worksheet.Name}' không phải là ngày 01.",
                                                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return new List<DevicePerformance>();
                                }

                                // Kiểm tra tính liên tục
                                DateTime lastDate = sortedDates.Last();
                                for (var date = sortedDates.First(); date <= lastDate; date = date.AddDays(1))
                                {
                                    if (!datesInSheet.Contains(date.ToString("dd.MM.yyyy")))
                                    {
                                        MessageBox.Show($"Ngày {date:dd.MM.yyyy} bị thiếu trong sheet '{worksheet.Name}'.",
                                                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return new List<DevicePerformance>();
                                    }
                                }
                            }

                            result.Add(perf);
                            index++;
                        }

                        // Thêm tập hợp ngày của sheet hiện tại vào danh sách
                        allDates.Add(datesInSheet);
                    }

                    // Kiểm tra các ngày giữa các sheet
                    if (allDates.Count > 1)
                    {
                        var firstSheetDates = allDates[0];
                        foreach (var sheetDates in allDates.Skip(1))
                        {
                            if (!firstSheetDates.SetEquals(sheetDates))
                            {
                                MessageBox.Show("Các ngày trong các sheet không giống nhau!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                return new List<DevicePerformance>(); // Hoặc xử lý theo cách bạn muốn
                            }
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

        public bool ReadExcelData(string filePath, out List<DevicePerformance> result)
        {
            result = new List<DevicePerformance>();   // gán giá trị cho biến out
            int index = 1;
            var allDates = new List<HashSet<string>>();

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

                        int firstDayCol = 7;
                        var datesInSheet = new HashSet<string>();

                        for (int row = 5; row <= rowCount; row++)
                        {
                            if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text))
                                continue;

                            var perf = new DevicePerformance
                            {
                                NO = index,
                                Factory = worksheet.Cells[row, 1].Text?.Trim().Substring(0, 2),
                                Item = worksheet.Cells[row, 1].Text?.Trim().Substring(3, 3),
                                Machine_Name = worksheet.Cells[row, 1].Text?.Trim()
                            };

                            for (int col = firstDayCol; col <= colCount; col += 6)
                            {
                                var cell = worksheet.Cells[3, col];
                                DateTime? parsedDate = cell.GetValue<DateTime?>();

                                if (!parsedDate.HasValue)
                                    continue;

                                string dateKey = parsedDate.Value.ToString("MM.dd");

                                // Kiểm tra ngày trùng lặp
                                if (datesInSheet.Contains(dateKey))
                                {
                                    MessageBox.Show($"Ngày trùng lặp: {dateKey} trong sheet '{worksheet.Name}'.",
                                                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return false;
                                }

                                datesInSheet.Add(dateKey);
                                perf.Date[dateKey] = parsedDate.Value.Day;

                                // Lấy giá trị hiệu suất từ sheet
                                if (double.TryParse(worksheet.Cells[row, col + 1].Text.Replace(",", "").Trim(),
                                                    NumberStyles.Any, CultureInfo.InvariantCulture, out var STtValue))
                                {
                                    perf.Performance_ST[dateKey] = STtValue;
                                }

                                if (double.TryParse(worksheet.Cells[row, col + 2].Text.Replace(",", "").Trim(),
                                                    NumberStyles.Any, CultureInfo.InvariantCulture, out var targetValue))
                                {
                                    perf.Performance_Target[dateKey] = targetValue;
                                }

                                if (double.TryParse(worksheet.Cells[row, col + 3].Text.Replace(",", "").Trim(),
                                                    NumberStyles.Any, CultureInfo.InvariantCulture, out var completedValue))
                                {
                                    perf.Performance_Completed[dateKey] = completedValue;
                                }

                                if (double.TryParse(worksheet.Cells[row, col + 4].Text.Replace("%", "").Trim(),
                                                    NumberStyles.Any, CultureInfo.InvariantCulture, out var perfValue))
                                {
                                    perf.DailyPerformance[dateKey] = perfValue;
                                }

                                perf.Reason[dateKey] = worksheet.Cells[row, col + 5].Text;
                            }

                            // Kiểm tra ngày đầu tiên
                            if (datesInSheet.Count > 0)
                            {
                                var sortedDates = datesInSheet
                                    .Select(d => DateTime.ParseExact(d, "MM.dd", CultureInfo.InvariantCulture))
                                    .OrderBy(d => d)
                                    .ToList();

                                if (sortedDates.First().Day != 1)
                                {
                                    MessageBox.Show($"Ngày đầu tiên trong sheet '{worksheet.Name}' không phải là ngày 01.",
                                                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return false;
                                }

                                DateTime lastDate = sortedDates.Last();
                                HashSet<string> dateSet = new HashSet<string>();

                                for (var date = sortedDates.First(); date <= lastDate; date = date.AddDays(1))
                                {
                                    string dateString = date.ToString("MM.dd");

                                    if (!datesInSheet.Contains(dateString))
                                    {
                                        MessageBox.Show($"Ngày {date:dd.MM.yyyy} bị thiếu trong sheet '{worksheet.Name}'.",
                                                        "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return false;
                                    }
                                }
                            }

                            result.Add(perf);
                            index++;
                        }

                        allDates.Add(datesInSheet);
                    }

                    // Kiểm tra các ngày giữa các sheet
                    if (allDates.Count > 1)
                    {
                        var firstSheetDates = allDates[0];
                        foreach (var sheetDates in allDates.Skip(1))
                        {
                            if (!firstSheetDates.SetEquals(sheetDates))
                            {
                                MessageBox.Show("Các ngày trong các sheet không giống nhau!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đọc file Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true; // thành công
        }
        public void InsertToDatabase(List<DevicePerformance> dataList, string fullname)
        {
            using (DbService db = new DbService(config))
            {
                foreach (var item in dataList)
                {
                    // Kiểm tra nếu item.Date không phải là null và không rỗng
                    if (item.Date != null && item.Date.Count > 0)
                    {
                        // Lấy danh sách các ngày từ Dictionary
                        var dates = item.Date.Keys.ToList(); // Lấy danh sách các khóa (ngày)

                        // Chuyển đổi từ định dạng "dd.MM" sang DateTime
                        var firstDateString = dates.First();
                        var lastDateString = dates.Last();

                        DateTime firstDate = DateTime.ParseExact(firstDateString, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                        DateTime lastDate = DateTime.ParseExact(lastDateString, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                        for (var date = firstDate; date <= lastDate; date = date.AddDays(1))
                        {
                            var formattedDate = date.ToString("dd.MM.yyyy");
                            var dateKey = date.ToString("dd.MM.yyyy");

                            var queryInsert = @"
                                INSERT INTO machine_performance 
                                (date, factory, device_type, device_name, st, qty_taget, qty_completed, daily_performance, reason, event_user, event_time) 
                                VALUES 
                                (@date, @factory, @device_type, @device_name, @st, @qty_taget, @qty_completed, @daily_performance, @reason, @even_user, CURRENT_TIMESTAMP)";

                            var parametersInsert = new List<MySqlParameter>
                            {
                                new MySqlParameter("@date", formattedDate),
                                new MySqlParameter("@factory", item.Factory ?? (object)DBNull.Value),
                                new MySqlParameter("@device_type", item.Item ?? (object)DBNull.Value),
                                new MySqlParameter("@device_name", item.Machine_Name ?? (object)DBNull.Value),
                                new MySqlParameter("@st", item.Performance_ST.ContainsKey(dateKey) ? (object)item.Performance_ST[dateKey] : (object)DBNull.Value),
                                new MySqlParameter("@qty_taget", item.Performance_Target.ContainsKey(dateKey) ? (object)item.Performance_Target[dateKey] : (object)DBNull.Value),
                                new MySqlParameter("@qty_completed", item.Performance_Completed.ContainsKey(dateKey) ? (object)item.Performance_Completed[dateKey] : (object)DBNull.Value),
                                new MySqlParameter("@daily_performance", item.DailyPerformance.ContainsKey(dateKey) ? (object)item.DailyPerformance[dateKey] : (object)DBNull.Value),
                                new MySqlParameter("@reason", item.Reason.ContainsKey(dateKey) ? item.Reason[dateKey] : (object)DBNull.Value),
                                new MySqlParameter("@even_user", fullname ?? (object)DBNull.Value),
                            };

                            db.ExecuteQuery(queryInsert, parametersInsert);
                        }
                    }
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
                            string factoryName = reader["factory"]?.ToString();
                            string item = reader["device_type"]?.ToString();
                            string deviceName = reader["device_name"]?.ToString();
                            string date = reader["date"]?.ToString();

                            double performanceST = reader["st"] != DBNull.Value ? Convert.ToDouble(reader["st"]) : 0;
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
                                    Item = item,
                                    Machine_Name = deviceName,
                                    Performance_ST = new Dictionary<string, double>(),
                                    DailyPerformance = new Dictionary<string, double>(),
                                    Performance_Target = new Dictionary<string, double>(),
                                    Performance_Completed = new Dictionary<string, double>(),
                                    Reason = new Dictionary<string, string>()
                                };
                            }
                            deviceDict[key].DailyPerformance[date] = dailyPerformance;
                            deviceDict[key].Performance_Target[date] = performanceTarget;
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
