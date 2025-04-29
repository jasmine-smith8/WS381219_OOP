namespace WS381219_OOP
{
    public partial class ScheduleExporter
    {
        private bool IsScheduleEmpty()
        {
            return !_scheduler.Schedule.Any();
        }
    }
}