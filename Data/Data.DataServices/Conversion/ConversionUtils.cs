using System;
using System.Net;

namespace Data.DataServices.Conversion
{
    static class ConversionUtils
    {
        public static uint IpStrToUInt32(string ipStr)
        {
            IPAddress address = IPAddress.Parse(ipStr);
            return BitConverter.ToUInt32(address.GetAddressBytes(), 0);
        }

        public static string UInt32ToIpStr(uint address)
        {
            return new IPAddress(BitConverter.GetBytes(address)).ToString();
        }
    }
}