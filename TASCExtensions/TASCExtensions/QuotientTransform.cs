using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class QuotientTransform : IndicatorBase
    {
        //parameterless constructor
        public QuotientTransform() : base()
        {
        }

        //for code based construction
        public QuotientTransform(TimeSeries source, Int32 period, Double k)
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
            AddParameter("Low-pass Period", ParameterTypes.Int32, 30);
            AddParameter("Quotient K", ParameterTypes.Double, 0.85);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 LPPeriod = Parameters[1].AsInt;
            Double K = Parameters[2].AsDouble;

            DateTimes = ds.DateTimes;

            if (LPPeriod <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = 2;
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            var HP = new TimeSeries(DateTimes);
            var Filt = new TimeSeries(DateTimes);
            var _Peak = new TimeSeries(DateTimes);

            //Highpass filter cyclic components whose periods are shorter than 100 bars
            double Deg2Rad = Math.PI / 180.0;
            double cosInDegrees = Math.Cos((.707 * 360 / 100d) * Deg2Rad);
            double sinInDegrees = Math.Sin((.707 * 360 / 100d) * Deg2Rad);
            double alpha1 = (cosInDegrees + sinInDegrees - 1) / cosInDegrees; 

            //a1 = expvalue(-1.414*3.14159 / LPPeriod);
            //b1 = 2*a1*Cosine(1.414*180 / LPPeriod);
            double a1 = Math.Exp(-1.414 * Math.PI / (double)LPPeriod);
            //double b1 = 2.0 * a1 * Math.Cos(1.414 * 180d / (double)LPPeriod);
            double b1 = 2.0 * a1 * Math.Cos((1.414 * 180d / (double)LPPeriod) * Deg2Rad);
            double c2 = b1;
            double c3 = -a1 * a1;
            double c1 = 1 - c2 - c3;
            double X = 0;

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar >= 2)
                {
                    HP[bar] = (1 - alpha1 / 2) * (1 - alpha1 / 2) * (ds[bar] - 2 * ds[bar - 1] + ds[bar - 2]) + 2 * (1 - alpha1) * HP[bar - 1] - (1 - alpha1) * (1 - alpha1) * HP[bar - 2];

                    //SuperSmoother Filter
                    Filt[bar] = c1 * (HP[bar] + HP[bar - 1]) / 2 + c2 * Filt[bar - 1] + c3 * Filt[bar - 2];

                    //Fast Attack - Slow Decay Algorithm
                    _Peak[bar] = .991 * _Peak[bar - 1];
                    if (Math.Abs(Filt[bar]) > _Peak[bar])
                        _Peak[bar] = Math.Abs(Filt[bar]);

                    //Normalized Roofing Filter
                    if (_Peak[bar] != 0)
                        X = Filt[bar] / _Peak[bar];
                    Values[bar] = (X + K) / (K * X + 1);
                }
                else
                {
                    Filt[bar] = 0d; _Peak[bar] = 0d; HP[bar] = 0; Values[bar] = 0d;
                }                
            }
        }

        public override string Name => "QuotientTransform";

        public override string Abbreviation => "QuotientTransform";

        public override string HelpDescription => "Created by John Ehlers (see article in August 2014 issue of Stocks and Commodities Magazine), the Quotient Transform is a zero lag filter that can be used to provide an early-onset identification of a trend.";

        public override string PaneTag => @"QuotientTransform";

        public override Color DefaultColor => Color.DarkOrange;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
