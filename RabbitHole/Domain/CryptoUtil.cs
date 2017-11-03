using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Utilities;

namespace RabbitHole.Domain
{
    public class CryptoUtil
    {
      

        public static byte[] Encrypt256(byte[] dataToEncrypt, byte[] IV, String password, int algorithm)
        {
            //Set up                   
            CbcBlockCipher engine = GetCryptoEngine(algorithm);
            if (algorithm != 1) Array.Resize(ref IV, 16); //only Rijndael uses 32 byte IV, the other use 16

            CbcBlockCipher blockCipher = new CbcBlockCipher(engine); //CBC
            PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding()); //Default scheme is PKCS5/PKCS7        

            var expandedKey = GetExpandedKey(password);
            KeyParameter keyParam = new KeyParameter(expandedKey);
            ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, IV);

            // Encrypt
            cipher.Init(true, keyParamWithIV);
            byte[] outputBytes = new byte[cipher.GetOutputSize(dataToEncrypt.Length)];
            int length = cipher.ProcessBytes(dataToEncrypt, outputBytes, 0);
            cipher.DoFinal(outputBytes, length); //Do the final block

            return outputBytes;
        }

      



        public static byte[] Decrypt256(byte[] encryptedBytes, byte[] IV, String password, int algorithm)
        {
            //Set up            
            CbcBlockCipher engine = GetCryptoEngine(algorithm);
            if (algorithm != 1) Array.Resize(ref IV, 16); //only Rijndael uses 32 byte IV, the other use 16

            CbcBlockCipher blockCipher = new CbcBlockCipher(engine); //CBC
            PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding()); //Default scheme is PKCS5/PKCS7

            var expandedKey = GetExpandedKey(password);
            KeyParameter keyParam = new KeyParameter(expandedKey);
            ParametersWithIV keyParamWithIV = new ParametersWithIV(keyParam, IV);

            //Decrypt            
            cipher.Init(false, keyParamWithIV);
            byte[] decryptedBytes = new byte[cipher.GetOutputSize(encryptedBytes.Length)];
            var length = cipher.ProcessBytes(encryptedBytes, decryptedBytes, 0);
            int finalBlockLengthWithoutPadding = cipher.DoFinal(decryptedBytes, length); //Do the final block    


            int blockSize = 32;
            if (algorithm != 1) blockSize = 16;

            Array.Resize(ref decryptedBytes, decryptedBytes.Length - (blockSize - finalBlockLengthWithoutPadding)); //remove padding

            return decryptedBytes;
        }


        private static CbcBlockCipher GetCryptoEngine(int algorithm)
        {
            if (algorithm == 1)
                return new CbcBlockCipher(new RijndaelEngine(256));
            else if (algorithm == 2)
                return new CbcBlockCipher(new SerpentEngine());
            else if (algorithm == 3)
                return new CbcBlockCipher(new TwofishEngine());

            else return null;
        }


        private static byte[] GetExpandedKey(string password)
        {
            byte[] expandedKey = GetSha256Hash(Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < 1000000; i++)
                expandedKey = GetSha256Hash(expandedKey); //do iterations
            return expandedKey;
        }

       

        public static void WriteRandomBytes(BinaryWriter binaryWriter, int noOfBytes, byte[] entropyBytes, Action<double> setProgressAction=null)
        {
            DigestRandomGenerator digestRandomGenerator = new DigestRandomGenerator(new Sha512Digest());
            digestRandomGenerator.AddSeedMaterial(entropyBytes);

            int blockSize = 1048576;
            int bytesWritten = 0;
            while (bytesWritten < noOfBytes - blockSize)
            {
                byte[] buffer = new byte[blockSize];
                digestRandomGenerator.NextBytes(buffer);
                binaryWriter.Write(buffer);
                bytesWritten += blockSize;
                if (setProgressAction!=null)
                    setProgressAction((double)bytesWritten / (double)noOfBytes * 100.0);
            }

            int remaingBytes = noOfBytes - bytesWritten;
            byte[] buffer2 = new byte[remaingBytes];
            digestRandomGenerator.NextBytes(buffer2);

            binaryWriter.Write(buffer2);



        }

        public static byte[] GetRandomBytes(byte[] entropyBytes, int noOfBytes)
        {
            byte[] randomBytes = new byte[noOfBytes];
            DigestRandomGenerator digestRandomGenerator = new DigestRandomGenerator(new Sha512Digest());
            digestRandomGenerator.AddSeedMaterial(entropyBytes);    //add entropy
            digestRandomGenerator.AddSeedMaterial(DateTime.Now.Ticks); //add additional bytes derived from timestamp
            digestRandomGenerator.NextBytes(randomBytes);
            return randomBytes;
        }

        public static byte[] GetSha256Hash(byte[] buffer)
        {
            Sha256Digest sha256Digest = new Sha256Digest();
            sha256Digest.BlockUpdate(buffer, 0, buffer.Length);
            byte[] hash = new byte[sha256Digest.GetDigestSize()];
            sha256Digest.DoFinal(hash, 0);
            return hash;
        }
    }
}
