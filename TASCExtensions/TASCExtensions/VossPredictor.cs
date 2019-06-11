using System;
using System.Collections.Generic;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class BandPass : IndicatorBase
    {
        //parameterless constructor
        public BandPass() : base()
        {
        }

        //for code based construction
        public BandPass(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period", ParameterTypes.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = Math.Max(3, period);
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            var Filt = new TimeSeries(DateTimes);

            double bandWidth = 0.25;
            double Deg2Rad = Math.PI / 180.0;
            var F1 = Math.Cos((360d / (double)period) * Deg2Rad);
            var G1 = Math.Cos((bandWidth * 360 / (double)period) * Deg2Rad);
            var S1 = 1d / G1 - Math.Sqrt(1d / (G1 * G1) - 1);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                Values[bar] = 0;
            }
            
            //BandPass Filter
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                if (bar <= 5)
                    Values[bar] = 0;

                if (bar > period)
                    Values[bar] = .5 * (1 - S1) * (ds[bar] - ds[bar - 2]) + F1 * (1 + S1) * Values[bar - 1] - S1 * Values[bar - 2];
            }
        }

        public override string Name => "BandPass";

        public override string Abbreviation => "BandPass";

        public override string HelpDescription => "Two-pole bandpass filter created by John Ehlers.";

        public override string PaneTag => @"Voss";

        public override Color DefaultColor => Color.Red;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }

    public class VossPredictor : IndicatorBase
    {
        //parameterless constructor
        public VossPredictor() : base()
        {
        }

        //for code based construction
        public VossPredictor(TimeSeries source, Int32 period, Int32 predict)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = predict;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period", ParameterTypes.Int32, 20);
            AddParameter("Predict", ParameterTypes.Int32, 3);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Int32 predict = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = Math.Max(3, period);
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            var Filt = new BandPass(ds, period);
            var voss = new TimeSeries(DateTimes);
            var order = 3 * predict;
            double SumC = 0;

            for (int bar = 0; bar < ds.Count; bar++)
            {
                SumC = 0;

                if (bar < FirstValidValue)
                    voss[bar] = 0;
                else
                {
                    for (int count = 0; count < order; count++)
                    {
                        SumC += ((count + 1) / (double)order) * voss[bar - (order - count)];
                    }

                    voss[bar] = ((3 + order) / 2) * Filt[bar] - SumC;
                }

                Values[bar] = voss[bar];
            }
        }

        public override string Name => "VossPredictor";

        public override string Abbreviation => "VossPredictor";

        public override string HelpDescription => "Created by John Ehlers, the Voss Predictor filter could help signal cyclic turning points.";

        public override string PaneTag => @"Voss";

        public override Color DefaultColor => Color.BlueViolet;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
