using Microsoft.ML;
using System;

namespace LightDancing.MLs
{
    public class ConsumeModel
    {
        private static string GRERE_MODEL_PATH = Environment.CurrentDirectory + "\\MLs\\Source\\GenreModel.zip";
        private static readonly Lazy<PredictionEngine<GenreData, GenrePrediction>> predictionEngine = new Lazy<PredictionEngine<GenreData, GenrePrediction>>(CreatePredictionEngine);
        // For more info on consuming ML.NET models, visit https://aka.ms/mlnet-consume
        // Method for consuming model in your app

        /// <summary>
        /// Allows GenreHelper to set MLs file manually
        /// </summary>
        /// <param name="path"></param>
        public static void SetMLsFilePath(string path)
        {
            GRERE_MODEL_PATH = path;
        }

        public static GenrePrediction Predict(GenreData input)
        {
            GenrePrediction result = predictionEngine.Value.Predict(input);
            return result;
        }

        public static PredictionEngine<GenreData, GenrePrediction> CreatePredictionEngine()
        {
            // Create new MLContext
            MLContext mlContext = new MLContext();

            // Load model & create prediction engine
            ITransformer mlModel = mlContext.Model.Load(GRERE_MODEL_PATH, out _);
            var predEngine = mlContext.Model.CreatePredictionEngine<GenreData, GenrePrediction>(mlModel);

            return predEngine;
        }
    }

}
