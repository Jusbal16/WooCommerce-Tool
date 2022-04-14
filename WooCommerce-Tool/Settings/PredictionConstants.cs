using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WooCommerce_Tool.Settings
{
    public class PredictionConstants
    {
        public List<string> ForecastingMethods => new List<string>() { "FastTree", "FastForest", "FastTreeTweedie", "LbfgsPoissonRegression", "Gam" };
        public int DataNormalizationMin => 0;
        public int DataNormalizationMax => 10;
        public int ForecastingPeriod => 3;

        // visualation
        public string Blue => "#FF2195F2";
        public string Red => "#FFF34336";
        public string yellow => "#FFFEC007";
        public string Grey => "#FF607D8A";
        public double OpacityConstant => 0.15;
        public SolidColorBrush OrderCountBrush => (SolidColorBrush)new BrushConverter().ConvertFrom(Blue);
        public SolidColorBrush OrderCountBrushFill => new SolidColorBrush { Color = ConvertStringToColor(Blue), Opacity = OpacityConstant };
        public SolidColorBrush TimeSeriesBrush => (SolidColorBrush)new BrushConverter().ConvertFrom(Red);
        public SolidColorBrush TimeSeriesBrushFill => new SolidColorBrush { Color = ConvertStringToColor(Red), Opacity = OpacityConstant };
        public SolidColorBrush RegresionBrush => (SolidColorBrush)new BrushConverter().ConvertFrom(yellow);
        public SolidColorBrush RegresionBrushFill => new SolidColorBrush { Color = ConvertStringToColor(yellow), Opacity = OpacityConstant };
        public SolidColorBrush NNBrush => (SolidColorBrush)new BrushConverter().ConvertFrom(Grey);
        public SolidColorBrush NNBrushFill => new SolidColorBrush { Color = ConvertStringToColor(Grey), Opacity = OpacityConstant };
        public Color ConvertStringToColor(String hex)
        {
            //remove the # at the front
            hex = hex.Replace("#", "");

            byte a = 255;
            byte r = 255;
            byte g = 255;
            byte b = 255;

            int start = 0;

            //handle ARGB strings (8 characters long)
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                start = 2;
            }

            //convert RGB characters to bytes
            r = byte.Parse(hex.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }

    }
}
