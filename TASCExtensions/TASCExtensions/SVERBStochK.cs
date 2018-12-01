using System;
using System.Collections.Generic;
using System.Text;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCIndicators
{
    public class SVERBStochK : IndicatorBase
    {
        //parameterless constructor
        public SVERBStochK() : base()
        {
        }

        //for code based construction
        public SVERBStochK(BarHistory bars, Int32 stochPeriod, Int32 smoothPeriod)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = stochPeriod;
            Parameters[2].Value = smoothPeriod;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterTypes.BarHistory, null);
            AddParameter("StochK Period", ParameterTypes.Int32, 30);
            AddParameter("Smoothing K Period", ParameterTypes.Int32, 3);
        }

        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 periodK = Parameters[1].AsInt;
            Int32 smoothK = Parameters[2].AsInt;

            DateTimes = bars.DateTimes;
            var period = Math.Max(periodK, smoothK);

            if (period <= 0 || DateTimes.Count == 0)
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

            TimeSeries RBC = (Rainbow + bars.AveragePriceHLC) / 2;
            TimeSeries nom = RBC - new Lowest(bars.Low, periodK);
            TimeSeries den = new Highest(bars.High, periodK) - new Lowest(RBC, periodK);

            var fastK = new TimeSeries(DateTimes);
            for (int bar = 0; bar < bars.Count; bar++)
            {
                if (bar >= periodK)
                    fastK[bar] = (Math.Min(100, Math.Max(0, 100 * nom[bar] / den[bar])));
                else
                    fastK[bar] = 0d;
            }

            var K = new FastSMA(fastK, smoothK);

            for (int bar = 0; bar < bars.Count; bar++)
            {
                Values[bar] = K[bar];
            }
        }

        public override string Name => "SVERBStochK";

        public override string Abbreviation => "SVERBStochK";

        public override string HelpDescription => "Sylvain Vervoort's SVERBStochK indicator from September 2013 issue of Stocks & Commodities magazine is a Stochastic K oscillator using the Rainbow data series.";

        public override string PaneTag => @"RainbowPane";   // Share pane between SVEZLRBPercB and SVERBStochK

        public override Color DefaultColor => Color.Red;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }    
}

