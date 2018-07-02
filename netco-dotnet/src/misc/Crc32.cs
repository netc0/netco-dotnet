using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace netco {
    public class Crc32{  
        static protected ulong[] Crc32Table;
        //生成CRC32码表  
        static public void GetCRC32Table() {  
            ulong Crc;  
            Crc32Table = new ulong[256];  
            int i, j;  
            for (i = 0; i < 256; i++) {  
                Crc = (ulong)i;  
                for (j = 8; j > 0; j--) {  
                    if ((Crc & 1) == 1)  
                        Crc = (Crc >> 1) ^ 0xEDB88320;  
                    else  
                        Crc >>= 1;  
                }  
                Crc32Table[i] = Crc;  
            }  
        }  
        //获取字符串的CRC32校验值  
        static public UInt32 GetCRC32Str(string sInputString) {  
            //生成码表  
            GetCRC32Table();  
            byte[] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(sInputString); ulong value = 0xffffffff;  
            int len = buffer.Length;  
            for (int i = 0; i < len; i++) {  
                value = (value >> 8) ^ Crc32Table[(value & 0xFF) ^ buffer[i]];  
            }  
            return (UInt32)(value ^ 0xffffffff);
        }  
    }  
}
