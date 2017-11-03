using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using RabbitHole.Domain;

namespace RabbitHole
{
    public class Program
    {

        //private static Folder _folder;
        private static Archive _currentArchive = new Archive("", "", 0); //dummy archive

        static void Main(string[] args)
        {
            Console.WriteLine("          ---=== RabbitHole v.0.1.2 Commitant 2017 ===---");

            DisplayArchiveAndVolumeNames();

           

            String line;
            while (!(line = Console.ReadLine()).Equals("exit"))
            {
                try
                {
                    var parts = line.Split(' ');

                    if (parts.Length == 0 || parts[0] == null || parts[0] == "")
                    {
                        Console.WriteLine("For help on use, write help or ?");
                    }
                    else if (parts[0].Equals("open"))
                    {
                        OpenArchive(parts);
                    }
                    else if (parts[0].Equals("list"))
                    {
                        listFiles();
                    }
                    else if (parts[0].Equals("put"))
                    {
                        AddFile(parts);
                        listFiles();
                    }
                    else if (parts[0].Equals("new"))
                    {
                        CreateNewArchive(parts);
                    }
                    else if (parts[0].Equals("save"))
                    {
                        SaveCurrentVolume(parts);
                    }
                    else if (parts[0].Equals("get"))
                    {
                        ExtractFileFromArchive(parts);
                    }
                    else if (parts[0].Equals("delete"))
                    {
                        DeleteFile(parts);
                    }
                    else if (parts[0].Equals("?") || parts[0].Equals("help"))
                    {
                        DisplayHelpScreen();
                    }
                    else if (parts[0].Equals("howto"))
                    {
                        Process.Start("https://github.com/eflite/RabbitHole/wiki/How-To-Use");
                    }
                    else
                    {
                        Console.WriteLine("Unknown command " + parts[0] + ". For help on use, write help or ?");
                    }

                    DisplayArchiveAndVolumeNames();
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occured while executing command. For help on command use, type help or ?");
                }

            }
            
           
        }

        private static void DisplayHelpScreen()
        {
            Console.WriteLine("\n");
            Console.WriteLine("RabbitHole commands and syntax\n");
            Console.WriteLine("{0,-8}{1,-30}{2,-30}", "new", "<path and archive name>", "<archive size in MB>");
            Console.WriteLine("{0,-8}{1,-30}{2,-30}", "open", "<path and file name>", "<password for intended volume>");
            Console.WriteLine("{0,-8}", "list");
            Console.WriteLine("{0,-8}{1,-30}", "put", "<path and file name");
            Console.WriteLine("{0,-8}{1,-30}{2,-30}", "get", "<file in archive>", "<destination path and file name>");
            Console.WriteLine("{0,-8}{1,-30}", "delete", "<file in archive>");
            Console.WriteLine("{0,-8}{1,-30}", "save", "<password for current volume>");
            Console.WriteLine("{0,-8}", "howto");
            Console.WriteLine("{0,-8}", "exit");
            
            
        }

        private static void DisplayArchiveAndVolumeNames()
        {
            var currentVolumeName = "";
            if (_currentArchive.CurrentVolume != null)
                currentVolumeName = ":" + _currentArchive.CurrentVolume.VolumeName;
            Console.Write("@" + _currentArchive.Name + currentVolumeName + "> ");
        }

        private static void DeleteFile(string[] parts)
        {
            if (!Validates(() => parts.Length == 2, "delete keyword usage: delete <file in archive>")) return;

            if (!Validates(() => _currentArchive !=null && _currentArchive.CurrentVolume != null, "No volume openend. Please open archive and volume using the open keyword")) return;

            if (!Validates(() => _currentArchive.CurrentVolume.Folder.ContainsFile(parts[1]), "File " + parts[1] + " not found in " + _currentArchive.CurrentVolume.VolumeName +". Use list command to view volume contents")) return;

           
            _currentArchive.CurrentVolume.Folder.DeleteFile(parts[1]);

            Console.WriteLine("File " + parts[1] +" deleted from current archive");
        }

