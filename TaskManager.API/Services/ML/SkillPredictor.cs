using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;

namespace TaskManager.API.Services.ML
{
    public class SkillPredictor
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;
        private readonly DataViewSchema _schema;

        public SkillPredictor(string modelPath)
        {
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException($"ML.NET model file not found at {modelPath}. Try training the model first.");
            }

            _mlContext = new MLContext(seed: 42);
            _model = _mlContext.Model.Load(modelPath, out _schema);
        }

        public IEnumerable<(string PredictedLabel, float Confidence)> PredictSkills(string resumeText, float confidenceThreshold = 0.65f)
        {
            // Create the prediction engine (not thread-safe, created transiently per prediction request)
            var engine = _mlContext.Model.CreatePredictionEngine<ResumeInputData, SkillPrediction>(_model);

            // Split the text into lines/sentences and remove empty entries
            var sentences = resumeText.Split(new[] { '.', '\n', '\r', ';', '•', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 3)
                .ToList();

            var predictions = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

            foreach (var sentence in sentences)
            {
                var input = new ResumeInputData { ResumeText = sentence };
                var result = engine.Predict(input);

                if (result == null || string.IsNullOrEmpty(result.PredictedSkill) || result.Score == null || result.Score.Length == 0)
                {
                    continue;
                }

                // SDCA multiclass classification returns a score array representing probabilities.
                // We find the index of the predicted label to get its confidence score.
                var maxScore = result.Score.Max();

                if (maxScore >= confidenceThreshold && !result.PredictedSkill.Equals("None", StringComparison.OrdinalIgnoreCase))
                {
                    // If we already predicted this skill, keep the highest confidence
                    if (predictions.TryGetValue(result.PredictedSkill, out var existingConfidence))
                    {
                        if (maxScore > existingConfidence)
                        {
                            predictions[result.PredictedSkill] = maxScore;
                        }
                    }
                    else
                    {
                        predictions.Add(result.PredictedSkill, maxScore);
                    }
                }
            }

            return predictions.Select(p => (PredictedLabel: p.Key, Confidence: p.Value));
        }
    }
}
