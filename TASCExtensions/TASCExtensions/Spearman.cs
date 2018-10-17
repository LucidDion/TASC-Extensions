using System;
using System.Collections.Generic;
using System.Text;
using QuantaculaCore;
using System.Drawing;
using QuantaculaIndicators;

namespace TASCIndicators
{
    // Spearman Indicator
    public class Spearman : IndicatorBase
    {
        //parameterless constructor
        public Spearman() : base()
        {
        }

        //for code based construction
        public Spearman(TimeSeries source, Int32 period)
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
            AddParameter("Period", ParameterTypes.Int32, 10);
        }

        //(BarHistory bars, BarHistory barsFirst, BarHistory barsSecond, int periodRegression, int periodRegressionMomentum,
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            int n = period;

            var coefcorr = new TimeSeries(DateTimes);
            var sc = new TimeSeries(DateTimes);
            var r1 = new TimeSeries(DateTimes);
            var r11 = new TimeSeries(DateTimes);
            var r2 = new TimeSeries(DateTimes);
            var r21 = new TimeSeries(DateTimes);
            var r22 = new TimeSeries(DateTimes);

            for (int k = period; k < ds.Count; k++)
            {
                for (int i = n; i >= 1; i--)
                {
                    r1[i] = i;
                    r22[i] = i;
                    r11[i] = ds[k - n + i];
                    r21[i] = ds[k - n + i];
                }

                int changed = 1;
                while (changed > 0)
                {
                    changed = 0;
                    double temp = 0;
                    for (int i = 1; i <= (n - 1); i++)
                    {
                        if (r21[i + 1] < r21[i])
                        {
                            temp = r21[i];
                            r21[i] = r21[i + 1];
                            r21[i + 1] = temp;
                            changed = 1;
                        }
                    }
                }

                for (int i = 1; i <= n; i++)
                {
                    int found = 0;
                    while (found < 1)
                    {
                        for (int j = 1; j <= n; j++)
                        {
                            if (r21[j] == r11[i])
                            {
                                r22[i] = j;
                                found = 1;
                            }
                        }
                    }
                }

                double absum = 0; double ab = 0; double ab2 = 0;
                for (int i = 1; i <= n; i++)
                {
                    ab = r1[i] - r22[i];
                    ab2 = Math.Pow(ab, 2.0);
                    absum += ab2;
                }

                coefcorr[k] = (1 - (6 * absum) / (n * (n * n - 1)));
                sc[k] = 100 * coefcorr[k];
                Values[k] = sc[k];
            }
        }


        public override string Name => "Spearman";

        public override string Abbreviation => "Spearman";

        public override string HelpDescription => "Spearman correlation indicator by Dan Valcu is a statistical tool that helps determine trend strength and turning points.";

        public override string PaneTag => @"Spearman";

        public override Color DefaultColor => Color.Blue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;

        public override double OverboughtLevel => -80;

        public override double OversoldLevel => 80;
    }
}

