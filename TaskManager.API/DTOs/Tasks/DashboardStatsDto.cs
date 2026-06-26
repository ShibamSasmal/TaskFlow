using System.Collections.Generic;

namespace TaskManager.API.DTOs.Tasks
{
    public class DashboardStatsDto
    {
        public int TotalTasks { get; set; }
        public int TodoCount { get; set; }
        public int InProgressCount { get; set; }
        public int DoneCount { get; set; }
        public int OverdueCount { get; set; }
        public int HighPriorityCount { get; set; }
        public double CompletionRate { get; set; }
        public IEnumerable<TaskResponseDto> UpcomingTasks { get; set; } = new List<TaskResponseDto>();
    }
}
