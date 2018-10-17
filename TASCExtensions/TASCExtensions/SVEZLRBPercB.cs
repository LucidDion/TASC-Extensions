using System;
using System.Collections.Generic;
using System.Text;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCIndicators
{
    public class SVEZLRBPercB : IndicatorBase
    {
        //parameterless constructor
        public SVEZLRBPercB() : base()
        {
        }

        //for code based construction
        public SVEZLRBPercB(BarHistory bars, Int32 smooth, Int32 sdPeriod)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = smooth;
            Parameters[2].Value = sdPeriod;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterTypes.BarHistory, null);
            AddParameter("Smoothing Period", ParameterTypes.Int32, 3);
            AddParameter("StdDev Period", ParameterTypes.Int32, 18);
        }

        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 smooth = Parameters[1].AsInt;
            Int32 sdPeriod = Parameters[2].AsInt;

            DateTimes = bars.DateTimes;
            var period = Math.Max(smooth, sdPeriod);

            if (period <= 0 || DateTimes.Count == 0 || bars.Count < period)
                return;

            FastSMA sma = new FastSMA(bars.Close, 2);
            TimeSeries sma1 = sma * 5;
            TimeSeries sma2 = new FastSMA(sma, 2) * 4;
            TimeSeries sma3 = new FastSMA(new FastSMA(sma, 2), 2) * 3;
            TimeSeries sma4 = new FastSMA(new FastSMA(new FastSMA(sma, 2), 2), 2) * 2;
            TimeSeries sma5 = new FastSMA(new FastSMA(new FastSMA(new FastSMA(sma, 2), 2), 2), 2);
            TimeSeries sma6 = new FastSMA(sma5, 2);
            TimeSeries sma7 = new FastSMA(sma6, 2);
            TimeSeries sma8 = new FastSMA(sma7, 2);
            TimeSeries sma9 = new FastSMA(sma8, 2);
            TimeSeries sma10 = new FastSMA(sma9, 2);
            TimeSeries Rainbow = (sma1 + sma2 + sma3 + sma4 + sma5 + sma6 + sma7 + sma8 + sma9 + sma10) / 20;

            TimeSeries ema1 = new EMA(Rainbow, smooth);
            TimeSeries ema2 = new EMA(ema1, smooth);
            TimeSeries diff = ema1 - ema2;
            TimeSeries ZLRB = ema1 + diff;
            TEMA tema = new TEMA(ZLRB, smooth);
            StdDev sd = new StdDev(tema, sdPeriod);
            WMA wma = new WMA(tema, sdPeriod);
            var PB = (tema + sd * 2 - wma) / (sd * 4) * 100;

            for (int bar = 0; bar < bars.Count; bar++)
            {
                base[bar] = PB[bar];
            }
        }
        
        public override string Name => "SVEZLRBPercB";

        public override string Abbreviation => "SVEZLRBPercB";

        public override string HelpDescription => "Sylvain Vervoort's SVEZLRBPercB indicator from September 2013 issue of Stocks & Commodities magazine is a Smoothed zero-lagging Percent b indicator on rainbow price series.";

        public override string PaneTag => @"RainbowPane";   // Share pane between SVEZLRBPercB and SVERBStochK

        public override Color DefaultColor => Color.DodgerBlue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }    
}
