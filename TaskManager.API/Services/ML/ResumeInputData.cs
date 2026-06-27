using Microsoft.ML.Data;

namespace TaskManager.API.Services.ML
{
    public class ResumeInputData
    {
        [LoadColumn(0)]
        public string ResumeText { get; set; } = string.Empty;

        [LoadColumn(1)]
        public string SkillLabel { get; set; } = string.Empty;
    }

    public class SkillPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedSkill { get; set; } = string.Empty;

        public float[] Score { get; set; } = System.Array.Empty<float>();
    }
}
