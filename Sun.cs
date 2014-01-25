using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReflectedLightAnalysis
{
    class Sun
    {
        /// <summary>
        /// 太阳倾角
        /// </summary>
        private double angle;

        public double Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        /// <summary>
        /// 太阳高度角
        /// </summary>
        private double elevation;

        public double Elevation
        {
            get { return elevation; }
            set { elevation = value; }
        }

        public double CalcElevation(double latitude, double longitude, double timeInHour)
        {
            Geometry geo = new Geometry();
             
            elevation = Math.Asin(
                Math.Sin(angle) *
                Math.Sin(latitude)
                + Math.Cos(angle) *
                Math.Cos(latitude) *
                Math.Cos(geo.angle2Rad(15 * timeInHour + geo.rad2Angle(longitude) - 300))
            );
            return elevation; 
        }

        /// <summary>
        /// 太阳方位角
        /// </summary>
        private double azimuth;

        public double Azimuth
        {
            get { return azimuth; }
            set { azimuth = value; }
        }

        public double CalcAzimuth(double latitude, double longitude, double timeInHour)
        {
            Geometry geo = new Geometry();

            double sinAlpha = -1 * Math.Cos(angle) *
                   Math.Sin(geo.angle2Rad(15 * timeInHour + geo.rad2Angle(longitude) - 300)) /
                   Math.Cos(elevation);

            double cosAlpha = (Math.Sin(elevation) * Math.Sin(latitude) - Math.Sin(angle)) / 
                Math.Cos(elevation) / Math.Cos(latitude);


            if (sinAlpha >= 0)
            {
                 azimuth = Math.Acos(cosAlpha);
            }
            else
            {
                if (cosAlpha > 0)
                {
                    azimuth = Math.Asin(sinAlpha);
                }
                else
                {
                    azimuth = Math.Acos(cosAlpha) * (-1.0);
                }
            }
            return azimuth;
        }
        
    }
}
