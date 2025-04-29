using ClosedXML.Excel;
namespace WS381219_OOP
{
    public class JobColourGenerator
    {
        // Helper method to generate a color mapping for Job IDs
        public Dictionary<int, XLColor> GenerateJobIdColors(Scheduler scheduler)
        {
            var colors = new List<XLColor>
            {
                XLColor.LightBlue,
                XLColor.Aquamarine,
                XLColor.LightYellow,
                XLColor.PaleTurquoise,
                XLColor.LightPink,
                XLColor.MayaBlue,
                XLColor.BubbleGum,
                XLColor.LavenderPink,
                XLColor.BabyBlueEyes,
                XLColor.LightPastelPurple,
            };

            var jobIdColors = new Dictionary<int, XLColor>();
            int colorIndex = 0;

            foreach (var jobId in scheduler.Schedule.Values.Select(task => task.JobID).Distinct())
            {
                jobIdColors[jobId] = colors[colorIndex % colors.Count];
                colorIndex++;
            }

            return jobIdColors;
        }
    }
}