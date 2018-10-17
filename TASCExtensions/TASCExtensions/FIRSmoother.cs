using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //FIRSmoother Indicator class
    public class FIRSmoother : IndicatorBase
    {
        //parameterless constructor
        public FIRSmoother() : base()
        {
        }

        //for code based construction
        public FIRSmoother(TimeSeries source)
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
            var FirstValidValue = ds.FirstValidIndex + 6;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] = 0;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
                Values[bar] = (2 * ds[bar] + 7 * ds[bar - 1] + 9 * ds[bar - 2] + 6 * ds[bar - 3]
                                         + 1 * ds[bar - 4] - 1 * ds[bar - 5] - 3 * ds[bar - 6]) / 21;
        }

        //This static method allows ad-hoc calculation of FIRSmoother (single calc mode)
        public static double Calculate(int bar, TimeSeries ds)
        {
            if (bar < 6)
                return 0;

            return (2 * ds[bar] + 7 * ds[bar - 1] + 9 * ds[bar - 2] + 6 * ds[bar - 3]
                                + 1 * ds[bar - 4] - 1 * ds[bar - 5] - 3 * ds[bar - 6]) / 21;
        }

        public override string Name => "FIRSmoother";

        public override string Abbreviation => "FIRSmoother";

        public override string HelpDescription => @"The FIR Data Smoother from the July 2002 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;

        public override bool IsSmoother => true;
    }
}
