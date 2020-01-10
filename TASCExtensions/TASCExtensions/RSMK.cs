using QuantaculaCore;
using QuantaculaIndicators;
using System;
using System.Drawing;
using TASCIndicators;

namespace TASCIndicators
{
    public class RSMK : IndicatorBase
    {
        public override string Name
        {
            get
            {
                return "RSMK";
            }
        }
        public override string Abbreviation
        {
            get
            {
                return "RSMK";
            }
        }
        public override string HelpDescription
        {
            get
            {
                return "RSMK is a relative strength indicator based on the article by Markos Katsanos in the March 2020 issue of Stocks & Commodities magazine.";
            }
        }
        public override string PaneTag
        {
            get
            {
                return "RSMK";
            }
        }
        public override Color DefaultColor
        {
            get
            {
                return Color.DarkGreen;
            }
        }

        //it's not a smoother
        public override bool IsSmoother => false;

        public RSMK()
        {
        }
        public RSMK(TimeSeries ds, TimeSeries index, int period = 90, int periodEMA = 3)
        {
            base.Parameters[0].Value = ds;
			base.Parameters[1].Value = index;
            base.Parameters[2].Value = period;
            base.Parameters[3].Value = periodEMA;
            this.Populate();
        }
        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close); 
			base.AddParameter("Index", ParameterTypes.TimeSeries, PriceComponents.Close); 
            base.AddParameter("Lookback Period", ParameterTypes.Int32, 90);
            base.AddParameter("EMA Period", ParameterTypes.Int32, 3);
        }
        public override void Populate()
        {
            TimeSeries ds = base.Parameters[0].AsTimeSeries;
			TimeSeries index = base.Parameters[1].AsTimeSeries;
            int period = base.Parameters[2].AsInt;
            int emaPeriod = base.Parameters[3].AsInt;
            this.DateTimes = ds.DateTimes;
            int FirstValidValue = Math.Max(period, emaPeriod) + 1;
            if (ds.Count < FirstValidValue)
            {
                return;
            }
			
			TimeSeries log1 = new TimeSeries(ds.DateTimes); 
			TimeSeries log2 = new TimeSeries(index.DateTimes);
			TimeSeries tmp1 = new TimeSeries(ds.DateTimes);
            tmp1 = ds * 0;

			for (int i = 0; i < FirstValidValue; i++)
            {
                log1[i] = 0;// checked(Math.Log( ds[i] / index[i] ));
				log2[i] = 0;
            }
			
            for (int i = FirstValidValue; i < ds.Count; i++)
            {
                log1[i] = checked(Math.Log(ds[i] / index[i]));
                log2[i] = checked(Math.Log((ds[i - period]) / index[i - period]));
                tmp1[i] = log1[i] - log2[i];
            }

            TimeSeries rsmk = EMA.Series(tmp1, emaPeriod) * 100d;
			
            for (int j = FirstValidValue; j < ds.Count; j++)
            {
				double val = rsmk[j];
                base.Values[j] = double.IsNaN(val) ? 0 : val;
            }
        }
    }
}
