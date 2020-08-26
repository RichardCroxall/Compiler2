using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Compiler2.Calculations
{
    public class SunSetRise
    {
        const int MiddayMinutes = 12 * 60;

        public TimeSpan Sunset { get; private set; }
        public TimeSpan Sunrise { get; private set; }

        public SunSetRise(double latitudeDegrees, double longitudeDegrees, DateTime date)
        {
            Debug.Assert(latitudeDegrees <= 90);
            Debug.Assert(latitudeDegrees >= -90);
            Debug.Assert(longitudeDegrees <= 180);
            Debug.Assert(longitudeDegrees >= -180);

            DateTime endOfYear = new DateTime(date.Year, 12, 31);
            int daysInYear = endOfYear.DayOfYear;
            Debug.Assert(daysInYear >= 365 && daysInYear <= 366);


            double fractionalYearGamma = 2 * Math.PI / daysInYear * (date.DayOfYear - 1 + (date.Hour - 12) / 24.0);

            double equationOfTimeMinutes = 229.18 * (0.000075
                                                     + 0.001868 * Math.Cos(fractionalYearGamma)
                                                     - 0.032077 * Math.Sin(fractionalYearGamma)
                                                     - 0.014615 * Math.Cos(2.0 * fractionalYearGamma)
                                                     - 0.040849 * Math.Sin(2.0 * fractionalYearGamma));
            double solarDeclinationRadians = 0.006918
                                      - 0.399912 * Math.Cos(fractionalYearGamma)
                                      + 0.070257 * Math.Sin(fractionalYearGamma)
                                      - 0.006758 * Math.Cos(2.0 * fractionalYearGamma)
                                      + 0.000907 * Math.Sin(2.0 * fractionalYearGamma)
                                      - 0.002697 * Math.Cos(3.0 * fractionalYearGamma)
                                      - 0.00148 * Math.Sin(3.0 * fractionalYearGamma);

            double solarDeclinationDegrees = Degrees(solarDeclinationRadians);
            const double maxSunDeclinationDegrees = 23.45;
            Debug.Assert(solarDeclinationDegrees <= maxSunDeclinationDegrees);
            Debug.Assert(solarDeclinationDegrees >= -maxSunDeclinationDegrees);

            double sunriseZenithRadians = Radians(90.833);
            double haRadians = Math.Acos(
                Math.Cos(sunriseZenithRadians) / (Math.Cos(Radians(latitudeDegrees)) * Math.Cos(solarDeclinationRadians))
                - Math.Tan(Radians(latitudeDegrees)) * Math.Tan(solarDeclinationRadians));

            double sunriseMinutes = MiddayMinutes - 4 * (longitudeDegrees + Degrees(haRadians)) - equationOfTimeMinutes;
            Sunrise = TimeSpan.FromMinutes(sunriseMinutes);
            double sunsetMinutes = MiddayMinutes - 4 * (longitudeDegrees - Degrees(haRadians)) - equationOfTimeMinutes;
            Sunset = TimeSpan.FromMinutes(sunsetMinutes);
        }


        private static double Radians(double degrees)
        {
            double radians = degrees * Math.PI / 180.0;
            return radians;
        }

        private static double Degrees(double radians)
        {
            double degrees = radians * 180.0 / Math.PI;
            return degrees;
        }
    }
}
