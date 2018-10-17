using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class ATRModified : IndicatorBase
    {        
        //parameterless constructor
        public ATRModified() : base()
        {
        }

        //for code based construction
        public ATRModified(BarHistory bars, int period)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = period;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
            AddParameter("Period", ParameterTypes.Int32, 5);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

            var HiLo = bars.High - bars.Low;
            var sma15HiLo = 1.5 * new FastSMA(HiLo, period);

            Values[0] = 0d;
            int pm1 = period - 1;
            for (int bar = 1; bar < DateTimes.Count; bar++)
            {
                // HiLo:=If(H-L<1.5*Mov(H-L,period,S), H-L, 1.5*Mov(H-L,period,S));
                double hilo = HiLo[bar] < sma15HiLo[bar] ? HiLo[bar] : sma15HiLo[bar];

                // Href:=If(L<=Ref(H,-1),H-Ref(C,-1),(H-Ref(C,-1))-(L-Ref(H,-1))/2);
                double Href = bars.Low[bar] <= bars.High[bar - 1] ? bars.High[bar] - bars.Close[bar - 1]
                    : (bars.High[bar] - bars.Close[bar - 1]) - (bars.Low[bar] - bars.High[bar - 1]) / 2;

                // Lref:=If(H>=Ref(L,-1),Ref(C,-1)-L,(Ref(C,-1)-L)-(Ref(L,-1)-H)/2);
                double Lref = bars.High[bar] >= bars.Low[bar - 1] ? bars.Close[bar - 1] - bars.Low[bar]
                    : (bars.Close[bar - 1] - bars.Low[bar]) - (bars.Low[bar - 1] - bars.High[bar]) / 2;

                double diff1 = Math.Max(hilo, Href);
                double diff2 = Math.Max(diff1, Lref);

                // Wilder averaging
                if (bar < period)
                {   // initialization
                    Values[bar] = (Values[bar - 1] * (bar - 1) + diff2) / bar;
                }
                else
                {
                    Values[bar] = (Values[bar - 1] * pm1 + diff2) / period;
                }
            }
        }



        public override string Name => "ATRModified";

        public override string Abbreviation => "ATRModified";

        public override string HelpDescription => @"ATRModified (Modified ATR) from the June 2009 issue of Technical Analysis of Stocks & Commodities magazine sets limits on extreme moves in True Range.";

        public override string PaneTag => @"ATR";

        public override Color DefaultColor => Color.LimeGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }
}
