using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //PCI Indicator class
    public class PCI : IndicatorBase
    {
        //parameterless constructor
        public PCI() : base()
        {
        }

        //for code based construction
        public PCI(TimeSeries source, Int32 period)
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
            AddParameter("Period", ParameterTypes.Int32, 30);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (ds.Count == 0)
                return;

            //Avoid exception errors
            if (period < 1 || period > ds.Count) period = ds.Count;

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + period - 1;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] = 0;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                double dP = 0;
                double dM = 0;
                double f = ds[bar - (period - 1)];
                double m = (ds[bar] - f) / (period - 1);
                for (int k = 1; k < period - 1; k++)
                {
                    double Gradient = f + k * m;
                    double tmp = ds[bar - (period - 1) + k] - Gradient;
                    if (tmp > 0) dP += tmp; else dM -= tmp;
                }
                if (dP + dM > 0) 
                    Values[bar] = 100 * dP / (dP + dM); 
                //else 
                //    Values[bar] = 0;
            }
        }

        //This static method allows ad-hoc calculation of PCI (single calc mode)
        public static double Calculate(int bar, TimeSeries ds, int period)
        {
            if (period < 1 || period > ds.Count)
                return 0;

            double dP = 0;
            double dM = 0;
            double f = ds[bar - (period - 1)];
            double m = (ds[bar] - f) / (period - 1);

            for (int k = 1; k < period - 1; k++)
            {
                double Gradient = f + k * m;
                double tmp = ds[bar - (period - 1) + k] - Gradient;
                if (tmp > 0) dP += tmp; else dM -= tmp;
            }
            
            if (dP + dM <= 0) return 0;
            return 100 * dP / (dP + dM);
        }

        public override string Name => "PCI";

        public override string Abbreviation => "PCI";

        public override string HelpDescription => "Phase Change Index (by M.H.Pee) from the May 2004 issue of Stocks & Commodities magazine.";

        public override string PaneTag => "PCI";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
