﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Generator
{
    public sealed class MD4 : BlockHashAlgorithm
    {
        public MD4() : base(64)
        {
            this.HashSizeValue = 128;
            this.finalBlock = new byte[BlockSize];

            this.Initialize();
        }

        private IntCounter counter = new IntCounter(2);
        private readonly uint[] state = new uint[4];
        private readonly byte[] finalBlock;

        public override void Initialize()
        {
            base.Initialize();

            counter.Clear();

            Array.Clear(finalBlock, 0, finalBlock.Length);

            InitializeState();
        }

        private void InitializeState()
        {
            state[0] = 0x67452301;
            state[1] = 0xefcdab89;
            state[2] = 0x98badcfe;
            state[3] = 0x10325476;
        }

        private static readonly uint[] constants = new uint[]
        {
            0x00000000,
            0x5a827999,
            0x6ed9eba1,
        };
        private uint[] buffer = new uint[16];

        protected override void ProcessBlock(byte[] array, int offset)
        {
            if (array.Length < offset + BlockSize)
                throw new ArgumentOutOfRangeException("offset");

            counter.Add(BlockSize << 3);

            // Fill buffer for transformations
            Buffer.BlockCopy(array, offset, buffer, 0, BlockSize);

            uint a = state[0];
            uint b = state[1];
            uint c = state[2];
            uint d = state[3];

            // Round 1
            for (int ii = 0; ii < 16; ii += 4)
            {
                a += buffer[ii + 0] + constants[0];
                a += (b & c) | (~b & d);
                a = a << 3 | a >> 29;

                d += buffer[ii + 1] + constants[0];
                d += (a & b) | (~a & c);
                d = d << 7 | d >> 25;

                c += buffer[ii + 2] + constants[0];
                c += (d & a) | (~d & b);
                c = c << 11 | c >> 21;

                b += buffer[ii + 3] + constants[0];
                b += (c & d) | (~c & a);
                b = b << 19 | b >> 13;
            }

            // Round 2
            for (int ii = 16, jj = 0; ii < 32; ii += 4, jj++)
            {
                a += buffer[jj + 00] + constants[1];
                a += (b & c) | (b & d) | (c & d);
                a = a << 3 | a >> 29;

                d += buffer[jj + 04] + constants[1];
                d += (a & b) | (a & c) | (b & c);
                d = d << 5 | d >> 27;

                c += buffer[jj + 08] + constants[1];
                c += (d & a) | (d & b) | (a & b);
                c = c << 9 | c >> 23;

                b += buffer[jj + 12] + constants[1];
                b += (c & d) | (c & a) | (d & a);
                b = b << 13 | b >> 19;
            }

            // Round 3
            for (int ii = 32, jj = 0; ii < 48; ii += 4, jj++)
            {
                int index = (jj << 1) + -3 * (jj >> 1); // jj * 2 + (jj / 2) * (-3);

                a += buffer[index + 00] + constants[2];
                a += b ^ c ^ d;
                a = a << 3 | a >> 29;

                d += buffer[index + 08] + constants[2];
                d += a ^ b ^ c;
                d = d << 9 | d >> 23;

                c += buffer[index + 04] + constants[2];
                c += d ^ a ^ b;
                c = c << 11 | c >> 21;

                b += buffer[index + 12] + constants[2];
                b += c ^ d ^ a;
                b = b << 15 | b >> 17;
            }

            // The end
            state[0] += a;
            state[1] += b;
            state[2] += c;
            state[3] += d;
        }

        protected override void ProcessFinalBlock(byte[] array, int offset, int length)
        {
            if (length >= BlockSize
                || length > array.Length - offset)
                throw new ArgumentOutOfRangeException("length");

            counter.Add(length << 3);

            byte[] messageLength = counter.GetBytes();

            counter.Clear();

            Buffer.BlockCopy(array, offset, finalBlock, 0, length);

            // padding message with 100..000 bits
            finalBlock[length] = 0x80;

            int endOffset = BlockSize - 8;

            if (length >= endOffset)
            {
                ProcessBlock(finalBlock, 0);

                Array.Clear(finalBlock, 0, finalBlock.Length);
            }

            for (int ii = 0; ii < 8; ii++)
                finalBlock[endOffset + ii] = messageLength[ii];

            // Processing of last block
            ProcessBlock(finalBlock, 0);
        }

        protected override byte[] Result
        {
            get
            {
                // pack result
                byte[] result = new byte[16];

                Buffer.BlockCopy(state, 0, result, 0, result.Length);

                return result;
            }
        }
    }
}
