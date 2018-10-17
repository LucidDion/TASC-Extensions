using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class MoneyFlowOscillator : IndicatorBase
    {
        //parameterless constructor
        public MoneyFlowOscillator() : base()
        {
        }

        //for code based construction
        public MoneyFlowOscillator(BarHistory source, Int32 period)
        : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.BarHistory, null);
            AddParameter("Period", ParameterTypes.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

            //DataSeries Multiplier = DataSeries.Abs((High - (Low>>1)) - ((High>>1)-Low)) /
            //	DataSeries.Abs((High - (Low>>1)) + ((High>>1)-Low));
            var Multiplier = ((bars.High - (bars.Low >> 1)) - ((bars.High >> 1) - bars.Low)) /
                ((bars.High - (bars.Low >> 1)) + ((bars.High >> 1) - bars.Low));
            var MFV = Multiplier * bars.Volume;
            var MFO = MFV.Sum(period) / bars.Volume.Sum(period);

            for (int bar = period; bar < bars.Count; bar++)
            {
                Values[bar] = MFO[bar];
            }
        }
        
        public override string Name => "MoneyFlowOscillator";

        public override string Abbreviation => "MoneyFlowOscillator";

        public override string HelpDescription => "Created by Vitali Apirine (see article in October 2015 issue of Stocks and Commodities Magazine), the Money Flow Oscillator measures buying and selling pressure over a specific period of time.";

        public override string PaneTag => @"MoneyFlowOscillator";

        public override Color DefaultColor => Color.Black;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }    
}
