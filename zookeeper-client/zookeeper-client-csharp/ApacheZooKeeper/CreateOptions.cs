namespace ApacheZooKeeper;

using ApacheZooKeeper.Data;

/**
 * Options for creating znode in ZooKeeper data tree.
 */
public class CreateOptions {
    private readonly CreateMode createMode;
    private readonly List<ACL> acl;
    private readonly long ttl;

    public CreateMode getCreateMode() {
        return createMode;
    }

    public List<ACL> getAcl() {
        return acl;
    }

    public long getTtl() {
        return ttl;
    }

    /**
     * Constructs a builder for {@link CreateOptions}.
     *
     * @param acl
     *                the acl for the node
     * @param createMode
     *                specifying whether the node to be created is ephemeral
     *                and/or sequential
     */
    public static Builder newBuilder(List<ACL> acl, CreateMode createMode) {
        return new Builder(createMode, acl);
    }

    private CreateOptions(CreateMode createMode, List<ACL> acl, long ttl) {
        this.createMode = createMode;
        this.acl = acl;
        this.ttl = ttl;
        //EphemeralType.validateTTL(createMode, ttl); // TODO: validateTTL
    }

    /**
     * Builder for {@link CreateOptions}.
     */
    public class Builder {
        private readonly CreateMode createMode;
        private readonly List<ACL> acl;
        private long ttl = -1;

        internal Builder(CreateMode createMode, List<ACL> acl) {
            this.createMode = Objects.requireNonNull(createMode, "create mode is mandatory for create options");
            this.acl = Objects.requireNonNull(acl, "acl is mandatory for create options");
        }

        public Builder withTtl(long ttl) {
            this.ttl = ttl;
            return this;
        }

        public CreateOptions build() {
            return new CreateOptions(createMode, acl, ttl);
        }
    }
}
