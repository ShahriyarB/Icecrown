// <copyright file="Utility.cs" company="Shahriyar Bazaei">
// Copyright (c) Shahriyar Bazaei. All rights reserved.
// </copyright>

namespace Icecrown.Hostbot.Utils
{
    /// <summary>
    /// Includes some useful functions.
    /// </summary>
    internal static class Utility
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Warcraft Related Encryption")]
        internal static uint XORRotateLeft(byte[] d)
        {
            uint i = 0;
            uint v = 0;

            if (d.Length > 3)
            {
                while (i < d.Length - 3)
                {
                    v = ROTL(v ^ (d[i] + ((uint)d[i + 1] << 8) + ((uint)d[i + 2] << 16) + ((uint)d[i + 3] << 24)), 3);
                    i += 4;
                }
            }

            while (i < d.Length)
            {
                v = ROTL(v ^ d[i], 3);
                ++i;
            }

            return v;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Warcraft Related Encryption")]
        internal static uint ROTL(uint x, int n)
        {
            return (x << n) | (x >> (32 - n));
        }

        /// <summary>
        /// Reads a null terminated string from binary reader.
        /// </summary>
        /// <param name="reader">Binary reader instance.</param>
        /// <returns>Returns read string.</returns>
        internal static string ReadNullTeminatedString(this BinaryReader reader)
        {
            string str = string.Empty;

            byte b;
            while ((b = reader.ReadByte()) != 0)
            {
                str += (char)b;
            }

            return str;
        }

        /// <summary>
        /// Writes a null terminated string to the binary writer.
        /// </summary>
        /// <param name="writer">Binary writer inastance.</param>
        /// <param name="text">Text to write.</param>
        internal static void WriteString(this BinaryWriter writer, string text)
        {
            writer.Write(Encoding.UTF8.GetBytes(text));
            writer.Write((byte)0);
        }

        /// <summary>
        /// Get time in seconds.
        /// </summary>
        /// <returns>Returns current time as seconds.</returns>
        internal static long GetTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Get time in milli seconds.
        /// </summary>
        /// <returns>Returns current time as milli seconds.</returns>
        internal static long GetTicks()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Creates a byte array using a set of game slots.
        /// </summary>
        /// <param name="slots">Game slots.</param>
        /// <param name="randomSeed">Random seed.</param>
        /// <param name="layoutStyle">Layout style.</param>
        /// <param name="playerSlots">Player slots.</param>
        /// <returns>Returns encoded byte array.</returns>
        internal static byte[] EncodeSlotInfo(List<IGameSlot> slots, uint randomSeed, byte layoutStyle, byte playerSlots)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((byte)slots.Count);

                foreach (var slot in slots)
                {
                    writer.Write(slot.ToByteArray());
                }

                writer.Write(randomSeed);
                writer.Write(layoutStyle);
                writer.Write(playerSlots);
            }

            return stream.ToArray();
        }

        /// <summary>
        /// Encodes warcraft stat string.
        /// </summary>
        /// <param name="stat">Wrarcft stat string byte array.</param>
        /// <returns>Returns encoded byte array.</returns>
        internal static byte[] EncodeStatString(byte[] stat)
        {
            List<byte> res = new ();
            byte m = 1;

            for (int i = 0; i < stat.Length; ++i)
            {
                if (stat[i] % 2 == 0)
                {
                    res.Add((byte)(stat[i] + 1));
                }
                else
                {
                    res.Add(stat[i]);
                    m |= (byte)(1 << ((i % 7) + 1));
                }

                if (i % 7 == 6 || i == stat.Length - 1)
                {
                    res.Insert(res.Count - 1 - (i % 7), m);
                    m = 1;
                }
            }

            return res.ToArray();
        }
    }
}
