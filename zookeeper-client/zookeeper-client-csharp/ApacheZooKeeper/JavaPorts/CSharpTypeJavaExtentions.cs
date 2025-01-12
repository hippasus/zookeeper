namespace ApacheZooKeeper;

internal static class CSharpTypeJavaExtentions
{
    public static bool isEmpty(this string s) => string.IsNullOrEmpty(s);
    public static int length(this string s) => s?.Length ?? 0;
    public static char charAt(this string s, int index) => s[index];
    public static char[] toCharArray(this string s) => s.ToCharArray();
    public static string replace(this string s, string oldValue, string newValue) => s.Replace(oldValue, newValue);
    public static string replace(this string s, char oldChar, char newChar) => s.Replace(oldChar, newChar);
    public static string[] split(this string s, string seprator) => s.Split(new string[] { seprator }, StringSplitOptions.None);

    public static int length<T>(this T[] t) => t?.Length ?? 0;

    public static TValue get<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key) => dic[key];

    public static TValue put<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
    {
        dic.TryGetValue(key, out var previousValue);
        dic[key] = value;

        return previousValue;
    }

    public static void clear<TKey, TValue>(this IDictionary<TKey, TValue> dic) => dic?.Clear();

    public static int size<TKey, TValue>(this IDictionary<TKey, TValue> dic) => dic.Count;
    public static List<TKey> keyList<TKey, TValue>(this IDictionary<TKey, TValue> dic) => dic.Keys.ToList();
    public static IEnumerable<TValue> values<TKey, TValue>(this IDictionary<TKey, TValue> dic) => dic.Values;

    public static TValue remove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            dictionary.Remove(key);
        }

        return value;
    }

    public static void addAll<T>(this HashSet<T> hashSet, IEnumerable<T> other) => hashSet.UnionWith(other);
    public static int size<T>(this HashSet<T> hashSet) => hashSet.Count;

    public static bool add<T>(this HashSet<T> hashSet, T value) => hashSet.Add(value);
    public static bool remove<T>(this HashSet<T> hashSet, T value) => hashSet.Remove(value);

    public static bool contains<T>(this HashSet<T> hashSet, T value) => hashSet.Contains(value);
}
