using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class DecyclerOscillator : IndicatorBase
    {
        //parameterless constructor
        public DecyclerOscillator() : base()
        {
        }

        //for code based construction
        public DecyclerOscillator(TimeSeries source, Int32 period, Double k)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = k;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period", ParameterTypes.Int32, 125);
            AddParameter("K", ParameterTypes.Double, 3);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Double K = Parameters[2].AsDouble;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;
            
            var FirstValidValue = period;
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            SimpleDecycler sd = new SimpleDecycler(ds, period);
            double Deg2Rad = Math.PI / 180.0;
            double cosInDegrees = Math.Cos((.707 * 360 / (0.5*period)) * Deg2Rad);
            double sinInDegrees = Math.Sin((.707 * 360 / (0.5*period)) * Deg2Rad);
            double alpha2 = (cosInDegrees + sinInDegrees - 1) / cosInDegrees;

            var DecyclerOsc = new TimeSeries(DateTimes);
            for (int bar = 0; bar < ds.Count; bar++)
            //for (int bar = period; bar < ds.Count; bar++)
            {
                DecyclerOsc[bar] = 0;
            }

            //Take a HighPass filter of Decycle to create the Decycleosc
            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar >= period + 2)
                    DecyclerOsc[bar] = (1 - alpha2 / 2) * (1 - alpha2 / 2) * (sd[bar] - 2 * sd[bar - 1] + sd[bar - 2]) + 2 * (1 - alpha2) *
                        DecyclerOsc[bar - 1] - (1 - alpha2) * (1 - alpha2) * DecyclerOsc[bar - 2];
                else
                {
                    DecyclerOsc[bar] = 0d; Values[bar] = 0d; sd[bar] = 0d;
                }                    
            }

            for (int bar = period; bar < ds.Count; bar++)
            {
                Values[bar] = 100 * K * DecyclerOsc[bar] / ds[bar];
            }            
        }




        public override string Name => "DecyclerOscillator";

        public override string Abbreviation => "DecyclerOscillator";

        public override string HelpDescription => @"Created by John Ehlers (see article in September 2015 issue of Stocks and Commodities Magazine), the Decycler Oscillator designed to indicate trend with virtually zero lag.";

        public override string PaneTag => @"DecyclerOscillator";

        public override Color DefaultColor => Color.DarkRed;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
