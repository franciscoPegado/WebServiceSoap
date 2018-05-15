using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Casablanca.Business.Utils
{
    public static class Extensions
    {
        public static string Right(this string str, int length)
        {
            int startIndex = (str.Length >= length) ? (str.Length - length) : 0;
            int len = (str.Length >= (length - startIndex)) ? length : str.Length;
            return str.Substring(startIndex, len);
        }

        public static string Left(this string str, int length)
        {
            int len = (str.Length >= length) ? length : str.Length;
            return str.Substring(0, len);
        }
        
        public static string toString2(this object prString)
        {
            if (prString == null)
                return "";
            else
                return prString.ToString();
        }
        
        public static int toInt(this object prString)
        {
            if (prString == null)
                return 0;
            else
                return Convert.ToInt32(prString.ToString());
        }

        public static long toLong(this object prString)
        {
            if (prString == null)
                return 0;
            else
                return Convert.ToInt64(prString.ToString());
        }

        public static double toDouble(this object prString)
        {
            if (prString == null)
                return 0;
            else
                return Convert.ToDouble(prString.ToString());
        }

        public static decimal toDecimal(this object prString)
        {
            if (prString == null)
                return 0;
            else
                return Convert.ToDecimal(prString.ToString());
        }

        public static decimal toDecimal(this object prString, int prDecimais)
        {
            if (prString == null)
                return 0;
            else
                return System.Math.Round(Convert.ToDecimal(prString.ToString()), prDecimais);
        }

        public static DateTime toDataTime(this object prString)
        {
            if (prString == null)
                return DateTime.MinValue;
            else
                return Convert.ToDateTime(prString.ToString());
        }

        public static string toString2(this long prLong)
        {
            if (prLong == null)
                return "0";
            else
                return Convert.ToString(prLong);
        }

        public static bool builderToBool(this object prString)
        {
            if (prString == null)
                return false;

            return prString == "S";
        }
    }
}
