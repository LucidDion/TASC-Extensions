using System;
using System.Collections.Generic;
using System.Text;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Drawing;

namespace TASCIndicators
{
    public class HHS : IndicatorBase
    {
        //parameterless constructor
        public HHS() : base()
        {
        }

        //for code based construction
        public HHS(BarHistory source, Int32 period)
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
            AddParameter("Highest Period", ParameterTypes.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

            var HHH = new TimeSeries(DateTimes);

            var hh = new Highest(bars.Low, period);
            var ll = new Lowest(bars.Low, period);

            //var FirstValidValue = period * 3;
            //for (int bar = FirstValidValue; bar < bars.Count; bar++)
            for (int bar = period; bar < bars.Count; bar++)
            {
                HHH[bar] = bars.High[bar] > bars.High[bar - 1] ?
                    ((bars.High[bar] - ll[bar]) /
                    (hh[bar] - ll[bar])) : 0;
            }

            var ema = new EMA(HHH, period);

            for (int bar = period; bar < bars.Count; bar++)
            {
                Values[bar] = ema[bar] * 100;
            }
        }


        public override string Name => "HHS";

        public override string Abbreviation => "HHS";

        public override string HelpDescription => "Created by V. Apirine, the higher high stochastic (HHS) is part of the higher high lower low stochastic (HHLLS) which is a momentum indicator–based system that helps determine the direction of a trend.";

        public override string PaneTag => @"HHLLS";

        public override Color DefaultColor => Color.Green;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }

    public class LLS : IndicatorBase
    {
        //parameterless constructor
        public LLS() : base()
        {
        }

        //for code based construction
        public LLS(BarHistory source, Int32 period)
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
            AddParameter("Lowest Period", ParameterTypes.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

            var LLL = new TimeSeries(DateTimes);
            var hh = new Highest(bars.Low, period);
            var ll = new Lowest(bars.Low, period);

            //var FirstValidValue = period * 3;
            //for (int bar = FirstValidValue; bar < bars.Count; bar++)
            for (int bar = period; bar < bars.Count; bar++)
            {
                LLL[bar] = bars.Low[bar] < bars.Low[bar - 1] ?
                    ((hh[bar] - bars.Low[bar]) /
                    (hh[bar] - ll[bar])) : 0;
            }

            var ema = new EMA(LLL, period);

            for (int bar = period; bar < bars.Count; bar++)
            {
                Values[bar] = ema[bar] * 100;
            }
        }


        public override string Name => "LLS";

        public override string Abbreviation => "LLS";

        public override string HelpDescription => "Created by V. Apirine, the lower low stochastic (LLS) is part of the higher high lower low stochastic (HHLLS) which is a momentum indicator–based system that helps determine the direction of a trend.";

        public override string PaneTag => @"HHLLS";

        public override Color DefaultColor => Color.Red;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
