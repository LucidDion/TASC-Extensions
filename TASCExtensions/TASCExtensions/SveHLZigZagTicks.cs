using System;
using System.Collections.Generic;
using System.Text;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCIndicators
{
    public class SVEHLZigZagTicks : IndicatorBase
    {
        public override string Name => "SVEHLZigZagTicks";

        public override string Abbreviation => "SVEHLZigZagTicks";

        public override string HelpDescription => "Sylvain Vervoort's SVEHLZigZagTicks indicator from September 2018 issue of Stocks & Commodities magazine";

        public override string PaneTag => @"SVEHLZigZagTicks";

        public override Color DefaultColor => Color.Blue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;

        //parameterless constructor
        public SVEHLZigZagTicks() : base()
        {
        }

        //for code based construction
        public SVEHLZigZagTicks(BarHistory bars, int change)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = change;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
            AddParameter("Number of Ticks", ParameterTypes.Int32, 200);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 change = Parameters[1].AsInt;
            var ticks = Ticks * bars.TickSize;
            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

            int CurrentTrend = 0;
            double Reverse = 0;
            double HPrice = 0;
            double LPrice = 0;

            for (int bar = period; bar < bars.Count; bar++)
            {
                double atrValue = atr[bar] * factor;

                if (CurrentTrend >= 0)   // trend is up, look for new swing high
                {
                    HPrice = Math.Max(bars.High[bar], HPrice);
                    Reverse = HPrice - ticks;
                    if (bars.Low[bar] <= Reverse)
                    {
                        CurrentTrend = -1;
                        LPrice = bars.Low[bar];
                        Reverse = LPrice + ticks;
                    }
                }
                if (CurrentTrend <= 0)   // trend is down, look for new swing low
                {
                    LPrice = Math.Min(bars.Low[bar], LPrice);
                    Reverse = LPrice + ticks;
                    if (bars.High[bar] >= Reverse)
                    {
                        CurrentTrend = 1;
                        HPrice = bars.High[bar];
                        Reverse = HPrice - ticks;
                    }
                }

                Values[bar] = Reverse;
            }
        }
    }
}