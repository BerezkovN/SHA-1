using System;
using System.Collections;
using System.Security.Cryptography;

namespace Hash
{
    public static class Operations
    {
        //Dude fuck, how much did they pay programmers to fucking implement this fucking piece of shit and garbage. I just want to fucking shoot everyone who was part of the team who implemented this garbage. I can do better for free dude, I'm so pissed off 

        //public static BitArray ROTL(BitArray word, int n, int w)
        //{
        //    var left = word.RightShift(n);
        //    foreach (bool b in left)
        //    {
        //        Console.Write(b ? 1 : 0);
        //    }
        //    Console.WriteLine();
        //    var right = word.LeftShift(w - n);
        //    foreach (bool b in right)
        //    {
        //        Console.Write(b ? 1 : 0);
        //    }
        //    Console.WriteLine();
        //    return (left.Or(right));
        //}
        public static uint ROTL(uint word, int n)
        {
            return (word << n) | (word >> (32 - n));
        }

        public static uint ROTR(uint word, int n)
        {
            return (word >> n) | (word << (32 - n));
        }

        public static BitArray ROTL(BitArray word, int n)
        {
            BitArray left = (BitArray)(word.Clone());
            BitArray right = (BitArray)(word.Clone());

            return (left.RightShift(n).Or(right.LeftShift(word.Count - n)));
        }

        public static BitArray ROTR(BitArray word, int n)
        {
            BitArray left = (BitArray)(word.Clone());
            BitArray right = (BitArray)(word.Clone());

            return (left.LeftShift(n).Or(right.RightShift(word.Count - n)));
        }
    }

    public static class SHA12
    {
        static uint h0 = 0x67452301;
        static uint h1 = 0xefcdab89;
        static uint h2 = 0x98badcfe;
        static uint h3 = 0x10325476;
        static uint h4 = 0xc3d2e1f0;

        static BitArray[] blocks;

        static void ResetValues()
        {
            h0 = 0x67452301;
            h1 = 0xefcdab89;
            h2 = 0x98badcfe;
            h3 = 0x10325476;
            h4 = 0xc3d2e1f0;
        }

        static void Print(BitArray b)
        {
            foreach(bool boo in b)
            {
                Console.Write(boo ? 1 : 0);
            }
            Console.WriteLine();
        }

        static BitArray PaddedBlock(BitArray message)
        {
            BitArray bitArray = new BitArray(512);
            int length = message.Count - (blocks.Length - 1) * 512;

            if (length > 448)
                throw new Exception("Imagine that I implemented this part of code");

            for (int ind = 0; ind < length; ind++)
            {
                bitArray[ind] = message[(blocks.Length - 1) * 512 + ind];
            }

            bitArray[length] = true;

            BitArray messageLength = new BitArray(new int[] { message.Count });

            for (int ind = length + 1; ind < 448 + 32; ind++) {
                bitArray[ind] = false;
            }

            for (int ind = 480 /*448 + 32*/; ind < 512; ind++)
            {
                bitArray[ind] = messageLength[31 - (ind - 480)];
            }

            return bitArray;
        }

        static void ParsingMessage(BitArray message)
        {
            if (message.Count % 512 == 0)
                blocks = new BitArray[message.Count / 512];
            else
                blocks = new BitArray[(int)Math.Floor((double)message.Count / 512) + 1];

            for (int iter = 0; iter < blocks.Length; iter++)
            {
                BitArray block;

                if (iter + 1 == blocks.Length && message.Count % 512 != 0)
                {
                    block = PaddedBlock(message);
                }
                else
                {
                    block = new BitArray(512);

                    for (int ind = 0; ind < 512; ind++)
                    {
                        block[ind] = message[ind + iter * 512];
                    }
                }

                blocks[iter] = block;
            }
        }

        static uint K(int t)
        {
            if (t <= 19)
            {
                return 0x5a827999;
            }
            else if (t <= 39)
            {
                return 0x6ed9eba1;
            }
            else if (t <= 59)
            {
                return 0x8f1bbcdc;
            }
            else if (t <= 79)
            {
                return 0xca62c1d6;
            }
            else
            {
                throw new Exception("Something went wrong");
            }
        } 

