using UnityEngine;

namespace LocalizationTool.Commons
{
    public abstract class Colors
    {
        public static readonly Color SUPER_DEEP_GRAY = new Color(150 / 255f, 150 / 255f, 150 / 255f, 1f);
        public static readonly Color DEEP_GRAY = new Color(180 / 255f, 180 / 255f, 180 / 255f, 1f);
        public static readonly Color DEEP_GRAY_A = new Color(180 / 255f, 180 / 255f, 180 / 255f, .5f);
        public static readonly Color DARK_GRAY = new Color(230 / 255f, 230 / 255f, 230 / 255f, 1f);
        public static readonly Color LIGHT_TURKEASE = new Color(170 / 255f, 225 / 255f, 230 / 255f, 1f);
        public static readonly Color LIGHT_RED = new Color(255 / 255f, 210 / 255f, 210 / 255f, 1f);
        public static readonly Color DEEP_DEEP_GRAY = new Color(50 / 255f, 50 / 255f, 50 / 255f, 1f);

        private Colors()
        {
        }

        public static Color Lighten(Color color, float intensity = 1f)
        {
            return Color.Lerp(color, Color.white, intensity);
        }

        public static Color Darken(Color color, float intensity = 1f)
        {
            return Color.Lerp(color, Color.black, intensity);
        }

        public static Color Alpha(Color color, float alpha = 1f)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static Color Invert(Color color, bool alphaChannel = false)
        {
            var invertedColor = new Color();
            if (alphaChannel)
            {
                invertedColor.a = 255 - color.a;
            }

            invertedColor.r = 255 - color.r;
            invertedColor.g = 255 - color.g;
            invertedColor.b = 255 - color.b;
            return invertedColor;
        }
    }
}