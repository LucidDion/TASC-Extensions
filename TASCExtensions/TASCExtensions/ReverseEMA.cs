using System;
using System.Collections.Generic;
using System.Text;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCIndicators
{
    //Reverse EMA Indicator (C) 2017 John F. Ehlers   
    public class ReverseEMA : IndicatorBase
    {
        //parameterless constructor
        public ReverseEMA() : base()
        {
        }

        //for code based construction
        public ReverseEMA(TimeSeries source, Double alpha)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = alpha;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Alpha", ParameterTypes.Double, 0.1);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Double alpha = Parameters[1].AsDouble;

            DateTimes = ds.DateTimes;

            if (ds.Count == 0)
                return;

            //Classic EMA
            var CC = 1.0 - alpha;
            var _EMA = ds * 0;
            for (int bar = 1; bar < ds.Count; bar++)
            {
                _EMA[bar] = alpha * ds[bar] + CC * _EMA[bar - 1];
            }

            //Compute Reverse EMA
            var RE1 = CC * _EMA + _EMA >> 1;
            var RE2 = Math.Pow(CC, 2) * RE1 + RE1 >> 1;
            var RE3 = Math.Pow(CC, 4) * RE2 + RE2 >> 1;
            var RE4 = Math.Pow(CC, 8) * RE3 + RE3 >> 1;
            var RE5 = Math.Pow(CC, 16) * RE4 + RE4 >> 1;
            var RE6 = Math.Pow(CC, 32) * RE5 + RE5 >> 1;
            var RE7 = Math.Pow(CC, 64) * RE6 + RE6 >> 1;
            var RE8 = Math.Pow(CC, 128) * RE7 + RE7 >> 1;

            //Indicator as difference
            var Wave = _EMA - alpha * RE8;

            for (int bar = ds.FirstValidIndex; bar < ds.Count; bar++)
            {
                Values[bar] = Wave[bar];
            }
        }

        public override string Name => "ReverseEMA";

        public override string Abbreviation => "ReverseEMA";

        public override string HelpDescription => "This is the Reverse EMA indicator by John F. Ehlers from the September 2017 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"ReverseEMA";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}
