using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class SuperPassband : IndicatorBase
    {
        //parameterless constructor
        public SuperPassband() : base()
        {
        }

        //for code based construction
        public SuperPassband(TimeSeries source, Int32 period1, Int32 period2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period1", ParameterTypes.Int32, 40);
            AddParameter("Period2", ParameterTypes.Int32, 60);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            var FirstValidValue = Math.Max(period1, period2);
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            if (FirstValidValue <= 0 || ds.Count == 0)
                return;

            double a1 = 5 / (double)period1;
            double a2 = 5 / (double)period2;

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar >= 2)
                {
                Values[bar] = (a1 - a2) * ds[bar] + (a2 * (1 - a1) - a1 * (1 - a2)) * ds[bar - 1] +
                        ((1 - a1) + (1 - a2)) * Values[bar - 1] - (1 - a1) * (1 - a2) * Values[bar - 2];
                }
                else
                    Values[bar] = 0;
            }            
        }


        public override string Name => "SuperPassband";

        public override string Abbreviation => "SuperPassband";

        public override string HelpDescription => "Created by John Ehlers (see article in July 2016 issue of Stocks and Commodities Magazine), the Super Passband oscillator is a filter with near-zero lag.";

        public override string PaneTag => @"SuperPassband";

        public override Color DefaultColor => Color.Red;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }

    public class SuperPassbandRMS : IndicatorBase
    {
        //parameterless constructor
        public SuperPassbandRMS() : base()
        {
        }

        //for code based construction
        public SuperPassbandRMS(TimeSeries source, Int32 period1, Int32 period2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period1", ParameterTypes.Int32, 40);
            AddParameter("Period2", ParameterTypes.Int32, 60);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            var FirstValidValue = Math.Max(period1, period2);
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            if (FirstValidValue <= 0 || ds.Count == 0)
                return;

            if (ds.Count < Math.Max(49, FirstValidValue)) return;

            var spb = new SuperPassband(ds, period1, period2);

            var RMS = new TimeSeries(DateTimes);
            for (int bar = 0; bar < ds.Count; bar++)
                RMS[bar] = 0d;

            for (int bar = 49; bar < ds.Count; bar++)
            {
                for (int count = 0; count <= 49; count++)
                    RMS[bar] = RMS[bar] + spb[bar - count] * spb[bar - count];

                RMS[bar] = Math.Sqrt(RMS[bar] / 50d);
                Values[bar] = RMS[bar];
            }            
        }

        public override string Name => "SuperPassbandRMS";

        public override string Abbreviation => "SuperPassbandRMS";

        public override string HelpDescription => "The root mean square (RMS) value of the cyclic output of the Super Passband filter by John Ehlers can be used as trigger points which can increase the efficiency of entries and exits.";

        public override string PaneTag => @"SuperPassband";

        public override Color DefaultColor => Color.Yellow;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
