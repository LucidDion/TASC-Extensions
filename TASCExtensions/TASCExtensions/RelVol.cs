using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class RelVol : IndicatorBase
    {
        //parameterless constructor
        public RelVol() : base()
        {
        }

        //for code based construction
        public RelVol(BarHistory bars, Int32 period)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = period;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterTypes.BarHistory, null);
            AddParameter("Period", ParameterTypes.Int32, 60);
        }

        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;            
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || DateTimes.Count == 0)
                return;

            var FirstValidValue = period;
            if (FirstValidValue > bars.Count || FirstValidValue < 0) FirstValidValue = bars.Count;

            for (int bar = 0; bar < bars.Count; bar++)
            {
                if (bar >= period)
                {
                    double av = new FastSMA(bars.Volume, period)[bar];
                    double sd = new StdDev(bars.Volume, period)[bar];
                    double relVol = (bars.Volume[bar] - av) / sd;
                    Values[bar] = relVol;
                }
                else
                {
                    Values[bar] = 0d;
                }
            }
        }

        public override string Name => "RelVol";

        public override string Abbreviation => "RelVol";

        public override string HelpDescription => "Created by Melvin Dickover (see article in April 2014 issue of Stocks and Commodities Magazine), Relative Volume is an auxiliary indicator that finds spikes of volume above a predefined number of standard deviations of the average volume of the lookback period.";

        public override string PaneTag => @"RelVol";

        public override Color DefaultColor => Color.Red;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Histogram;
    }    
}
