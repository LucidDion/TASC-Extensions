using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //PTBar Indicator class
    public class PTBar : IndicatorBase
    {
        //parameterless constructor
        public PTBar() : base()
        {
        }

        //for code based construction
        public PTBar(TimeSeries source, Double reversal)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[3].Value = reversal;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("ReversalPct", ParameterTypes.Double, 3.3);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Double reversal = Parameters[1].AsDouble;

            DateTimes = ds.DateTimes;

            if (ds.Count == 0)
                return;

            var PeakReversalFactor = 100 / (100 + reversal);
            var TroughReversalFactor = (100 + reversal) / 100;

            // locate Bar2 as the earliest bar that goes "far enough" from first bar
            int Bar1 = ds.FirstValidIndex;
            int Bar2 = 0;
            for (Bar2 = Bar1 + 1; Bar2 < ds.Count; Bar2++)
                if (ds[Bar2] >= ds[Bar1] * TroughReversalFactor 
                 || ds[Bar2] <= ds[Bar1] * PeakReversalFactor) 
                    break;

            //Assign first bar that contains indicator data
            var FirstValidValue = Bar2;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] = 0;

            //Rest of series: go on, alternating peaks and troughs
            for (int bar = Bar2; bar < ds.Count; bar++)
            {
                if (ds[Bar2] > ds[Bar1]) // looking for a new peak
                {
                    if (ds[bar] >= ds[Bar2]) 
                        Bar2 = bar; // found higher high
                    else if (ds[bar] <= ds[Bar2] * PeakReversalFactor) 
                        { Bar1 = Bar2; Bar2 = bar; } // reversal triggered at Bar, peak at Bar2 is confirmed
                }
                else // ds[Bar2] < ds[Bar1], looking for a new trough
                {
                    if (ds[bar] <= ds[Bar2]) 
                        Bar2 = bar; // found lower low
                    else if (ds[bar] >= ds[Bar2] * TroughReversalFactor) 
                        { Bar1 = Bar2; Bar2 = bar; } // reversal triggered at Bar, trough at Bar2 is confirmed
                }
                // Bar1 is last confirmed peak/trough bar (as seen at Bar)
                Values[bar] = Bar1;
            }
        }
        
        public override string Name => "PTBar";

        public override string Abbreviation => "PTBar";

        public override string HelpDescription => @"Peaks&Troughs Indicator (PTBar) - as described in November 2006 Stocks&Commodities (Siligardos' article on ""Active Trend Lines"")";

        public override string PaneTag => "PTBar";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}
