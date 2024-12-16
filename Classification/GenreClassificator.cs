using System;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;
using GenreClassification.Models;

namespace GenreClassification.Classification
{
    public class GenreClassificator
    {
        private static ITransformer? model;
        private static string dataPath = Path.Combine(Environment.CurrentDirectory, "Data", "book_data_goodread.csv");

        public GenreClassificator()
        {
            if (model == null)
                InitModel();
        }

        public string GetPredict(Book book)
        {

            var mlContext = new MLContext();
            Console.WriteLine(book.Title);
            var predictionEngine = mlContext.Model.CreatePredictionEngine<Book, GenrePrediction>(model);
            var prediction = predictionEngine.Predict(book);

            Console.WriteLine($"Predicted genre: {prediction.Genre}");

            return prediction.Genre;
        }

        public void InitModel()
        {
            if (!File.Exists(dataPath))
                throw new FileNotFoundException($"File not found: {dataPath}");

            var mlContext = new MLContext();

            IDataView dataView = mlContext.Data.LoadFromTextFile<Book>(
                dataPath,
                separatorChar: ';',
                hasHeader: true
            );

            var splitData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            var trainData = splitData.TrainSet;
            var testData = splitData.TestSet;

            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", "Title")
                .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", "Genre"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            model = pipeline.Fit(trainData);

            var predictions = model.Transform(testData);
            var metrics = mlContext.MulticlassClassification.Evaluate(predictions, "Label", "Score");
            Console.WriteLine("Metrics:");
            Console.WriteLine($"Macro Accuracy: {metrics.MacroAccuracy:F2}");
            Console.WriteLine($"Micro Accuracy: {metrics.MicroAccuracy:F2}");
        }
    }
}
