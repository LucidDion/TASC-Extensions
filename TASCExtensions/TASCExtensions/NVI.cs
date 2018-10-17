using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //NVI Indicator class
    public class NVI : IndicatorBase
    {
        //parameterless constructor
        public NVI() : base()
        {
        }

        //for code based construction
        public NVI(BarHistory source)
        : base()
        {
            Parameters[0].Value = source;

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
            BarHistory bars = Parameters[0].AsBarHistory;

            DateTimes = bars.DateTimes;

            //Assign first bar that contains indicator data
            var FirstValidValue = 1;
            if (FirstValidValue > bars.Count) FirstValidValue = bars.Count;

            //Rest of series
            double Value = 0;
            for (int bar = FirstValidValue; bar < bars.Count; bar++)
            {
                if (bars.Volume[bar] <= bars.Volume[bar - 1])
                    Value += 100 * bars.Close[bar] / bars.Close[bar - 1] - 100;
                Values[bar] = Value;
            }
        }

        public override string Name => "NVI";

        public override string Abbreviation => "NVI";

        public override string HelpDescription => "NVI (Negative Volume Index) from the April 2003 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => "NVI";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
