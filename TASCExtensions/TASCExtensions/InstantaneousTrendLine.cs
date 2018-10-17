using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //InstantaneousTrendLine Indicator class
    public class InstantaneousTrendLine : IndicatorBase
    {
        //parameterless constructor
        public InstantaneousTrendLine() : base()
        {
        }

        //for code based construction
        public InstantaneousTrendLine(TimeSeries source, Int32 period)
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
            AddParameter("Period", ParameterTypes.Int32, 30);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;
            
            //Avoid exception errors
            if (period < 1 || period > ds.Count) period = ds.Count;
            var sma = new FastSMA(ds, period);

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + period + 2;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //     Values[bar] = 0;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                 double smoothedslope = ds[bar] - ds[bar - period + 1] 
                                      + ds[bar - 3] - ds[bar - period + 1 - 3] +
                                   2 * (ds[bar - 1] - ds[bar - period + 1 - 1] 
                                      + ds[bar - 2] - ds[bar - period + 1 - 2]);
                 Values[bar] = sma[bar] + smoothedslope / 12;
            }
        }

        //This static method allows ad-hoc calculation of InstantaneousTrendLine (single calc mode)
        public static double Calculate(int bar, TimeSeries ds, int period)
        {
            if (bar < period + 2 || period > ds.Count)
                return 0;

            double smoothedslope = ds[bar] - ds[bar - period + 1] 
                                 + ds[bar - 3] - ds[bar - period + 1 - 3] + 
                              2 * (ds[bar - 1] - ds[bar - period + 1 - 1] 
                                 + ds[bar - 2] - ds[bar - period + 1 - 2]);
            return FastSMA.Calculate(bar, ds, period) + smoothedslope / 12;
        }

        public override bool IsSmoother => true;

        public override string Name => "InstantaneousTrendLine";

        public override string Abbreviation => "InstantaneousTrendLine";

        public override string HelpDescription => "For a comprehensive explanation please refer to John Ehlers´ article 'Modeling the Market = Building Trading Strategies' in the August 2006 issue of Stocks and Commodities Magazine.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}
