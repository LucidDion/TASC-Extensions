using System;
using System.Drawing;
using QuantaculaCore;
using QuantaculaIndicators;

namespace TASCIndicators
{
    public class CorrelationAngle : IndicatorBase
    {
        public override string Name
        {
            get
            {
                return "CorrelationAngle";
            }
        }
        public override string Abbreviation
        {
            get
            {
                return "CorrelationAngle";
            }
        }
        public override string HelpDescription
        {
            get
            {
                return "The CorrelationAngle indicator by Dr. John Ehlers from S&C June 2020 issue could help to identify the onset of a trend or the switch to a cyclic mode.";
            }
        }
        public override string PaneTag
        {
            get
            {
                return "CorrelationAngle";
            }
        }
        public override Color DefaultColor
        {
            get
            {
                return Color.DarkBlue;
            }
        }

        //it's not a smoother
        public override bool IsSmoother => false;

        public CorrelationAngle()
        {
        }
        public CorrelationAngle(TimeSeries ds, int period = 14)
        {
            base.Parameters[0].Value = ds;
			base.Parameters[1].Value = period;
            this.Populate();
        }
        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close); 
            base.AddParameter("Lookback Period", ParameterTypes.Int32, 14);
        }
        public override void Populate()
        {
            TimeSeries ds = base.Parameters[0].AsTimeSeries;
            int period = base.Parameters[1].AsInt;

            this.DateTimes = ds.DateTimes;
            int FirstValidValue = period;
            if (ds.Count < FirstValidValue)
            {
                return;
            }
			
            TimeSeries Real = new TimeSeries(DateTimes);
			TimeSeries Imag = new TimeSeries(DateTimes);
            TimeSeries Angle = new TimeSeries(DateTimes);

            for (int idx = 0; idx < ds.Count; idx++)
			{
                Real[idx] = 0d; Imag[idx] = 0d; Angle[idx] = 0d;
            }
			
            for (int bar = 0; bar < ds.Count; bar++)
            {
                double Sx = 0, Sy = 0, Sxx = 0, Sxy = 0, Syy = 0;

                for (int count = 0; count < period; count++)
                {
                    var X = (bar - count > 0) ? ds[bar - count - 1] : ds[0];
                    var Y = Math.Cos(360 * (count - 1) / period) * (180 / Math.PI);
                    Sx += X;
                    Sy += Y;
                    Sxx += (X * X);
                    Sxy += (X * Y);
                    Syy += (Y * Y);
                }

                if ((((period * Sxx) - (Sx * Sx)) > 0) & (((period * Syy) - (Sy * Sy)) > 0))
                {
					Real[bar] = (period * Sxy - Sx * Sy) / Math.Sqrt((period * Sxx - Sx * Sx) * (period * Syy - Sy * Sy));
                }
				
				Sx = 0; Sy = 0; Sxx = 0; Sxy = 0; Syy = 0;
				
				for (int count = 0; count < period; count++)
                {
                    var X = (bar - count > 0) ? ds[bar - count - 1] : ds[0];
                    var Y = -Math.Sin(360 * (count - 1) / period) * (180 / Math.PI);
                    Sx += X;
                    Sy += Y;
                    Sxx += (X * X);
                    Sxy += (X * Y);
                    Syy += (Y * Y);
                }
				
				if ((period * Sxx - Sx * Sx > 0) && (period * Syy - Sy * Sy > 0))
                Imag[bar] = (period * Sxy - Sx * Sy) / Math.Sqrt((period * Sxx - Sx * Sx) * (period * Syy - Sy * Sy));				
				
                //Compute the angle as an arctangent function and resolve ambiguity
                if (Imag[bar] != 0)
                    Angle[bar] = 90 + Math.Atan(Real[bar] / Imag[bar]) * (180 / Math.PI);
                if (Imag[bar] > 0)
                    Angle[bar] = Angle[bar] - 180;

                //Do not allow the rate change of angle to go negative
                if (bar == 0)
                    Angle[bar] = Angle[0];
                else
                if ((Angle[bar - 1] - Angle[bar] < 270) && (Angle[bar] < Angle[bar - 1]))
                    Angle[bar] = Angle[bar - 1];

                base.Values[bar] = Angle[bar];
            }
        }
    }
}