        private static void ExtractFileFromArchive(string[] parts)
        {
            if (!Validates(() => parts.Length == 3, "get keyword usage: get <file in archive> <destination path and filename>")) return;

            if (!Validates(() => !System.IO.File.Exists(parts[2]), "Destination file " + parts[2] + " allready exists")) return;

            if (!Validates(() => (_currentArchive != null && _currentArchive.CurrentVolume != null), "No volume openend. Please open archive and volume using the open keyword")) return;

            if (!Validates(() => _currentArchive.CurrentVolume.Folder.ContainsFile(parts[1]) && _currentArchive.CurrentVolume.Folder.GetFileData(parts[1]) != null, "File " + parts[1] + " not found in current archive/volume. Use command list to view volume contents")) return;

           
            var sourceFileName = parts[1];
            var destinationfileName = parts[2];
            
            _currentArchive.ExtractFile(sourceFileName, destinationfileName);

            Console.WriteLine("File " + sourceFileName + " extracted to " + destinationfileName);

        }

        private static void SaveCurrentVolume(String[] parts)
        {
            if (!Validates(() => parts.Length == 2, "No password provided. save keyword usage: save <password>, example: save !#MySecretPa$$wrd")) return;
            
            var success = _currentArchive.SaveCurrentVolume(parts[1]);

            if (success)
                Console.WriteLine("Volume saved");
        }

       

        private static void OpenArchive(string[] parts)
        {
            if(!Validates(() => parts.Length == 3, "open archive keyword usage: open <path and file name> <password>, example: open c:\\stuff\\myArchive.rabbit !#MySecretPa$$wrd")) return;           

            String path = parts[1];
            String fileName="";

            if (!path.ToLower().EndsWith(".rabbit"))
                path += ".rabbit";

            if (path.Contains("\\"))
                fileName = path.Substring(path.LastIndexOf("\\") + 1);
            else
            {
                fileName = path;
            }

            fileName = fileName.Substring(0, fileName.LastIndexOf("."));

            if (!Validates(() => System.IO.File.Exists(path), "Could not find archive with filename " + path)) return;
           

            int length = (int)new System.IO.FileInfo(path).Length;

            _currentArchive = new Archive(fileName, path, length);

            _currentArchive.UserConfirmationFunc = UserConfirmation;

            
            Console.WriteLine("Archive opened. Decrypting volume...");

            bool volumeOpened = _currentArchive.OpenVolume(parts[2]);
          
            if (!volumeOpened)
                Console.WriteLine("No volume could be opened using specified password. Please check that you typed it correctly.");
            else
                Console.WriteLine("Volume opened");

        }

        private static bool UserConfirmation(String confirmationPrompt)
        {
            char confirmation = ' ';

            Console.Write(confirmationPrompt);
            while ((confirmation = Console.ReadKey(false).KeyChar) != 'y' && confirmation != 'n')
            {
                Console.Write(confirmationPrompt);    
            }

            Console.WriteLine("\n");
            return confirmation == 'y';
        }

        private static bool OpenVolume(String password)
        {
          
            return _currentArchive.OpenVolume(password);

        }

        private static void CreateNewArchive(string[] parts)
        {
            if (!Validates(() => parts.Length == 3 && IsNumber(parts[2]), "new keyword usage: new <fileName> <size in MB>, example: new myArchive 10")) return;
            
            var fileName = parts[1];
            if (!fileName.ToLower().EndsWith(".rabbit"))
                fileName += ".rabbit";

            if (!Validates(() => !System.IO.File.Exists(fileName), "\nFile allready exists")) return;

            if (!Validates(() => HasWritePermissionsToFolder(fileName), "You don't seem to have write-permissions to the folder. " +
                                                                        "Open the application as administrator by " +
                                                                        "right clicking on the .exe-file and choosing \"Run as Administrator\", or specify a path to another folder")) return;
          

            byte[] entropyBytes =  CollectRandomInput();

           
            Console.WriteLine("Finished collecting data, please wait while archive is filled with random bytes.\n");
            var binaryWriter = new BinaryWriter(new FileStream(fileName, FileMode.CreateNew));
            CryptoUtil.WriteRandomBytes(binaryWriter, int.Parse(parts[2]) * 1048576, entropyBytes, SetProgress);
            binaryWriter.Flush();
            binaryWriter.Close();


            Console.WriteLine("Finished creating archive.");

            Console.WriteLine("Please choose crypto algoritm: 1=AES, 2=Serpent, 3=Twofish");
            var algorithmNo = int.Parse((Console.ReadLine()));
            

            Console.Write("Please enter the total number of volumes you want inside your archive: ");
            var noOfVolumes = int.Parse(Console.ReadLine());
            String[] passwords = new String[noOfVolumes];

            for (int i = 0; i < noOfVolumes; i++)
            {
                Console.Write("Please enter the password for volume " + (i + 1)+": ");
                passwords[i] = Console.ReadLine();
            }


            _currentArchive = new Archive(parts[0], fileName, int.Parse(parts[2]) * 1048576); //MB x bytes
            _currentArchive.CreateVolumes(algorithmNo, passwords);

            Console.WriteLine("\nVolumes created.");

            Console.WriteLine("To open one of the volumes you just created, open the archive using the password corresponding with the desired volume, example: open <path and file name> <password>" );



        }

