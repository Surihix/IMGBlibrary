using System;
using System.IO;
using System.Text;

/// <summary>
/// Provides endianness for BinaryReader methods.
/// </summary>
internal static class BinaryReaderHelpers
{
    /// <summary>
    /// Reads a 2-byte Unsigned integer from the current stream 
    /// and advances the position of the stream by two bytes.
    /// </summary>
    /// <returns>
    /// A 2-byte Unsigned integer read from this stream.
    /// </returns>
    /// <param name="isBigEndian">Indicates whether the bytes are read in Big Endian.</param>
    public static ushort ReadBytesUInt16(this BinaryReader reader, bool isBigEndian)
    {
        var readValueBuffer = reader.ReadBytes(2);
        ReverseBuffer(isBigEndian, readValueBuffer);

        return BitConverter.ToUInt16(readValueBuffer, 0);
    }

    /// <summary>
    /// Reads a 4-byte Unsigned integer from the current stream 
    /// and advances the position of the stream by four bytes.
    /// </summary>
    /// <returns>
    /// A 4-byte Unsigned integer read from this stream.
    /// </returns>
    /// <param name="isBigEndian">Indicates whether the bytes are read in Big Endian.</param>
    public static uint ReadBytesUInt32(this BinaryReader reader, bool isBigEndian)
    {
        var readValueBuffer = reader.ReadBytes(4);
        ReverseBuffer(isBigEndian, readValueBuffer);

        return BitConverter.ToUInt32(readValueBuffer, 0);
    }

    /// <summary>
    /// Reads the specified number of bytes from the current stream and builds a string. then it advances the
    /// current position of the stream by the number of bytes read.
    /// </summary>
    /// <returns>
    /// A string built from the bytes read from the current stream. encoding of the string will be UTF8.
    /// </returns>
    /// <param name="readCount">The number of bytes to read.</param>
    /// <param name="shouldReverse">Indicates whether the bytes should be reversed.</param>
    public static string ReadBytesString(this BinaryReader reader, int readCount, bool shouldReverse)
    {
        var readValueBuffer = reader.ReadBytes(readCount);
        ReverseBuffer(shouldReverse, readValueBuffer);

        return Encoding.UTF8.GetString(readValueBuffer).Replace("\0", "");
    }


    private static void ReverseBuffer(bool isBigEndian, byte[] readValueBuffer)
    {
        if (isBigEndian)
        {
            Array.Reverse(readValueBuffer);
        }
    }
}