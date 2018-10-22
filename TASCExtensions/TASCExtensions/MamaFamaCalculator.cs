using QuantaculaCore;
using System;

namespace TASCExtensions
{
    public class MamaFamaCalculator
    {
        //generate the indicators in the constructor
        public MamaFamaCalculator(TimeSeries source, double fast, double slow)
        {
            //create timeseries for indicators
            _mama = new TimeSeries(source.DateTimes);
            _fama = new TimeSeries(source.DateTimes);

            //create series, zero fill
            TimeSeries smooth = new TimeSeries(source.DateTimes, 0.0);
            TimeSeries detrender = new TimeSeries(source.DateTimes, 0.0);
            TimeSeries period = new TimeSeries(source.DateTimes, 0.0);
            TimeSeries Q1 = new TimeSeries(source.DateTimes, 0.0);
            TimeSeries I1 = new TimeSeries(source.DateTimes, 0.0);
            TimeSeries jI = new TimeSeries(source.DateTimes, 0.0);
            TimeSeries jQ = new TimeSeries(source.DateTimes, 0.0);
            TimeSeries I2 = new TimeSeries(source.DateTimes, 0.0);
            TimeSeries Q2 = new TimeSeries(source.DateTimes, 0.0);
            TimeSeries Re = new TimeSeries(source.DateTimes, 0.0);
            TimeSeries Im = new TimeSeries(source.DateTimes, 0.0);
            TimeSeries smoothPeriod = new TimeSeries(source.DateTimes, 0.0);
            TimeSeries phase = new TimeSeries(source.DateTimes, 0.0);
            double deltaPhase = 0;
            double alpha = 0;

            //calculate MAMA
            for (int n = 6; n < source.Count; n++)
            {
                smooth[n] = (4.0 * source[n] + 3.0 * source[n - 1] + 2.0 * source[n - 2] + source[n - 3]) / 10.0;
                detrender[n] = (0.0962 * smooth[n] + 0.5769 * smooth[n - 2] - 0.5769 * smooth[n - 4] - 0.0962 * smooth[n - 6]) * (0.075 * period[n - 1] + 0.54);

                //compute in-phase and quadrature components
                Q1[n] = (0.0962 * detrender[0] + 0.5769 * detrender[n - 2] - 0.5769 * detrender[n - 4] - 0.0962 * detrender[n - 6]) * (0.075 * period[n - 1] + 0.54);
                I1[n] = detrender[n - 3];

                //advance the phase of I1 and Q1 by 90 degrees
                jI[n] = (0.0962 * I1[n] + 0.5769 * I1[n - 2] - 0.5769 * I1[n - 4] - 0.0962 * I1[n - 6]) * (0.075 * period[n - 1] + 0.54);
                jQ[n] = (0.0962 * Q1[n] + 0.5769 * Q1[n - 2] - 0.5769 * Q1[n - 4] - 0.0962 * Q1[n - 6]) * (0.075 * period[n - 1] + 0.54);

                //phasor addition for 3 bar averaging
                I2[n] = I1[n] - jQ[n];
                Q2[n] = Q1[n] + jI[n];

                //smooth the I and Q components before applying the discriminator
                I2[n] = 0.2 * I2[n] + 0.8 * I2[n - 1];
                Q2[n] = 0.2 * Q2[n] + 0.8 * Q2[n - 1];

                //homodyne discriminator
                Re[n] = I2[n] * I2[n - 1] + Q2[n] * Q2[n - 1];
                Im[n] = I2[n] * Q2[n - 1] - Q2[n] * I2[n - 1];
                Re[n] = 0.2 * Re[n] + 0.8 * Re[n - 1];
                Im[n] = 0.2 * Im[n] + 0.8 * Im[n - 1];
                if (Im[n] != 0 && Re[n] != 0)
                {
                    double atn = Math.Atan(Im[n] / Re[n]);
                    atn = atn.ToDegrees();
                    period[n] = 360.0 / atn;
                }
                if (period[n] > 1.5 * period[n - 1])
                    period[n] = 1.5 * period[n - 1];
                if (period[n] < 0.67 * period[n - 1])
                    period[n] = 0.67 * period[n - 1];
                if (period[n] < 6.0)
                    period[n] = 6.0;
                if (period[n] > 50.0)
                    period[n] = 50.0;
                period[n] = 0.2 * period[n] + 0.8 * period[n - 1];
                smoothPeriod[n] = 0.33 * period[n] + 0.67 * smoothPeriod[n - 1];

                if (I1[n] != 0)
                {
                    double atn = Q1[n] / I1[n];
                    atn = atn.ToDegrees();
                    phase[n] = atn;
                }
                deltaPhase = phase[n - 1] - phase[n];
                if (deltaPhase < 1.0)
                    deltaPhase = 1.0;
                alpha = fast / deltaPhase;
                if (alpha < slow)
                    alpha = slow;
                if (alpha > fast)
                    alpha = fast;
                _mama.Values[n] = alpha * source[n];
                if (!Double.IsNaN(_mama.Values[n - 1]))
                    _mama.Values[n] += (1.0 - alpha) * _mama.Values[n - 1];
                _fama.Values[n] = 0.5 * alpha * _mama.Values[n];
                if (!Double.IsNaN(_fama.Values[n - 1]))
                    _fama.Values[n] += (1.0 - 0.5 * alpha) * _fama.Values[n - 1];
            }
        }

        //access the indicator time series
        public TimeSeries MAMA => _mama;
        public TimeSeries FAMA => _fama;

        //private members
        private TimeSeries _mama;
        private TimeSeries _fama;
    }
}