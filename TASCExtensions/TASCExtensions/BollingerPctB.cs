using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //BBandPercentB Indicator class
    public class BollingerPctB : IndicatorBase
    {
        //parameterless constructor
        public BollingerPctB() : base()
        {
        }

        //for code based construction
        public BollingerPctB(TimeSeries source, Int32 period)
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

            var _sd = new StdDev(ds, period);
            var _sma = new FastSMA(ds, period);

            for (int bar = period; bar < ds.Count; bar++)
            {
                Values[bar] = 100 * (ds[bar] + 2 * _sd[bar] - _sma[bar]) / (4 * _sd[bar]);
            }
        }



        public override string Name => "BBandPercentB";

        public override string Abbreviation => "BBandPercentB";

        public override string HelpDescription => @"Bollinger %b referenced in the May 2010 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"BBandPercentB";

        public override Color DefaultColor => Color.Green;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    } 

}
