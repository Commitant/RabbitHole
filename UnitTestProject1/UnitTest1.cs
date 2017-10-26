using System;
using System.CodeDom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitHole;
using System.IO;
using System.Linq;
using RabbitHole.Domain;
using System.Text;
using System.Security.Cryptography;
using File = System.IO.File;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestSerializeFile()
        {

            RabbitHole.File file = CreateFile("test1.txt", 100);
            MemoryStream stream = new MemoryStream();
            file.Serialize(stream);


            stream.Seek(0, SeekOrigin.Begin); //start at beginning of stream
            var deserializedFile = RabbitHole.File.Deserialize(stream);

            Assert.IsTrue(file.Name.Equals(deserializedFile.Name));
            Assert.IsTrue(file.Data.SequenceEqual(deserializedFile.Data));
           
        }

        public RabbitHole.File CreateFile(String fileName, int noOfDataBytes)
        {
            RabbitHole.File file = new RabbitHole.File();
            file.Name = fileName;
            file.Data = new byte[noOfDataBytes];
            new Random().NextBytes(file.Data);
            return file;
        }

        [TestMethod]
        public void TestSerializeFolder()
        {
            RabbitHole.File file1 = CreateFile("test1.txt", 100);
            RabbitHole.File file2 = CreateFile("test2.txt", 100);
            RabbitHole.File file3 = CreateFile("test3.txt", 100);

            Folder folder = new Folder();
            folder.Files.Add(file1);
            folder.Files.Add(file2);
            folder.Files.Add(file3);

            MemoryStream stream = new MemoryStream();
            folder.Serialize(stream);

            stream.Seek(0, SeekOrigin.Begin); //start at beginning of stream
            Folder deserializedFolder = Folder.Deserialize(stream);

            Assert.IsTrue(deserializedFolder.Files.Count == folder.Files.Count);
            for (int i =0;i<folder.Files.Count;i++)
            {
                Assert.IsTrue(folder.Files[i].Data.SequenceEqual(deserializedFolder.Files[i].Data));
            }
        }

        [TestMethod]
        public void TestCrypto()
        {
            String plainText = "This is the unencrypted secret message";
            String password = "password";
            byte[] IV = CryptoUtil.GetRandomBytes(new byte[32], 32);
            byte[] encrypted = CryptoUtil.EncryptUsingAES256(Encoding.UTF8.GetBytes(plainText), IV, password);

            byte[] decrypted = CryptoUtil.DecryptUsingAES256(encrypted, IV, password);

            Assert.AreEqual(plainText, Encoding.UTF8.GetString(decrypted));

        }

        [TestMethod]
        public void TestSerializeAndEncryptVolume()
        {
            Volume volume = new Volume();
            volume.AddFile(CreateFile("test1.txt", 100));
            volume.AddFile(CreateFile("test2.txt", 100));
            volume.AddFile(CreateFile("test3.txt", 100));

            MemoryStream memoryStream = new MemoryStream();
            volume.Serialize(memoryStream, "password");

            memoryStream.Seek(0, SeekOrigin.Begin); //start at beginning of stream
            var deserializedVolume = Volume.Deserialize(memoryStream, "password", int.MaxValue);

            Assert.IsNotNull(deserializedVolume);
            Assert.IsTrue(deserializedVolume.Folder.Files.Count==volume.Folder.Files.Count);
            for (int i = 0; i < volume.Folder.Files.Count; i++)
            {
                Assert.IsTrue(volume.Folder.Files[i].Data.SequenceEqual(deserializedVolume.Folder.Files[i].Data));
            }


           
        }

        [TestMethod]
        public void TestArchiveOperations()
        {
            //create file on disk
            var fileName = "testArchive2.Rabbit";

            if (File.Exists(fileName))
                File.Delete(fileName);

            var sizeInMb = 1;
            var binaryWriter = new BinaryWriter(new FileStream(fileName, FileMode.CreateNew));
            CryptoUtil.WriteRandomBytes(binaryWriter, sizeInMb * 1048576, new byte[32]);
            binaryWriter.Flush();
            binaryWriter.Close();

            Assert.IsTrue(File.Exists(fileName));

            //create archive with volumes
            var archive  = new Archive(fileName, fileName, sizeInMb * 1048576); //MB x bytes
            archive.CreateVolumes(new string[] {"p1", "p2", "p3"});

            Assert.IsTrue(archive.OpenVolume("p1"));
            Assert.IsTrue(archive.OpenVolume("p2"));
            Assert.IsTrue(archive.OpenVolume("p3"));

            //test add file
            var testFile = CreateFile("test1.txt", 100);
            archive.OpenVolume("p1");
            archive.CurrentVolume.AddFile(testFile);
            archive.SaveCurrentVolume("p1");

            //reopen volume and check added file
            archive.OpenVolume("p1");
            Assert.IsTrue(archive.CurrentVolume.Folder.ContainsFile("test1.txt"));
            Assert.IsTrue(archive.CurrentVolume.Folder.Files[0].Data.SequenceEqual(testFile.Data));

            //extract file
            archive.ExtractFile("test1.txt", "test1.txt");
            Assert.IsTrue(File.Exists("test1.txt"));
            var buffer = File.ReadAllBytes("test1.txt");
            Assert.IsTrue(archive.CurrentVolume.Folder.Files[0].Data.SequenceEqual(buffer));
            File.Delete("test1.txt");

            //remove file
            archive.CurrentVolume.Folder.DeleteFile("test1.txt");
            archive.SaveCurrentVolume("p1");
            archive.OpenVolume("p1"); //reopen
            Assert.IsTrue(archive.CurrentVolume.Folder.Files.Count==0);


            var size = 100 * 1048576;
            var counter = 0;
            while (size > 1) 
            {
                counter++;
                size -= 1068;
                size /= 2;
            }

            int i = 0;  //1MB = 10 volumes, 10 MB = 14 volumes, 

        }
    }
}
