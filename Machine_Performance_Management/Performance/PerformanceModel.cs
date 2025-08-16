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

        public void InsertToDatabase(List<DevicePerformance1> dataList)
        {
            using (DbService db = new DbService(config))
            {
                foreach (var item in dataList)
                {
                    var queryInsert = @"
                INSERT INTO machine_performance 
                (date, factory, device_name, qty_taget, qty_completed, daily_performance, reason) 
                VALUES 
                (@date, @factory, @device_name, @qty_taget, @qty_completed, @daily_performance, @reason)";

                    var parametersInsert = new List<MySqlParameter>
            {
                new MySqlParameter("@date", item.Date ?? (object)DBNull.Value),
                new MySqlParameter("@factory", item.Factory ?? (object)DBNull.Value),
                new MySqlParameter("@device_name", item.Machine_Name ?? (object)DBNull.Value),
                new MySqlParameter("@qty_taget", item.Performance_Target),
                new MySqlParameter("@qty_completed", item.Performance_Completed),
                new MySqlParameter("@daily_performance", item.DailyPerformance),
                new MySqlParameter("@reason", item.Reason ?? (object)DBNull.Value),
            };

                    db.ExecuteQuery(queryInsert, parametersInsert);
                }
            }
        }

    }
}
