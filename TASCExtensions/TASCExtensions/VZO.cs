using System;
using System.Collections.Generic;
using System.Text;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCIndicators
{
    public class VZO : IndicatorBase
    {
        //parameterless constructor
        public VZO() : base()
        {
        }

        //for code based construction
        public VZO(BarHistory bars, Int32 period)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = period;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterTypes.BarHistory, null);
            AddParameter("Period", ParameterTypes.Int32, 14);
        }

        //(BarHistory bars, BarHistory barsFirst, BarHistory barsSecond, int periodRegression, int periodRegressionMomentum,
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period= Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            var R = new TimeSeries(DateTimes);
            var TV = new EMA(bars.Volume, period);
            R[0] = 0d;

            for (int bar = 0; bar < bars.Count; bar++)
            {
                if (bar > 0)
                    R[bar] = Math.Sign(bars.Close[bar] - bars.Close[bar - 1]) * bars.Volume[bar];
            }
            var VP = new EMA(R, period);

            for (int bar = 0; bar < bars.Count; bar++)
            {
                if (TV[bar] != 0)
                    Values[bar] = 100 * VP[bar] / TV[bar];
            }
        }

        public override string Name => "VZO";

        public override string Abbreviation => "VZO";

        public override string HelpDescription => "The Volume Zone Oscillator by W.Khalil and D.Steckler takes into account both time and volume fluctuations from bearish to bullish and is designed to work in trending and non-trending conditions.";

        public override string PaneTag => @"VZO";

        public override Color DefaultColor => Color.DarkBlue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;

    }    
}
