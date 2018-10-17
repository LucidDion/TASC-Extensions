using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class APTR : IndicatorBase
    {
        public override string Name => "APTR";

        public override string Abbreviation => "APTR";

        public override string HelpDescription => @"Created by Vitali Apirine (see article in November 2015 issue of Stocks and Commodities Magazine), the APTR indicator is a normalized ATR indicator similar to ATRP.";

        public override string PaneTag => @"APTR";

        public override Color DefaultColor => Color.RoyalBlue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;

        //parameterless constructor
        public APTR() : base()
        {
        }

        //for code based construction
        public APTR(BarHistory bars, int period)
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
            AddParameter("Period", ParameterTypes.Int32, 14);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

            TimeSeries LH = bars.High - bars.Low;
            TimeSeries HC = (bars.High - (bars.Close >> 1)).Abs();
            TimeSeries LC = (bars.Low - (bars.Close >> 1)).Abs();
            TimeSeries M = new TimeSeries(DateTimes);
            TimeSeries MM = new TimeSeries(DateTimes);
            TimeSeries atrs_ = new TimeSeries(DateTimes);
            double atr1 = 0, MiD1 = 0, atr2 = 0, MiD2 = 0, atr3 = 0, MiD3 = 0, atrs = 0;

            for (int bar = 1; bar < bars.Count; bar++)
            {
                M[bar] = Math.Max(LH[bar], HC[bar]);
                MM[bar] = Math.Max(M[bar], LC[bar]);

                atr1 = MM[bar] == HC[bar] ? HC[bar] : 0;
                MiD1 = atr1 > 0 ? (bars.Close[bar - 1] + (HC[bar] / 2.0)) : 0.00001;
                atr2 = ((MM[bar] == LC[bar]) && atr1 == 0) ? LC[bar] : 0;
                MiD2 = atr2 > 0 ? (bars.Low[bar] + (LC[bar] / 2.0)) : 0.00001;
                atr3 = ((MM[bar] == LH[bar]) && atr1 == 0 && atr2 == 0) ? LH[bar] : 0;
                MiD3 = atr3 > 0 ? (bars.Low[bar] + (LH[bar] / 2.0)) : 0.00001;
                atrs = atr1 > 0 ? atr1 / MiD1 :
                        atr2 > 0 ? atr2 / MiD2 :
                            atr3 > 0 ? atr3 / MiD3 : 0;

                atrs_[bar] = atrs;
            }

            for (int bar = period + 2; bar < bars.Count; bar++)
            {
                Values[bar] = new WilderMA(atrs_, 14)[bar] * 100;
            }
        }        
    }
}
