using System;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class Trendflex : IndicatorBase
    {
        //parameterless constructor
        public Trendflex() : base()
        {
        }

        //for code based construction
        public Trendflex(TimeSeries ds, Int32 period)
            : base()
        {
            Parameters[0].Value = ds;
            Parameters[1].Value = period;
            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("TimeSeries", ParameterTypes.TimeSeries, PriceComponents.Close);
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

            var FirstValidValue = period + 2;
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            TimeSeries Filt = new TimeSeries(ds.DateTimes, 0.0);
            TimeSeries Slope = new TimeSeries(ds.DateTimes, 0.0);
            TimeSeries _tf = new TimeSeries(ds.DateTimes, 0.0);
            TimeSeries MS = new TimeSeries(ds.DateTimes, 0.0);

            //Gently smooth the data in a SuperSmoother
            double Deg2Rad = Math.PI / 180.0;
            double a1 = Math.Exp(-1.414 * Math.PI / (0.5 * period));
            double b1 = 2.0 * a1 * Math.Cos((1.414 * 180d / (0.5 * period)) * Deg2Rad);
            double c2 = b1;
            double c3 = -a1 * a1;
            double c1 = 1 - c2 - c3;

            for (int i = 0; i < FirstValidValue; i++)
            {
                Values[i] = 0;
                Filt[i] = 0;
            }

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Filt[bar] = c1 * (ds[bar] + ds[bar - 1]) / 2d + c2 * Filt[bar - 1] + c3 * Filt[bar - 2];

                //Sum the differences
                double Sum = 0;
                for (int count = 1; count <= period; count++)
                {
                    //Sum = Sum + Filt - Filt[count];
                    Sum = Sum + Filt[bar] - Filt[bar - count];
                }
                Sum /= period;

                //Normalize in terms of Standard Deviations
                MS[bar] = .04 * Sum * Sum + .96 * MS[bar - 1];
                if (MS[bar] != 0)
                    _tf[bar] = Sum / Math.Sqrt(MS[bar]);

                Values[bar] = _tf[bar];
            }
        }

        public override string Name => "Trendflex";

        public override string Abbreviation => "Trendflex";

        public override string HelpDescription => "The Trendflex by John Ehlers from February 2020 issue of Stocks & Commodities";

        public override string PaneTag => @"Reflex";

        public override Color DefaultColor => Color.Red;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}