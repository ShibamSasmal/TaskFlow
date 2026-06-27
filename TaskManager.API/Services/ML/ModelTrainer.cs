using System;
using System.IO;
using Microsoft.ML;

namespace TaskManager.API.Services.ML
{
    public class ModelTrainer
    {
        public static void TrainAndSaveModel(string dataPath, string modelPath)
        {
            if (!File.Exists(dataPath))
            {
                throw new FileNotFoundException($"Training data file not found at {dataPath}");
            }

            var mlContext = new MLContext(seed: 42);

            // Load data from CSV file
            var data = mlContext.Data.LoadFromTextFile<ResumeInputData>(
                dataPath, separatorChar: ',', hasHeader: true, allowQuoting: true);

            // Count rows in the training data
            var rowCount = 0;
            using (var reader = new StreamReader(dataPath))
            {
                while (reader.ReadLine() != null) rowCount++;
            }

            IDataView trainingData = data;
            
            // Define pipeline
            var pipeline = mlContext.Transforms.Conversion
                .MapValueToKey("Label", nameof(ResumeInputData.SkillLabel))
                .Append(mlContext.Transforms.Text.FeaturizeText("Features", nameof(ResumeInputData.ResumeText)))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            ITransformer model;

            // Train model
            if (rowCount > 10)
            {
                // We have enough data to split and train
                var trainTestSplit = mlContext.Data.TrainTestSplit(data, testFraction: 0.1);
                model = pipeline.Fit(trainTestSplit.TrainSet);

                try
                {
                    var predictions = model.Transform(trainTestSplit.TestSet);
                    var metrics = mlContext.MulticlassClassification.Evaluate(predictions);
                    Console.WriteLine($"[ML.NET] Model successfully trained. MacroAccuracy: {metrics.MacroAccuracy:P2}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ML.NET] Model trained with evaluation warning: {ex.Message}");
                }
            }
            else
            {
                // Not enough data to split, train on whole set
                model = pipeline.Fit(data);
                Console.WriteLine("[ML.NET] Model trained on the entire small dataset (no test split).");
            }

            // Ensure output directory exists
            var directory = Path.GetDirectoryName(modelPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Save model
            mlContext.Model.Save(model, data.Schema, modelPath);
            Console.WriteLine($"[ML.NET] Saved trained model zip file to: {modelPath}");
        }
    }
}
