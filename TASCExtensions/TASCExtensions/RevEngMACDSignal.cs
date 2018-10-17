using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;
using System.Linq;

namespace TASCIndicators 
{
    public class sMACD : IndicatorBase
    {
        //parameterless constructor
        public sMACD() : base()
        {
        }

        //for code based construction
        public sMACD(TimeSeries source, Int32 period1, Int32 period2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Shorter SMA", ParameterTypes.Int32, 12);
            AddParameter("Longer SMA", ParameterTypes.Int32, 26);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;
            int period = Math.Max(period1, period2);

            if (period <= 0 || ds.Count == 0)
                return;


            var FirstValidValue = Math.Max(period1, period2);

            if (FirstValidValue > ds.Count || FirstValidValue < 0) 
                FirstValidValue = ds.Count;
            if (ds.Count < Math.Max(period1,period2)) 
                return;

            TASCIndicators.FastSMA ema1 = new FastSMA(ds, period1);
            TASCIndicators.FastSMA ema2 = new FastSMA(ds, period2);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                Values[bar] = ema1[bar] - ema2[bar];
            }
        }

        public override string Name => "sMACD";

        public override string Abbreviation => "sMACD";

        public override string HelpDescription => "sMACD by Johnny Dough is a MACD indicator based on simple moving averages.";

        public override string PaneTag => @"sMACD";

