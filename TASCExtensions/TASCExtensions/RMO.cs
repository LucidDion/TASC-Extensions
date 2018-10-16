using QuantaculaCore;
using QuantaculaIndicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace TASCExtensions
{
    public class RMO : IndicatorBase
    {
        //parameterless constructor
        public RMO() : base()
        {
        }

        //for code based construction
        public RMO(TimeSeries source, int lpPeriod, int hpPeriod)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = lpPeriod;
            Parameters[2].Value = hpPeriod;
            Populate();
        }

        //Name
        public override string Name
        {
            get
            {
                return "Recursive Median Oscillator";
            }
        }

        //Abbreviation
        public override string Abbreviation
        {
            get
            {
                return "RMO";
            }
        }

        //Help description
        public override string HelpDescription
        {
            get
            {
                return "John Ehlers' Recursive Median Oscillator indicator from the March 2018 issue of Technical Analysis of Stocks & Commodities magazine.";
            }
        }

        //Plot in new pane
        public override string PaneTag
        {
            get
            {
                return "RMO";
            }
        }

        //color
        public override Color DefaultColor
        {
            get
            {
                return Color.DarkRed;
            }
        }

        //populate
        public override void Populate()
        {
            TimeSeries source = Parameters[0].AsTimeSeries;
            Int32 lpPeriod = Parameters[1].AsInt;
            Int32 hpPeriod = Parameters[1].AsInt;
            DateTimes = source.DateTimes;
            if (hpPeriod >= source.Count || lpPeriod >= source.Count || hpPeriod < 3)
                return;

            //highpass filter cyclic components
            double rad = 0.707 * 360.0 / hpPeriod;
            rad = rad.ToRadians();
            double alpha2 = (Math.Cos(rad) + Math.Sin(rad) - 1.0) / Math.Cos(rad);

            //obtain RMF
            IndicatorBase rmf = new RMF(source, lpPeriod);

            //calculate RMO
            for (int n = hpPeriod - 1; n < source.Count; n++)
            {
                double rmo1 = Values[n - 1];
                double rmo2 = Values[n - 2];
                double rmo = (1.0 - alpha2 / 2.0) * (1.0 - alpha2 / 2.0) * (rmf[n] - 2.0 * rmf[n - 1] + rmf[n - 2]);
                if (!Double.IsNaN(rmo1) && !Double.IsNaN(rmo2))
                {
                    rmo += 2.0 * (1.0 - alpha2) * rmo1;
                    rmo -= (1.0 - alpha2) * (1.0 - alpha2) * rmo2;
                }
                Values[n] = rmo;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterTypes.TimeSeries, PriceComponents.Close);
            AddParameter("LP Period", ParameterTypes.Int32, 12);
            AddParameter("HP Period", ParameterTypes.Int32, 30);
        }
    }
}