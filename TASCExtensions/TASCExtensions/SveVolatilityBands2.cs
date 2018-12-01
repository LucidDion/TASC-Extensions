using System;
using System.Collections.Generic;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class SVESmoothedVolatilityBandUpper : IndicatorBase
    {
        public override string Name => "SVESmoothedVolatilityBandUpper";

        public override string Abbreviation => "SVESmoothedVolatilityBandUpper";

        public override string HelpDescription => "Upper Band of Smoothed Volaitility Bands from the January 2019 issue of Stocks & Commodities Magazine.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.Silver;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Bands;

        public override List<string> Companions => new List<string>() { "SVESmoothedVolatilityBandLower" };

        //parameterless constructor
        public SVESmoothedVolatilityBandUpper() : base()
        {
        }

        //for code based construction
        public SVESmoothedVolatilityBandUpper(BarHistory bars, int bandAverage, int volSumPeriod, double devFactor, double lowBandAdj)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = bandAverage;
            Parameters[2].Value = volSumPeriod;
            Parameters[3].Value = devFactor;
            Parameters[4].Value = lowBandAdj;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
            AddParameter("Band average", ParameterTypes.Int32, 20);
            AddParameter("Volatility Summing period", ParameterTypes.Int32, 20);
            AddParameter("Deviation factor", ParameterTypes.Double, 2.4);
            AddParameter("Low band adjustment", ParameterTypes.Double, 0.9);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 BandsPeriod = Parameters[1].AsInt;
            Int32 MiddlePeriod = Parameters[2].AsInt;
            Double DevFact = Parameters[3].AsDouble;
            Double LowBandAdjust = Parameters[4].AsDouble;

            DateTimes = bars.DateTimes;
            int period = Math.Max(BandsPeriod, MiddlePeriod);

            if (period <= 0 || bars.Count == 0)
                return;

            if (bars.Count < Math.Max(BandsPeriod, MiddlePeriod))
                return;

            var TypicalPrice = bars.AveragePriceHLC;
            TimeSeries typical = new TimeSeries(DateTimes);
            TimeSeries deviation = new TimeSeries(DateTimes);
            TimeSeries medianaverage = new TimeSeries(DateTimes);
            TimeSeries devhigh = new TimeSeries(DateTimes);
            TimeSeries devlow = new TimeSeries(DateTimes);
            TimeSeries UpperBand = new TimeSeries(DateTimes);

            // Use a price range
            var TempBuf = new TimeSeries(DateTimes);

            for (int i = 0; i < bars.Count; i++)
            {
                if (i == bars.Count - 1)
                    TempBuf[i] = bars.High[i] - bars.Low[i];
                else
                    TempBuf[i] = Math.Max(bars.High[i], bars.Close[i + 1]) - Math.Min(bars.Low[i], bars.Close[i + 1]);
            }

            // calculate price deviation
            var AtrBuf = new TimeSeries(DateTimes);
            AtrBuf = new SMA(TempBuf, BandsPeriod * 2 - 1) * DevFact;

            // Create upper, lower channel and middle line
            var HighChannel = new TimeSeries(DateTimes);
            var LowChannel = new TimeSeries(DateTimes);

            HighChannel = new WMA(bars.Close, BandsPeriod) + new WMA(bars.Close, BandsPeriod) * AtrBuf / bars.Close;
            //LowChannel = new WMA.Series(bars.Close, BandsPeriod) - new WMA.Series(bars.Close, BandsPeriod) * AtrBuf * LowBandAdjust / bars.Close;

            for (int bar = Math.Max(BandsPeriod, MiddlePeriod); bar < bars.Count; bar++)
            {
                Values[bar] = HighChannel[bar];
            }
        }
    }

    public class SVESmoothedVolatilityBandLower : IndicatorBase
    {
        public override string Name => "SVESmoothedVolatilityBandLower";

        public override string Abbreviation => "SVESmoothedVolatilityBandLower";

        public override string HelpDescription => "Lower Band of Smoothed Volaitility Bands from the January 2019 issue of Stocks & Commodities Magazine.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.Silver;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Bands;

        public override List<string> Companions => new List<string>() { "SVESmoothedVolatilityBandUpper" };

        //parameterless constructor
        public SVESmoothedVolatilityBandLower() : base()
        {
        }

        //for code based construction
        public SVESmoothedVolatilityBandLower(BarHistory bars, int bandAverage, int volSumPeriod, double devFactor, double lowBandAdj)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = bandAverage;
            Parameters[2].Value = volSumPeriod;
            Parameters[3].Value = devFactor;
            Parameters[4].Value = lowBandAdj;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
            AddParameter("Band average", ParameterTypes.Int32, 20);
            AddParameter("Volatility Summing period", ParameterTypes.Int32, 20);
            AddParameter("Deviation factor", ParameterTypes.Double, 2.4);
            AddParameter("Low band adjustment", ParameterTypes.Double, 0.9);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 BandsPeriod = Parameters[1].AsInt;
            Int32 MiddlePeriod = Parameters[2].AsInt;
            Double DevFact = Parameters[3].AsDouble;
            Double LowBandAdjust = Parameters[4].AsDouble;

            DateTimes = bars.DateTimes;
            int period = Math.Max(BandsPeriod, MiddlePeriod);

            if (period <= 0 || bars.Count == 0)
                return;

            if (bars.Count < Math.Max(BandsPeriod, MiddlePeriod))
                return;

            var TypicalPrice = bars.AveragePriceHLC;
            TimeSeries typical = new TimeSeries(DateTimes);
            TimeSeries deviation = new TimeSeries(DateTimes);
            TimeSeries medianaverage = new TimeSeries(DateTimes);
            TimeSeries devhigh = new TimeSeries(DateTimes);
            TimeSeries devlow = new TimeSeries(DateTimes);
            TimeSeries UpperBand = new TimeSeries(DateTimes);

            // Use a price range
            var TempBuf = new TimeSeries(DateTimes);

            for (int i = 0; i < bars.Count; i++)
            {
                if (i == bars.Count - 1)
                    TempBuf[i] = bars.High[i] - bars.Low[i];
                else
                    TempBuf[i] = Math.Max(bars.High[i], bars.Close[i + 1]) - Math.Min(bars.Low[i], bars.Close[i + 1]);
            }

            // calculate price deviation
            var AtrBuf = new TimeSeries(DateTimes);
            AtrBuf = new SMA(TempBuf, BandsPeriod * 2 - 1) * DevFact;

            // Create upper, lower channel and middle line
            var HighChannel = new TimeSeries(DateTimes);
            var LowChannel = new TimeSeries(DateTimes);

            //HighChannel = new WMA(bars.Close, BandsPeriod) + new WMA(bars.Close, BandsPeriod) * AtrBuf / bars.Close;
            LowChannel = new WMA(bars.Close, BandsPeriod) - new WMA(bars.Close, BandsPeriod) * AtrBuf * LowBandAdjust / bars.Close;

            for (int bar = Math.Max(BandsPeriod, MiddlePeriod); bar < bars.Count; bar++)
            {
                Values[bar] = LowChannel[bar];
            }
        }
    }

}