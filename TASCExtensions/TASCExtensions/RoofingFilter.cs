using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class RoofingFilter : IndicatorBase
    {
        //parameterless constructor
        public RoofingFilter() : base()
        {
        }

        //for code based construction
        public RoofingFilter(TimeSeries source)
            : base()
        {
            Parameters[0].Value = source;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            DateTimes = ds.DateTimes;

            var FirstValidValue = 2;
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            if (ds.Count < 2)
                return;

            var HP = new TimeSeries(DateTimes);

            double Deg2Rad = Math.PI / 180.0;
            double cosInDegrees = Math.Cos((.707 * 360 / 48d) * Deg2Rad);
            double sinInDegrees = Math.Sin((.707 * 360 / 48d) * Deg2Rad);
            double alpha1 = (cosInDegrees + sinInDegrees - 1) / cosInDegrees; 

            double a1 = Math.Exp(-1.414 * Math.PI / 10d);
            //double b1 = 2.0 * a1 * Math.Cos(1.414 * 180d / 10d);
            double b1 = 2.0 * a1 * Math.Cos((1.414 * 180d / 10d)* Deg2Rad);
            double c2 = b1;
            double c3 = -a1 * a1;
            double c1 = 1 - c2 - c3;

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar > 1)
                {
                    //Highpass filter cyclic components whose periods are shorter than 48 bars
                    double hp = (1d - alpha1 / 2d) * (1d - alpha1 / 2d) * (ds[bar] - 2 * ds[bar - 1] + ds[bar - 2]);
                    double hp1 = 2 * (1d - alpha1) * HP[bar - 1];
                    double hp2 = (1d - alpha1) * (1d - alpha1) * HP[bar - 2];
                    HP[bar] = hp + hp1 - hp2;

                    //Smooth with a Super Smoother Filter 
                    double Filt2 = c1 * (HP[bar] + HP[bar - 1]) / 2d + c2 * Values[bar - 1] + c3 * Values[bar - 2];
                    Values[bar] = Filt2;
                }
                else
                {
                    HP[bar] = Values[bar] = 0d;
                }
            }
        }

        public override string Name => "RoofingFilter";

        public override string Abbreviation => "RoofingFilter";

        public override string HelpDescription => "Rapid RSI Indicator, from Ian Copsey's article in the October 2006 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"RoofingFilter";

        public override Color DefaultColor => Color.DarkBlue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }    
}
