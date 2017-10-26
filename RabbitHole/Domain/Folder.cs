using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitHole.Domain
{
    public class Folder
    {
        public List<File> Files { get; set; }

        public Folder()
        {
            Files = new List<File>();
        }

        public void Serialize(Stream stream)
        {
            StreamUtil.WriteBytes(stream, BitConverter.GetBytes(Files.Count)); //write no of files
            foreach (File file in Files)
            {
                file.Serialize(stream);
            }
        }

        public int GetTotalDataSize()
        {
            return Files.Sum(file => file.Data.Length);
        }


        public static Folder Deserialize(Stream stream)
        {
            Folder folder = new Folder();
            int noOfFiles = BitConverter.ToInt32(StreamUtil.ReadBytes(stream, 4), 0); 

            for (int i=0;i<noOfFiles;i++)
            {
                folder.Files.Add(File.Deserialize(stream));
            }

            return folder;
        }

        public bool ContainsFile(string fileName)
        {
            foreach (File file in Files)
            {
                if (file.Name.Equals(fileName))
                    return true;
            }

            return false;
        }

        public byte[] GetFileData(string sourceFileName)
        {
            foreach(File file in Files)
            {
                if (file.Name.Equals(sourceFileName))
                    return file.Data;
            }

            return null;
        }

        public void DeleteFile(string fileName)
        {
            foreach (File file in Files)
            {
                if (file.Name.Equals(fileName))
                {
                    Files.Remove(file);
                    return;
                }
            }
        }
    }
}
