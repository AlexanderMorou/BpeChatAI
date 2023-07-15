using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#if x64
using IntrinsicPointerType = System.UInt64;
#else
using IntrinsicPointerType = System.UInt32;
#endif
namespace BpeTokenizer;

/// <summary>Provides extension methods to sealed types.</summary>
/// <remarks>These extension methods are used to provide functionality that
/// is not available to the sealed types.</remarks>
public static class ExtensionsToSealedTypes
{
    /// <summary>Holds the bit array of printable characters according to Python's definition of printable.</summary>
    private static readonly BitArray _pyPrintable;

    static ExtensionsToSealedTypes()
    {
        // Load the py_printable resource used to implement IsPyPrintable.
        // Since it's not entirely clear how Python defines printable, I simplified
        // and exported the isprintable as an array of bytes, where each bit of the array
        // represents whether the character is printable or not. This was further condensed
        // by using GZip to compress the array of bytes. It only increases the library size
        // by 593 bytes. The original array of bytes was 8192 bytes to represent the entire
        // unicode character set.
        var assembly = typeof(ExtensionsToSealedTypes).Assembly;
        try
        {
            using var stream = assembly.GetManifestResourceStream("BpeTokenizer.py_printable.blob.gz");
            if (stream == null)
                throw new InvalidOperationException("py_printable resource was not able to be loaded.");

            using var decompressionStream = new GZipStream(stream, CompressionMode.Decompress);
            using var ms = new MemoryStream();
            decompressionStream.CopyTo(ms);
            var buffer = ms.ToArray();

            _pyPrintable = new BitArray(buffer);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("py_printable resource was not able to be loaded.", e);
        }
    }

    /// <summary>Returns whether a character is printable according to Python's definition of printable.</summary>
    /// <param name="c">The character to check.</param>
    /// <returns><c>true</c> if the character is printable; otherwise, <c>false</c>.</returns>
    public static bool IsPyPrintable(this char c)
    // Used by the gpt2 tokenizer to deconstruct its binary payload using
    // BytePairEncodingLoader.DataGymToMergeableBpeRanksAsync.
    => _pyPrintable[c];

    internal static unsafe bool StartsWith(this byte[] array, byte[] otherArray)
    {
        if (array == null)
            return otherArray == null;
        if (otherArray == null)
            return false;
        var rLen = otherArray.Length;
        if (array.Length < rLen)
            return false;
        switch (rLen)
        {
            case 0:
                return true;
            case 1:
                return array[0] == otherArray[0];
            case 2:
                return Unsafe.ReadUnaligned<ushort>(ref array[0]) == Unsafe.ReadUnaligned<ushort>(ref otherArray[0]);
            case 3:
                return Unsafe.ReadUnaligned<ushort>(ref array[0]) == Unsafe.ReadUnaligned<ushort>(ref otherArray[0])
                    && array[2] == otherArray[2];
            case 4:
                return Unsafe.ReadUnaligned<uint>(ref array[0]) == Unsafe.ReadUnaligned<uint>(ref otherArray[0]);
            case 5:
                return Unsafe.ReadUnaligned<uint>(ref array[0]) == Unsafe.ReadUnaligned<uint>(ref otherArray[0])
                    && array[4] == otherArray[4];
            case 6:
                return Unsafe.ReadUnaligned<uint>(ref array[0]) == Unsafe.ReadUnaligned<uint>(ref otherArray[0])
                    && array[4] == otherArray[4]
                    && array[5] == otherArray[5];
            case 7:
                return Unsafe.ReadUnaligned<uint>(ref array[0]) == Unsafe.ReadUnaligned<uint>(ref otherArray[0])
                    && array[4] == otherArray[4]
                    && array[5] == otherArray[5]
                    && array[6] == otherArray[6];
#if x64
            case 8:
                return Unsafe.ReadUnaligned<ulong>(ref array[0]) == Unsafe.ReadUnaligned<ulong>(ref otherArray[0]);
            case 9:
                return Unsafe.ReadUnaligned<ulong>(ref array[0]) == Unsafe.ReadUnaligned<ulong>(ref otherArray[0])
                    && array[8] == otherArray[8];
            case 10:
                return Unsafe.ReadUnaligned<ulong>(ref array[0]) == Unsafe.ReadUnaligned<ulong>(ref otherArray[0])
                    && array[8] == otherArray[8]
                    && array[9] == otherArray[9];
            case 11:
                return Unsafe.ReadUnaligned<ulong>(ref array[0]) == Unsafe.ReadUnaligned<ulong>(ref otherArray[0])
                    && array[8] == otherArray[8]
                    && array[9] == otherArray[9]
                    && array[10] == otherArray[10];
#endif

            default:
                // For all other cases, we'll just do an int-by-int comparison, and
                // then compare the remaining bytes.
                const int sizeOfElement = sizeof(IntrinsicPointerType);
                var intLen = rLen / sizeOfElement;
                var byteRemainder = rLen % sizeOfElement;
                var byteRemainderOffset = rLen - byteRemainder;
        
                // Get the pointer to the first element of the array as an int pointer.
                fixed (byte* lPtr = array)
                fixed (byte* rPtr = otherArray)
                {
                    IntrinsicPointerType* lIntPtr = (IntrinsicPointerType*)lPtr;
                    IntrinsicPointerType* rIntPtr = (IntrinsicPointerType*)rPtr;
                    for (int i = 0; i < intLen; i++, lIntPtr++, rIntPtr++)
                        if (*lIntPtr != *rIntPtr)
                            return false;
                }
                // If the length is not a multiple of 4, we need to check the last few bytes individually.
                for (int i = byteRemainderOffset; i < rLen; i++)
                    if (array[i] != otherArray[i])
                        return false;
                return true;

        }
    }

}
