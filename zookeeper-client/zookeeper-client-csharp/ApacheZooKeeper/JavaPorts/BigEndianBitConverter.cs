namespace ApacheZooKeeper;

using System.Runtime.InteropServices;

internal static class BigEndianBitConverter
{
    /// <summary>
    /// Returns a Boolean value converted from one byte at a specified position in a byte array.
    /// </summary>
    /// <param name="value">An array of bytes.</param>
    /// <param name="startIndex">The starting position within value.</param>
    /// <returns>true if the byte at startIndex in value is nonzero; otherwise, false.</returns>
    public static bool ToBoolean(byte[] value, int startIndex)
    {
        CheckByteArgument(value, startIndex, 1);
        return BitConverter.ToBoolean(value, startIndex);
    }

    /// <summary>
    /// Returns a 32-bit signed integer converted from four bytes at a specified position in a byte array.
    /// </summary>
    /// <param name="value">An array of bytes.</param>
    /// <param name="startIndex">The starting position within value.</param>
    /// <returns>A 32-bit signed integer formed by four bytes beginning at startIndex.</returns>
    public static int ToInt32(byte[] value, int startIndex)
    {
        return unchecked((int)(CheckedFromBytes(value, startIndex, 4)));
    }

    /// <summary>
    /// Returns a 64-bit signed integer converted from eight bytes at a specified position in a byte array.
    /// </summary>
    /// <param name="value">An array of bytes.</param>
    /// <param name="startIndex">The starting position within value.</param>
    /// <returns>A 64-bit signed integer formed by eight bytes beginning at startIndex.</returns>
    public static long ToInt64(byte[] value, int startIndex)
    {
        return CheckedFromBytes(value, startIndex, 8);
    }

    /// <summary>
    /// Returns a double-precision floating point number converted from eight bytes
    /// at a specified position in a byte array.
    /// </summary>
    /// <param name="value">An array of bytes.</param>
    /// <param name="startIndex">The starting position within value.</param>
    /// <returns>A double precision floating point number formed by eight bytes beginning at startIndex.</returns>
    public static double ToDouble(byte[] value, int startIndex)
    {
        return Int64BitsToDouble(ToInt64(value, startIndex));
    }

    /// <summary>
    /// Returns a single-precision floating point number converted from four bytes
    /// at a specified position in a byte array.
    /// </summary>
    /// <param name="value">An array of bytes.</param>
    /// <param name="startIndex">The starting position within value.</param>
    /// <returns>A single precision floating point number formed by four bytes beginning at startIndex.</returns>
    public static float ToSingle(byte[] value, int startIndex)
    {
        return Int32BitsToSingle(ToInt32(value, startIndex));
    }

    /// <summary>
    /// Checks the given argument for validity.
    /// </summary>
    /// <param name="value">The byte array passed in</param>
    /// <param name="startIndex">The start index passed in</param>
    /// <param name="bytesRequired">The number of bytes required</param>
    /// <exception cref="ArgumentNullException">value is a null reference</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// startIndex is less than zero or greater than the length of value minus bytesRequired.
    /// </exception>
    private static void CheckByteArgument(byte[] value, int startIndex, int bytesRequired)
    {
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }

