using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RabbitHole.Domain
{
    public class Volume
    {
        
        public Folder Folder = new Folder();
        public int VolumePosition {get; set; }  //This volume's position within the archive file
        

        public String VolumeName { get; set; }

        public int VolumeNo { get; set; }


        public Volume()
        {
            
        }

        public Volume(int volumePosition )
        {
            VolumePosition = volumePosition;
        }


        private byte[] CreateIV(byte[] entropyBytes, String password)
        {
            if (entropyBytes.Length<32)
                Array.Resize(ref entropyBytes, 32);

            var cryptographicHash = CryptoUtil.GetSha256Hash(Encoding.UTF8.GetBytes(password));
            Array.Copy(cryptographicHash, entropyBytes, cryptographicHash.Length); //write 32 hash-bytes to start of entropy
            return CryptoUtil.GetRandomBytes(entropyBytes, 32); //32 byte IV for AES-256 encryption
        }

        private static int CalculateOffset(string password)
        {
            
            var cryptographicHash = CryptoUtil.GetSha256Hash(Encoding.UTF8.GetBytes(password));
            int hashInteger = (Int32)BitConverter.ToUInt64(cryptographicHash, 0);
            if (hashInteger <= 0)
                hashInteger *= -1;
            return hashInteger % 1024; //offset between 0 and 1024 bytes
        }

        public void Serialize(Stream stream, string password)
        {

            //Volume specification
            //<offset>:IV:length:EncryptedData(passwordHash, folder)

            var offset = CalculateOffset(password);
            var IV = CreateIV(BitConverter.GetBytes(DateTime.Now.Ticks), password);

            StreamUtil.WriteBytes(stream, CryptoUtil.GetRandomBytes(BitConverter.GetBytes(DateTime.Now.Ticks), offset)); //fill offset with random data
            StreamUtil.WriteBytes(stream, IV); //serialize IV

            //Encrypt volume contents
            var cryptographicHash = CryptoUtil.GetSha256Hash(Encoding.UTF8.GetBytes(password));
            MemoryStream encryptedStream = new MemoryStream();
            StreamUtil.WriteBytes(encryptedStream, cryptographicHash); //write the hashed password usded for volume verification, 32 bytes
            Folder.Serialize(encryptedStream); //write volume's folder and contained files to encrypted stream
            byte[] encryptedContents = CryptoUtil.EncryptUsingAES256(encryptedStream.ToArray(), IV, password);

            StreamUtil.WriteBytes(stream, BitConverter.GetBytes(encryptedContents.Length)); // serialize the length of the following encrypted data
            StreamUtil.WriteBytes(stream, encryptedContents);   //serialize encrypted data

            
        }

        public int GetTotalVolumeSize(String password)
        {
            var offset = CalculateOffset(password);
            int IVlength = 32;
            int encryptedDataLength = 4; 
            int passwordHash = 32;

            return offset + IVlength + encryptedDataLength + passwordHash + Folder.GetTotalDataSize();

        }

        public static Volume Deserialize(Stream stream, String password, int maxPossibleSize)
        {
            Volume volume = new Volume();
            var offset = CalculateOffset(password);

            //deserialize header
            stream.Seek(offset, SeekOrigin.Current); //moves stream cursor passed offset
            var IV = StreamUtil.ReadBytes(stream, 32);  //read IV
            int encryptedDataLength = BitConverter.ToInt32(StreamUtil.ReadBytes(stream, 4), 0); //read length of the following encrypted data

            if (encryptedDataLength < 32 || encryptedDataLength > maxPossibleSize) //encryptedDataLength is not valid, so the specified password cannot be for this volume.
                return null;                 

            byte[] encryptedBytes = StreamUtil.ReadBytes(stream, encryptedDataLength);
            

            //decrypt volume
            byte[] decryptedBytes = CryptoUtil.DecryptUsingAES256(encryptedBytes, IV, password);

            //deserialize decrypted volume
            MemoryStream decryptedStream = new MemoryStream(decryptedBytes);
            byte[] cryptographicHash = StreamUtil.ReadBytes(decryptedStream, 32);
            
            //verify volume
            if (!cryptographicHash.SequenceEqual(CryptoUtil.GetSha256Hash(Encoding.UTF8.GetBytes(password))))
                return null;

            volume.Folder = Folder.Deserialize(decryptedStream);

            return volume; 


        }

        public void AddFile(File file)
        {
            Folder.Files.Add(file);
        }


        public void WriteToFile(Stream fileStream, String password)
        {            
            fileStream.Seek(VolumePosition, SeekOrigin.Begin); //move stream cursor to volume start position (volume offset is part of serialized data)
            Serialize(fileStream, password);
        }

        public bool VolumeSizeExceedAllocatedSpace(int noOfBytesInArchive, String password)
        {
            int spaceAvailableForCurrentVolume = noOfBytesInArchive / 2;
            for (int i = 0; i < VolumeNo; i++)
            {
                spaceAvailableForCurrentVolume /= 2; //halve available space for each successive volume
            }

            return GetTotalVolumeSize(password) > spaceAvailableForCurrentVolume;
        }
    }
}
