using System;
using System.Linq;

namespace TelcobrightMediation
{
    public static class HuaweiBinCdr
    {
        public static int BitStringToIntFromByte(byte a, int startBitPosition, byte bitLengthToRead)
        {
            long aa = 0;
            a >>= startBitPosition - 1;
            switch (bitLengthToRead)
            {
                case 1:
                    byte b = 1;
                    aa = a & b;
                    break;
                case 2:
                    b = 3;
                    aa = a & b;
                    break;
                case 3:
                    b = 7;
                    aa = a & b;
                    break;
                case 4:
                    b = 15;
                    aa = a & b;
                    break;
                case 5:
                    b = 31;
                    aa = a & b;
                    break;
                case 6:
                    b = 63;
                    aa = a & b;
                    break;
                case 7:
                    b = 127;
                    aa = a & b;
                    break;
                case 8:
                    b = 255;
                    aa = a & b;
                    break;
            }//switch
            return Convert.ToInt32(aa);
        }
        public static string ByteArrayToDateTime(byte[] a)
        {
            string yy = a[0].ToString("D2");
            string MM = a[1].ToString("D2");
            string dd = a[2].ToString("D2");
            string hh = a[3].ToString("D2");
            string mm = a[4].ToString("D2");
            string ss = a[5].ToString("D2");
            string fullDataTime = "20" + yy + "-" + MM + "-" + dd + " " + hh + ":" + mm + ":" + ss;
            return fullDataTime;
        }
        public static UInt16 ByteArrayToUInt16(byte[] a)
        {
            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. 
            if (BitConverter.IsLittleEndian) Array.Reverse(a);
            UInt16 i = BitConverter.ToUInt16(a, 0);
            return i;
        }
        public static UInt32 ByteArraySingleToInt(byte[] a)
        {
            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. 
            if (BitConverter.IsLittleEndian) Array.Reverse(a);
            //UInt16 i = BitConverter.ToUInt16(a, 0);
            byte b = a[0];
            UInt32 i = Convert.ToUInt32(b);
            return i;
        }
        public static UInt16 ByteArrayToUInt16BigIndian(byte[] a)
        {
            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. 
            if (BitConverter.IsLittleEndian)  Array.Reverse(a);
            UInt16 i = BitConverter.ToUInt16(a, 0);
            return i;
        }
        public static UInt32 ByteArrayToUInt32(byte[] a)
        {
            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. if (BitConverter.IsLittleEndian) Array.Reverse(a);
            UInt32 i = BitConverter.ToUInt32(a, 0);
            return i;
        }
       
