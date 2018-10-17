using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //Gapo Indicator class
    public class Gapo : IndicatorBase
    {
        //parameterless constructor
        public Gapo() : base()
        {
        }

        //for code based construction
        public Gapo(BarHistory source, Int32 period)
        : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
            AddParameter("Period", ParameterTypes.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            //Remember parameters
            var range = new Highest(ds.High, period) - new Lowest(ds.Low, period);
            var logperiod = Math.Log10(period);

            //Assign first bar that contains indicator data
            var FirstValidValue = range.FirstValidIndex + period;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] = 0;

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
                Values[bar] = Math.Log10(range[bar]) / logperiod;
        }

        //This static method allows ad-hoc calculation of RSS (single calc mode)
        public static double Calculate(int bar, BarHistory ds, int period)
        {
            //Avoid exception errors
            if (period < 2) return Double.NaN;

            double range = Highest.Calculate(bar, ds.High, period) - Lowest.Calculate(bar, ds.Low, period);
            if (range <= 0) return Double.NaN;

            return Math.Log10(range) / Math.Log10(period);
        }

        public override string Name => "Gapo";

        public override string Abbreviation => "Gapo";

        public override string HelpDescription => "Gopalakrishnan Range Index, from the January 2000 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"Gapo";

        public override Color DefaultColor => Color.CadetBlue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}
