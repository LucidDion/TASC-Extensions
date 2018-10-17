using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Linq;

namespace TASCIndicators
{
    //RSS Indicator class
    public class RSS : IndicatorBase
    {
        //parameterless constructor
        public RSS() : base()
        {
        }

        //for code based construction
        public RSS(TimeSeries source, Int32 period)
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
            AddParameter("Fast SMA Period", ParameterTypes.Int32, 10);
            AddParameter("Slow SMA Period", ParameterTypes.Int32, 40);
            AddParameter("RSI Period", ParameterTypes.Int32, 5);
            AddParameter("Smoothing Period", ParameterTypes.Int32, 5);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 fastsmaperiod = Parameters[1].AsInt;
            Int32 slowsmaperiod = Parameters[2].AsInt;
            Int32 rsiperiod = Parameters[3].AsInt;
            Int32 smoothperiod = Parameters[4].AsInt;

            var period = new List<int> { fastsmaperiod, slowsmaperiod, rsiperiod, smoothperiod }.Max();

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            //Remember parameters 
            var spread = new SMA(ds, fastsmaperiod) - new SMA(ds, slowsmaperiod);
            var relstr = new RSI(spread, rsiperiod);
            var smooth = new SMA(relstr, smoothperiod);

            //Assign first bar that contains indicator data
            var FirstValidValue = (fastsmaperiod > slowsmaperiod ? fastsmaperiod : slowsmaperiod) + rsiperiod + smoothperiod;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar < FirstValidValue)
                    Values[bar] = 0;
                else
                    Values[bar] = smooth[bar];
            }
        }

        public override string Name => "RSS";

        public override string Abbreviation => "RSS";

        public override string HelpDescription => "Relative Spread Strength indicator, from Ian Copsey's article in the October 2006 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"RSS";

        public override Color DefaultColor => Color.BlueViolet;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}
