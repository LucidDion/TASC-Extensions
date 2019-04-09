using System;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCExtensions
{
    public class FourierSeries : IndicatorBase
    {
        //parameterless constructor
        public FourierSeries() : base()
        {
        }

        //for code based construction
        public FourierSeries(TimeSeries ds, Int32 period)
            : base()
        {
            Parameters[0].Value = ds;
            Parameters[1].Value = period;

            Populate();
        }

        //static Series method
        public static FourierSeries Series(TimeSeries ds, Int32 period)
        {
            return new FourierSeries(ds, period);
        }

        public override string Name => "FourierSeries";

        public override string Abbreviation => "FourierSeries";

        public override string HelpDescription => "Created by John Ehlers (see article in June 2019 issue of Stocks and Commodities Magazine), the Fourier series indicator represents market activity based on analysis principles established by J.M. Hurst.";

        public override string PaneTag => @"FourierSeries";

        public override Color DefaultColor => Color.Blue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            int FirstValidValue = period + 2;
            if (period <= 0 || ds.Count < FirstValidValue)
            {
                return;
            }

            double Bandwidth = 0.1;
            double G1 = 0, S1 = 0, L1 = 0, G2 = 0, S2 = 0, L2 = 0, G3 = 0, S3 = 0, L3 = 0;
            TimeSeries BP1 = new TimeSeries(DateTimes), BP2 = new TimeSeries(DateTimes), BP3 = new TimeSeries(DateTimes),
                Q1 = new TimeSeries(DateTimes), Q2 = new TimeSeries(DateTimes), Q3 = new TimeSeries(DateTimes),
                P1 = new TimeSeries(DateTimes), P2 = new TimeSeries(DateTimes), P3 = new TimeSeries(DateTimes);
            double Deg2Rad = Math.PI / 180.0;

            BP1.Description = string.Format("BP1({0},{1})", ds, period);
            BP2.Description = string.Format("BP2({0},{1})", ds, period);
            BP3.Description = string.Format("BP3({0},{1})", ds, period);

            Q1.Description = string.Format("Q1({0},{1})", ds, period);
            Q2.Description = string.Format("Q2({0},{1})", ds, period);
            Q3.Description = string.Format("Q3({0},{1})", ds, period);

            P1.Description = string.Format("P1({0},{1})", ds, period);
            P2.Description = string.Format("P2({0},{1})", ds, period);
            P3.Description = string.Format("P3({0},{1})", ds, period);

            //compute filter coefficients once
            L1 = Math.Cos(360d / period * Deg2Rad);
            G1 = Math.Cos(Bandwidth * 360 / period * Deg2Rad);
            S1 = 1d / G1 - Math.Sqrt(1d / (G1 * G1) - 1);
            L2 = Math.Cos(360d / (period / 2d) * Deg2Rad);
            G2 = Math.Cos(Bandwidth * 360 / (period / 2d) * Deg2Rad);
            S2 = 1d / G2 - Math.Sqrt(1d / (G2 * G2) - 1);
            L3 = Math.Cos(360 / (period / 3d) * Deg2Rad);
            G3 = Math.Cos(Bandwidth * 360 / (period / 3d) * Deg2Rad);
            S3 = 1d / G3 - Math.Sqrt(1d / (G3 * G3) - 1);

            for (int i = 0; i < FirstValidValue; i++)
            {
                BP1[i] = BP2[i] = BP3[i] = 0;
                Q1[i] = Q2[i] = Q3[i] = 0;
                P1[i] = P2[i] = P3[i] = 0;
            }

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                //Fundamental Band-Pass
                if (bar >= 2)
                    BP1[bar] = .5 * (1 - S1) * (ds[bar] - ds[bar - 2]) + L1 * (1 + S1) * BP1[bar - 1] - S1 * BP1[bar - 2];
                if (bar <= 3)
                {
                    BP1[bar] = 0;
                    BP2[bar] = 0;
                    BP3[bar] = 0;
                }
                //Fundamental Quadrature
                if (bar >= 1)
                    Q1[bar] = (period / 6.28) * (BP1[bar] - BP1[bar - 1]);
                if (bar <= 4)
                {
                    Q1[bar] = 0;
                    Q2[bar] = 0;
                    Q3[bar] = 0;
                }

                //Second Harmonic Band-Pass
                if (bar >= 2)
                    BP2[bar] = .5 * (1 - S2) * (ds[bar] - ds[bar - 2]) + L2 * (1 + S2) * BP2[bar - 1] - S2 * BP2[bar - 2];

                //Second Harmonic Quadrature
                if (bar >= 1)
                    Q2[bar] = (period / 6.28) * (BP2[bar] - BP2[bar - 1]);

                //Third Harmonic Band-Pass
                if (bar >= 2)
                    BP3[bar] = .5 * (1 - S3) * (ds[bar] - ds[bar - 2]) + L3 * (1 + S3) * BP3[bar - 1] - S3 * BP3[bar - 2];

                //Third Harmonic Quadrature
                if (bar >= 1)
                    Q3[bar] = (period / 6.28) * (BP3[bar] - BP3[bar - 1]);

                //Sum power of each harmonic at each bar over the Fundamental period
                P1[bar] = 0;
                P2[bar] = 0;
                P3[bar] = 0;
                for (int count = 0; count < period; count++)
                {
                    P1[bar] = P1[bar] + BP1[bar - count] * BP1[bar - count] + Q1[bar - count] * Q1[bar - count];
                    P2[bar] = P2[bar] + BP2[bar - count] * BP2[bar - count] + Q2[bar - count] * Q2[bar - count];
                    P3[bar] = P3[bar] + BP3[bar - count] * BP3[bar - count] + Q3[bar - count] * Q3[bar - count];
                }

                //Add the three harmonics together using their relative amplitudes
                if (P1[bar] != 0)
                    Values[bar] = BP1[bar] + Math.Sqrt(P2[bar] / P1[bar]) * BP2[bar] + Math.Sqrt(P3[bar] / P1[bar]) * BP3[bar];
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("TimeSeries", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Fundamental cycle period", ParameterTypes.Int32, 20);
        }
    }

    public class FourierCTS: IndicatorBase
    {
        //parameterless constructor
        public FourierCTS() : base()
        {
        }

        //for code based construction
        public FourierCTS(TimeSeries ds, Int32 period)
            : base()
        {
            Parameters[0].Value = ds;
            Parameters[1].Value = period;

            Populate();
        }

        //static Series method
        public static FourierCTS Series(TimeSeries ds, Int32 period)
        {
            return new FourierCTS(ds, period);
        }

        public override string Name => "FourierCTS";

        public override string Abbreviation => "FourierCTS";

        public override string HelpDescription => "Created by John Ehlers (see article in June 2019 issue of Stocks and Commodities Magazine), the Fourier series cyclic trading signal is the companion to the Fourier series indicator.";

        public override string PaneTag => @"FourierSeries";

        public override Color DefaultColor => Color.Red;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            int FirstValidValue = period + 2;
            if (period <= 0 || ds.Count < FirstValidValue)
            {
                return;
            }

            double Bandwidth = 0.1;
            double G1 = 0, S1 = 0, L1 = 0, G2 = 0, S2 = 0, L2 = 0, G3 = 0, S3 = 0, L3 = 0;
            TimeSeries Wave = new TimeSeries(DateTimes),
                BP1 = new TimeSeries(DateTimes), BP2 = new TimeSeries(DateTimes), BP3 = new TimeSeries(DateTimes),
                Q1 = new TimeSeries(DateTimes), Q2 = new TimeSeries(DateTimes), Q3 = new TimeSeries(DateTimes),
                P1 = new TimeSeries(DateTimes), P2 = new TimeSeries(DateTimes), P3 = new TimeSeries(DateTimes);
            double Deg2Rad = Math.PI / 180.0;

            BP1.Description = string.Format("BP1({0},{1})", ds, period);
            BP2.Description = string.Format("BP2({0},{1})", ds, period);
            BP3.Description = string.Format("BP3({0},{1})", ds, period);

            Q1.Description = string.Format("Q1({0},{1})", ds, period);
            Q2.Description = string.Format("Q2({0},{1})", ds, period);
            Q3.Description = string.Format("Q3({0},{1})", ds, period);

            P1.Description = string.Format("P1({0},{1})", ds, period);
            P2.Description = string.Format("P2({0},{1})", ds, period);
            P3.Description = string.Format("P3({0},{1})", ds, period);

            //compute filter coefficients once
            L1 = Math.Cos(360d / period * Deg2Rad);
            G1 = Math.Cos(Bandwidth * 360 / period * Deg2Rad);
            S1 = 1d / G1 - Math.Sqrt(1d / (G1 * G1) - 1);
            L2 = Math.Cos(360d / (period / 2d) * Deg2Rad);
            G2 = Math.Cos(Bandwidth * 360 / (period / 2d) * Deg2Rad);
            S2 = 1d / G2 - Math.Sqrt(1d / (G2 * G2) - 1);
            L3 = Math.Cos(360 / (period / 3d) * Deg2Rad);
            G3 = Math.Cos(Bandwidth * 360 / (period / 3d) * Deg2Rad);
            S3 = 1d / G3 - Math.Sqrt(1d / (G3 * G3) - 1);

            for (int i = 0; i < FirstValidValue; i++)
            {
                BP1[i] = BP2[i] = BP3[i] = 0;
                Q1[i] = Q2[i] = Q3[i] = 0;
                P1[i] = P2[i] = P3[i] = 0;
            }

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                //Fundamental Band-Pass
                if (bar >= 2)
                    BP1[bar] = .5 * (1 - S1) * (ds[bar] - ds[bar - 2]) + L1 * (1 + S1) * BP1[bar - 1] - S1 * BP1[bar - 2];
                if (bar <= 3)
                {
                    BP1[bar] = 0;
                    BP2[bar] = 0;
                    BP3[bar] = 0;
                }
                //Fundamental Quadrature
                if (bar >= 1)
                    Q1[bar] = (period / 6.28) * (BP1[bar] - BP1[bar - 1]);
                if (bar <= 4)
                {
                    Q1[bar] = 0;
                    Q2[bar] = 0;
                    Q3[bar] = 0;
                }

                //Second Harmonic Band-Pass
                if (bar >= 2)
                    BP2[bar] = .5 * (1 - S2) * (ds[bar] - ds[bar - 2]) + L2 * (1 + S2) * BP2[bar - 1] - S2 * BP2[bar - 2];

                //Second Harmonic Quadrature
                if (bar >= 1)
                    Q2[bar] = (period / 6.28) * (BP2[bar] - BP2[bar - 1]);

                //Third Harmonic Band-Pass
                if (bar >= 2)
                    BP3[bar] = .5 * (1 - S3) * (ds[bar] - ds[bar - 2]) + L3 * (1 + S3) * BP3[bar - 1] - S3 * BP3[bar - 2];

                //Third Harmonic Quadrature
                if (bar >= 1)
                    Q3[bar] = (period / 6.28) * (BP3[bar] - BP3[bar - 1]);

                //Sum power of each harmonic at each bar over the Fundamental period
                P1[bar] = 0;
                P2[bar] = 0;
                P3[bar] = 0;
                for (int count = 0; count < period; count++)
                {
                    P1[bar] = P1[bar] + BP1[bar - count] * BP1[bar - count] + Q1[bar - count] * Q1[bar - count];
                    P2[bar] = P2[bar] + BP2[bar - count] * BP2[bar - count] + Q2[bar - count] * Q2[bar - count];
                    P3[bar] = P3[bar] + BP3[bar - count] * BP3[bar - count] + Q3[bar - count] * Q3[bar - count];
                }

                //Add the three harmonics together using their relative amplitudes
                if (P1[bar] != 0)
                    Wave[bar] = BP1[bar] + Math.Sqrt(P2[bar] / P1[bar]) * BP2[bar] + Math.Sqrt(P3[bar] / P1[bar]) * BP3[bar];

                //Optional cyclic trading signal
                //Rate of change crosses zero at cyclic turning points
                Values[bar] = (period / 12.57) * (Wave[bar] - Wave[bar - 2]);
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("TimeSeries", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Fundamental cycle period", ParameterTypes.Int32, 20);
        }
    }
}
