using ClosedXML.Excel;

namespace WS381219_OOP
{
    public partial class ScheduleExporter
    {
        private readonly Scheduler _scheduler;
        public ScheduleExporter(Scheduler scheduler)
        {
            _scheduler = scheduler;
        }
        // This method checks if the schedule is empty, and if not exports the schedule to an XLSX file.
        public void ExportScheduleToXLSX(string filePath)
        {
            if (IsScheduleEmpty())
            {
                Console.WriteLine("The schedule is empty. No data to export.");
                return;
            }
            // Include helper methods from other files
            var subdivisions = GetSubdivisions();
            var timeRows = InitialiseTimeRows(subdivisions);
            PopulateTimeRows(timeRows, subdivisions);
            GenerateExcelFile(filePath, subdivisions, timeRows);
        }
    }
}