        if (startIndex < 0 || startIndex > value.Length - bytesRequired)
        {
            throw new ArgumentOutOfRangeException("startIndex");
        }
    }

    /// <summary>
    /// Checks the arguments for validity before calling FromBytes
    /// (which can therefore assume the arguments are valid).
    /// </summary>
    /// <param name="value">The bytes to convert after checking</param>
    /// <param name="startIndex">The index of the first byte to convert</param>
    /// <param name="bytesToConvert">The number of bytes to convert</param>
    /// <returns></returns>
    static long CheckedFromBytes(byte[] value, int startIndex, int bytesToConvert)
    {
        CheckByteArgument(value, startIndex, bytesToConvert);
        return FromBytes(value, startIndex, bytesToConvert);
    }

    /// <summary>
    /// Returns a value built from the specified number of bytes from the given buffer,
    /// starting at index.
    /// </summary>
    /// <param name="buffer">The data in byte array format</param>
    /// <param name="startIndex">The first index to use</param>
    /// <param name="bytesToConvert">The number of bytes to use</param>
    /// <returns>The value built from the given bytes</returns>
    private static long FromBytes(byte[] buffer, int startIndex, int bytesToConvert)
    {
        long ret = 0;
        for (int i = 0; i < bytesToConvert; i++)
        {
            ret = unchecked((ret << 8) | buffer[startIndex + i]);
        }

        return ret;
    }

    /// <summary>
    /// Copies the given number of bytes from the least-specific
    /// end of the specified value into the specified byte array, beginning
    /// at the specified index.
    /// This is used to implement the other CopyBytes methods.
    /// </summary>
    /// <param name="value">The value to copy bytes for</param>
    /// <param name="bytes">The number of significant bytes to copy</param>
    /// <param name="buffer">The byte array to copy the bytes into</param>
    /// <param name="index">The first index into the array to copy the bytes into</param>
    private static void CopyBytes(long value, int bytes, byte[] buffer, int index)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException("buffer", "Byte array must not be null");
        }

        if (buffer.Length < index + bytes)
        {
            throw new ArgumentOutOfRangeException("buffer", "Buffer not big enough for value");
        }

        CopyBytesImpl(value, bytes, buffer, index);
    }

    /// <summary>
    /// Copies the specified number of bytes from value to buffer, starting at index.
    /// </summary>
    /// <param name="value">The value to copy</param>
    /// <param name="bytes">The number of bytes to copy</param>
    /// <param name="buffer">The buffer to copy the bytes into</param>
    /// <param name="index">The index to start at</param>
    private static void CopyBytesImpl(long value, int bytes, byte[] buffer, int index)
    {
        int endOffset = index + bytes - 1;
        for (int i = 0; i < bytes; i++)
        {
            buffer[endOffset - i] = unchecked((byte)(value & 0xff));
            value = value >> 8;
        }
    }

    /// <summary>
    /// Copies the specified Boolean value into the specified byte array,
    /// beginning at the specified index.
    /// </summary>
    /// <param name="value">A Boolean value.</param>
    /// <param name="buffer">The byte array to copy the bytes into</param>
    /// <param name="index">The first index into the array to copy the bytes into</param>
    public static void CopyBytes(bool value, byte[] buffer, int index)
    {
        CopyBytes(value ? 1 : 0, 1, buffer, index);
    }

    /// <summary>
    /// Copies the specified 32-bit signed integer value into the specified byte array,
    /// beginning at the specified index.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <param name="buffer">The byte array to copy the bytes into</param>
    /// <param name="index">The first index into the array to copy the bytes into</param>
    public static void CopyBytes(int value, byte[] buffer, int index)
    {
        CopyBytes(value, 4, buffer, index);
    }

    /// <summary>
    /// Copies the specified 64-bit signed integer value into the specified byte array,
    /// beginning at the specified index.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <param name="buffer">The byte array to copy the bytes into</param>
    /// <param name="index">The first index into the array to copy the bytes into</param>
    public static void CopyBytes(long value, byte[] buffer, int index)
    {
        CopyBytes(value, 8, buffer, index);
    }

    /// <summary>
    /// Copies the specified single-precision floating point value into the specified byte array,
    /// beginning at the specified index.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <param name="buffer">The byte array to copy the bytes into</param>
    /// <param name="index">The first index into the array to copy the bytes into</param>
    public static void CopyBytes(float value, byte[] buffer, int index)
    {
        CopyBytes(SingleToInt32Bits(value), 4, buffer, index);
    }

    /// <summary>
    /// Copies the specified double-precision floating point value into the specified byte array,
    /// beginning at the specified index.
    /// </summary>
    /// <param name="value">The number to convert.</param>
    /// <param name="buffer">The byte array to copy the bytes into</param>
    /// <param name="index">The first index into the array to copy the bytes into</param>
    public static void CopyBytes(double value, byte[] buffer, int index)
    {
        CopyBytes(DoubleToInt64Bits(value), 8, buffer, index);
    }

    private static long DoubleToInt64Bits(double value)
    {
        return BitConverter.DoubleToInt64Bits(value);
    }

    private static double Int64BitsToDouble(long value)
    {
        return BitConverter.Int64BitsToDouble(value);
    }

    private static float Int32BitsToSingle(int value)
    {
        return new Int32SingleUnion(value).AsSingle;
    }

    private static int SingleToInt32Bits(float value)
    {
        return new Int32SingleUnion(value).AsInt32;
    }

    /// <summary>
    /// Union used solely for the equivalent of DoubleToInt64Bits and vice versa.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    private struct Int32SingleUnion
    {
        /// <summary>
        /// Int32 version of the value.
        /// </summary>
        [FieldOffset(0)]
        int i;

        /// <summary>
        /// Single version of the value.
        /// </summary>
        [FieldOffset(0)]
        float f;

        /// <summary>
        /// Creates an instance representing the given integer.
        /// </summary>
        /// <param name="i">The integer value of the new instance.</param>
        internal Int32SingleUnion(int i)
        {
            this.f = 0; // Just to keep the compiler happy
            this.i = i;
        }

        /// <summary>
        /// Creates an instance representing the given floating point number.
        /// </summary>
        /// <param name="f">The floating point value of the new instance.</param>
        internal Int32SingleUnion(float f)
        {
            this.i = 0; // Just to keep the compiler happy
            this.f = f;
        }

        /// <summary>
        /// Returns the value of the instance as an integer.
        /// </summary>
        internal int AsInt32
        {
            get { return i; }
        }

        /// <summary>
        /// Returns the value of the instance as a floating point number.
        /// </summary>
        internal float AsSingle
        {
            get { return f; }
        }
    }
}
