namespace ApacheZooKeeper;

using ApacheZooKeeper.Data;

using Id = ApacheZooKeeper.Data.ZKId;

public static class ZooDefs
{
    public static readonly String CONFIG_NODE = "/zookeeper/config";

    public static readonly String ZOOKEEPER_NODE_SUBTREE = "/zookeeper/";

    public enum OpCode {

        notification = 0,

        create = 1,

        delete = 2,

        exists = 3,

        getData = 4,

        setData = 5,

        getACL = 6,

        setACL = 7,

        getChildren = 8,

        sync = 9,

        ping = 11,

        getChildren2 = 12,

        check = 13,

        multi = 14,

        create2 = 15,

        reconfig = 16,

        checkWatches = 17,

        removeWatches = 18,

        createContainer = 19,

        deleteContainer = 20,

        createTTL = 21,

        multiRead = 22,

        auth = 100,

        setWatches = 101,

        sasl = 102,

        getEphemerals = 103,

        getAllChildrenNumber = 104,

        setWatches2 = 105,

        addWatch = 106,

        whoAmI = 107,

        createSession = -10,

        closeSession = -11,

        error = -1,

    }

    [Flags]
    public enum Perms {

        READ = 1 << 0,

        WRITE = 1 << 1,

        CREATE = 1 << 2,

        DELETE = 1 << 3,

        ADMIN = 1 << 4,

        ALL = READ | WRITE | CREATE | DELETE | ADMIN,

    }

    public static class Ids {

        /**
         * This Id represents anyone.
         */
        public static readonly Id ANYONE_ID_UNSAFE = new Id("world", "anyone");

        /**
         * This Id is only usable to set ACLs. It will get substituted with the
         * Id's the client authenticated with.
         */
        public static readonly Id AUTH_IDS = new Id("auth", "");

        /**
         * This is a completely open ACL .
         */
        //@SuppressFBWarnings(value = "MS_MUTABLE_COLLECTION", justification = "Cannot break API")
        public static readonly List<ACL> OPEN_ACL_UNSAFE = new(new [] { new ACL((int)Perms.ALL, ANYONE_ID_UNSAFE) });

        /**
         * This ACL gives the creators authentication id's all permissions.
         */
        //@SuppressFBWarnings(value = "MS_MUTABLE_COLLECTION", justification = "Cannot break API")
        public static readonly List<ACL> CREATOR_ALL_ACL = new(new [] { new ACL((int)Perms.ALL, AUTH_IDS) });

        /**
         * This ACL gives the world the ability to read.
         */
        //@SuppressFBWarnings(value = "MS_MUTABLE_COLLECTION", justification = "Cannot break API")
        public static readonly List<ACL> READ_ACL_UNSAFE = new(new [] { new ACL((int)Perms.READ, ANYONE_ID_UNSAFE) });

    }

    //@InterfaceAudience.Public
    public enum AddWatchModes {
        persistent = 0, // matches AddWatchMode.PERSISTENT

        persistentRecursive = 1,  // matches AddWatchMode.PERSISTENT_RECURSIVE
    }

}
