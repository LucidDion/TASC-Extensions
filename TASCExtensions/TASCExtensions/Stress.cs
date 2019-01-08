using System;
using System.Collections.Generic;
using System.Text;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCIndicators
{
    public class Stress : IndicatorBase
    {
        //parameterless constructor
        public Stress() : base()
        {
            OverboughtLevel = 10;
            OversoldLevel = 90;
        }

        //for code based construction
        public Stress(BarHistory bars, BarHistory index, Int32 period)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = index;
            Parameters[2].Value = period;
            OverboughtLevel = 10;
            OversoldLevel = 90;
            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterTypes.BarHistory, null);
            AddParameter("Index", ParameterTypes.BarHistory, null);
            AddParameter("Stochastic period", ParameterTypes.Int32, 60);
        }

        //(BarHistory bars, BarHistory barsFirst, BarHistory barsSecond, int periodRegression, int periodRegressionMomentum,
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            BarHistory barsIndex = Parameters[1].AsBarHistory;
            Int32 period = Parameters[2].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || DateTimes.Count == 0)
                return;

            if (bars.Count < period)
                return;

            var S = new StochK(bars, period);
            var I = new StochK(barsIndex, period);
            var D = S.Abs() - I;
            var Stress = 100 * (D - new Lowest(D, period)) / (new Highest(D, period) - new Lowest(D, period));

            for (int bar = 0; bar < bars.Count; bar++)
            {
                if (bar < period)
                    Values[bar] = 0d;
                else
                    Values[bar] = Stress[bar];
            }
        }

        public override string Name => "Stress";

        public override string Abbreviation => "Stress";

        public override string HelpDescription => "Perry J. Kaufman's Stress indicator from March 2014 issue of Stocks & Commodities magazine is an intermarket Stochastic indicator showing whether the symbol is oversold relative to an index.";

        public override string PaneTag => @"Stress";

        public override Color DefaultColor => Color.Coral;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }
}
