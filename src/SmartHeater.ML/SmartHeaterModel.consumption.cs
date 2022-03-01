﻿// This file was auto-generated by ML.NET Model Builder. 

using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Microsoft.ML.Transforms.TimeSeries;

namespace SmartHeater.ML
{
    public partial class SmartHeaterModel
    {
        /// <summary>
        /// model input class for SmartHeaterModel.
        /// </summary>
        #region model input class
        public class ModelInput
        {
            [ColumnName(@"turned_on")]
            public float Turned_on { get; set; }

        }

        #endregion

        /// <summary>
        /// model output class for SmartHeaterModel.
        /// </summary>
        #region model output class
        public class ModelOutput
        {
            [ColumnName(@"turned_on")]
            public float[] Turned_on { get; set; }

            [ColumnName(@"turned_on_LB")]
            public float[] Turned_on_LB { get; set; }

            [ColumnName(@"turned_on_UB")]
            public float[] Turned_on_UB { get; set; }

        }

        #endregion

        private static string MLNetModelPath = Path.GetFullPath(@"C:\Users\tommi\source\repos\tommilostny\IBT_2022\src\SmartHeater.ML\SmartHeaterModel.zip");

        public static readonly Lazy<TimeSeriesPredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<TimeSeriesPredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);

        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public static ModelOutput Predict(ModelInput? input = null, int? horizon = null)
        {
            var predEngine = PredictEngine.Value;
            return predEngine.Predict(input, horizon);
        }

        private static TimeSeriesPredictionEngine<ModelInput, ModelOutput> CreatePredictEngine()
        {
            var mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out var schema);
            return mlModel.CreateTimeSeriesEngine<ModelInput, ModelOutput>(mlContext);
        }
    }
}

