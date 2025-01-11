namespace ApacheZooKeeper;

public class Arrays
{
    public static byte[] copyOf(byte[] original, int newLength)
    {
        if (newLength == original.Length)
        {
            return original.ToArray();
        }

        byte[] copy = new byte[newLength];
        Array.Copy(original, 0, copy, 0,
            Math.Min(original.Length, newLength));

        return copy;
    }
}
