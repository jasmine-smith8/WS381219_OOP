using ClosedXML.Excel;

namespace WS381219_OOP
{
    public partial class ScheduleExporter
    {
        private void GenerateExcelFile(string filePath, List<string> subdivisions, Dictionary<int, Dictionary<string, Task>> timeRows)
        {
            var jobColourGenerator = new JobColourGenerator();
            var jobIdColors = jobColourGenerator.GenerateJobIdColors(_scheduler);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Schedule");
                WriteHeaderRow(worksheet, subdivisions);
                WriteScheduleRows(worksheet, subdivisions, timeRows, jobIdColors);
                WriteSummaryStatistics(worksheet, subdivisions);

                var range = worksheet.RangeUsed();
                if (range != null)
                {
                    range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                workbook.SaveAs(filePath);
            }

            Console.WriteLine($"Schedule exported to {filePath}");
        }

        private void WriteHeaderRow(IXLWorksheet worksheet, List<string> subdivisions)
        {
            worksheet.Cell(1, 1).Value = "StartTime";
            worksheet.Cell(1, 1).Style.Font.Bold = true;

            for (int i = 0; i < subdivisions.Count; i++)
            {
                worksheet.Cell(1, i + 2).Value = subdivisions[i];
                worksheet.Cell(1, i + 2).Style.Font.Bold = true;
            }
        }

        private void WriteScheduleRows(IXLWorksheet worksheet, List<string> subdivisions, Dictionary<int, Dictionary<string, Task>> timeRows, Dictionary<int, XLColor> jobIdColors)
        {
            int rowIndex = 2;

            foreach (var time in timeRows.Keys.OrderBy(t => t))
            {
                TimeSpan startTime = TimeSpan.FromHours(9) + TimeSpan.FromHours(time);
                TimeSpan endTime = startTime + TimeSpan.FromHours(1);
                worksheet.Cell(rowIndex, 1).Value = $"{startTime:hh\\:mm}-{endTime:hh\\:mm}";

                for (int i = 0; i < subdivisions.Count; i++)
                {
                    var task = timeRows[time][subdivisions[i]];
                    if (task != null)
                    {
                        int startRow = rowIndex;
                        int endRow = startRow + task.ProcessingTime - 1;

                        worksheet.Range(startRow, i + 2, endRow, i + 2).Merge();
                        worksheet.Cell(startRow, i + 2).Value = $"JobID: {task.JobID} OpID: {task.OperationID}";
                        worksheet.Range(startRow, i + 2, endRow, i + 2).Style.Fill.BackgroundColor = jobIdColors[task.JobID];
                    }
                }

                rowIndex++;
            }

            worksheet.Range(rowIndex, 1, rowIndex, subdivisions.Count + 1).Merge();
            worksheet.Cell(rowIndex, 1).Value = "End of Schedule";
            worksheet.Cell(rowIndex, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(rowIndex, 1).Style.Font.Bold = true;
            worksheet.Cell(rowIndex, 1).Style.Fill.BackgroundColor = XLColor.Red;
            worksheet.Cell(rowIndex, 1).Style.Font.FontColor = XLColor.White;
        }
    }
}