using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //InverseFisher Indicator class
    public class InverseFisher : IndicatorBase
    {
        //parameterless constructor
        public InverseFisher() : base()
        {
        }

        //for code based construction
        public InverseFisher(TimeSeries source)
            : base()
        {
            Parameters[0].Value = source;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            DateTimes = ds.DateTimes;

            if (ds.Count == 0)
                return;

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                double e2y = Math.Exp(2 * ds[bar]);
                Values[bar] = (e2y - 1) / (e2y + 1);
            }
        }

        //This static method allows ad-hoc calculation of InverseFisher (single calc mode)
        public static double Calculate(int bar, TimeSeries ds)
        {
            double e2y = Math.Exp(2 * ds[bar]);
            return (e2y - 1) / (e2y + 1);
        }

        public override bool IsSmoother => true;

        public override string Name => "InverseFisher";

        public override string Abbreviation => "InverseFisher";

        public override string HelpDescription => "InverseFisher Transform Indicator from the May 2004 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}
