using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    /// <summary>
    /// Coded by thodder (Tim Hodder)
    /// </summary>
    public class VFI : IndicatorBase
    {
        //parameterless constructor
        public VFI() : base()
        {
        }

        //for code based construction
        public VFI(BarHistory bars, BarHistory x, BarHistory y, Int32 period)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = x;
            Parameters[2].Value = y;
            Parameters[3].Value = period;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterTypes.BarHistory, null);
            AddParameter("Period", ParameterTypes.Int32, 130);
            AddParameter("EMA period", ParameterTypes.Int32, 3);
            AddParameter("Cutoff", ParameterTypes.Double, 0.2);
            AddParameter("Curtail coefficient", ParameterTypes.Double, 2.5);
        }

        //(BarHistory bars, BarHistory barsFirst, BarHistory barsSecond, int periodRegression, int periodRegressionMomentum,
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;
            Int32 emaPeriod = Parameters[2].AsInt;
            Double cutoffConst = Parameters[3].AsDouble;
            Double curtailCoeff = Parameters[4].AsDouble;

            DateTimes = bars.DateTimes;

            var FirstValidValue = Math.Max(period, emaPeriod) * 3;
            if (FirstValidValue > bars.Count || FirstValidValue < 0) FirstValidValue = bars.Count;

            if (FirstValidValue <= 0 || DateTimes.Count == 0)
                return;

            // Based on WL4 coding: http://wl4.wealth-lab.com/cgi-bin/WealthLab.DLL/libraryview?item=245
            var dsInter = new TimeSeries(DateTimes);
            dsInter[0] = 0d;

            // 'Typical' is the AveragePriceC.Series in WLP
            for (int bar = 1; bar < bars.Count; bar++)
            {
                dsInter[bar] = Math.Log(bars.AveragePriceHLC[bar]) - Math.Log(bars.AveragePriceHLC[bar - 1]);
            }

            // StdDev over 30-day time period (30 bars)
            dsInter = new StdDev(dsInter, 30);
            var dsCutoff = dsInter * cutoffConst * bars.Close;

            // dsAve = Average volume with 1 bar delay
            var dsAve = new FastSMA(bars.Volume, period) >> 1;
            var dsMax = dsAve * curtailCoeff;

            var dsMF = bars.AveragePriceHLC - (bars.AveragePriceHLC >> 1);
            var dsSer = new TimeSeries(DateTimes);
            var dsVFI = new TimeSeries(DateTimes);

            // Optimized summation over period.
            double PrevVfi = 0.0;
            for (int bar = 0; bar < bars.Count; bar++)
            {
                double Old = 0.0;
                if (bar > period)
                    Old = dsSer[bar - period - 1];

                double Value = Math.Min(bars.Volume[bar], dsMax[bar]);
                double New = 0.0;
                if (dsMF[bar] > dsCutoff[bar])
                {
                    New = Value;
                }
                else if (dsMF[bar] < (-1 * dsCutoff[bar]))
                {
                    New = -1 * Value;
                }

                dsSer[bar] = New;
                dsVFI[bar] = PrevVfi - Old + New;
                PrevVfi = dsVFI[bar];
            }

            dsVFI /= dsAve;

            // Smooth VFI and store in result series
            var dsEma = new EMA(dsVFI, emaPeriod);
            for (int bar = 0; bar < bars.Count; bar++)
            {
                Values[bar] = dsEma[bar];
            }
        }

        public override string Name => "VFI";

        public override string Abbreviation => "VFI";

        public override string HelpDescription => @"Created by Markos Katsanos, Volume Flow Indicator (VFI) (see article in July 2004 issue of Stocks and Commodities Magazine) is based on OBV but with three modifications.";

        public override string PaneTag => @"VFI";

        public override Color DefaultColor => Color.Black;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;

    }
}