        private static bool HasWritePermissionsToFolder(string fileName)
        {
            fileName += ".test";

            try
            {
                var binaryWriter = new BinaryWriter(new FileStream(fileName, FileMode.CreateNew));
                binaryWriter.Write(" ");
                binaryWriter.Flush();
                binaryWriter.Close();


                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static void SetProgress(double percent)
        {          
            //Console.Write(percent + " % done      ");
            Console.Write("{0:N2} % done        ", (percent));
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        private static byte[] CollectRandomInput()
        {
            Console.WriteLine(
                "To create some non-deterministic seed data for the random generator, please type a bunch of charaters. " +
                "\nThis is not a key, what you type is irrelevant.\n\n");

            var lastChar = ' ';
            char c;
            MemoryStream memoryStream = new MemoryStream();
            int requiredNoOfBytes = 500;
            while (true)
            { 
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var keyPress = Console.ReadKey(true);
                c = keyPress.KeyChar;                
                if (c != lastChar)
                {
                    stopwatch.Stop();
                    memoryStream.WriteByte((byte) c); 
                    var diffByte = (byte) ((int) c - (int) lastChar);                  
                    memoryStream.WriteByte(diffByte);

                    memoryStream.Write(BitConverter.GetBytes(stopwatch.ElapsedTicks), 0, 8);                    
                    lastChar = c;

                    
                    if (memoryStream.Length >= requiredNoOfBytes)
                        break;
                }

                double percentDone = (double)memoryStream.Length / (double)requiredNoOfBytes * 100.00;
                if (percentDone > 100)
                    percentDone = 100;
                Console.Write(percentDone + " % done      ");
                Console.SetCursorPosition(0, Console.CursorTop);
            }
            Console.Write("100 % done      ");
            Console.WriteLine("\n");

            return memoryStream.GetBuffer();
        }

        private static bool IsNumber(string s)
        {
            try
            {
                int.Parse(s);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void listFiles()
        {

            if (!Validates(() => _currentArchive != null, "Please open an archive or create a new one.")) return;

            if (!Validates(() => _currentArchive.CurrentVolume != null, "Please open a volume in the current archive by specifying the password associated with the volume you want to open.")) return;
            
            var folder = _currentArchive.CurrentVolume.Folder;

            Console.WriteLine("\n" +folder.Files.Count + " files in volume " + _currentArchive.CurrentVolume.VolumeName);

            foreach(File file in folder.Files)
            {
                Console.WriteLine(file.Name + " " + file.Data.Length + " bytes");
            }

            if (folder.Files.Count==0)
                Console.WriteLine("To add files use: put <filename>");

        }

        private static void AddFile(string[] parts)
        {
            if (!Validates(() => _currentArchive.CurrentVolume != null, "Please open an archive and volume before adding files.")) return;
          
            if (!Validates(() => parts.Length==2, "put keyword usage: put <fileName>, example: put c:\\temp\\someFile.ext")) return; 
          
                       
            var folder = _currentArchive.CurrentVolume.Folder;

            var path = parts[1];
            var fileName = parts[1];
            if (path.Contains("\\"))
                fileName = path.Substring(path.LastIndexOf("\\") + 1);

            if (!Validates(() => !folder.ContainsFile(fileName), "\nThe arhive allready contains a file with the name " + fileName)) return;

            if (!Validates(() => System.IO.File.Exists(path), "\nFile not found")) return;
          

            byte[] buffer =System.IO.File.ReadAllBytes(path);
           
            File file = new File();
            file.Name = fileName;
            file.Data = buffer;
            folder.Files.Add(file);

            Console.WriteLine("File added");
                
        }

        

        private static bool Validates(Func<bool> predicate, String failedMessage)
        {
            if (!predicate())    
                Console.WriteLine(failedMessage);

            return predicate();
        }

    }
}
