namespace ApacheZooKeeper.Client;

public abstract record Chroot
{
    static Chroot ofNullable(string chroot)
    {
        if (chroot == null)
        {
            return new Root();
        }
        return new NotRoot(chroot);
    }

    /**
     * Creates server path by prepending chroot to given client path.
     *
     * @param clientPath client path
     * @return sever path with chroot prepended
     */
    public abstract string prepend(string clientPath);

    /**
     * Creates client path by stripping chroot from given sever path.
     *
     * @param serverPath sever path with chroot prepended
     * @return client path with chroot stripped
     * @throws ArgumentException if given server path contains no chroot
     */
    public abstract string strip(string serverPath);

    /**
     * Creates a delegating callback to strip chroot from created node name.
     */
    //AsyncCallback.StringCallback interceptCallback(AsyncCallback.StringCallback callback); //TODO:

    /**
     * Creates a delegating callback to strip chroot from created node name.
     */
    //AsyncCallback.Create2Callback interceptCallback(AsyncCallback.Create2Callback callback); //TODO:

    /**
     * Creates a delegating watcher to strip chroot from {@link WatchedEvent#getPath()} for given watcher.
     */
    public abstract Watcher interceptWatcher(Watcher watcher);

    public sealed record Root : Chroot
    {
        public override string prepend(string clientPath) { return clientPath; }

        public override string strip(string serverPath) { return serverPath; }

        /*
        @Override
        public AsyncCallback.StringCallback interceptCallback(AsyncCallback.StringCallback callback) {
            return callback;
        }

        @Override
        public AsyncCallback.Create2Callback interceptCallback(AsyncCallback.Create2Callback callback) {
            return callback;
        }
        */

        public override Watcher interceptWatcher(Watcher watcher) {
            return watcher;
        }
    }

    public sealed record NotRoot : Chroot
    {
        private readonly string chroot;

        public NotRoot(string chroot)
        {
            this.chroot = Objects.requireNonNull(chroot);
        }

        public override string prepend(string clientPath) {
            // handle clientPath = "/"
            if (clientPath.Length == 1)
            {
                return chroot;
            }
            return chroot + clientPath;
        }

        public override string strip(string serverPath)
        {
            if (!serverPath.StartsWith(chroot)) {
                string msg = string.Format("server path {0} does no start with chroot {1}", serverPath, chroot);
                throw new ArgumentException(msg);
            }
            if (chroot.Length == serverPath.Length) {
                return "/";
            } else {
                return serverPath.Substring(chroot.Length);
            }
        }

        /*
        @Override
        public AsyncCallback.StringCallback interceptCallback(AsyncCallback.StringCallback callback) {
            return new ChrootCreateCallback(this, callback);
        }

        @Override
        public AsyncCallback.Create2Callback interceptCallback(AsyncCallback.Create2Callback callback) {
            return new ChrootCreateCallback(this, callback);
        }
        */

        public override Watcher interceptWatcher(Watcher watcher) {
            return new ChrootWatcher(this, watcher);
        }

        /*
        @Override
        public boolean equals(Object other) {
            if (other instanceof NotRoot) {
                return Objects.equals(chroot, ((NotRoot) other).chroot);
            }
            return false;
        }

        @Override
        public int hashCode() {
            return Objects.hash(chroot);
        }
        */

        public override string ToString() { return chroot; }
    }
}
