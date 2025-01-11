namespace ApacheZooKeeper.Server.Watch;

public class PathParentIterator
{
    private string path;
    private readonly int maxLevel;

    /**
     * Return a new PathParentIterator that iterates from the
     * given path to all parents.
     *
     * @param path initial path
     */
    public static PathParentIterator forAll(String path)
    {
        return new PathParentIterator(path, int.MaxValue);
    }

    /**
     * Return a new PathParentIterator that only returns the given path - i.e.
     * does not iterate to parent paths.
     *
     * @param path initial path
     */
    public static PathParentIterator forPathOnly(String path)
    {
        return new PathParentIterator(path, 0);
    }

    private PathParentIterator(String path, int maxLevel)
    {
        // NOTE: asserts that the path has already been validated
        this.path = path;
        this.maxLevel = maxLevel;
    }

    public IEnumerable<string> asIterable()
    {
        var path = this.path;
        var level = -1;

        while (hasNext())
        {
            string localPath = path;
            ++level;
            if (path == "/")
            {
                path = "";
            }
            else
            {
                var lastIndexOfSlash = path.LastIndexOf('/');
                if (lastIndexOfSlash <= 0)
                {
                    path = "/";
                }
                else
                {
                    path = path.Substring(0, lastIndexOfSlash);
                }
            }

            yield return localPath;
        }

        bool hasNext()
        {
            return !string.IsNullOrEmpty(path) && (level < maxLevel);
        }
    }
}
