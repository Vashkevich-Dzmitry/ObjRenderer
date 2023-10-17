using System.Globalization;

namespace ObjRenderer.Helpers
{
    public static class StringExtension
    {
        public static double ToDouble(this string s)
        {
            return double.Parse(s, CultureInfo.InvariantCulture);
        }

        public static float ToFloat(this string s)
        {
            return float.Parse(s, CultureInfo.InvariantCulture);
        }

        public static float? ToNullableFloat(this string s)
        {
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : null;
        }

        public static int ToInt(this string s)
        {
            return int.Parse(s, CultureInfo.InvariantCulture);
        }

        public static int? ToNullableInt(this string s)
        {
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : null;
        }
    }
}
