using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    //CandleCode Indicator class
    public class CandleCode : IndicatorBase
    {
        //parameterless constructor
        public CandleCode() : base()
        {
        }

        //for code based construction
        public CandleCode(BarHistory bars, int period)
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


        //Constructor
        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;
            DateTimes = ds.DateTimes;

            if (ds.Count == 0)
                return;


            //Assign first bar that contains indicator data
            var FirstValidValue = 20;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            var US = new TimeSeries(DateTimes); 
            var BS = new TimeSeries(DateTimes);
            var LS = new TimeSeries(DateTimes);

            //Build aux series
            for (int bar = 0; bar < ds.Count; bar++)
                if (ds.Open[bar] > ds.Close[bar])
                {
                    US[bar] = ds.High [bar] - ds.Open [bar];
                    BS[bar] = ds.Open [bar] - ds.Close[bar];
                    LS[bar] = ds.Close[bar] - ds.Low  [bar];
                }
                else
                {
                    US[bar] = ds.High[bar] - ds.Close[bar];
                    BS[bar] = ds.Close[bar] - ds.Open [bar];
                    LS[bar] = ds.Open [bar] - ds.Low  [bar];
                }

            var U_Upper = new BBUpper(US, 20, 0.5);
            var U_Lower = new BBLower(US, 20, 0.5);
            var B_Upper = new BBUpper(BS, 20, 0.5);
            var B_Lower = new BBLower(BS, 20, 0.5);
            var L_Upper = new BBUpper(LS, 20, 0.5);
            var L_Lower = new BBLower(LS, 20, 0.5);

            //Initialize start of series with zeroes
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] = 0;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                int Code = 32 + 8 + 2;
                if (US[bar] > U_Upper[bar]) Code += 16; else if (US[bar] < U_Lower[bar]) Code -= 16;
                if (BS[bar] > B_Upper[bar]) Code +=  4; else if (BS[bar] < B_Lower[bar]) Code -=  4;
                if (LS[bar] > L_Upper[bar]) Code +=  1; else if (LS[bar] < L_Lower[bar]) Code -=  1;
                if (ds.Open[bar] > ds.Close[bar]) Code = -Code;
                Values[bar] = Code;
            }
        }


        public override string Name => "CandleCode";

        public override string Abbreviation => "CandleCode";

        public override string HelpDescription => @"In the March, 2001 issue of Stocks & Commodities magazine, Viktor Likhovidov shares a method of coding candlesticks.";

        public override string PaneTag => @"CandleCode";

        public override Color DefaultColor => Color.DarkGreen;

        public override PlotStyles DefaultPlotStyle => PlotStyles.ThickLine;

    }
}
