using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //TTF Indicator class
    public class TTF : IndicatorBase
    {
        //parameterless constructor
        public TTF() : base()
        {
        }

        //for code based construction
        public TTF(BarHistory bars, Int32 period)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = period;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterTypes.BarHistory, null);
            AddParameter("Period", ParameterTypes.Int32, 15);
        }

        //(BarHistory bars, BarHistory barsFirst, BarHistory barsSecond, int periodRegression, int periodRegressionMomentum,
        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || DateTimes.Count == 0)
                return;

            var HH = new Highest(ds.High, period);
            var LL = new Lowest(ds.Low, period);

            //Avoid exception errors
            if (period < 1 || period > ds.Count + 1) period = ds.Count + 1;

            //Assign first bar that contains indicator data
            var FirstValidValue = 2 * period - 1;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            for (int bar = 0; bar < FirstValidValue; bar++)
                Values[bar] = 0;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                double BuyPwr = HH[bar] - LL[bar - period];
                double SellPwr = HH[bar - period] - LL[bar];
                if (BuyPwr + SellPwr != 0)
                    Values[bar] = 200 * (BuyPwr - SellPwr) / (BuyPwr + SellPwr);
                //else
                //    Values[bar] = 0;
            }
        }

        //This static method allows ad-hoc calculation of TTF (single calc mode)
        public static double Calculate (int bar, BarHistory ds, int period)
        {
            if (bar < 2 * period - 1)
                return 0;

            double BuyPwr = Highest.Calculate(bar, ds.High, period) - Lowest.Calculate(bar - period, ds.Low, period);
            double SellPwr = Highest.Calculate(bar - period, ds.High, period) - Lowest.Calculate(bar, ds.Low, period);
            if (BuyPwr + SellPwr == 0)
                return 0;

            return 200 * (BuyPwr - SellPwr) / (BuyPwr + SellPwr);
        }
        
        public override string Name => "TTF";

        public override string Abbreviation => "TTF";

        public override string HelpDescription => "Trend Trigger Factor from the December 2004 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"TTF";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }
}
