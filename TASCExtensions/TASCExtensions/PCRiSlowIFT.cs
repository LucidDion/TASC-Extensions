using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    /// <summary>
    /// PCRiSlowIFT (Slow Put/Call Ratio Indicator)
    /// </summary>
    public class PCRiSlowIFT : IndicatorBase
    {
        //parameterless constructor
        public PCRiSlowIFT() : base()
        {
        }

        //for code based construction
        public PCRiSlowIFT(TimeSeries source, Int32 rainbowPeriod, Int32 wmaPeriod, Int32 rsiPeriod)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = rainbowPeriod;
            Parameters[2].Value = wmaPeriod;
            Parameters[3].Value = rsiPeriod;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Raw P/C Ratio", ParameterTypes.TimeSeries, PriceComponents.Close);   //raw Put/Call Ratio of Equities
            AddParameter("Rainbow Period", ParameterTypes.Int32, 4);
            AddParameter("WMA Smooth Period", ParameterTypes.Int32, 2);
            AddParameter("RSI Period", ParameterTypes.Int32, 8);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 rainbowPeriod = Parameters[1].AsInt;
            Int32 wmaSmoothingPeriod = Parameters[2].AsInt;
            Int32 rsiPeriod = Parameters[3].AsInt;

            DateTimes = ds.DateTimes;
            int period = Math.Max(Math.Max(rainbowPeriod, wmaSmoothingPeriod), rsiPeriod);

            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = Math.Max(rainbowPeriod * 3, wmaSmoothingPeriod);
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            // Clip the raw Put/Call Ratio data
            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (ds[bar] > 0.9) ds[bar] = 0.9;
                else if (ds[bar] < 0.45) ds[bar] = 0.45;
            }

            // Rainbow smoothing			
            var rbw = new TimeSeries(DateTimes);
            var ma = new WMA(ds, rainbowPeriod);

            for (int w = 1; w <= 9; w++)
            {
                ma = new WMA(ma, rainbowPeriod);
                rbw += ma;
            }
            rbw /= 10d;
            var pcri = new WMA(rbw, wmaSmoothingPeriod);
            var x = 0.1 * (new RSI(pcri, rsiPeriod) - 50);

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                double e2y = Math.Exp(2 * x[bar]);
                Values[bar] = 50 * ((e2y - 1) / (e2y + 1) + 1);       // with scaling                
            }
        }

        public override string Name => "PCRiSlowIFT";

        public override string Abbreviation => "PCRiSlowIFT";

        public override string HelpDescription => "PCRiSlowIFT is the Slow Put/Call Ratio indicator Inverse Fisher Transform from the November 2011 issue of TASC Magazine.  Use the CBOE Provider to pass the raw Put/Call Ratio for Equities as the DataSeries input.";

        public override string PaneTag => "PCRiSlowIFT";

        public override Color DefaultColor => Color.Blue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;

    }
}