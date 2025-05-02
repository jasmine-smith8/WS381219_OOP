using ClosedXML.Excel;
namespace WS381219_OOP
{
    public class JobColourGenerator
    {
        // Helper method to generate a color mapping for Job IDs
        public Dictionary<int, XLColor> GeneratejobIdColours(Scheduler scheduler)
        {
            var colours = new List<XLColor>
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
            // Create a dictionary to store the color mapping for each Job ID
            var jobIdColours = new Dictionary<int, XLColor>();
            int colourIndex = 0;
            // Iterate through the scheduled tasks and assign colors to Job IDs
            foreach (var jobId in scheduler.Schedule.Values.Select(task => task.JobID).Distinct())
            {
                jobIdColours[jobId] = colours[colourIndex % colours.Count];
                colourIndex++;
            }

            return jobIdColours;
        }
    }
}