        static uint f(uint x, uint y, uint z, int t)
        {
            if (t <= 19)
            {
                return (x & y) ^ (~x & z);
            }
            else if (t <= 39)
            {
                return x ^ y ^ z;
            }
            else if (t <= 59)
            {
                return (x & y) ^ (x & z) ^ (y & z);
            }
            else if (t <= 79)
            {
                return x ^ y ^ z;
            }
            else
            {
                throw new Exception("Something went wrong");
            }
            
        }
        //Obeme putin vodka
        public static string Generate(BitArray message)
        {
            ResetValues();
            ParsingMessage(message);

            for (int i = 0; i < blocks.Length; i++)
            {
                //Message schedule
                BitArray[] W = new BitArray[80];
                BitArray[] M = Converter.BlockToWords(blocks[i]);

                for (int t = 0; t < 80; t++)
                {
                    if (t <= 15)
                    {
                        W[t] = M[t];
                    }
                    else
                    {
                        Print(W[t - 3]);
                        Print(W[t - 8]);
                        Print(W[t - 14]);
                        Print(W[t - 16]);
                        W[t] = Operations.ROTL(W[t - 3].Xor(W[t - 8].Xor(W[t - 14].Xor(W[t - 16]))), 1);
                        Print(W[t]);
                    }
                }

                uint a = h0;
                uint b = h1;
                uint c = h2;
                uint d = h3;
                uint e = h4;

                for (int t = 0; t < 80; t++)
                {
                    uint T = Operations.ROTL(a, 5) + f(b, c, d, t) + e + K(t) + Converter.WordToUint(W[t]);
                    e = d;
                    d = c;
                    c = Operations.ROTL(b, 30);
                    b = a;
                    a = T;
                }

                h0 = a + h0;
                h1 = b + h1;
                h2 = c + h2;
                h3 = d + h3;
                h4 = e + h4;
            }

            return h0.ToString("x") + h1.ToString("x") + h2.ToString("x") + h3.ToString("x") + h4.ToString("x");
        }
    }

    public static class Converter
    {
        public static uint WordToUint(BitArray bitArray)
        {
            uint res = 0;

            for (int ind = 0; ind < bitArray.Count; ind++)
            {
                res += (uint)Math.Pow(2, ind) * (uint)(bitArray[bitArray.Count - ind - 1] ? 1 : 0);
            }

            return res;
        }

        public static BitArray[] BlockToWords(BitArray block)
        {
            BitArray[] words = new BitArray[16 /*512÷32*/];

            for (int iter = 0; iter < 16; iter++)
            {
                words[iter] = new BitArray(32);

                for (int ind = 0; ind < 32; ind++)
                {
                    words[iter][ind] = block[ind + iter * 32];
                }
            }

            return words;
        }

        public static BitArray StringToBitArray(string s)
        {
            byte[] bytearray = new byte[s.Length];

            for (int ind = 0; ind < s.Length; ind++)
            {
                if (s[ind] > 255)
                    throw new Exception("StringToBitArray consideres characters to be 8bit size");

                bytearray[ind] = ReversedByte((byte)s[ind]);
            }

            return new BitArray(bytearray);
        }

        public static byte ReversedByte(byte b)
        {
            BitArray input = new BitArray(new byte[] { b });
            BitArray output = new BitArray(8);

            for (int ind = 0; ind < input.Count; ind++)
            {
                output[ind] = input[input.Count - ind - 1];
            }

            return ConvertToByte(output);
        }

        public static byte ConvertToByte(BitArray bits)
        {
            if (bits.Count != 8)
            {
                throw new ArgumentException("bits");
            }
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }
    }

    class Program
    {
        static byte[] StringToByte(string input)
        {
            byte[] b = new byte[input.Length];

            for (int ind = 0; ind < input.Length; ind++)
            {
                b[ind] = (byte)input[ind];
            }

            return b;
        }

        static void Main(string[] args)
        {
            string input = Console.ReadLine();

            //BitArray bar = Converter.StringToBitArray(input);

            //Console.WriteLine(Converter.WordToUint(bar));

            //Console.WriteLine(SHA1.Generate(bar));
        }

        

        
    }
}
