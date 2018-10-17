using QuantaculaCore;
using QuantaculaIndicators;
using System;
using System.Drawing;

namespace TASCIndicators
{
    public class FastSMA : IndicatorBase
    {
        //parameterless constructor
        public FastSMA() : base()
        {
        }

        //for code based construction
        public FastSMA(TimeSeries source, Int32 period)
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
            AddParameter("Length", ParameterTypes.Int32, 14);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = period - 1 + ds.FirstValidIndex;
            if (period < 1 || period > ds.Count + 1)
            {
                period = ds.Count + 1;
            }

            double sum = 0;
            int max_count = ds.Count;
            if (period > max_count)
            {
                for (int bar = 0; bar < max_count; bar++)
                {
                    double cur_ds = Double.IsNaN(ds[bar]) ? 0 : ds[bar];
                    sum += cur_ds;
                    Values[bar] = sum / (bar + 1);
                }
            }
            else
            {
                double[] ds_cache = new double[period];
                int cache_index = 0;
                int prev_cache_index = 0;
                int max_index = period - 1;
                double cur_ds = 0;
                for (int bar = 0; bar < period; bar++)
                {
                    cur_ds = Double.IsNaN(ds[bar]) ? 0 : ds[bar];
                    sum += cur_ds;
                    Values[bar] = sum / (bar + 1);
                    ds_cache[cache_index] = cur_ds;
                    cache_index++;
                }

                double period_mul = 1d / period;
                for (int bar = period; bar < max_count; bar++)
                {
                    cur_ds = Double.IsNaN(ds[bar]) ? 0 : ds[bar];

                    sum += cur_ds;
                    sum -= ds_cache[prev_cache_index];
                    Values[bar] = sum * period_mul;

                    cache_index++;
                    if (cache_index > max_index) cache_index = 0;

                    prev_cache_index++;
                    if (prev_cache_index > max_index) prev_cache_index = 0;
                    ds_cache[cache_index] = cur_ds;
                }
            }
        }

        public override bool IsSmoother => true;

        public override string Name => "FastSMA";

        public override string Abbreviation => "FastSMA";

        public override string HelpDescription => @"FastSMA uses a new, faster SMA algorithm - thanks to Aleksey Kuznetsov)";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.Blue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;

        public static double Calculate(int bar, TimeSeries ds, int period)
        {
            double result;
            if (ds.Count < period)
            {
                result = 0.0;
            }
            else
            {
                double num = 0.0;
                for (int i = bar; i > bar - period; i--)
                {
                    num += ds[i];
                }
                result = num / (double)period;
            }
            return result;
        }
    }
}