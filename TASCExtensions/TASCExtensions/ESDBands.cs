using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class EStdDev : IndicatorBase
    {
        //parameterless constructor
        public EStdDev() : base()
        {
        }

        //for code based construction
        public EStdDev(TimeSeries source, Int32 period)
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
            AddParameter("Length", ParameterTypes.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0 || ds.Count < period)
                return;

            var weight = 2d / (period + 1);
            var esd = new TimeSeries(DateTimes);
            var ema = new TimeSeries(DateTimes);

            esd[period - 1] = 0d;
            ema[period - 1] = 0d;

            //Based on formula by Alex Matulich
            //http://unicorn.us.com/trading/src/_xStdDev.txt
            //http://unicorn.us.com/trading/xstdev2.xls

            for (int bar = period; bar < ds.Count; bar++)
            {
                ema[bar] = weight * ds[bar] + (1 - weight) * ema[bar - 1];
                esd[bar] = Math.Sqrt(weight * (ds[bar] - ema[bar]) * (ds[bar] - ema[bar]) + (1d - weight) * esd[bar - 1] * esd[bar - 1]);
                Values[bar] = esd[bar];
            }
		}

        public override string Name => "EStdDev";

        public override string Abbreviation => "EStdDev";

        public override string HelpDescription => "Exponential standard deviation from the February 2017 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => "ESDBandsPane";

        public override Color DefaultColor => Color.Navy;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }

 
    public class ESDBandUpper : IndicatorBase
    {
        //parameterless constructor
        public ESDBandUpper() : base()
        {
        }

        //for code based construction
        public ESDBandUpper(TimeSeries source, Int32 period, Double deviations)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = deviations;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Length", ParameterTypes.Int32, 20);
            AddParameter("Deviations", ParameterTypes.Double, 2.0);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Double deviations = Parameters[2].AsDouble;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0 || ds.Count < period)
                return;

            var ema = new EMA(ds, 20);
            var esd = new EStdDev(ds, period);

            for (int bar = period; bar < ds.Count; bar++)
            {
                Values[bar] = ema[bar] + esd[bar] * deviations;
            }
        }

        public override string Name => "ESDBandUpper";

        public override string Abbreviation => "ESDBandUpper";

        public override string HelpDescription => "Upper Band of Exponential Standard Deviation Bands from the February 2017 issue of Stocks & Commodities Magazine.";

        public override string PaneTag => "Price";

        public override Color DefaultColor => Color.Navy;

        //bands
        public override PlotStyles DefaultPlotStyle => PlotStyles.Bands;

        public override List<string> Companions => new List<string>() { "ESDBandLower" };
    }

    public class ESDBandLower : IndicatorBase
    {
        //parameterless constructor
        public ESDBandLower() : base()
        {
        }

        //for code based construction
        public ESDBandLower(TimeSeries source, Int32 period, Double deviations)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = deviations;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Length", ParameterTypes.Int32, 20);
            AddParameter("Deviations", ParameterTypes.Double, 2.0);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Double deviations = Parameters[2].AsDouble;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0 || ds.Count < period)
                return;

            var ema = new EMA(ds, 20);
            var esd = new EStdDev(ds, period);

            for (int bar = period; bar < ds.Count; bar++)
            {
                Values[bar] = ema[bar] - esd[bar] * deviations;
            }
        }

        public override string Name => "ESDBandLower";

        public override string Abbreviation => "ESDBandLower";

        public override string HelpDescription => "Lower Band of Exponential Standard Deviation Bands from the February 2017 issue of Stocks & Commodities Magazine.";

        public override string PaneTag => "Price";

        public override Color DefaultColor => Color.Navy;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Bands;

        public override List<string> Companions => new List<string>() { "ESDBandUpper" };
    }
}
