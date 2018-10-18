using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //BollingerPctBSmoothed Indicator class
    public class BollingerPctBSmoothed : IndicatorBase
    {
        //parameterless constructor
        public BollingerPctBSmoothed() : base()
        {
        }

        //for code based construction
        public BollingerPctBSmoothed(BarHistory source, Int32 periodPctB, Int32 periodSmooth)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = periodPctB;
            Parameters[2].Value = periodSmooth;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterTypes.BarHistory, null);
            AddParameter("Period", ParameterTypes.Int32, 18);
            AddParameter("Smooth Period", ParameterTypes.Int32, 8);
        }

        //populate
        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;
            Int32 periodSmooth = Parameters[1].AsInt;
            DateTimes = ds.DateTimes;

            if (period <= 0 || periodSmooth <= 0 || ds.Count == 0)
                return;

            //Assign first bar that contains indicator data
            var FirstValidValue = Math.Max(periodSmooth, period);
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            if (ds.Count < 2) return;

            var _avg = (ds.Open + ds.High + ds.Low + ds.Close) / 4d;

            //TODO fill with 0 in the loop
            var _haOpen = new TimeSeries(DateTimes);
            var _haC = new TimeSeries(DateTimes);
            _haOpen[0] = _avg[0];
			_haC[0] = _avg[0];
            
			for(int bar = 1; bar < ds.Count; bar++)
			{
				_haOpen[bar] = ( _avg[bar-1] + _haOpen[bar-1] ) / 2d;
				_haC[bar] = ( _avg[bar] + _haOpen[bar] 
					+ Math.Max(ds.High[bar], _haOpen[bar]) 
					+ Math.Min(ds.Low[bar], _haOpen[bar]) ) / 4d;
			}

            var TMA1 = new TEMA_TASC(_haC, periodSmooth);
            var TMA2 = new TEMA_TASC(TMA1, periodSmooth);
            var Diff = TMA1 - TMA2;
            var ZLHA = TMA1 + Diff;

            var temaZLHA = new TEMA_TASC(ZLHA, periodSmooth);
            var _sd = new StdDev(temaZLHA, period);
            var _wma = new WMA(temaZLHA, period);

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Values[bar] = 100 * (temaZLHA[bar] + 2 * _sd[bar] - _wma[bar]) / (4 * _sd[bar]);
            }
        }



        public override string Name => "BollingerPctBSmoothed";

        public override string Abbreviation => "BollingerPctBSmoothed";

        public override string HelpDescription => @"A smoothed version of the Bollinger %b by Sylvain Vervoot in the May 2010 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"BBandPercentB";

        public override Color DefaultColor => Color.RoyalBlue;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;
    }
}
