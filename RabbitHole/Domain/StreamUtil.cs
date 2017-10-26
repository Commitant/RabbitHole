using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitHole
{
    public class StreamUtil
    {
        public static byte[] ReadBytes(Stream stream, int noOfBytes)
        {
            byte[] buffer = new byte[noOfBytes];
            stream.Read(buffer, 0, noOfBytes);
            return buffer;
        }

        public static byte[] ReadBytes(Stream stream, int noOfBytes, byte[] buffer)
        {          
            stream.Read(buffer, 0, noOfBytes);
            return buffer;
        }

        public static void  WriteBytes(Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length); 
        }

        
    }
}
