using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Linq;

namespace TASCIndicators
{
    public class SchaffTrendCycle : IndicatorBase
    {
        //parameterless constructor
        public SchaffTrendCycle() : base()
        {
        }

        //for code based construction
        public SchaffTrendCycle(TimeSeries source, Int32 tcLength, Int32 macdPeriod1, Int32 macdPeriod2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = tcLength;
            Parameters[2].Value = macdPeriod1;
            Parameters[3].Value = macdPeriod2;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("TC Length", ParameterTypes.Int32, 10);
            AddParameter("MACD Period1", ParameterTypes.Int32, 23);
            AddParameter("MACD Period2", ParameterTypes.Int32, 50);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 tcLength = Parameters[1].AsInt;
            Int32 ma1 = Parameters[2].AsInt;
            Int32 ma2 = Parameters[3].AsInt;
            var period = new List<int> { tcLength, ma1, ma2 }.Max();

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            //Assign first bar that contains indicator data
            var FirstValidValue = tcLength + Math.Max(ma1, ma2);
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            var xMac = new EMA(ds, ma1) - new EMA(ds, ma2);

            // 1st Stochastic: Calculate Stochastic
            var val1 = new Lowest(xMac, tcLength);
            var val2 = new Highest(xMac, tcLength) - val1;

            var frac1 = new TimeSeries(DateTimes);
            var pf = new TimeSeries(DateTimes);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                frac1[bar] = 0d;
                pf[bar] = 0d;
            }

            for (int bar = 1; bar < ds.Count; bar++)
            {
                if (val2[bar] > 0)
                    frac1[bar] = 100 * ((xMac[bar] - val1[bar]) / val2[bar]);
                else
                    frac1[bar] = frac1[bar - 1];

                // Smoothed Calculation for % Fast D
                pf[bar] = pf[bar - 1] + (0.5 * (frac1[bar] - pf[bar - 1]));
            }

            // 1st Stochastic: Calculate Stochastic
            var val3 = new Lowest(pf, tcLength);
            var val4 = new Highest(pf, tcLength) - val3;

            var frac2 = new TimeSeries(DateTimes);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                frac2[bar] = 0d;
            }

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar > 0)
                {
                    if (val4[bar] > 0)
                        frac2[bar] = 100 * ((pf[bar] - val3[bar]) / val4[bar]);
                    else
                        frac2[bar] = frac2[bar - 1];

                    // Smoothed Calculation for % Fast D
                    Values[bar] = Values[bar - 1] + (0.5 * (frac2[bar] - Values[bar - 1]));
                }
                else
                    Values[bar] = 0d;                
            }
        }

        public override string Name => "SchaffTrendCycle";

        public override string Abbreviation => "SchaffTrendCycle";

        public override string HelpDescription => "Schaff Trend Cycle from the April 2010 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"SchaffTrendCycle";

        public override Color DefaultColor => Color.Gold;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
