using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //FRAMA Indicator class
    public class FRAMA : IndicatorBase
    {
        //parameterless constructor
        public FRAMA() : base()
        {
        }

        //for code based construction
        public FRAMA(TimeSeries source, Int32 period, Double k)
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
            AddParameter("Period", ParameterTypes.Int32, 20);
            AddParameter("K", ParameterTypes.Double, 4.6);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Double k = Parameters[2].AsDouble;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            var halfperiod = period / 2;
            var HH = new Highest(ds, halfperiod);            
            var LL = new Lowest(ds, halfperiod);
            var Log2 = Math.Log(2);

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + 2 * halfperiod - 1;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series
            for (int bar = 0; bar < FirstValidValue; bar++)
                Values[bar] = ds[bar];

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                //First half 
                double hh1 = HH[bar - halfperiod];
                double ll1 = LL[bar - halfperiod];
                double n1 = (hh1 - ll1) / halfperiod;
    
                //Second half 
                double hh2 = HH[bar];
                double ll2 = LL[bar];
                double n2 = (hh2 - ll2) / halfperiod;
    
                //Complete interval 
                double hh3 = Math.Max(hh1, hh2);
                double ll3 = Math.Min(ll1, ll2);
                double n3 = (hh3 - ll3) / (2 * halfperiod);
             
                //Set Dimension variable 
                double n = n1 + n2;
                double Dimen = 1;
                if (n > 0) Dimen = Math.Log(n / n3) / Log2; // always less than 2
             
                //Formula for exp. moving average
                if (Dimen < 1) Dimen = 1;
                double alpha = Math.Exp(-k * (Dimen - 1));
                
                Values[bar] = alpha * ds[bar] + (1 - alpha) * Values[bar - 1];
            }
        }
        
        public override bool IsSmoother => true;

        public override string Name => "FRAMA";

        public override string Abbreviation => "FRAMA";

        public override string HelpDescription => "Fractal Adaptive Moving Average presented by John Ehlers in October issue of the Stocks and Commodities Magazine.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}
