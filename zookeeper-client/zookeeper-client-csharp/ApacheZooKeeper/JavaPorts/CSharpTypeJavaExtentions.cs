namespace ApacheZooKeeper;

using System.Text;
using System.Text.RegularExpressions;

internal static class CSharpTypeJavaExtentions
{
    public static bool isEmpty(this string s) => string.IsNullOrEmpty(s);
    public static int length(this string s) => s?.Length ?? 0;
    public static char charAt(this string s, int index) => s[index];
    public static char[] toCharArray(this string s) => s.ToCharArray();
    public static string replace(this string s, string oldValue, string newValue) => s.Replace(oldValue, newValue);
    public static string replace(this string s, char oldChar, char newChar) => s.Replace(oldChar, newChar);
    public static string replaceAll(this string s, string pattern, string replacement) => Regex.Replace(s, pattern, replacement);
    public static string[] split(this string s, string seprator) => s.Split(new string[] { seprator }, StringSplitOptions.None);
    public static int indexOf(this string s, char c) => s.IndexOf(c);
    public static int lastIndexOf(this string s, char c) => s.LastIndexOf(c);
    public static bool startsWith(this string s, string value) => s.StartsWith(value);
    public static string substring(this string s, int start) => s.Substring(start);
    public static string substring(this string s, int start, int length) => s.Substring(start, length);

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

    public static StringBuilder append(this StringBuilder sb, string value)
    {
        sb.Append(value);
        return sb;
    }

    public static StringBuilder append(this StringBuilder sb, int value)
    {
        sb.Append(value);
        return sb;
    }

    public static string toString(this StringBuilder sb) => sb.ToString();

    public static void close(this Stream stream) => stream.Close();
    public static byte[] toByteArray(this MemoryStream stream) => stream.ToArray();

    public static TResult apply<T, TResult>(this Func<T, TResult> func, T arg) => func(arg);
}
