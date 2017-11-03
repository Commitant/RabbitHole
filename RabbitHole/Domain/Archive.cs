using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RabbitHole.Domain
{

    
    public class Archive
    {

        public String Name { get;  }
        public String FileName { get;  }
        private int _noOfBytesInArchive;
        private List<Volume> _volumes = new List<Volume>();
        public Volume CurrentVolume { get; set; }
        public Func<String, bool> UserConfirmationFunc { get; set; }

        public int AlgorithmNo { get; set; } = 1;


        public Archive(String name, String fileName, int noOfBytesInArchive)
        {
            Name = name;
            FileName = fileName;
            _noOfBytesInArchive = noOfBytesInArchive;            
        }

        public void CreateVolumes(int algorithmNo, params String[] volumePasswords)
        {

            int volumePosition = 0;
            var binaryWriter = new BinaryWriter(new FileStream(FileName, FileMode.Open));

            foreach (String password in volumePasswords)
            {
                var volume = new Volume(volumePosition);
                _volumes.Add(volume);
                volume.WriteToFile(binaryWriter.BaseStream, password, algorithmNo);

                volumePosition += (_noOfBytesInArchive - volumePosition)/2;   //each successive volume starts at the half point of the previous volumes size
            }    
            
            binaryWriter.Flush();
            binaryWriter.Close();        
            
        }

        public void WriteCurrentVolumeToFile(String password)
        {
            var binaryWriter = new BinaryWriter(new FileStream(FileName, FileMode.Open));
            CurrentVolume.WriteToFile(binaryWriter.BaseStream, password, AlgorithmNo);
            binaryWriter.Flush();
            binaryWriter.Close();
        }


        public bool OpenVolume(string password)
        {

            var binaryReader = new BinaryReader(new FileStream(FileName, FileMode.Open));

            int volumePosition = 0;
            int volumeNo = 0;
            while (volumePosition < _noOfBytesInArchive -32)  //try opening volumes and checking whether they verify correctly
            {
                binaryReader.BaseStream.Seek(volumePosition, SeekOrigin.Begin);
                Volume volume = Volume.Deserialize(binaryReader.BaseStream, password, _noOfBytesInArchive-volumePosition);

                if (volume != null)
                {
                    volume.VolumeName = "Volume" + (volumeNo +1);
                    AlgorithmNo = volume.AlgorithmNo;
                    volume.VolumePosition = volumePosition;
                    volume.VolumeNo = volumeNo;
                    CurrentVolume = volume;
                    binaryReader.Close();
                    return true;
                }

                volumePosition += (_noOfBytesInArchive - volumePosition)/2; //move to next possible volume start position
                volumeNo++;

            }

            binaryReader.Close();


            return false; 

        }

        public bool SaveCurrentVolume(String password)
        {
            
            if (CurrentVolume.VolumeSizeExceedAllocatedSpace(_noOfBytesInArchive, password))
            {
                var confirmationPrompt = "The size of " + CurrentVolume.VolumeName +
                                         " exceeds the default allocated space, if your archive has a" +
                                         " volume " + (CurrentVolume.VolumeNo + 2) +
                                         " that volume and possibly later volumes will be corrupted." +
                                         " Are you sure you want to continue? (y/n) ";

                if (!UserConfirmationFunc(confirmationPrompt))
                    return false;
            }
                

            var binaryWriter = new BinaryWriter(new FileStream(FileName, FileMode.Open));
            binaryWriter.BaseStream.Seek(CurrentVolume.VolumePosition, SeekOrigin.Begin);

            CurrentVolume.WriteToFile(binaryWriter.BaseStream, password, AlgorithmNo);
          
            binaryWriter.Flush();
            binaryWriter.Close();

            return true;
        }

        public void ExtractFile(String sourceFileName, String destinationPathAndFileName)
        {
            var destinationfileName = destinationPathAndFileName;
            var binaryWriter = new BinaryWriter(new FileStream(destinationfileName, FileMode.CreateNew));
            binaryWriter.Write(CurrentVolume.Folder.GetFileData(sourceFileName));
            binaryWriter.Flush();
            binaryWriter.Close();
        }

        
    }
}
