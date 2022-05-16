﻿// This file was partially auto-generated by ML.NET Model Builder
// and uses parts from bike tentals forecasting tutorial
// @ https://docs.microsoft.com/en-us/dotnet/machine-learning/tutorials/time-series-demand-forecasting

using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace SmartHeater.ML;

public static class SmartHeaterModel
{
    public static async Task EnsureTrainedAsync(string mlProjectPath, bool overwrite = false)
    {
        //Setup
        //Check if model base is trained already (ensured existence, can skip).
        if (!overwrite && File.Exists(MLContants.DefaultModelFilePath))
        {
            return;
        }
        //Model is not trained, check if the training file exists (if not, generate it).
        if (!File.Exists(MLContants.TrainingFileName) || !File.Exists(MLContants.TestingFileName))
        {
            var weatherFilePath = Path.Combine(mlProjectPath, "Data", "B2MBUD01_T_N.csv");
            var generator = new MLDataGenerator(weatherFilePath);
            await generator.RunAsync();
        }

        //Training
        var mlContext = new MLContext();
        var trainingData = LoadDataFromCsv(mlContext, MLContants.TrainingFileName);
        var model = TrainPipeline(mlContext, trainingData);

        using var predEngine = model.CreateTimeSeriesEngine<ModelInput, ModelOutput>(mlContext);
        predEngine.CheckPoint(mlContext, MLContants.DefaultModelFilePath);

        var trainingResult = File.Exists(MLContants.DefaultModelFilePath);
        Console.WriteLine(trainingResult ? "Model is trained." : "Model is NOT trained.");

        //Testing
        var testingData = LoadDataFromCsv(mlContext, MLContants.TestingFileName);
        Evaluate(testingData, model, mlContext);
    }

    #region Consumption
    /// <summary>
    /// Use this method to predict on <see cref="ModelInput"/>.
    /// </summary>
    /// <param name="input">model input.</param>
    /// <returns><seealso cref=" ModelOutput"/></returns>
    public static ModelOutput Forecast(string? heaterIpAddress, ModelInput? input = null, int horizon = MLContants.Horizon, bool saveModelCheckpoint = true)
    {
        #if DEBUG
            Console.WriteLine($"starting forecast for input: {input?.TemperatureDiff}");
        #endif

        var mlContext = new MLContext();
        var modelPath = ModelPathFromIP(heaterIpAddress);

        using var predEngine = CreatePredictEngine(mlContext, modelPath);
        var result = predEngine.Predict(input!, horizon);

        if (saveModelCheckpoint)
            predEngine.CheckPoint(mlContext, modelPath);
        
        #if DEBUG
            foreach (var item in result.TemperatureDiff)
            {
                Console.WriteLine(item);
            }
        #endif
        
        return result;
    }

    private static TimeSeriesPredictionEngine<ModelInput, ModelOutput> CreatePredictEngine(MLContext mlContext,
                                                                                           string modelPath)
    {
        if (!File.Exists(modelPath))
        {
            File.Copy(MLContants.DefaultModelFilePath, modelPath);
        }
        var mlModel = mlContext.Model.Load(modelPath, inputSchema: out var _);
        return mlModel.CreateTimeSeriesEngine<ModelInput, ModelOutput>(mlContext);
    }
    #endregion

    #region Training
    private static ITransformer TrainPipeline(MLContext context, IDataView trainData)
    {
        var pipeline = context.Forecasting
            .ForecastBySsa(windowSize: 4,
                           seriesLength: 12,
                           trainSize: 1051200,
                           horizon: 10,
                           confidenceLevel: 0.95f,
                           outputColumnName: @"temperatureDiff",
                           inputColumnName: @"temperatureDiff",
                           confidenceLowerBoundColumn: @"temperatureDiff_LB",
                           confidenceUpperBoundColumn: @"temperatureDiff_UB");
        var model = pipeline.Fit(trainData);
        return model;
    }
    #endregion

    private static string ModelPathFromIP(string? ipAddress)
    {
        return ipAddress is not null
            ? $"SmartHeaterModel_{ipAddress}.zip"
            : MLContants.DefaultModelFilePath;
    }

    private static IDataView LoadDataFromCsv(MLContext mlContext, string path)
    {
        return mlContext.Data.LoadFromTextFile<ModelInput>(path, separatorChar: ';', hasHeader: true);
    }

    private static void Evaluate(IDataView testData, ITransformer model, MLContext mlContext)
    {
        // Make predictions
        IDataView predictions = model.Transform(testData);

        // Actual values
        var actual = mlContext.Data.CreateEnumerable<ModelInput>(testData, true)
                .Select(observed => observed.TemperatureDiff)
                .ToList();

        // Predicted values
        var forecast = mlContext.Data.CreateEnumerable<ModelOutput>(predictions, true)
                .Select(prediction => prediction.TemperatureDiff[0])
                .ToList();

        // Calculate error (actual - forecast)
        var metrics = actual.Zip(forecast, (actualValue, forecastValue) => actualValue - forecastValue);

        // Get metric averages
        var MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
        var RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error

        // Output metrics
        Console.WriteLine("Evaluation Metrics");
        Console.WriteLine("---------------------");
        Console.WriteLine($"Mean Absolute Error: {MAE:F3}");
        Console.WriteLine($"Root Mean Squared Error: {RMSE:F3}\n");

        for (int i = 0; i < MLContants.Horizon; i++)
        {
            Console.WriteLine($"Predicted: {forecast[i]}\tActual: {actual[i]}");
        }
    }
}
