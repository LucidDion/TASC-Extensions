using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class FOM : IndicatorBase
    {
        //parameterless constructor
        public FOM() : base()
        {
        }

        //for code based construction
        public FOM(BarHistory source, Int32 period)
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
            AddParameter("Period", ParameterTypes.Int32, 60);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

            var FirstValidValue = period;
            if (FirstValidValue > bars.Count || FirstValidValue < 0) FirstValidValue = bars.Count;

            var RelVol = new TimeSeries(DateTimes);
            if (FirstValidValue > 1)
                RelVol[FirstValidValue - 1] = 0d;

            for (int bar = FirstValidValue; bar < bars.Count; bar++)
            {
                double av = FastSMA.Calculate(bar, bars.Volume, period);
                double sd = new StdDev(bars.Volume, period)[bar];
                double relVol = (bars.Volume[bar] - av) / sd;
                RelVol[bar] = relVol;
            }

            var aMove = ((bars.Close - (bars.Close >> 1)) / bars.Close >> 1).Abs();
            var theMin = new Lowest(aMove, period);
            var theMax = new Highest(aMove, period);
            var theMove = new TimeSeries(DateTimes);
            var theVol = new TimeSeries(DateTimes);

            for (int bar = FirstValidValue; bar < bars.Count; bar++)
            {
                if ((theMax[bar] - theMin[bar]) > 0)
                    theMove[bar] = 1.0 + ((aMove[bar] - theMin[bar]) * (10d - 1d)) / (theMax[bar] - theMin[bar]);
            }

            var theMinV = new Lowest(RelVol, period);
            var theMaxV = new Highest(RelVol, period);

            for (int bar = FirstValidValue; bar < bars.Count; bar++)
            {
                if ((theMaxV[bar] - theMinV[bar]) > 0)
                    theVol[bar] = 1.0 + ((RelVol[bar] - theMinV[bar]) * (10d - 1d)) / (theMaxV[bar] - theMinV[bar]);
            }

            var vByM = theVol / theMove;
            var avF = new SMA(vByM, period);
            var sdF = new StdDev(vByM, period);
            var theFoM = (vByM - avF) / sdF;
            
            for (int bar = 0; bar < bars.Count; bar++)
            {
                Values[bar] = theFoM[bar];
            }
        }

        public override string Name => "FOM";

        public override string Abbreviation => "FOM";

        public override string HelpDescription => "Created by Melvin Dickover (see article in April 2014 issue of Stocks and Commodities Magazine), Freedom of Movement is a normalized measure of relative price change divided by relative volume expressed in standard deviations. FoM spikes beyond a threshold indicate important restrictions on price movement.";

        public override string PaneTag => @"FOM";

        public override Color DefaultColor => Color.Black;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickHistogram;
    }    
}
