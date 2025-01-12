namespace ApacheZooKeeper;

using ApacheZooKeeper.Logging;

/// <summary>
/// CreateMode value determines how the znode is created on ZooKeeper.
/// </summary>
public enum CreateMode
{
    /// <summary>
    /// The znode will not be automatically deleted upon client's disconnect.
    /// </summary>
    PERSISTENT = 0,

    /// <summary>
    /// The znode will not be automatically deleted upon client's disconnect,
    /// and its name will be appended with a monotonically increasing number.
    /// </summary>
    PERSISTENT_SEQUENTIAL = 2,

    /// <summary>
    /// The znode will be deleted upon the client's disconnect.
    /// </summary>
    EPHEMERAL = 1,

    /// <summary>
    /// The znode will be deleted upon the client's disconnect,
    /// and its name will be appended with a monotonically increasing number.
    /// </summary>
    EPHEMERAL_SEQUENTIAL = 3,

    /// <summary>
    /// The znode will be a container node.
    /// </summary>
    CONTAINER = 4,

    /// <summary>
    /// The znode will not be automatically deleted upon client's disconnect.
    /// However, if the znode has not been modified within the given TTL,
    /// it will be deleted once it has no children.
    /// </summary>
    PERSISTENT_WITH_TTL = 5,

    /// <summary>
    /// The znode will not be automatically deleted upon client's disconnect,
    /// and its name will be appended with a monotonically increasing number.
    /// However, if the znode has not been modified within the given TTL,
    /// it will be deleted once it has no children.
    /// </summary>
    PERSISTENT_SEQUENTIAL_WITH_TTL = 6
}

public static class CreateModeExtensions
{
    private static readonly ILogger LOG = LoggerManager.GetLogger<CreateMode>();

    /// <summary>
    /// Checks if the znode is ephemeral.
    /// </summary>
    public static bool isEphemeral(this CreateMode mode) =>
        mode == CreateMode.EPHEMERAL || mode == CreateMode.EPHEMERAL_SEQUENTIAL;

    /// <summary>
    /// Checks if the znode is sequential.
    /// </summary>
    public static bool isSequential(this CreateMode mode) =>
        mode == CreateMode.PERSISTENT_SEQUENTIAL ||
        mode == CreateMode.EPHEMERAL_SEQUENTIAL ||
        mode == CreateMode.PERSISTENT_SEQUENTIAL_WITH_TTL;

    /// <summary>
    /// Checks if the znode is a container.
    /// </summary>
    public static bool isContainer(this CreateMode mode) =>
        mode == CreateMode.CONTAINER;

    /// <summary>
    /// Checks if the znode has a TTL.
    /// </summary>
    public static bool isTTL(this CreateMode mode) =>
        mode == CreateMode.PERSISTENT_WITH_TTL || mode == CreateMode.PERSISTENT_SEQUENTIAL_WITH_TTL;

    public static int toFlag(this CreateMode mode) => (int)mode;

    /// <summary>
    /// Maps an integer value to a CreateMode value.
    /// </summary>
    public static CreateMode fromFlag(int flag)
    {
        return flag switch
        {
            0 => CreateMode.PERSISTENT,
            1 => CreateMode.EPHEMERAL,
            2 => CreateMode.PERSISTENT_SEQUENTIAL,
            3 => CreateMode.EPHEMERAL_SEQUENTIAL,
            4 => CreateMode.CONTAINER,
            5 => CreateMode.PERSISTENT_WITH_TTL,
            6 => CreateMode.PERSISTENT_SEQUENTIAL_WITH_TTL,
            _ => BadArgument(),
        };

        CreateMode BadArgument()
        {
            string errMsg = "Received an invalid flag value: " + flag + " to convert to a CreateMode";
            LOG.error(errMsg);
            throw new KeeperException.BadArgumentsException(errMsg);
        }
    }

    /// <summary>
    /// Maps an integer value to a CreateMode value with a default.
    /// </summary>
    public static CreateMode fromFlag(int flag, CreateMode defaultMode)
    {
        return flag switch
        {
            0 => CreateMode.PERSISTENT,
            1 => CreateMode.EPHEMERAL,
            2 => CreateMode.PERSISTENT_SEQUENTIAL,
            3 => CreateMode.EPHEMERAL_SEQUENTIAL,
            4 => CreateMode.CONTAINER,
            5 => CreateMode.PERSISTENT_WITH_TTL,
            6 => CreateMode.PERSISTENT_SEQUENTIAL_WITH_TTL,
            _ => defaultMode
        };
    }
}
