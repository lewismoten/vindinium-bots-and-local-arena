using System;
using System.Text;

namespace Vindinium.Common
{
    public static class MapFormatter
    {
        public static string FormatTokensAsMap(string tokens)
        {
            var size = (int) Math.Sqrt(tokens.Length/2.0);
            var sb = new StringBuilder();
            string border = string.Format("+{0}+", "-".PadLeft(size*2, '-'));
            sb.AppendLine(border);
            for (int i = 0; i < size; i++)
            {
                sb.Append("|");
                sb.Append(tokens.Substring(i*size*2, size*2));
                sb.AppendLine("|");
            }
            sb.Append(border);
            return sb.ToString();
        }
    }
}