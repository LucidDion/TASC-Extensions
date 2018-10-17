using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //BearPowerVG Indicator class
    public class BearPowerVG : IndicatorBase
    {
        //parameterless constructor
        public BearPowerVG() : base()
        {
        }

        //for code based construction
        public BearPowerVG(BarHistory bars, int period)
            : base()
        {
            Parameters[0].Value = bars;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
        }

        //populate
        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;
            DateTimes = ds.DateTimes;

            if (ds.Count == 0)
                return;

            //Rest of series
            for (int bar = 1; bar < ds.Count; bar++)
            {
                double C = ds.Close[bar];
                double O = ds.Open[bar];
                double C1 = ds.Close[bar - 1];
                double H = ds.High[bar];
                double L = ds.Low[bar];
                double r1 = Math.Max(C1 - O, H - L);
                double r2 = Math.Max(H - C, Math.Max(C1, O) - L);
                if (C < O) /* black candle */ Values[bar] = r1;
                else if (C > O) /* white candle */ Values[bar] = r2;
                else if (H - C > C - L) /* doji, longer upper shadow */ Values[bar] = r1;
                else if (H - C < C - L) /* doji, longer lower shadow */ Values[bar] = r2;
                else if (C < C1) /* symmetrical doji, going down */ Values[bar] = r1;
                else /* symmetrical doji, going up or no change */ Values[bar] = r2;
            }
        }



        public override string Name => "BearPowerVG";

        public override string Abbreviation => "BearPowerVG";

        public override string HelpDescription => @"The BearPower component of the BBB indicator from the October 2003 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"BearPower";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;

    }
}
