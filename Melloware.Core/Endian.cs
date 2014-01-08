/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/
using System;

namespace Melloware.Core {
    /// <summary>
    /// Consider this custom "Endian" class a utility class like "System.BitConvertor".
    /// While the answers above here are valid, I don't really like to have to transfer an integer into an array,
    /// swap it, reconvert it, etc. Nor do I like to make calls to the System.Net.IPAddress utility functions.
    /// It works though.
    /// </summary>
    public class Endian {

        private static readonly bool _LittleEndian;

        static Endian()

        {
            _LittleEndian = BitConverter.IsLittleEndian;
        }

        /// <summary>
        /// Converts a short to its correct Endian representation.
        /// </summary>
        /// <param name="v">the short to convert</param>
        /// <returns>a valid BigEndian short</returns>
        public static short ConvertInt16(short v) {
            return (IsBigEndian ? v : Endian.SwapInt16(v));
        }

        /// <summary>
        /// Converts an int to its correct Endian representation.
        /// </summary>
        /// <param name="v">the int to convert</param>
        /// <returns>a valid BigEndian int</returns>
        public static int ConvertInt32(int v) {
            return (IsBigEndian ? v : Endian.SwapInt32(v));
        }

        /// <summary>
        /// Converts a long to its correct Endian representation.
        /// </summary>
        /// <param name="v">the long to convert</param>
        /// <returns>a valid BigEndian long</returns>
        public static long ConvertInt64(long v) {
            return (IsBigEndian ? v : Endian.SwapInt64(v));
        }

        /// <summary>
        /// Converts a ushort to its correct Endian representation.
        /// </summary>
        /// <param name="v">the ushort to convert</param>
        /// <returns>a valid BigEndian ushort</returns>
        public static ushort ConvertUInt16(ushort v) {
            return (IsBigEndian ? v : Endian.SwapUInt16(v));
        }

        /// <summary>
        /// Converts a uint to its correct Endian representation.
        /// </summary>
        /// <param name="v">the uint to convert</param>
        /// <returns>a valid BigEndian uint</returns>
        public static uint ConvertUInt32(uint v) {
            return (IsBigEndian ? v : Endian.SwapUInt32(v));
        }

        /// <summary>
        /// Converts a ulong to its correct Endian representation.
        /// </summary>
        /// <param name="v">the ulong to convert</param>
        /// <returns>a valid BigEndian ulong</returns>
        public static ulong ConvertUInt64(ulong v) {
            return (IsBigEndian ? v : Endian.SwapUInt64(v));
        }

        private static short SwapInt16(short v) {
            return (short)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
        }

        private static ushort SwapUInt16(ushort v) {
            return (ushort)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
        }

        private static int SwapInt32(int v) {
            return (int)(((SwapInt16((short)v) & 0xffff) << 0x10) |
                         (SwapInt16((short)(v >> 0x10)) & 0xffff));
        }

        private static uint SwapUInt32(uint v) {
            return (uint)(((SwapUInt16((ushort)v) & 0xffff) << 0x10) |
                          (SwapUInt16((ushort)(v >> 0x10)) & 0xffff));
        }

        private static long SwapInt64(long v) {
            return (long)(((SwapInt32((int)v) & 0xffffffffL) << 0x20) |
                          (SwapInt32((int)(v >> 0x20)) & 0xffffffffL));
        }

        private static ulong SwapUInt64(ulong v) {
            return (ulong)(((SwapUInt32((uint)v) & 0xffffffffL) << 0x20) |
                           (SwapUInt32((uint)(v >> 0x20)) & 0xffffffffL));
        }

        private static bool IsBigEndian {
            get {
                return !_LittleEndian;
            }
        }

        private static bool IsLittleEndian {
            get {
                return _LittleEndian;
            }
        }
    }

}

