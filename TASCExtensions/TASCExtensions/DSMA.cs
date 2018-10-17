using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaIndicators;
using QuantaculaCore;

namespace TASCIndicators
{
    public class DSMA : IndicatorBase
    {
        //parameterless constructor
        public DSMA() : base()
        {
        }

        //for code based construction
        public DSMA(TimeSeries source, Int32 period)
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
            AddParameter("Period", ParameterTypes.Int32, 40);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = Math.Max(2, period);
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            TimeSeries Filt = new TimeSeries(DateTimes);

            //Smooth with a Super Smoother
            double Deg2Rad = Math.PI / 180.0;
            double a1 = Math.Exp(-1.414 * Math.PI / (0.5 * period));
            double b1 = 2.0 * a1 * Math.Cos((1.414 * 180d / (0.5 * period)) * Deg2Rad);
            double c2 = b1;
            double c3 = -a1 * a1;
            double c1 = 1 - c2 - c3;

            //Produce Nominal zero mean with zeros in the transfer response 
            //at DC and Nyquist with no spectral distortion
            //Nominally whitens the spectrum because of 6 dB per octave rolloff
            var Zeros = ds - (ds >> 2);

            for(int idx = 0; idx < ds.Count; idx++)
                Filt[idx] = 0d;

            //SuperSmoother Filter
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Filt[bar] = c1 * (Zeros[bar] + Zeros[bar - 1]) / 2d + c2 * Filt[bar - 1] + c3 * Filt[bar - 2];

                //Compute Standard Deviation
                double RMS = 0;
                for (int count = 0; count < period - 1; count++)
                {
                    if (bar > period)
                        RMS += Math.Pow(Filt[bar - count], 2);
                }
                RMS = Math.Sqrt(RMS / (double)period);

                //Rescale Filt in terms of Standard Deviations
                var ScaledFilt = Filt[bar] / RMS;
                var alpha1 = Math.Abs(ScaledFilt) * 5 / (double)period;

                if (bar > period)
                    Values[bar] = (alpha1 * ds[bar]) + (1 - alpha1) * Values[bar - 1];
                else
                    Values[bar] = 0d;
            }
        }

        public override bool IsSmoother => true;

        public override string Name => "DSMA";

        public override string Abbreviation => "DSMA";

        public override string HelpDescription => "Created by John Ehlers, the DSMA is an adaptive moving average that rapidly adapts to volatility in price movement.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.DarkViolet;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
