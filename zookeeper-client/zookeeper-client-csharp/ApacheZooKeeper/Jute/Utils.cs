namespace ApacheZooKeeper.Jute;

public static class Utils
{
    public static int compareBytes(byte[] b1, int off1, int len1, byte[] b2, int off2, int len2)
    {
        int i;
        for (i = 0; i < len1 && i < len2; i++)
        {
            if (b1[off1 + i] != b2[off2 + i])
            {
                return b1[off1 + i] < b2[off2 + i] ? -1 : 1;
            }
        }

        if (len1 != len2)
        {
            return len1 < len2 ? -1 : 1;
        }

        return 0;
    }

    public static int CompareTo(this byte[] my, byte[] ur) => compareBytes(my, 0, my.Length, ur, 0, ur.Length);
}
