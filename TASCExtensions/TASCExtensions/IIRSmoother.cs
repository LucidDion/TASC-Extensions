using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //IIRSmoother Indicator class
    public class IIRSmoother : IndicatorBase
    {
        //parameterless constructor
        public IIRSmoother() : base()
        {
        }

        //for code based construction
        public IIRSmoother(TimeSeries source)
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
            var FirstValidValue = ds.FirstValidIndex + 4;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            for (int bar = 0; bar < FirstValidValue - 1; bar++)
                Values[bar] = 0;

            if(FirstValidValue > 1)
                Values[FirstValidValue - 1] = ds[FirstValidValue - 1];

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
                Values[bar] = 0.2 * (2 * ds[bar] - ds[bar - 4]) + 0.8 * Values[bar - 1];
        }

        public override bool IsSmoother => true;

        public override string Name => "IIRSmoother";

        public override string Abbreviation => "IIRSmoother";

        public override string HelpDescription => "John Ehlers' IIR Data Smoother from the July 2002 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}
