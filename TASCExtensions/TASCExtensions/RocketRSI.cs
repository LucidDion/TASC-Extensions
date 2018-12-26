using QuantaculaCore;
using QuantaculaIndicators;
using System;
using System.Drawing;

namespace TASCExtensions
{
    //Ehlers' RocketRSI - May 2018
    public class RocketRSI : IndicatorBase
    {
        //constructors
        public RocketRSI()
        {
            OverboughtLevel = 2;
            OversoldLevel = -2;
        }
        public RocketRSI(TimeSeries source, int smoothLegth, int rsiLength)
        {
            Parameters[0].Value = source;
            Parameters[1].Value = smoothLegth;
            Parameters[2].Value = rsiLength;
            OverboughtLevel = 2;
            OversoldLevel = -2;
            Populate();
        }

        //Name
        public override string Name
        {
            get
            {
                return "Rocket RSI";
            }
        }

        //abbreviation
        public override string Abbreviation
        {
            get
            {
                return "RocketRSI";
            }
        }

        //help description
        public override string HelpDescription
        {
            get
            {
                return "John Ehlers' Rocket RSI indicator from the May 2018 issue of Technical Analysis of Stocks & Commodities magazine.";
            }
        }

        //it's own pane
        public override string PaneTag
        {
            get
            {
                return "RocketRSI";
            }
        }

        //color
        public override Color DefaultColor
        {
            get
            {
                return Color.DarkOrange;
            }
        }

        //populate
        public override void Populate()
        {
            //obtain parameter values
            TimeSeries source = Parameters[0].AsTimeSeries;
            int smoothPeriod = Parameters[1].AsInt;
            int rsiPeriod = Parameters[2].AsInt;
            DateTimes = source.DateTimes;
            if (Count < smoothPeriod || Count < rsiPeriod)
                return;

            //super smooth coefficient
            double a1 = Math.Exp(-1.414 * Math.PI / smoothPeriod);
            double b1 = 1.414 * 180.0 / smoothPeriod;
            b1 = b1.ToRadians();
            b1 = 2.0 * a1 * Math.Cos(b1);
            double c2 = b1;
            double c3 = -a1 * a1;
            double c1 = 1.0 - c2 - c3;

            //create half dominant cycle momentum
            TimeSeries mom = source - (source >> rsiPeriod);

            //compute RocketRSI
            TimeSeries filt = new TimeSeries(source.DateTimes);
            for(int n = rsiPeriod + 1; n < source.Count; n++)
            {
                //supersmooth filter
                filt[n] = c1 * (mom[n] + mom[n - 1]) / 2.0;
                if (n > rsiPeriod + 1)
                    filt[n] += c2 * filt[n - 1];
                if (n > rsiPeriod + 2)
                    filt[n] += c3 * filt[n - 2];
                if (n <= rsiPeriod * 2)
                    continue;

                //accumulate closes up and closes down
                double cu = 0;
                double cd = 0;
                for(int r = 0; r < rsiPeriod; r++)
                {
                    int idx = n - r;
                    if (filt[idx] - filt[idx - 1] > 0)
                        cu += filt[idx] - filt[idx - 1];
                    if (filt[idx] - filt[idx - 1] < 0)
                        cd += filt[idx - 1] - filt[idx];
                }
                if (cu + cd != 0)
                    Values[n] = (cu - cd) / (cu + cd);
                else
                    Values[n] = 0;

                //limit Rocket RSI values to +/- 3 standard deviations
                if (Values[n] > 0.999)
                    Values[n] = 0.999;
                else if (Values[n] < -0.999)
                    Values[n] = -0.999;

                //apply fisher transform to establish gaussian probability distribution
                Values[n] = 0.5 * Math.Log((1.0 + Values[n]) / (1.0 - Values[n]));
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Smooth Period", ParameterTypes.Int32, 8);
            AddParameter("RSI Period", ParameterTypes.Int32, 10);
        }
    }
}