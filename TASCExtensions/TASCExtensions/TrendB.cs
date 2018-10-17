using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Linq;

namespace TASCIndicators
{
    public class TrendB : IndicatorBase
    {
        bool CrossOver(int bar, TimeSeries ds1, TimeSeries ds2)
        {
            if (bar < 1) return false;
            return (ds1[bar] > ds2[bar] && ds1[bar - 1] <= ds2[bar - 1]);
        }

        bool CrossUnder(int bar, TimeSeries ds1, TimeSeries ds2)
        {
            if (bar < 1) return false;
            return (ds1[bar] < ds2[bar] && ds1[bar - 1] >= ds2[bar - 1]);
        }

        //parameterless constructor
        public TrendB() : base()
        {
        }
        
        //for code based construction
        public TrendB(TimeSeries source, Int32 period1, Int32 period2, Int32 m, Int32 n, Int32 c, bool UseRMSNoise)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;
            Parameters[3].Value = m;
            Parameters[4].Value = n;
            Parameters[5].Value = c;
            Parameters[6].Value = UseRMSNoise;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period1", ParameterTypes.Int32, 10);
            AddParameter("Period2", ParameterTypes.Int32, 40);
            AddParameter("m", ParameterTypes.Int32, 8);
            AddParameter("n", ParameterTypes.Int32, 250);
            AddParameter("c", ParameterTypes.Int32, 4);
            AddParameter("UseRMSNoise", ParameterTypes.Boolean, true);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;
            Int32 m = Parameters[3].AsInt;
            Int32 n = Parameters[4].AsInt;
            Int32 c = Parameters[5].AsInt;
            Boolean useRMSNoise = Parameters[6].AsBoolean;

            DateTimes = ds.DateTimes;
            var period = new List<int> { period1, period2, m, n, c }.Max();

            if (period <= 0 || ds.Count == 0)
                return;

            //Assign first bar that contains indicator data
            if (n == 0)
                n = 1;
            n = Math.Abs(n);
            var FirstValidValue = n;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            if (ds.Count < n || ds == null)
                return;

            /* CPC and CPCTrend*/
            var lpf1 = new EMA(ds, period1);
            var lpf2 = new EMA(ds, period2);

            var hCPC = new TimeSeries(DateTimes);
            var hTrend = new TimeSeries(DateTimes);

            double expnt = 2d / (1 + m);
            double cumSum = 0d;            
            int xBar = 0;
            var mom = new Momentum(ds, 1);
            hCPC[0] = hTrend[0] = 0d;
            for (int bar = 0; bar < ds.Count; bar++)
            {
                hTrend[bar] = 0d;
            }

            for (int bar = 1; bar < ds.Count; bar++)
            {
                if (CrossOver(bar, lpf1, lpf2) || CrossUnder(bar, lpf1, lpf2))
                {
                    cumSum = 0d;
                    xBar = bar;
                }
                else
                    cumSum += mom[bar];

                hCPC[bar] = cumSum;

                /* Calculate the Trend with a piecewise EMA */
                if (bar - xBar > 0)
                {
                    double diff = expnt * (hCPC[bar] - hTrend[bar - 1]);
                    hTrend[bar] = hTrend[bar - 1] + diff;
                }
            }

            /* Trend-Noise Balance*/
            TimeSeries hDT = hCPC - hTrend;
            TimeSeries hNoise;

            if (useRMSNoise)
            {
                var hDTms = new SMA(hDT * hDT, n);
                hNoise = new TimeSeries(DateTimes);
                for (int bar = 0; bar < ds.Count; bar++)                
                    hNoise[bar] = Math.Sqrt(hDTms[bar]);                
            }
            else
            {
                hNoise = new FastSMA(hDT.Abs(), n);                
            }
            hNoise *= c;

            hNoise = hNoise.Abs();
            hTrend = hTrend.Abs();

            var hB = hTrend + hNoise;
            hB = 100 * hTrend / hB;

            for (int bar = 0; bar < ds.Count; bar++)
                Values[bar] = hB[bar];
        }
        
        public override string Name => "TrendB";

        public override string Abbreviation => "TrendB";

        public override string HelpDescription => "Trend Noise Balance (B-Indicator) from the April 2004 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"TrendB";

        public override Color DefaultColor => Color.Blue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;        
    }
}
