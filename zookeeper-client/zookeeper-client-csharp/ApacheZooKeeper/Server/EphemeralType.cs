namespace ApacheZooKeeper.Server;

public enum EphemeralType
{
    /// <summary>
    /// Not ephemeral
    /// </summary>
    VOID,

    /// <summary>
    /// Standard, pre-3.5.x EPHEMERAL
    /// </summary>
    NORMAL,

    /// <summary>
    /// Container node
    /// </summary>
    CONTAINER,

    /// <summary>
    /// TTL node
    /// </summary>
    TTL
}

public static class EphemeralTypeExtensions
{
    public static readonly long CONTAINER_EPHEMERAL_OWNER = long.MinValue;
    public static readonly long MAX_EXTENDED_SERVER_ID = 0xfe;  // 254

    private static readonly long EXTENDED_MASK = -72057594037927936L; // 0xff00000000000000L;
    private static readonly long EXTENDED_BIT_TTL = 0x0000;
    private static readonly long RESERVED_BITS_MASK = 0x00ffff0000000000L;
    private static readonly long RESERVED_BITS_SHIFT = 40;

    private static readonly long EXTENDED_FEATURE_VALUE_MASK = ~(EXTENDED_MASK | RESERVED_BITS_MASK);

    private static readonly Dictionary<long, EphemeralType> extendedFeatureMap = new()
    {
        { EXTENDED_BIT_TTL, EphemeralType.TTL },
    };

    private static readonly string EXTENDED_TYPES_ENABLED_PROPERTY = "zookeeper.extendedTypesEnabled";
    private static readonly string TTL_3_5_3_EMULATION_PROPERTY = "zookeeper.emulate353TTLNodes";

    public static void validateTTL(CreateMode mode, long ttl) {
        if (mode.isTTL()) {
            TTL.validate(ttl);
        } else if (ttl >= 0) {
            throw new ArgumentException("ttl not valid for mode: " + mode);
        }
    }

    public static class TTL
    {
        public static long maxValue() {
            return EXTENDED_FEATURE_VALUE_MASK;  // 12725 days, about 34 years
        }

        public static void validate(long ttl) {
            if ((ttl > TTL.maxValue()) || (ttl <= 0)) {
                throw new ArgumentException("ttl must be positive and cannot be larger than: " + TTL.maxValue());
            }
        }
    }
}
