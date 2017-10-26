using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitHole
{
    public class File
    {
        public String Name { get; set; }

        public byte[] Data { get; set; }

        public void Serialize(Stream stream)
        {
            stream.WriteByte(BitConverter.GetBytes(Name.Length)[0]); //write file name length as one byte
            StreamUtil.WriteBytes(stream, Encoding.UTF8.GetBytes(Name)); //write name as bytes
            StreamUtil.WriteBytes(stream, BitConverter.GetBytes(Data.Length)); //write data length as 4 bytes
            StreamUtil.WriteBytes(stream, Data); //write data as bytes        
        }

        public static File Deserialize(Stream stream)
        {
            File file = new File();
            int nameLength = stream.ReadByte();
            file.Name = Encoding.UTF8.GetString(StreamUtil.ReadBytes(stream, nameLength));
            int dataLength = BitConverter.ToInt32(StreamUtil.ReadBytes(stream, 4), 0);
            file.Data = StreamUtil.ReadBytes(stream, dataLength);

            return file;
        }
    }
}
