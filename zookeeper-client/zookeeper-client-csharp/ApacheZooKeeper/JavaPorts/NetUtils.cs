namespace ApacheZooKeeper;

public static class NetUtils
{
    /**
     * Separates host and port from given host port string if host port string is enclosed
     * within square bracket.
     *
     * @param hostPort host port string
     * @return String[]{host, port} if host port string is host:port
     * or String[] {host, port:port} if host port string is host:port:port
     * or String[] {host} if host port string is host
     * or String[]{} if not a ipv6 host port string.
     */
    public static String[] getIPV6HostAndPort(String hostPort) {
        if (hostPort.startsWith("[")) {
            int i = hostPort.lastIndexOf(']');
            if (i < 0) {
                throw new ArgumentException(
                    hostPort + " starts with '[' but has no matching ']'");
            }
            String host = hostPort.substring(1, i);
            if (host.isEmpty()) {
                throw new ArgumentException(host + " is empty.");
            }
            if (hostPort.length() > i + 1) {
                return getHostPort(hostPort, i, host);
            }
            return new String[] { host };
        } else {
            //Not an IPV6 host port string
            return new String[] {};
        }
    }


    private static String[] getHostPort(String hostPort, int indexOfClosingBracket, String host) {
        // [127::1]:2181 , check separator : exits
        if (hostPort.charAt(indexOfClosingBracket + 1) != ':') {
            throw new ArgumentException(hostPort + " does not have : after ]");
        }
        // [127::1]: scenario
        if (indexOfClosingBracket + 2 == hostPort.length()) {
            throw new ArgumentException(hostPort + " doesn't have a port after colon.");
        }
        //do not include
        String port = hostPort.substring(indexOfClosingBracket + 2);
        return new String[] { host, port };
    }
}
