using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class ExpDev : IndicatorBase
    {
        //parameterless constructor
        public ExpDev() : base()
        {
        }

        //for code based construction
        public ExpDev(TimeSeries source, Int32 period, bool useEMA)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
			Parameters[2].Value = useEMA;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period", ParameterTypes.Int32, 20);
			AddParameter("Use EMA", ParameterTypes.Boolean, false);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
			Boolean useEMA = Parameters[2].AsBoolean;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0 || ds.Count < period)
                return;

            var weight = 2d / (period + 1);
            TimeSeries sma = FastSMA.Series(ds, period);
            TimeSeries ema = EMA.Series(ds, period);
			TimeSeries ma = !useEMA ? sma : ema;

			double mdev = 0;
            var exd = new TimeSeries(DateTimes);
            var absDif = (ma - ds).Abs();
            var Rate = 2 / (double)(period + 1);

            for (int i = 0; i > period; i--)
            {
                mdev += Math.Abs(ma[i] - ds[i]);
            }

            mdev /= period;

            for (int bar = 1; bar < ds.Count; bar++)
            {
                if (bar <= period)
                    exd[bar] = mdev;
                else
                    exd[bar] = absDif[bar] * Rate + exd[bar - 1] * (1.0 - Rate);
            }

            for (int bar = 0; bar < ds.Count; bar++)
            {
                Values[bar] = exd[bar];
            }

		}

        public override string Name => "ExpDev";

        public override string Abbreviation => "ExpDev";

        public override string HelpDescription => "Exponential deviation from the July 2019 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => "ExpDevBandsPane";

        public override Color DefaultColor => Color.Navy;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }

 
    public class ExpDevBandUpper : IndicatorBase
    {
        //parameterless constructor
        public ExpDevBandUpper() : base()
        {
        }

        //for code based construction
        public ExpDevBandUpper(TimeSeries source, Int32 period, Double deviations, Boolean useEMA)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = deviations;
			Parameters[3].Value = useEMA;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period", ParameterTypes.Int32, 20);
            AddParameter("Deviations", ParameterTypes.Double, 2.0);
			AddParameter("Use EMA", ParameterTypes.Boolean, false);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Double deviations = Parameters[2].AsDouble;
			Boolean useEMA = Parameters[3].AsBoolean;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0 || ds.Count < period)
                return;

			TimeSeries sma = FastSMA.Series(ds, period);
            TimeSeries ema = EMA.Series(ds, period);
            TimeSeries ma = !useEMA ? sma : ema;
            ExpDev exd = new ExpDev(ds, period, useEMA);

            for (int bar = period; bar < ds.Count; bar++)
            {
                Values[bar] = ma[bar] + exd[bar] * deviations;
            }
        }

        public override string Name => "ExpDevBandUpper";

        public override string Abbreviation => "ExpDevBandUpper";

        public override string HelpDescription => "Upper Band of Exponential Deviation Bands from the July 2019 issue of Stocks & Commodities Magazine.";

        public override string PaneTag => "Price";

        public override Color DefaultColor => Color.Navy;

        //bands
        public override PlotStyles DefaultPlotStyle => PlotStyles.Bands;

        public override List<string> Companions => new List<string>() { "ExpDevBandLower" };
    }

    public class ExpDevBandLower : IndicatorBase
    {
        //parameterless constructor
        public ExpDevBandLower() : base()
        {
        }

        //for code based construction
        public ExpDevBandLower(TimeSeries source, Int32 period, Double deviations, Boolean useEMA)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = deviations;
			Parameters[3].Value = useEMA;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period", ParameterTypes.Int32, 20);
            AddParameter("Deviations", ParameterTypes.Double, 2.0);
			AddParameter("Use EMA", ParameterTypes.Boolean, false);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Double deviations = Parameters[2].AsDouble;
			Boolean useEMA = Parameters[3].AsBoolean;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0 || ds.Count < period)
                return;

			TimeSeries sma = FastSMA.Series(ds, period);
            TimeSeries ema = EMA.Series(ds, period);
            TimeSeries ma = !useEMA ? sma : ema;
            ExpDev exd = new ExpDev(ds, period, useEMA);

            for (int bar = period; bar < ds.Count; bar++)
            {
                Values[bar] = ma[bar] - exd[bar] * deviations;
            }
        }

        public override string Name => "ExpDevBandLower";

        public override string Abbreviation => "ExpDevBandLower";

        public override string HelpDescription => "Lower Band of Exponential Deviation Bands from the July 2019 issue of Stocks & Commodities Magazine.";

        public override string PaneTag => "Price";

        public override Color DefaultColor => Color.Navy;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Bands;

        public override List<string> Companions => new List<string>() { "ExpDevBandUpper" };
    }
}
