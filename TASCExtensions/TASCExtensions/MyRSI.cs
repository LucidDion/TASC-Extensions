using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class MyRSI : IndicatorBase
    {
        //parameterless constructor
        public MyRSI() : base()
        {
        }

        //for code based construction
        public MyRSI(TimeSeries source, Int32 period, Double k)
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
            AddParameter("Smoothing Length", ParameterTypes.Int32, 8);
            AddParameter("RSI Length", ParameterTypes.Int32, 10);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 smoothLength = Parameters[1].AsInt;
            Int32 rsiLength = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            var FirstValidValue = Math.Max(smoothLength, rsiLength) +  1;
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            if (FirstValidValue <= 0 || ds.Count == 0)
                return;

            var Filt = new TimeSeries(DateTimes);

            //Compute Super Smoother coefficients once
            double a1 = Math.Exp(-1.414 * Math.PI / smoothLength);
            double b1 = 2 * a1 * Math.Cos(1.414 * Math.PI / smoothLength);
            double c2 = b1;
            double c3 = -a1 * a1;
            double c1 = 1 - c2 - c3;

            //SuperSmoother Filter
            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar >= 2)
                    Filt[bar] = c1 * (ds[bar] + ds[bar - 1]) / 2 + c2 * Filt[bar - 1] + c3 * Filt[bar - 2];
                else
                    Filt[bar] = 0;
            }

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                //Accumulate “Closes Up” and “Closes Down”
                double CU = 0, CD = 0;
                for (int count = 0; count < rsiLength; count++)
                {
                    var diff = Filt[bar - count] - Filt[bar - count - 1];

                    if (diff > 0)
                        CU += diff;
                    if (diff < 0)
                        CD -= diff;
                }

                if ((CU + CD) != 0)
                    Values[bar] = (CU - CD) / (CU + CD);
            }            
        }

        public override string Name => "MyRSI";

        public override string Abbreviation => "MyRSI";

        public override string HelpDescription => "Created by John Ehlers (see article in May 2018 issue of Stocks and Commodities Magazine), MyRSI uses his SuperSmoother filter for smoothing an RSI-like oscillator.";

        public override string PaneTag => "MyRSI";

        public override Color DefaultColor => Color.Red;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
