﻿﻿// This file was auto-generated by ML.NET Model Builder. 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML;

namespace SmartHeater.ML
{
    public partial class SmartHeaterModel
    {
        public static ITransformer RetrainPipeline(MLContext context, IDataView trainData)
        {
            var pipeline = BuildPipeline(context);
            var model = pipeline.Fit(trainData);

            return model;
        }

        /// <summary>
        /// build the pipeline that is used from model builder. Use this function to retrain model.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <returns></returns>
        public static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
        {
            // Data process configuration with pipeline data transformations
            var pipeline = mlContext.Forecasting.ForecastBySsa(windowSize:2,seriesLength:10,trainSize:527040,horizon:60,outputColumnName:@"turned_on",inputColumnName:@"turned_on",confidenceLowerBoundColumn:@"turned_on_LB",confidenceUpperBoundColumn:@"turned_on_UB");

            return pipeline;
        }
    }
}