        public override Color DefaultColor => Color.Black;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;

    }

    public class sMACD_Signal : IndicatorBase
    {
        //parameterless constructor
        public sMACD_Signal() : base()
        {
        }

        //for code based construction
        public sMACD_Signal(TimeSeries source, Int32 period1, Int32 period2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Shorter SMA", ParameterTypes.Int32, 12);
            AddParameter("Longer SMA", ParameterTypes.Int32, 26);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;
            int period = Math.Max(period1, period2);

            if (period <= 0 || ds.Count == 0)
                return;


            var FirstValidValue = Math.Max(period1, period2);

            if (FirstValidValue > ds.Count || FirstValidValue < 0)
                FirstValidValue = ds.Count;
            if (ds.Count < Math.Max(period1, period2))
                return;

            sMACD smacd = new sMACD(ds, period1, period2);
            FastSMA sma = new FastSMA(smacd, 9);

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Values[bar] = sma[bar];
            }
        }

        public override string Name => "sMACD_Signal";

        public override string Abbreviation => "sMACD_Signal";

        public override string HelpDescription => "This is the Signal Line of the sMACD indicator.";

        public override string PaneTag => @"sMACD";

        public override Color DefaultColor => Color.Black;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;

    }

    public class PsMACDsignal : IndicatorBase
    {
        //parameterless constructor
        public PsMACDsignal() : base()
        {
        }

        //for code based construction
        public PsMACDsignal(TimeSeries source, Int32 period1, Int32 period2, Int32 period3)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;
            Parameters[3].Value = period3;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("Period 1", ParameterTypes.Int32, 12);
            AddParameter("Period 2", ParameterTypes.Int32, 26);
            AddParameter("Period 3", ParameterTypes.Int32, 9);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;
            Int32 period3 = Parameters[3].AsInt;

            DateTimes = ds.DateTimes;
            var period = new List<int> { period1, period2, period3 }.Max();

            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = period;
            if (FirstValidValue > ds.Count || FirstValidValue < 0)
                FirstValidValue = ds.Count;
            if (ds.Count < Math.Max(Math.Max(period1, period2), period3))
                return;
            if (period3 <= 1)
                return;

            sMACD smacd = new sMACD(ds, period1, period2);
            int XY = period1 * period2;
            int XZ = period1 * period3;
            int YZ = period2 * period3;
            int XYZ = period1 * period2 * period3;

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Values[bar] = (smacd[bar] * (XYZ - XY) -
                       XYZ * new FastSMA(smacd, period3)[bar] -
                       ds[period1 + 1] * (YZ - period2) +
                       ds[period2 + 1] * (XZ - period1) +
                       XY * smacd[period3 + 1]) / (XZ - YZ - period1 + period2);
            }
        }

        public override string Name => "PsMACDsignal";

        public override string Abbreviation => "PsMACDsignal";

        public override string HelpDescription => "PsMACDsignal auxiliary";

        public override string PaneTag => @"PsMACDsignal";

        public override Color DefaultColor => Color.Black;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }

    public class RevEngMACDSignal : IndicatorBase
    {
        private double PMACDeq(int bar, TimeSeries price, int periodX, int periodY)
		{		
			double alphaX = 2.0 / (1.0 + periodX );
			double alphaY = 2.0 / (1.0 + periodY );

            var ema1 = new EMA(price, periodX);
            var ema2 = new EMA(price, periodY);

            return (ema1[bar] * alphaX - ema2[bar] * alphaY) / (alphaX - alphaY);
		}

        // returns price where sMACD is equal to previous bar sMACD
        private double PsMACDeq(int bar, TimeSeries price, int periodX, int periodY)
        {
            return (periodX * price[periodY + 1] - periodY * price[periodX + 1]) / (periodX - periodY);
        }

        private double PMACDlevel(double level, int bar, TimeSeries price, int periodX, int periodY)
		{		
			double alphaX = 2.0 / (1.0 + periodX );
			double alphaY = 2.0 / (1.0 + periodY );
			double OneAlphaX = 1.0 - alphaX;
			double OneAlphaY = 1.0 - alphaY;

            var ema1 = new EMA(price, periodX);
            var ema2 = new EMA(price, periodY);

            return (level + ema2[bar] * OneAlphaY - ema1[bar] * OneAlphaX) / (alphaX - alphaY);
		}

        // returns price where MACD cross signal line or MACD histogram crosses 0
        private double PMACDsignal(int bar, TimeSeries price, int periodX, int periodY, int periodZ)
        {
            double alphaX = 2.0 / (1.0 + periodX);
            double alphaY = 2.0 / (1.0 + periodY);
            double alphaZ = 2.0 / (1.0 + periodZ);
            double OneAlphaX = 1.0 - alphaX;
            double OneAlphaY = 1.0 - alphaY;
            double OneAlphaZ = 1.0 - alphaZ;    // leftover? not used in the formula
            var ema1 = new EMA(price, periodX);
            var ema2 = new EMA(price, periodY);
            var macdex = ema1 - ema2;
            var macdexSignal = new EMA(macdex, periodZ);
            double MACDvalue = macdex[bar];
            double MACDsignal = macdexSignal[bar];

            return (MACDsignal - ema1[bar] * OneAlphaX + ema2[bar] * OneAlphaY) / (alphaX - alphaY);
        }

        private double PMACDzero(int bar, TimeSeries price, int periodX, int periodY)
		{		
			return PMACDlevel( 0, bar, price, periodX, periodY );
		}

        // returns price where sMACD cross 0
        private double PsMACDzero(int bar, TimeSeries price, int periodX, int periodY)
        {
            double result = (periodX * periodY * (FastSMA.Calculate(bar, price, periodX) - FastSMA.Calculate(bar, price, periodY)) +
                       periodX * price[periodY + 1] -
                       periodY * price[periodX + 1]) /
                        (periodX - periodY);

            return result;
        }

        //parameterless constructor
        public RevEngMACDSignal() : base()
        {
        }

        //for code based construction
        public RevEngMACDSignal(TimeSeries source, Int32 period1, Int32 period2, Int32 period3, bool useSMA)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;
            Parameters[3].Value = period3;
            Parameters[4].Value = useSMA;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("MACD Period 1", ParameterTypes.Int32, 12);
            AddParameter("MACD Period 2", ParameterTypes.Int32, 26);
            AddParameter("MACD Signal Period", ParameterTypes.Int32, 9);
            AddParameter("Use SMA?", ParameterTypes.Boolean, true);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;
            Int32 period3 = Parameters[3].AsInt;
            bool useSMA = Parameters[4].AsBoolean;

            DateTimes = ds.DateTimes;
            var period = new List<int> { period1, period2, period3 }.Max();

            if (period <= 0 || ds.Count == 0)
                return;

            //var FirstValidValue = Math.Max(period1, period2) * 3;

            var psms = new TimeSeries(DateTimes);
            if (useSMA)
                psms = new PsMACDsignal(ds, period1, period2, period3);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                Values[bar] = useSMA == false ? PMACDsignal(bar, ds, period1, period2, period3) : psms[bar];
            }
        }

        public override string Name => "RevEngMACDSignal";

        public override string Abbreviation => "RevEngMACDSignal";

        public override string HelpDescription => "From S&C November 2013 article 'Reversing MACD: The Sequel' by Johnny Dough. Returns the price value required for the MACD line and signal line to cross on the next bar.";

        public override string PaneTag => @"Price";

        public override Color DefaultColor => Color.Black;

        public override PlotStyles DefaultPlotStyle => PlotStyles.Line;
    }    
}

