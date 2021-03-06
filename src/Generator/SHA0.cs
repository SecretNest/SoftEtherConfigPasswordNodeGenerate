﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Generator
{
    public sealed class SHA0 : BlockHashAlgorithm
    {
        public SHA0() : base(64)
        {
            this.HashSizeValue = 160;
            this.finalBlock = new byte[BlockSize];

            this.Initialize();
        }

        private readonly IntCounter counter = new IntCounter(2);
        private readonly uint[] state = new uint[5];
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
            state[4] = 0xc3d2e1f0;
        }

        private static uint[] constants = new uint[4]
        {
            // round 1
            0x5a827999,
            // round 2
            0x6ed9eba1,
            // round 3
            0x8f1bbcdc,
            // round 4
            0xca62c1d6
        };

        private uint[] buffer = new uint[80];

        protected override void ProcessBlock(byte[] array, int offset)
        {
            if (array.Length < offset + BlockSize)
                throw new ArgumentOutOfRangeException("offset");

            counter.Add(BlockSize << 3);

            // Fill buffer for transformations
            BigEndianBuffer.BlockCopy(array, offset, buffer, 0, BlockSize);

            for (int ii = 16; ii < 80; ii++)
                buffer[ii] = buffer[ii - 3] ^ buffer[ii - 8] ^ buffer[ii - 14] ^ buffer[ii - 16];

            uint a = state[0];
            uint b = state[1];
            uint c = state[2];
            uint d = state[3];
            uint e = state[4];

            // round 1
            for (int ii = 0; ii < 20; ii += 5)
            {
                e += buffer[ii + 0] + constants[0];
                e += (b & c) ^ (~b & d);
                e += a << 5 | a >> 27;
                b = b << 30 | b >> 2;

                d += buffer[ii + 1] + constants[0];
                d += (a & b) ^ (~a & c);
                d += e << 5 | e >> 27;
                a = a << 30 | a >> 2;

                c += buffer[ii + 2] + constants[0];
                c += (e & a) ^ (~e & b);
                c += d << 5 | d >> 27;
                e = e << 30 | e >> 2;

                b += buffer[ii + 3] + constants[0];
                b += (d & e) ^ (~d & a);
                b += c << 5 | c >> 27;
                d = d << 30 | d >> 2;

                a += buffer[ii + 4] + constants[0];
                a += (c & d) ^ (~c & e);
                a += b << 5 | b >> 27;
                c = c << 30 | c >> 2;
            }

            // round 2
            for (int ii = 20; ii < 40; ii += 5)
            {
                e += buffer[ii + 0] + constants[1];
                e += b ^ c ^ d;
                e += a << 5 | a >> 27;
                b = b << 30 | b >> 2;

                d += buffer[ii + 1] + constants[1];
                d += a ^ b ^ c;
                d += e << 5 | e >> 27;
                a = a << 30 | a >> 2;

                c += buffer[ii + 2] + constants[1];
                c += e ^ a ^ b;
                c += d << 5 | d >> 27;
                e = e << 30 | e >> 2;

                b += buffer[ii + 3] + constants[1];
                b += d ^ e ^ a;
                b += c << 5 | c >> 27;
                d = d << 30 | d >> 2;

                a += buffer[ii + 4] + constants[1];
                a += c ^ d ^ e;
                a += b << 5 | b >> 27;
                c = c << 30 | c >> 2;
            }

            // round 3
            for (int ii = 40; ii < 60; ii += 5)
            {
                e += buffer[ii + 0] + constants[2];
                e += (b & c) ^ (b & d) ^ (c & d);
                e += a << 5 | a >> 27;
                b = b << 30 | b >> 2;

                d += buffer[ii + 1] + constants[2];
                d += (a & b) ^ (a & c) ^ (b & c);
                d += e << 5 | e >> 27;
                a = a << 30 | a >> 2;

                c += buffer[ii + 2] + constants[2];
                c += (e & a) ^ (e & b) ^ (a & b);
                c += d << 5 | d >> 27;
                e = e << 30 | e >> 2;

                b += buffer[ii + 3] + constants[2];
                b += (d & e) ^ (d & a) ^ (e & a);
                b += c << 5 | c >> 27;
                d = d << 30 | d >> 2;

                a += buffer[ii + 4] + constants[2];
                a += (c & d) ^ (c & e) ^ (d & e);
                a += b << 5 | b >> 27;
                c = c << 30 | c >> 2;
            }

            // round 4
            for (int ii = 60; ii < 80; ii += 5)
            {
                e += buffer[ii + 0] + constants[3];
                e += b ^ c ^ d;
                e += a << 5 | a >> 27;
                b = b << 30 | b >> 2;

                d += buffer[ii + 1] + constants[3];
                d += a ^ b ^ c;
                d += e << 5 | e >> 27;
                a = a << 30 | a >> 2;

                c += buffer[ii + 2] + constants[3];
                c += e ^ a ^ b;
                c += d << 5 | d >> 27;
                e = e << 30 | e >> 2;

                b += buffer[ii + 3] + constants[3];
                b += d ^ e ^ a;
                b += c << 5 | c >> 27;
                d = d << 30 | d >> 2;

                a += buffer[ii + 4] + constants[3];
                a += c ^ d ^ e;
                a += b << 5 | b >> 27;
                c = c << 30 | c >> 2;
            }

            state[0] += a;
            state[1] += b;
            state[2] += c;
            state[3] += d;
            state[4] += e;
        }

        protected override void ProcessFinalBlock(byte[] array, int offset, int length)
        {
            if (length >= BlockSize
                || length > array.Length - offset)
                throw new ArgumentOutOfRangeException("length");

            counter.Add(length << 3); // arg * 8

            byte[] messageLength = counter.GetBytes();

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
                finalBlock[endOffset + ii] = messageLength[7 - ii];

            // Processing of last block
            ProcessBlock(finalBlock, 0);
        }

        protected override byte[] Result
        {
            get
            {
                // pack result
                byte[] result = new byte[20];

                BigEndianBuffer.BlockCopy(state, 0, result, 0, result.Length);

                return result;
            }
        }
    }
}
