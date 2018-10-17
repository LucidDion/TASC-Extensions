using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //EMA2 Indicator class
    public class EMA2 : IndicatorBase
    {
        //parameterless constructor
        public EMA2() : base()
        {
        }

        //for code based construction
        public EMA2(TimeSeries source, Double period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Length", ParameterTypes.Double, 14);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Double period = Parameters[1].AsDouble;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            var expnt = 2 / (1 + period);

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + 1;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] = 0;

            //Rest of series
            double Value = ds[FirstValidValue - 1];
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Value += expnt * (ds[bar] - Value);
                Values[bar] = Value;
            }
        }

        public override bool IsSmoother => true;

        public override string Name => "EMA2";

        public override string Abbreviation => "EMA2";

        public override string HelpDescription => @"EMA2 duplicates the standard EMA calculation, except that EMA2 accepts a non-integer Period. It was created specifically to support the requirements of the 'Adaptive Price Zone' ChartScript from the September 2006 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }    
}
