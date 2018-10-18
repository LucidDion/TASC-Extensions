using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    /// <summary>
    /// TEMA (Triple-Smoothed Exponential Moving Average) Indicator
    /// </summary>
    public class TEMA_TASC : IndicatorBase
    {
        //parameterless constructor
        public TEMA_TASC() : base()
        {
        }

        //for code based construction
        public TEMA_TASC(TimeSeries source, Int32 period)
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
            AddParameter("Period", ParameterTypes.Int32, 14);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            //Rest of series
            var ema1 = new EMA(ds, period);
            var ema2 = new EMA(ema1, period);
            var ema3 = new EMA(ema2, period);

            for (int bar = period; bar < ds.Count; bar++)
            {
                Values[bar] = 3 * ema1[bar] - 3 * ema2[bar] + ema3[bar];
            }            
        }



        public override string Name => "TEMA";

        public override string Abbreviation => "TEMA";

        public override string HelpDescription => @"TEMA is the Triple-smoothed Exponential Moving Average based on TECHNICAL ANALYSIS FROM A TO Z, 2nd Ed., pg. 328-330.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.DarkMagenta;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }    
}
