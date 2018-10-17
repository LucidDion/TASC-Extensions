using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //FractDim Indicator class
    public class FractDim : IndicatorBase
    {
        // Approximates D, the fractal dimension of a time series (or any wave that does not cross itself) by sampling Period points.
        // D = 1.5 = Brownian Motion
        // D = 1   = straight line
        // D = 2   = wave fills the plane
        // Biased, underestimates true D, but not much -- maybe 5%, estimate gets better as Period --> infinity
        
        private static double hypot(double a, double b)
        {
            return Math.Sqrt(a * a + b * b);
        }

        //parameterless constructor
        public FractDim() : base()
        {
        }

        //for code based construction
        public FractDim(TimeSeries source, Int32 period)
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
            AddParameter("Period", ParameterTypes.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            //Avoid exception errors
            if (period < 2 || period > ds.Count + 1) period = ds.Count + 1;
            var hh = new Highest(ds, period);
            var ll = new Lowest(ds, period);
            var ln2p = Math.Log(2 * (period - 1));

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + period - 1;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] := 0;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                double Range = hh[bar] - ll[bar];
                if (Range <= 0)
                    Values[bar] = 1;
                else
                {
                    // Calculate length
                    double L = 0;
                    for (int j = bar - period + 2; j <= bar; j++)
                    {
                        // Transform Y
                        double dY = ds[j] - ds[j - 1];
                        // Calculate Length - X is transformed to bars
                        L += hypot(dY / Range, 1.0 / (period - 1));
                    }
                    // Calculate Fractal Dimension Approximation
                    Values[bar] = 1 + Math.Log(L) / ln2p;
                }
            }
        }

          //This static method allows ad-hoc calculation of FractDim (single calc mode)
        public static double Calculate(int bar, TimeSeries ds, int period)
        {
            if (period < 2 || period > bar + 1) return 0;

            // Calculate length
            double Range = Highest.Calculate(bar, ds, period) - Lowest.Calculate(bar, ds, period);
            if (Range <= 0) return 0;
            
            double L = 0;
            for (int j = bar - period + 2; j <= bar; j++)
            {
                // Transform Y
                double dY = ds[j] - ds[j - 1];
                // Calculate Length - X is transformed to bars
                L += hypot(dY / Range, 1 / (period - 1));
            }
            
            // Calculate Fractal Dimension Approximation
            return 1 + Math.Log(L) / Math.Log(2 * (period - 1));
        }

        public override string Name => "FractDim";

        public override string Abbreviation => "FractDim";

        public override string HelpDescription => "The fractal dimension of a series over a time window. Period = 10 is a good compromise between accuracy and speed of processing.  See also FractDim, a different (more noisy) Fractal Dimension construction.";

        public override string PaneTag => @"FractDim";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }
}
