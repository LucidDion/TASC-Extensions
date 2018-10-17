using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //TrueLow Indicator class
    public class TrueHigh : IndicatorBase
    {
        //parameterless constructor
        public TrueHigh() : base()
        {
        }

        //for code based construction
        public TrueHigh(BarHistory bars)
            : base()
        {
            Parameters[0].Value = bars;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterTypes.BarHistory, null);
        }

        //(BarHistory bars, BarHistory barsFirst, BarHistory barsSecond, int periodRegression, int periodRegressionMomentum,
        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;

            DateTimes = ds.DateTimes;

            if (DateTimes.Count == 0)
                return;

            //Assign first bar that contains indicator data
            var FirstValidValue = 1;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            for (int bar = 0; bar < FirstValidValue; bar++)
                Values[bar] = 0;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
                Values[bar] = Math.Max(ds.High[bar], ds.Close[bar - 1]);
        }

        //This static method allows ad-hoc calculation of TrueLow (single calc mode)
        public static double Calculate(int bar, BarHistory ds)
        {
            return Math.Max(ds.High[bar], ds.Close[bar - 1]);
        }

        public override string Name => "TrueHigh";

        public override string Abbreviation => "TrueHigh";

        public override string HelpDescription => "";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}