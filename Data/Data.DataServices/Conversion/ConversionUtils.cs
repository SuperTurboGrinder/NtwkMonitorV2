using System;
using System.Net;

namespace Data.DataServices.Conversion {

class ConversionUtils {
    public static uint IPStrToUInt32(string ipStr) {
        IPAddress addr = IPAddress.Parse(ipStr);
        return System.BitConverter.ToUInt32(addr.GetAddressBytes(), 0);
    }

    public static string UInt32ToIPStr(uint addr) {
        return new IPAddress(BitConverter.GetBytes(addr)).ToString();
    }

    /*public static DateTime DateTimeFromJSDateTime(double jsDateTime) {
        DateTime baseDateTime = new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);
        TimeSpan time = TimeSpan.FromMilliseconds(jsDateTime);
        return baseDateTime + time;
    }

    public static double JSDateTimeFromDateTime(DateTime dateTime) {
        return dateTime
               .Subtract(new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc))
               .TotalMilliseconds;
    }*/
}

}