        public static string ByteArrayToIpAddress(byte[] a)
        {
            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. 
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(a);
            int cnt = a.Count();
            int i = 0;
            string ipAddress = "";
            for (i = 0; i < cnt; i++)
            {
                ipAddress += a[i].ToString() + ".";
            }
            //trim last dot and return
            return ipAddress.Substring(0, ipAddress.Length - 1);
        }
        public static string ByteArrayToIpAddressReversedOctets(byte[] a)
        {
            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. 
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(a);
            int cnt = a.Count();
            int i = 0;
            string ipAddress = "";
            for (i = 0; i < cnt; i++)
            {
                ipAddress += a[i].ToString() + ".";
            }
            //trim last dot and return
            return string.Join(".", ipAddress.Substring(0, ipAddress.Length - 1)
                .Split('.').Reverse());

        }
        public static string ByteArrayToZteSpc(byte[] a)
        {
            //taking two bytes only,one byte is for network indicator, have to figure out later
            byte[] b = new byte[2];
            b[0] = a[2];
            b[1] = a[1];
            Array.Reverse(b);
            UInt16 i = BitConverter.ToUInt16(b, 0);
            string spc = i.ToString();
            return spc;
        }
        public static string BcdToDigitString(byte[] a)
        {
            string str = BitConverter.ToString(a).Replace("-", "");
            int posF = str.IndexOf('F');
            str = str.Substring(0, posF);
            //str = str.Split('F')[0];
            return str;
        }
        public static string BcdToDigitStringZte(byte[] a)
        {
            string str = BitConverter.ToString(a).Replace("-", "");
            //swap the hex values within bytes first
            ////debug
            //if (str.Substring(0, 6) == "881027")
            //{
            //    int r = 1;
            //}
            char[] c = str.ToCharArray();
            int j = 0;
            for (j = 0; j <= 30; j += 2)
            {
                char swap = c[j];
                c[j] = c[j + 1];
                c[j + 1] = swap;
                //if filler 0 is found, break
                if ((c[j] == '0') || (c[j + 1] == '0'))
                {
                    break;
                }
            }
            str = new string(c);
            int posF = str.IndexOf('0'); //in ZTE 0 is filler
            str = str.Substring(0, posF);
            str = str.Replace('A', '0');
            str = str.Replace('B', '*');
            str = str.Replace('C', '#');
            //ZTE support character A-F in BCD digit:
            //BCD Special code notation: 
            //The value 0xA is equal to “ 0” 
            //The value 0xB is equal to “ *” 
            //The value 0xC is equal to “#”
            //The value 0xD is equal to “D” 
            //The value 0xE is equal to “ E” 
            //The value 0xF is equal to “ F” 
            return str;
        }
        public static UInt32 ByteArrayToZteSeconds(byte[] a)
        {
            Array.Reverse(a);
            UInt32 i = BitConverter.ToUInt32(a, 0);
            return i;
        }
    }
    public static class BinCdr
    {
        public static int BitStringToIntFromByte(byte a, int startBitPosition, byte bitLengthToRead)
        {
            long aa = 0;
            a >>= startBitPosition - 1;
            switch (bitLengthToRead)
            {
                case 1:
                    byte b = 1;
                    aa = a & b;
                    break;
                case 2:
                    b = 3;
                    aa = a & b;
                    break;
                case 3:
                    b = 7;
                    aa = a & b;
                    break;
                case 4:
                    b = 15;
                    aa = a & b;
                    break;
                case 5:
                    b = 31;
                    aa = a & b;
                    break;
                case 6:
                    b = 63;
                    aa = a & b;
                    break;
                case 7:
                    b = 127;
                    aa = a & b;
                    break;
                case 8:
                    b = 255;
                    aa = a & b;
                    break;
            }//switch
            return Convert.ToInt32(aa);
        }
        public static string ByteArrayToDateTime(byte[] a)
        {
            string yy = a[0].ToString("D2");
            string MM = a[1].ToString("D2");
            string dd = a[2].ToString("D2");
            string hh = a[3].ToString("D2");
            string mm = a[4].ToString("D2");
            string ss = a[5].ToString("D2");
            string fullDataTime = "20" + yy + "-" + MM + "-" + dd + " " + hh + ":" + mm + ":" + ss;
            return fullDataTime;
        }
        public static UInt16 ByteArrayToUInt16(byte[] a)
        {
            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. 
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(a);
            UInt16 i = BitConverter.ToUInt16(a, 0);
            return i;
        }
        public static UInt32 ByteArraySingleToInt(byte[] a)
        {
            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. 
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(a);
            //UInt16 i = BitConverter.ToUInt16(a, 0);
            byte b = a[0];
            UInt32 i = Convert.ToUInt32(b);
            return i;
        }
        public static UInt16 ByteArrayToUInt16BigIndian(byte[] a)
        {
            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. 
            Array.Reverse(a);
            UInt16 i = BitConverter.ToUInt16(a, 0);
            return i;
        }
        public static UInt32 ByteArrayToUInt32(byte[] a)
        {
            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. 
            if (BitConverter.IsLittleEndian) Array.Reverse(a);
            UInt32 i = BitConverter.ToUInt32(a, 0);
            return i;
        }
        public static UInt32 ByteArrayToUInt32BigEndian(byte[] a)
        {
            // reverse the bytearray first for big endian
            Array.Reverse(a);
            UInt32 i = BitConverter.ToUInt32(a, 0);
            return i;
        }
        public static string ByteArrayToIpAddress(byte[] a)
        {
            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. 
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(a);
            int cnt = a.Count();
            int i = 0;
            string ipAddress = "";
            for (i = 0; i < cnt; i++)
            {
                ipAddress += a[i].ToString() + ".";
            }
            //trim last dot and return
            return ipAddress.Substring(0, ipAddress.Length - 1);
        }
        public static string ByteArrayToIpAddressReversedOctets(byte[] a)
        {
            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. 
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(a);
            int cnt = a.Count();
            int i = 0;
            string ipAddress = "";
            for (i = 0; i < cnt; i++)
            {
                ipAddress += a[i].ToString() + ".";
            }
            //trim last dot and return
            return string.Join(".", ipAddress.Substring(0, ipAddress.Length - 1)
                .Split('.').Reverse());
                
        }
        public static string ByteArrayToZteSpc(byte[] a)
        {
            //taking two bytes only,one byte is for network indicator, have to figure out later
            byte[] b = new byte[2];
            b[0] = a[2];
            b[1] = a[1];
            Array.Reverse(b);
            UInt16 i = BitConverter.ToUInt16(b, 0);
            string spc = i.ToString();
            return spc;
        }
        public static string BcdToDigitString(byte[] a)
        {
            string str = BitConverter.ToString(a).Replace("-", "");
            int posF = str.IndexOf('F');
            str = str.Substring(0, posF);
            //str = str.Split('F')[0];
            return str;
        }
        public static string BcdToDigitStringZte(byte[] a)
        {
            string str = BitConverter.ToString(a).Replace("-", "");
            //swap the hex values within bytes first
            ////debug
            //if (str.Substring(0, 6) == "881027")
            //{
            //    int r = 1;
            //}
            char[] c = str.ToCharArray();
            int j = 0;
            for (j = 0; j <= 30; j += 2)
            {
                char swap = c[j];
                c[j] = c[j + 1];
                c[j + 1] = swap;
                //if filler 0 is found, break
                if ((c[j] == '0') || (c[j + 1] == '0'))
                {
                    break;
                }
            }
            str = new string(c);
            int posF = str.IndexOf('0'); //in ZTE 0 is filler
            str = str.Substring(0, posF);
            str = str.Replace('A', '0');
            str = str.Replace('B', '*');
            str = str.Replace('C', '#');
            //ZTE support character A-F in BCD digit:
            //BCD Special code notation: 
            //The value 0xA is equal to “ 0” 
            //The value 0xB is equal to “ *” 
            //The value 0xC is equal to “#”
            //The value 0xD is equal to “D” 
            //The value 0xE is equal to “ E” 
            //The value 0xF is equal to “ F” 
            return str;
        }
        public static UInt32 ByteArrayToZteSeconds(byte[] a)
        {
            Array.Reverse(a);
            UInt32 i = BitConverter.ToUInt32(a, 0);
            return i;
        }
    }
}
