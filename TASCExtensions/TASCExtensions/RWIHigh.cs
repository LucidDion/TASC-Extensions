using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //RWIHigh Indicator class
    public class RWIHigh : IndicatorBase
    {
        //parameterless constructor
        public RWIHigh() : base()
        {
        }

        //for code based construction
        public RWIHigh(BarHistory bars, Int32 minPeriod, Int32 maxPeriod)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = minPeriod;
            Parameters[2].Value = maxPeriod;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterTypes.BarHistory, null);
            AddParameter("Min Period", ParameterTypes.Int32, 8);
            AddParameter("Max Period", ParameterTypes.Int32, 64);
        }

        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;
            Int32 minperiod = Parameters[1].AsInt;
            Int32 maxperiod = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;
            var period = Math.Max(minperiod, maxperiod);

            if (period <= 0 || DateTimes.Count == 0)
                return;

            //Avoid exception errors
            if (minperiod < 2 || minperiod > ds.Count + 1) minperiod = ds.Count + 1;
            if (maxperiod < minperiod || maxperiod > ds.Count + 1) maxperiod = ds.Count + 1;

            //Assign first bar that contains indicator data
            var FirstValidValue = maxperiod;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] = 0;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                double SumTR = 0;
                for (int per = 1; per < minperiod; per++)
                    SumTR += TR.Calculate(bar - per + 1, ds);
                double Value = 0;
                for (int per = minperiod; per <= maxperiod; per++)
                {
                    SumTR += TR.Calculate(bar - per + 1, ds);
                    if (SumTR > 0)
                        Value = Math.Max(Value, (ds.High[bar] - ds.Low[bar - per + 1]) * Math.Sqrt(per) / SumTR);
                }
                Values[bar] = Value;
            }
        }

        //This static method allows ad-hoc calculation of RWIHigh (single calc mode)
        public static double Calculate(int bar, BarHistory ds, int minperiod, int maxperiod)
        {
            if (bar < minperiod - 1 || bar < maxperiod - 1)
                return 0;

            double SumTR = 0;
            for (int per = 1; per < minperiod; per++) SumTR += TR.Calculate(bar - per + 1, ds);
            double Value = 0;
            for (int per = minperiod; per <= maxperiod; per++)
            {
                SumTR += TR.Calculate(bar - per + 1, ds);
                if (SumTR > 0)
                    Value = Math.Max(Value, (ds.High[bar] - ds.Low[bar - per + 1]) * Math.Sqrt(per) / SumTR);
            }
            return Value;
        }

        public override string Name => "RWIHigh";

        public override string Abbreviation => "RWIHigh";

        public override string HelpDescription => "Presented in Technical Analysis Of Stocks and Commodities by Michael Poulos (see TASC, January 1992 and September 1992).";

        public override string PaneTag => @"RWI";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}
