using ClosedXML.Excel;

namespace WS381219_OOP
{
    public partial class ScheduleExporter
    {
        private void WriteSummaryStatistics(IXLWorksheet worksheet, List<string> subdivisions)
        {
            int rowIndex = worksheet.LastRowUsed().RowNumber() + 1;

            worksheet.Cell(rowIndex, 1).Value = "Total Makespan";
            worksheet.Cell(rowIndex, 1).Style.Font.Bold = true;
            worksheet.Cell(rowIndex, 2).Value = $"{_scheduler.Schedule.Values.Max(t => t.EndTime)}";

            worksheet.Cell(rowIndex + 1, 1).Value = "Average Tasks per Job";
            worksheet.Cell(rowIndex + 1, 1).Style.Font.Bold = true;
            worksheet.Cell(rowIndex + 1, 2).Value = $"{_scheduler.Schedule.Values.GroupBy(t => t.JobID).Average(g => g.Count())}";

            worksheet.Cell(rowIndex + 2, 1).Value = "Number of Jobs";
            worksheet.Cell(rowIndex + 2, 1).Style.Font.Bold = true;
            worksheet.Cell(rowIndex + 2, 2).Value = $"{_scheduler.Schedule.Values.Select(t => t.JobID).Distinct().Count()}";

            worksheet.Cell(rowIndex + 3, 1).Value = "Number of Subdivisions";
            worksheet.Cell(rowIndex + 3, 1).Style.Font.Bold = true;
            worksheet.Cell(rowIndex + 3, 2).Value = $"{subdivisions.Count}";

            worksheet.Cell(rowIndex + 4, 1).Value = "Total Number of Tasks";
            worksheet.Cell(rowIndex + 4, 1).Style.Font.Bold = true;
            worksheet.Cell(rowIndex + 4, 2).Value = $"{Math.Ceiling((double)_scheduler.Schedule.Values.Count)}";
        }
    }
}