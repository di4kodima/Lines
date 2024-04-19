namespace Линии
{
    internal class WaveConverter
    {
        public static void WavelengthToRGB(double Wavelength, out int R, out int G, out int B)
        {
            double Red = 0;
            double Green = 0;
            double Blue = 0;
            double Gamma = 0.80;
            int IntensityMax = 255;
            double factor = 1;
            if ((Wavelength < 380) || (Wavelength > 780))
            {
                R = 0; G = 0; B = 0;
                return;
            }
            if ((Wavelength >= 380) & (Wavelength < 440))
            {
                Red = -(Wavelength - 440) / (440 - 380);
                Green = 0.0;
                Blue = 1.0;
            }
            else
            if ((Wavelength >= 440) & (Wavelength < 490))
            {
                Red = 0.0;
                Green = (Wavelength - 440) / (490 - 440);
                Blue = 1.0;
            }
            else
            if ((Wavelength >= 490) & (Wavelength < 510))
            {
                Red = 0.0;
                Green = 1.0;
                Blue = -(Wavelength - 510) / (510 - 490);
            }
            else
            if ((Wavelength >= 510) & (Wavelength < 580))
            {
                Red = (Wavelength - 510) / (580 - 510);
                Green = 1.0;
                Blue = 0.0;
            }
            else
            if ((Wavelength >= 580) & (Wavelength < 645))
            {
                Red = 1.0;
                Green = -(Wavelength - 645) / (645 - 580);
                Blue = 0.0;
            }
            else
            if ((Wavelength >= 645) & (Wavelength <= 780))
            {
                Red = 1.0;
                Green = 0.0;
                Blue = 0.0;
            }
            if ((Wavelength >= 380) & (Wavelength < 420))
                factor = 0.3 + 0.7 * (Wavelength - 380) / (420 - 380);
            else
            if ((Wavelength >= 420) & (Wavelength < 700)) factor = 1.0;
            else
            if ((Wavelength >= 700) & (Wavelength <= 780))
                factor = 0.3 + 0.7 * (780 - Wavelength) / (780 - 700);
            if (Red > 0) R = (int)(IntensityMax * Math.Pow(Red * factor, Gamma));
            else R = 0;
            if (Green > 0) G = (int)(IntensityMax * Math.Pow(Green * factor, Gamma));
            else G = 0;
            if (Blue > 0) B = (int)(IntensityMax * Math.Pow(Blue * factor, Gamma));
            else B = 0;
        }
    }
}
