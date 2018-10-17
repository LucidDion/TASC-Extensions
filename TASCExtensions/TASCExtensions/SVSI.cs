using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class SVSI : IndicatorBase
    {
        //parameterless constructor
        public SVSI() : base()
        {
        }

        //for code based construction
        public SVSI(BarHistory bars, Int32 svsiPeriod, Int32 wmaPeriod)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = svsiPeriod;
            Parameters[2].Value = wmaPeriod;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterTypes.BarHistory, null);
            AddParameter("SVSI period", ParameterTypes.Int32, 6);
            AddParameter("WilderMA period", ParameterTypes.Int32, 14);
        }

        //(BarHistory bars, BarHistory barsFirst, BarHistory barsSecond, int periodRegression, int periodRegressionMomentum,
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 svsiPeriod = Parameters[1].AsInt;
            Int32 wmaPeriod = Parameters[2].AsInt;

            DateTimes = bars.DateTimes;
            var period = Math.Max(svsiPeriod, wmaPeriod);

            if (period <= 0 || DateTimes.Count == 0)
                return;

            EMA R1 = new EMA(bars.Close, svsiPeriod);
            var V2 = new TimeSeries(DateTimes);
            var V3 = new TimeSeries(DateTimes);

            for (int bar = 0; bar < bars.Count; bar++)
            {
                V2[bar] = bars.Close[bar] > R1[bar] ? bars.Volume[bar] : 0;
                V3[bar] = bars.Close[bar] < R1[bar] ? bars.Volume[bar] : 0;
            }

            var V4 = new WilderMA(V2, wmaPeriod);
            var V5 = new WilderMA(V3, wmaPeriod);

            for (int bar = 0; bar < bars.Count; bar++)
            {
                Values[bar] = V5[bar] == 0 ? 100 : 100 - (100 / (1 + (V4[bar] / V5[bar])));
            }
        }

        public override string Name => "SVSI";

        public override string Abbreviation => "SVSI";

        public override string HelpDescription => "SVSI (Slow Volume Strength Index) by Vitali Apirine from June 2015 issue of Technical Analysis of Stocks & Commodities magazine is a momentum volume oscillator that measures change in buying and selling pressure relative to a price exponential moving average (EMA), oscillating between 0 and 100.";

        public override string PaneTag => @"SVSI";

        public override Color DefaultColor => Color.Coral;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;

        public override double OverboughtLevel => 80;

        public override double OversoldLevel => 20;
    }    
}
