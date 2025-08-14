using Machine_Performance_Management.Common;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Machine_Performance_Management.Performance
{
	public class PerformanceModel
	{
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

                        // Giả sử cột ngày bắt đầu từ L (12)
                        int firstDayCol = 7;

                        for (int row = 5; row <= rowCount; row++)
                        {
                            // Bỏ qua dòng trống
                            if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text))
                                continue;

                            var perf = new DevicePerformance
                            {
                                NO = row - 4, // Tự đánh số NO
                                //Factory = worksheet.Cells[row, 1].Text?.Trim(), // Cột 1 = Factory
                                Machine_Name = worksheet.Cells[row, 1].Text?.Trim(),
                            };

                            perf.Factory = worksheet.Cells[row, 1].Text?.Trim().Substring(0, 2);

                            // Đọc dữ liệu hiệu suất từng ngày
                            for (int col = firstDayCol; col <= colCount; col += 6)
                            {
                                string dateHeader = worksheet.Cells[3, col].Text?.Trim();
                                if (string.IsNullOrEmpty(dateHeader))
                                    continue;

                                // Lấy capa/일 (Performance_Target) → giả sử cách ngày 1 cột
                                string targetText = worksheet.Cells[row, col + 2].Text;
                                if (double.TryParse(targetText.Replace(",", "").Trim(),
                                                    NumberStyles.Any,
                                                    CultureInfo.InvariantCulture,
                                                    out var targetValue))
                                {
                                    perf.Performance_Target[dateHeader] = targetValue;
                                }

                                // Lấy 생산량 (Performance_Completed) → giả sử cách ngày 2 cột
                                string completedText = worksheet.Cells[row, col + 3].Text;
                                if (double.TryParse(completedText.Replace(",", "").Trim(),
                                                    NumberStyles.Any,
                                                    CultureInfo.InvariantCulture,
                                                    out var completedValue))
                                {
                                    perf.Performance_Completed[dateHeader] = completedValue;
                                }

                                // Lấy 가동율 (DailyPerformance) → giả sử cách ngày 4 cột
                                string perfText = worksheet.Cells[row, col + 4].Text;
                                if (double.TryParse(perfText.Replace("%", "").Trim(),
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

    }
}
