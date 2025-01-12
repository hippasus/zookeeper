namespace ApacheZooKeeper;

public static class Objects
{
    public static T requireNonNull<T>(T obj)
    {
        if (obj == null)
        {
            throw new NullReferenceException();
        }

        return obj;
    }

    public static T requireNonNull<T>(T obj, string message)
    {
        if (obj == null)
        {
            throw new NullReferenceException(message);
        }

        return obj;
    }
}
