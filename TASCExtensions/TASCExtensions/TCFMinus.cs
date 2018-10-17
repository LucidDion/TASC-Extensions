using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //TCFMinus Indicator class
    public class TCFMinus : IndicatorBase
    {
        //parameterless constructor
        public TCFMinus() : base()
        {
        }

        //for code based construction
        public TCFMinus(TimeSeries source, Int32 period)
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
            AddParameter("Period", ParameterTypes.Int32, 35);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            //Avoid exception errors
            if (period < 1 || period > ds.Count + 1) period = ds.Count + 1;

            //Build intermediate series
            var ChangeMinus = new TimeSeries(DateTimes);
            var CFPlus = new TimeSeries(DateTimes);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar >= ds.FirstValidIndex + 1)
                {
                    double xChange = ds[bar] - ds[bar - 1];
                    if (xChange > 0)
                    {
                        ChangeMinus[bar] = 0;
                        CFPlus[bar] = CFPlus[bar - 1] + xChange;
                    }
                    else
                    {
                        ChangeMinus[bar] = -xChange;
                        CFPlus[bar] = 0;
                    }
                }
                else
                {
                    CFPlus[bar] = 0d; ChangeMinus[bar] = 0d;
                }                
            }
            var SumChangeMinus = ChangeMinus.Sum(period);
            var SumCFPlus = CFPlus.Sum(period);

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + period;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            for (int bar = 0; bar < FirstValidValue; bar++)
                Values[bar] = 0d;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
                Values[bar] = SumChangeMinus[bar] - SumCFPlus[bar];
        }

        public override string Name => "TCFMinus";

        public override string Abbreviation => "TCFMinus";

        public override string HelpDescription => "Trend Continuation - Indicator from the February 2002 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"TCF";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}
