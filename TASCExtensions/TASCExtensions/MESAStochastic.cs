using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class MESAStochastic : IndicatorBase
    {
        //parameterless constructor
        public MESAStochastic() : base()
        {
        }

        //for code based construction
        public MESAStochastic(TimeSeries source, Int32 period)
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

            var HP = new TimeSeries(DateTimes);
            var RoofingFilter = new TimeSeries(DateTimes);
            var Stoc = new TimeSeries(DateTimes);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                HP[bar] = 0d; RoofingFilter[bar] = 0d; Stoc[bar] = 0d;
            }

            double Deg2Rad = Math.PI / 180.0;
            double cosInDegrees = Math.Cos((.707 * 360 / 48d) * Deg2Rad);
            double sinInDegrees = Math.Sin((.707 * 360 / 48d) * Deg2Rad);
            double alpha1 = (cosInDegrees + sinInDegrees - 1) / cosInDegrees; 

            double a1 = Math.Exp(-1.414 * Math.PI / 10d);
            //double b1 = 2.0 * a1 * Math.Cos(1.414 * 180d / 10d);
            double b1 = 2.0 * a1 * Math.Cos((1.414 * 180d / 10d) * Deg2Rad);
            double c2 = b1;
            double c3 = -a1 * a1;
            double c1 = 1 - c2 - c3;

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar < period + 2)
                {
                    Values[bar] = 0d;
                }
                else
                {
                    //Highpass filter cyclic components whose periods are shorter than 48 bars
                    double hp = (1d - alpha1 / 2d) * (1d - alpha1 / 2d) * (ds[bar] - 2 * ds[bar - 1] + ds[bar - 2]);
                    double hp1 = 2 * (1d - alpha1) * HP[bar - 1];
                    double hp2 = (1d - alpha1) * (1d - alpha1) * HP[bar - 2];
                    HP[bar] = hp + hp1 - hp2;

                    //Smooth with a Super Smoother Filter 
                    double Filt2 = c1 * (HP[bar] + HP[bar - 1]) / 2d + c2 * RoofingFilter[bar - 1] + c3 * RoofingFilter[bar - 2];
                    RoofingFilter[bar] = Filt2;

                    double HighestC = Filt2;
                    double LowestC = Filt2;

                    for (int count = bar - period + 1; count <= bar; count++)
                    {
                        if (RoofingFilter[count] > HighestC)
                            HighestC = RoofingFilter[count];
                        if (RoofingFilter[count] < LowestC)
                            LowestC = RoofingFilter[count];
                    }

                    Stoc[bar] = (RoofingFilter[bar] - LowestC) / (HighestC - LowestC);
                    Values[bar] = c1 * (Stoc[bar] + Stoc[bar - 1]) / 2 + c2 * Values[bar - 1] + c3 * Values[bar - 2];
                }
            }
        }

        public override double OverboughtLevel => 0.8;

        public override double OversoldLevel => 0.2;

        public override string Name => "MESAStochastic";

        public override string Abbreviation => "MESAStochastic";

        public override string HelpDescription => "Created by John Ehlers (see article in January 2014 issue of Stocks and Commodities Magazine), the MESAStochastic oscillator is a Stochastic successor with a roofing filter applied to remove aliasing noise and spectral dilation.";

        public override string PaneTag => @"MESAStochastic";

        public override Color DefaultColor => Color.DarkRed;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }    
}
