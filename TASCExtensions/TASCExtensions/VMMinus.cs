using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class VMMinus : IndicatorBase
    {
        //parameterless constructor
        public VMMinus() : base()
        {
        }

        //for code based construction
        public VMMinus(BarHistory bars, Int32 period)
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
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || DateTimes.Count == 0)
                return;

            //Avoid exceptions
            if (period < 1 || period > bars.Count + 1) period = bars.Count + 1;

            var _tr = new TR(bars).Sum(period);
            var _vmMinus = (bars.Low - (bars.High >> 1)).Abs().Sum(period);

            for (int bar = period; bar < bars.Count; bar++)
            {
                Values[bar] = _vmMinus[bar] / _tr[bar];
            }
        }

        public override string Name => "VMMinus";

        public override string Abbreviation => "VMMinus";

        public override string HelpDescription => "Vortex Movement (VM-) from the January 2010 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"VM";

        public override Color DefaultColor => Color.Red;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}