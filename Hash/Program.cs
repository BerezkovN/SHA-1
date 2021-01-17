using System;
using System.Text;

namespace Hash
{
    public static class Operations
    {
        public static uint ROTL(uint original, int bits)
        {
            return (original << bits) | (original >> (32 - bits));
        }

        public static uint ROTR(uint original, int bits)
        {
            return (original >> bits) | (original << (32 - bits));
        }
    }

    public static class SHA1
    {
        static uint h0 = 0x67452301;
        static uint h1 = 0xefcdab89;
        static uint h2 = 0x98badcfe;
        static uint h3 = 0x10325476;
        static uint h4 = 0xc3d2e1f0;

        static byte[][] blocks;

        static void ResetValues()
        {
            h0 = 0x67452301;
            h1 = 0xefcdab89;
            h2 = 0x98badcfe;
            h3 = 0x10325476;
            h4 = 0xc3d2e1f0;
        }

        static void Print ()
        {
            foreach(var b in blocks[0])
            {
                Console.Write(Convert.ToString(b, 2) + " ");
            }
        }

        static void ParsingMessage(byte[] message)
        {
            int iter = (int)Math.Ceiling((double)message.Length / 64);
            int freebytes = iter * 64 - message.Length;

            //We need 1 byte to set one at the end of the message and also we need 8 bytes for length
            if (freebytes >= 9)
            {
                blocks = new byte[iter][];
            }
            else
            {
                throw new NotImplementedException();
            }

            for (int ind = 0; ind < iter; ind++)
            {
                blocks[ind] = new byte[64]; //64 * 8 = 512 bits for block

                if (ind + 1 == iter && message.Length % 64 != 0)
                {
                    //Padding the message
                    for (int i = 0; i < blocks[ind].Length - freebytes; i++)
                    {
                        blocks[ind][i] = message[i + ind * 64];
                    }

                    //Appending the bit “1” to the end of the message
                    blocks[ind][blocks[ind].Length - freebytes] = (byte)1 << 7;

                    //Appending the 64-bit block
                    byte[] length = Converter.LongToByte((long)message.Length * 8);

                    for (int i = 0; i < length.Length; i++)
                    {
                        blocks[ind][blocks[ind].Length - length.Length + i] = length[i];
                    }

                    break;
                }

                //Setting blocks
                for (int i = 0; i < blocks[ind].Length; i++)
                {
                    blocks[ind][i] = message[i + ind * 64];
                }
            }
        }

        static uint K(int t)
        {
            if (t >= 0 && t <= 19)
                return 0x5a827999;

            if (t >= 20 && t <= 39)
                return 0x6ed9eba1;

            if (t >= 40 && t <= 59)
                return 0x8f1bbcdc;

            if (t >= 60 && t <= 79)
                return 0xca62c1d6;

            throw new Exception();
        } 

        static uint f(uint x, uint y, uint z, int t)
        {
            if (t >= 0 && t <= 19)
                return (x & y) ^ (~x & z);

            if (t >= 20 && t <= 39)
                return x ^ y ^ z;

            if (t >= 40 && t <= 59)
                return (x & y) ^ (x & z) ^ (y & z);

            if (t >= 60 && t <= 79)
                return x ^ y ^ z;

            throw new Exception();
        }
        
        public static byte[] Generate(byte[] message)
        {
            ResetValues();
            ParsingMessage(message);

            for (int i = 0; i < blocks.Length; i++)
            {
                //Message schedule
                uint[] W = new uint[80];

                for (int t = 0; t < W.Length; t++)
                {
                    if (t <= 15)
                    {
                        byte[] word = new byte[4];

                        for (int ind = 0; ind < word.Length; ind++)
                        {
                            word[ind] = blocks[i][ind + t * 4];
                        }

                        W[t] = Converter.BytesToUint32(word);
                    }
                    else
                    {
                        W[t] = Operations.ROTL(W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16], 1);
                    }
                }

                uint a = h0;
                uint b = h1;
                uint c = h2;
                uint d = h3;
                uint e = h4;

                for (int t = 0; t < 80; t++)
                {
                    uint T = Operations.ROTL(a, 5) + f(b, c, d, t) + e + K(t) + W[t];
                    e = d;
                    d = c;
                    c = Operations.ROTL(b, 30);
                    b = a;
                    a = T;
                }

                h0 += a;
                h1 += b;
                h2 += c;
                h3 += d;
                h4 += e;
            }

            return Converter.UIntArrayToByteArray(new uint[] { h0, h1, h2, h3, h4 });
        }
    }

    //I guess the name fully describes what it does
    public static class Converter
    {
        public static byte[] UIntArrayToByteArray(uint[] words)
        {
            byte[] result = new byte[words.Length * 4];

            for (int a = 0; a < words.Length; a++)
            {
                byte[] word = BitConverter.GetBytes(words[a]);
                Array.Reverse(word);

                for (int b = 0; b < 4; b++)
                {
                    result[b + a * 4] = word[b];
                }
            }

            return result;
        }

        public static uint BytesToUint32(byte[] bytes)
        {
            return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | ((uint)bytes[3]);
        }

        public static byte[] LongToByte(long l)
        {
            byte[] array = BitConverter.GetBytes(l);
            Array.Reverse(array);
            return array;
        }

        public static byte[] StringToByte(string input)
        {
            byte[] b = new byte[input.Length];

            for (int ind = 0; ind < input.Length; ind++)
            {
                b[ind] = (byte)input[ind];
            }

            return b;
        }

        public static string ToHex(this byte[] bytes)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString("x2"));

            return result.ToString();
        }
    }

    class Program
    {
        static void Test(string s)
        {
            byte[] message = Converter.StringToByte(s);

            //Using algorithm that Microsoft guys already did
            string s1;
            using (var hash = System.Security.Cryptography.SHA1.Create())
            {
                s1 = Converter.ToHex(hash.ComputeHash(message));
            }

            Console.WriteLine("Microsoft - " + s1);

            //My implemetation
            string s2 = Converter.ToHex(SHA1.Generate(message));

            Console.WriteLine("Me        - " + s2);
        }

        static void Main(string[] args)
        {
            string input = Console.ReadLine();

            Test(input);
        }
    }
}
