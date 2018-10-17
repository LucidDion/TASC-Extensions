using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //RevEngBBandCross Indicator class
    public class RevEngBBandCross : IndicatorBase
    {
        /*
    var A: integer = SMASeries(#Close, Period - 1);
    var SD: integer = StdDevSeries(#Close, Period - 1);
    var k: float = 0;
    var Bar: integer;
    for Bar := BarCount - 1 downto BarCount - Period do
      k := k + @Series[Bar] * @Series[Bar];
    k := sqrt(max(0, k - Period * @A[BarCount - 1]));
    if abs(k / sqrt(Period) - @SD[BarCount - 1]) < abs(k / sqrt(Period - 1) - @SD[BarCount - 1])
    then k := Devs * sqrt( Period      / (Period - 1 - Devs * Devs))
    else k := Devs * sqrt((Period - 1) / (Period - 1 - Devs * Devs));
    for Bar := BarCount - 1 downto Period  - 2 do
      @Result[Bar] := @A[Bar] - k * @SD[Bar];         
         */

        //parameterless constructor
        public RevEngBBandCross() : base()
        {
        }

        //for code based construction
        public RevEngBBandCross(TimeSeries source, Int32 period, Double devs, bool upper)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = devs;
            Parameters[3].Value = upper;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period", ParameterTypes.Int32, 20);
            AddParameter("Deviations", ParameterTypes.Double, 2);
            AddParameter("Upper?", ParameterTypes.Boolean, true);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Double devs = Parameters[2].AsDouble;
            bool Upper = Parameters[3].AsBoolean;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            //Avoid exception errors
            if (period < 1 || period > ds.Count) period = ds.Count;

            int upper = 1;
            var ds2 = ds * ds;
            var period_sr = Math.Sqrt(period);
            var periodM1_sr = Math.Sqrt(period - 1);
            var devs2 = devs * devs;
            if (!Upper) upper = -1;
            var _smaSer = new FastSMA(ds, period);
            var _sdSer = new StdDev(ds, period);
            var _sumK = ds2.Sum(period);
            var k1 = devs * Math.Sqrt(period / (period - 1 - devs2));
            var k2 = devs * Math.Sqrt((period - 1)/ (period - 1 - devs2));
            
            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + period - 1;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                double k = _sumK[bar];
                k = Math.Sqrt(Math.Max(0d, k - period * _smaSer[bar]));
                if (Math.Abs(k / period_sr - _sdSer[bar]) < Math.Abs(k / periodM1_sr - _sdSer[bar]))
                    k = k1;
                else
                    k = k2;

                Values[bar] = _smaSer[bar] - upper * k * _sdSer[bar];
            }
        }


        public override string Name => "RevEngBBandCross";

        public override string Abbreviation => "RevEngBBandCross";

        public override string HelpDescription => "Reverse-engineered Bollinger Band cross based on the WL4 indicators by gbeltrame. Shows the DataSeries price required to cross the specified BBand on the next bar.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.Black;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}
