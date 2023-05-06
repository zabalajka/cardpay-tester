using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TesterWeb.Utils
{
    public static class Extensions
    {
        public static StringBuilder AddQuery(this StringBuilder sb, string key, string value)
        {
            bool isFirst = !sb.ToString().Contains('?'); // :(
            sb.Append('\n');
            sb.Append(isFirst ? '?' : '&');
            sb.Append(key).Append('=').Append(WebUtility.UrlEncode(value));
            return sb;
        }

        public static StringBuilder AddQueryOptional(this StringBuilder sb, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return sb;
            }

            return sb.AddQuery(key, value);
        }
    }
}