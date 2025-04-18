namespace Asteria
{
    public static class StringExtension
    {
        public static bool CustomStartsWith(this string a, string b)
        {
            var aLen = a.Length;
            var bLen = b.Length;
            var ap = 0;
            var bp = 0;

            while (ap < aLen && bp < bLen && a[ap] == b[bp])
            {
                ap++;
                bp++;
            }

            return bp == bLen && aLen >= bLen || ap == aLen && bLen >= aLen;
        }

        public static bool CustomEndsWith(this string a, string b)
        {
            var ap = a.Length - 1;
            var bp = b.Length - 1;

            while (ap >= 0 && bp >= 0 && a[ap] == b[bp])
            {
                ap--;
                bp--;
            }

            return bp < 0 && a.Length >= b.Length || ap < 0 && b.Length >= a.Length;
        }
    }
}
