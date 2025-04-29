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

        public void ExportScheduleToXLSX(string filePath)
        {
            if (IsScheduleEmpty())
            {
                Console.WriteLine("The schedule is empty. No data to export.");
                return;
            }

            var subdivisions = GetSubdivisions();
            var timeRows = InitializeTimeRows(subdivisions);
            PopulateTimeRows(timeRows, subdivisions);
            GenerateExcelFile(filePath, subdivisions, timeRows);
        }
    }
}