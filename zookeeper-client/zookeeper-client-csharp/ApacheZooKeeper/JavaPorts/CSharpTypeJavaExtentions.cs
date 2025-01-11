namespace ApacheZooKeeper;

internal static class CSharpTypeJavaExtentions
{
    public static int size<TKey, TValue>(this IDictionary<TKey, TValue> dic) => dic.Count;

    public static TValue remove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            dictionary.Remove(key);
        }

        return value;
    }
}
