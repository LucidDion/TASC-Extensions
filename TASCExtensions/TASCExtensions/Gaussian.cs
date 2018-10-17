using QuantaculaCore;
using System;
using System.Drawing;

namespace QuantaculaIndicators
{
    public class Gaussian : IndicatorBase
    {
        //parameterless constructor
        public Gaussian() : base()
        {
        }

        //for code based construction
        public Gaussian(TimeSeries source, Int32 period, Int32 poles)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = poles;

            Populate();
        }

        //name
        public override string Name
        {
            get
            {
                return "Gaussian";
            }
        }

        //abbreviation
        public override string Abbreviation
        {
            get
            {
                return "Gaussian";
            }
        }

        //description
        public override string HelpDescription
        {
            get
            {
                return "Gaussian is a smoothing filter presented by John Ehlers in 'Rocket Science for Traders' that "
                    + "rejects high frequencies (fast movements) better than an EMA, and it has lower lag. " 
                    + "\r\n\r\nA Gaussian filter with 1 pole is equivalent to an EMA filter with "
                    + "one-third the period; 2 poles is equivalent to EMA(EMA()), and so on up to 4 poles ";
            }
        }

        //price pane
        public override string PaneTag
        {
            get
            {
                return "Price";
            }
        }

        //default color
        public override Color DefaultColor
        {
            get
            {
                return Color.FromArgb(255, 0, 0, 0);
            }
        }

        //default plot style
        public override PlotStyles DefaultPlotStyle
        {
            get
            {
                return PlotStyles.Line;
            }
        }

        public override bool IsSmoother
        {
            get
            {
                return true; 
            }
        }

        //populate
        public override void Populate()
        {
            double c, c1, c2, c3, c4;

            TimeSeries source = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Int32 poles = Parameters[2].AsInt;

            DateTimes = source.DateTimes;

            int firstValidIndex = source.FirstValidIndex + 4;
            if (FirstValidIndex > source.Count) firstValidIndex = source.Count;

            double w = 2 * Math.PI / period; // omega
            double b = (1 - Math.Cos(w)) / (Math.Pow(2, 1.0 / poles) - 1);
            double a = -b + Math.Sqrt(b * b + 2 * b);
            double a1 = 1 - a;

            c4 = c3 = c2 = c1 = c = 0;
            switch (poles)
            {
                case 1:
                    c1 = a1;
                    c = a;
                    break;
                case 2:
                    c2 = -a1 * a1;
                    c1 = 2 * a1;
                    c = a * a;
                    break;
                case 3:
                    c3 = a1 * a1 * a1;
                    c2 = -3 * a1 * a1;
                    c1 = 3 * a1;
                    c = a * a * a;
                    break;
                case 4:
                    c4 = -a1 * a1 * a1 * a1;
                    c3 = 4 * a1 * a1 * a1;
                    c2 = -6 * a1 * a1;
                    c1 = 4 * a1;
                    c = a * a * a * a;
                    break;
            }

            for (int bar = 0; bar < firstValidIndex; bar++)
                this[bar] = source[bar];

            //modify the code below to implement your own indicator calculation
            for (int n = firstValidIndex; n < source.Count; n++)
            {
                Values[n] = c * source[n] + c1 * this[n - 1] + c2 * this[n - 2]
                                        + c3 * this[n - 3] + c4 * this[n - 4];
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("period", ParameterTypes.Int32, 50);
            AddParameter("poles", ParameterTypes.Int32, 3);

        }
    }
}
