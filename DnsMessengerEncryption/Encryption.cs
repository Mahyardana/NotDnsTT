using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DnsMessengerEncryption
{
    public static class Base32Lower
    {
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyz234567";

        public static string Encode(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            StringBuilder result = new StringBuilder();
            int buffer = data[0];
            int next = 1;
            int bitsLeft = 8;

            while (bitsLeft > 0 || next < data.Length)
            {
                if (bitsLeft < 5)
                {
                    if (next < data.Length)
                    {
                        buffer <<= 8;
                        buffer |= data[next++] & 0xff;
                        bitsLeft += 8;
                    }
                    else
                    {
                        int pad = 5 - bitsLeft;
                        buffer <<= pad;
                        bitsLeft += pad;
                    }
                }

                int index = (buffer >> (bitsLeft - 5)) & 0x1f;
                bitsLeft -= 5;
                result.Append(Alphabet[index]);
            }

            // RFC 4648 padding
            int paddingcount = 0;
            while ((result.Length + paddingcount) % 8 != 0)
                paddingcount++;

            return result.ToString();
        }
        public static byte[] Decode(string base32)
        {
            if (string.IsNullOrEmpty(base32))
                return Array.Empty<byte>();

            base32 = base32.TrimEnd('=').ToLowerInvariant();

            int buffer = 0;
            int bitsLeft = 0;
            byte[] result = new byte[base32.Length * 5 / 8];
            int index = 0;

            foreach (char c in base32)
            {
                int val = Alphabet.IndexOf(c);
                if (val < 0)
                    throw new FormatException("Invalid Base32 character.");

                buffer <<= 5;
                buffer |= val & 0x1f;
                bitsLeft += 5;

                if (bitsLeft >= 8)
                {
                    result[index++] = (byte)((buffer >> (bitsLeft - 8)) & 0xff);
                    bitsLeft -= 8;
                }
            }

            Array.Resize(ref result, index);
            return result;
        }
    }
    public static class Crc32
    {
        private static readonly uint[] Table = CreateTable();

        private static uint[] CreateTable()
        {
            uint[] table = new uint[256];
            const uint poly = 0xEDB88320;

            for (uint i = 0; i < table.Length; i++)
            {
                uint crc = i;
                for (int j = 0; j < 8; j++)
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ poly : crc >> 1;
                table[i] = crc;
            }
            return table;
        }

        public static uint Compute(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            foreach (byte b in data)
                crc = (crc >> 8) ^ Table[(crc ^ b) & 0xFF];
            return ~crc;
        }
    }
    public static class Encryption
    {
        public static byte[] key = Array.Empty<byte>();
        static Random rand = new Random();
        public static AesGcm? aes = null;
        public static string[] encryptmsg(int connid,int packetnum, byte[] data, int maxlength)
        {
            if (aes == null)
            {
                aes = new AesGcm(key.Take(16).ToArray(), 16);
            }
            byte[] iv = new byte[12];
            rand.NextBytes(iv);
            var enclist = new List<string>();
            var packetnumbytes = BitConverter.GetBytes(packetnum);
            int seq = 0;
            var ciphertext = new byte[data.Length];
            var tag = new byte[16];
            aes.Encrypt(iv, data, ciphertext, tag);
            ciphertext = tag.Concat(ciphertext).ToArray();
            var chunks = (ciphertext.Length + maxlength - 1) / maxlength;
            enclist.Add(Base32Lower.Encode(packetnumbytes.Concat(BitConverter.GetBytes(connid)).Concat(BitConverter.GetBytes(chunks)).Concat(iv).ToArray()));
            for (int i = 0; i < chunks; i++)
            {
                var tosend = ciphertext.Take(maxlength);
                enclist.Add(Base32Lower.Encode(tosend.ToArray()));
                ciphertext = ciphertext.Skip(maxlength).ToArray();
                seq++;
            }
            return enclist.ToArray();
        }

        public static string decryptmsg(string[] parts)
        {
            if (aes == null)
            {
                aes = new AesGcm(key.Take(16).ToArray(), 16);
            }
            byte[] iv = Base32Lower.Decode(parts[0]).Skip(12).Take(12).ToArray();
            var tagandcipher = new List<byte>();
            for (int i = 1; i < parts.Length; i++)
            {
                tagandcipher.AddRange(Base32Lower.Decode(parts[i]).Skip(8));
            }
            var tag = tagandcipher.Take(16).ToArray();
            var cipher = tagandcipher.Skip(16).ToArray();
            var plaintext = new byte[cipher.Length];
            aes.Decrypt(iv, cipher, tag, plaintext);
            return Encoding.ASCII.GetString(plaintext);
        }
        public static byte[] decryptmsg(string[] parts, byte[] iv)
        {
            if (aes == null)
            {
                aes = new AesGcm(key.Take(16).ToArray(), 16);
            }
            var tagandcipher = new List<byte>();
            for (int i = 0; i < parts.Length; i++)
            {
                tagandcipher.AddRange(Base32Lower.Decode(parts[i]));
            }
            var tag = tagandcipher.Take(16).ToArray();
            var cipher = tagandcipher.Skip(16).ToArray();
            var plaintext = new byte[cipher.Length];
            aes.Decrypt(iv, cipher, tag, plaintext);
            return plaintext;
        }
    }
}
