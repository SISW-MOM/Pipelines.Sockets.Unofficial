﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;

namespace Pipelines.Sockets.Unofficial.Internal
{
    internal static class Utils
    {
#if BITOPS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LeadingZeroCount(uint value)
            => BitOperations.LeadingZeroCount(value); // handles intrinsics etc internally
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LeadingZeroCount(uint value)
        {   // see: https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/Numerics/BitOperations.cs
            return value == 0 ? 32 : (31 - Log2SoftwareFallback(value));
        }
        private static int Log2SoftwareFallback(uint value)
        {
            // No AggressiveInlining due to large method size
            // Has conventional contract 0->0 (Log(0) is undefined)

            // Fill trailing zeros with ones, eg 00010010 becomes 00011111
            value |= value >> 01;
            value |= value >> 02;
            value |= value >> 04;
            value |= value >> 08;
            value |= value >> 16;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
                ref MemoryMarshal.GetReference(Log2DeBruijn),
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (IntPtr)(int)((value * 0x07C4ACDDu) >> 27));
        }
        private static ReadOnlySpan<byte> Log2DeBruijn => new byte[32]
{
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };
#endif
    }
}
