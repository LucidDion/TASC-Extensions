using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class VolatilitySwitch : IndicatorBase
    {
        //parameterless constructor
        public VolatilitySwitch() : base()
        {
        }

        //for code based construction
        public VolatilitySwitch(TimeSeries source, Int32 period)
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
            AddParameter("Period", ParameterTypes.Int32, 21);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            if (ds.Count < period) return;

            ROC dailyChange = new ROC(ds, 1);
            var histVola = new StdDev(dailyChange, period) / 100d; // Excel STDEV is based on sample
            var voltSwitch = new TimeSeries(DateTimes);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar >= period)
                {
                    int cnt = 0;

                    for (int i = bar; i >= bar - period; i--)
                    {
                        if (histVola[i] <= histVola[bar])
                            cnt++;
                    }

                    Values[bar] = (double)cnt / (double)period;
                }
                else
                    Values[bar] = 0d;                
            }
        }

        public override string Name => "VolatilitySwitch";

        public override string Abbreviation => "VolatilitySwitch";

        public override string HelpDescription => "Based on an article by Ron McEwan, published in the February 2013 issue of Stocks and Commodities Magazine. The Volatility (Regime) Switch indicator is a method for adapting a trading strategy when the market changes from a trending mode to a mean reverting one.";

        public override string PaneTag => @"VolatilitySwitch";

        public override Color DefaultColor => Color.DarkGreen;

        public override double OverboughtLevel => 0.5;

        public override double OversoldLevel => 0.499;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
