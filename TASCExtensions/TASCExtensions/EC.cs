using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class LeastError : IndicatorBase
    {
        public override string Name => "LeastError";

        public override string Abbreviation => "LeastError";

        public override string HelpDescription => "Internal indicator, companion to EC";

        public override string PaneTag => "LeastError";

        //parameterless constructor
        public LeastError() : base()
        {
        }

        //for code based construction
        public LeastError(TimeSeries source, Int32 period, Int32 gain)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = gain;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period", ParameterTypes.Int32, 32);
            AddParameter("Gain", ParameterTypes.Int32, 22);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 Length = Parameters[1].AsInt;
            Int32 GainLimit = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            if (Length <= 0 || ds.Count == 0)
                return;
            
            double alpha = 2d / (Length + 1d);
            double LeastError = 0;
            double Gain = 0;
            double Error = 0;
            double BestGain = 0;
            var ema = new EMA(ds, Length);
            var ec = new TimeSeries(DateTimes);
            var LE = new TimeSeries(DateTimes);

            ec[Length - 1] = 0d;
            LE[Length - 1] = 0d;

            for (int bar = Length; bar < ds.Count; bar++)
            {
                ema[bar] = alpha * ds[bar] + (1 - alpha) * ema[bar - 1];
                LeastError = 1000000;

                for (int Value1 = -GainLimit; Value1 <= GainLimit; Value1++)
                {
                    Gain = Value1 / 10;
                    ec[bar] = alpha * (ema[bar] + Gain * (ds[bar] - ec[bar - 1])) +
                        (1 - alpha) * ec[bar - 1];
                    Error = ds[bar] - ec[bar];
                    if (Math.Abs(Error) < LeastError)
                    {
                        LeastError = Math.Abs(Error);
                        BestGain = Gain;
                    }
                }

                LE[bar] = 100 * LeastError / ds[bar];
                ec[bar] = alpha * (ema[bar] + BestGain * (ds[bar] - ec[bar - 1])) + (1 - alpha) * ec[bar - 1];

                Values[bar] = LE[bar];
            }
        }
    }

    public class EC : IndicatorBase
    {
        //parameterless constructor
        public EC() : base()
        {
        }

        //for code based construction
        public EC(TimeSeries source, Int32 length, Int32 gain)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = length;
            Parameters[2].Value = gain;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Length", ParameterTypes.Int32, 32);
            AddParameter("Gain Limit", ParameterTypes.Int32, 22);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 Length = Parameters[1].AsInt;
            Int32 GainLimit = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            if (Length <= 0 || ds.Count == 0)
                return;
            
            double alpha = 2d / (Length + 1d);
            double LeastError = 0;
            double Gain = 0;
            double Error = 0;
            double BestGain = 0;
            var ema = new EMA(ds, Length);
            var ec = new TimeSeries(DateTimes);
            var LE = new TimeSeries(DateTimes);

            ec[Length - 1] = 0d;
            LE[Length - 1] = 0d;

            for (int bar = Length; bar < ds.Count; bar++)
            {
                ema[bar] = alpha * ds[bar] + (1 - alpha) * ema[bar - 1];
                LeastError = 1000000;

                for (int Value1 = -GainLimit; Value1 <= GainLimit; Value1++)
                {
                    Gain = Value1 / 10;
                    ec[bar] = alpha * (ema[bar] + Gain * (ds[bar] - ec[bar - 1])) +
                        (1 - alpha) * ec[bar - 1];
                    Error = ds[bar] - ec[bar];
                    if (Math.Abs(Error) < LeastError)
                    {
                        LeastError = Math.Abs(Error);
                        BestGain = Gain;
                    }
                }

                LE[bar] = 100 * LeastError / ds[bar];
                ec[bar] = alpha * (ema[bar] + BestGain * (ds[bar] - ec[bar - 1])) + (1 - alpha) * ec[bar - 1];

                Values[bar] = ec[bar];
            }            
		}

        public override string Name => "EC";

        public override string Abbreviation => "EC";

        public override string HelpDescription => "EC by John Ehlers from the November 2010 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.Yellow;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;

        public override bool IsSmoother => true;
    }    
}
