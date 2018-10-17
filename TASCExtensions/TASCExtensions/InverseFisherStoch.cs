using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    /// <summary>
    /// InverseFisherStoch (Stochastic Inverse Fisher Transform Indicator, Vervoot)
    /// </summary>
    public class InverseFisherStoch : IndicatorBase
    {
        //parameterless constructor
        public InverseFisherStoch() : base()
        {
        }

        //for code based construction
        public InverseFisherStoch(TimeSeries source, Int32 stoPeriod, Int32 smooth)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = stoPeriod;
            Parameters[1].Value = smooth;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Stoch Period", ParameterTypes.Int32, 30);
            AddParameter("Smooth Period", ParameterTypes.Int32, 5);
        }

        // Stochastic D of a DataSeries (as opposed to Bars) per Vervoot
        private TimeSeries StochD(TimeSeries ds, int stoPer, int smooth)
        {
            var result = new TimeSeries(ds.DateTimes);
            var ll = new Lowest(ds, stoPer);
            var hh = new Highest(ds, stoPer);

            int start = stoPer + smooth;
            if (start >= ds.Count) start = ds.Count;

            for (int bar = 0; bar < start; bar++)
                result[bar] = 50;

            for (int bar = start; bar < ds.Count; bar++)
            {
                double s1 = 0;
                double s2 = 0;
                for (int n = 0; n < smooth; n++)
                {
                    double LL = ll[bar - n];
                    s1 = s1 + ds[bar - n] - LL;
                    s2 = s2 + hh[bar - n] - LL;
                }
                result[bar] = 100 * (s1 / (s2 + 0.0001));
            }
            return result;
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 stochPeriod = Parameters[1].AsInt;
            Int32 smoothingPeriod = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            if (stochPeriod <= 0 || smoothingPeriod <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = Math.Max(stochPeriod, smoothingPeriod);
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;
            

            // Assymetric Rainbow smoothing, more weight on recent samples			
            var ma = new WMA(ds, 2);
            int k = 5;            
            var rbw = k * ma;

            for (int w = 4; w > -5; w--)
            {
                ma = new WMA(ma, 2);
                k = w > 0 ? w : 1;
                var kSer = k * ma;
                rbw += kSer;
            }
            rbw /= 20d;
            //rbw.Description = "StochRainbow";
            var rbwStoch = StochD(rbw, stochPeriod, smoothingPeriod);
            
            var x = 0.1 * (rbwStoch - 50);

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                double e2y = Math.Exp(2 * x[bar]);
                Values[bar] = 50 * ((e2y - 1) / (e2y + 1) + 1);       // with scaling                
            }
        }

        public override string Name => "InverseFisherStoch";

        public override string Abbreviation => "InverseFisherStoch";

        public override string HelpDescription => "InverseFisherStoch is Sylvain Vervoort's Inverse Fisher Stochastic of a DataSeries used in the November 2011 issue of TASC Magazine.";

        public override string PaneTag => @"InverseFisherStoch";

        public override Color DefaultColor => Color.Blue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }
}