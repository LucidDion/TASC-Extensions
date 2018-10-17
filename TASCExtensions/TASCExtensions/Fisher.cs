using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //Fisher Indicator class
    public class Fisher : IndicatorBase                
    {
        //parameterless constructor
        public Fisher() : base()
        {
        }

        //for code based construction
        public Fisher(TimeSeries source, Int32 period)
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
            AddParameter("Period", ParameterTypes.Int32, 30);
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

            var Lo = new Lowest(ds, period);
            var Hi = new Highest(ds, period);
            double Value1 = 0, Value = 0;

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + period - 1;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] = 0;

            //
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Value1 = 0.67 * Value1;
                if (Lo[bar] < Hi[bar])
                    Value1 += 0.33 * (2 * (ds[bar] - Lo[bar]) / (Hi[bar] - Lo[bar]) - 1);
                Value = 0.5 * Value;
                Value += 0.5 * Math.Log((1 + Value1) / (1 - Value1));
                Values[bar] = Value;
            }
        }

        public override string Name => "Fisher";

        public override string Abbreviation => "Fisher";

        public override string HelpDescription => "John Ehlers' Fisher Transform indicator as presented in the November 2002 issue of Stocks & Commodities magazine.";
    
        public override string PaneTag => @"Fisher";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }
}
