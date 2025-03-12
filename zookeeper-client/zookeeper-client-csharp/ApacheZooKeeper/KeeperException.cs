namespace ApacheZooKeeper;

public abstract class KeeperException : Exception
{

    /**
     * All multi-requests that result in an exception retain the results
     * here so that it is possible to examine the problems in the catch
     * scope.  Non-multi requests will get a null if they try to access
     * these results.
     */
    private List<OpResult> results;

    /**
     * All non-specific keeper exceptions should be constructed via
     * this factory method in order to guarantee consistency in error
     * codes and such.  If you know the error code, then you should
     * construct the special purpose exception directly.  That will
     * allow you to have the most specific possible declarations of
     * what exceptions might actually be thrown.
     *
     * @param code The error code.
     * @param path The ZooKeeper path being operated on.
     * @return The specialized exception, presumably to be thrown by
     *  the caller.
     */
    public static KeeperException create(Code code, String path) {
        KeeperException r = create(code);
        r.path = path;
        return r;
    }

    /**
     * All non-specific keeper exceptions should be constructed via
     * this factory method in order to guarantee consistency in error
     * codes and such.  If you know the error code, then you should
     * construct the special purpose exception directly.  That will
     * allow you to have the most specific possible declarations of
     * what exceptions might actually be thrown.
     *
     * @param code The error code of your new exception.  This will
     * also determine the specific type of the exception that is
     * returned.
     * @return The specialized exception, presumably to be thrown by
     * the caller.
     */
    public static KeeperException create(Code code) {
        switch (code) {
        case Code.SYSTEMERROR:
            return new SystemErrorException();
        case Code.RUNTIMEINCONSISTENCY:
            return new RuntimeInconsistencyException();
        case Code.DATAINCONSISTENCY:
            return new DataInconsistencyException();
        case Code.CONNECTIONLOSS:
            return new ConnectionLossException();
        case Code.MARSHALLINGERROR:
            return new MarshallingErrorException();
        case Code.UNIMPLEMENTED:
            return new UnimplementedException();
        case Code.OPERATIONTIMEOUT:
            return new OperationTimeoutException();
        case Code.NEWCONFIGNOQUORUM:
            return new NewConfigNoQuorum();
        case Code.RECONFIGINPROGRESS:
            return new ReconfigInProgress();
        case Code.BADARGUMENTS:
            return new BadArgumentsException();
        case Code.APIERROR:
            return new APIErrorException();
        case Code.NONODE:
            return new NoNodeException();
        case Code.NOAUTH:
            return new NoAuthException();
        case Code.BADVERSION:
            return new BadVersionException();
        case Code.NOCHILDRENFOREPHEMERALS:
            return new NoChildrenForEphemeralsException();
        case Code.NODEEXISTS:
            return new NodeExistsException();
        case Code.INVALIDACL:
            return new InvalidACLException();
        case Code.AUTHFAILED:
            return new AuthFailedException();
        case Code.NOTEMPTY:
            return new NotEmptyException();
        case Code.SESSIONEXPIRED:
            return new SessionExpiredException();
        case Code.INVALIDCALLBACK:
            return new InvalidCallbackException();
        case Code.SESSIONMOVED:
            return new SessionMovedException();
        case Code.NOTREADONLY:
            return new NotReadOnlyException();
        case Code.EPHEMERALONLOCALSESSION:
            return new EphemeralOnLocalSessionException();
        case Code.NOWATCHER:
            return new NoWatcherException();
        case Code.RECONFIGDISABLED:
            return new ReconfigDisabledException();
        case Code.SESSIONCLOSEDREQUIRESASLAUTH:
            return new SessionClosedRequireAuthException();
        case Code.REQUESTTIMEOUT:
            return new RequestTimeoutException();
        case Code.QUOTAEXCEEDED:
            return new QuotaExceededException();
        case Code.THROTTLEDOP:
            return new ThrottledOpException();
        case Code.OK:
        default:
            throw new ArgumentException("Invalid exception code:" + (int)code);
        }
    }

    /**
     * Set the code for this exception
     * @param code error code
     * @deprecated deprecated in 3.1.0, exceptions should be immutable, this
     * method should not be used
     */
    /*
    @Deprecated
    public void setCode(int code) {
        this._code = CodeExtensions.get(code);
    }
    */

    /** This interface contains the original static final int constants
     * which have now been replaced with an enumeration in Code. Do not
     * reference this class directly, if necessary (legacy code) continue
     * to access the constants through Code.
     * Note: an interface is used here due to the fact that enums cannot
     * reference constants defined within the same enum as said constants
     * are considered initialized _after_ the enum itself. By using an
     * interface as a super type this allows the deprecated constants to
     * be initialized first and referenced when constructing the enums. I
     * didn't want to have constants declared twice. This
     * interface should be private, but it's declared public to enable
     * javadoc to include in the user API spec.
     */
    private static class CodeDeprecated
    {
        public const int Ok = 0;

        public const int SystemError = -1;

        public const int RuntimeInconsistency = -2;

        public const int DataInconsistency = -3;

        public const int ConnectionLoss = -4;

        public const int MarshallingError = -5;

        public const int Unimplemented = -6;

        public const int OperationTimeout = -7;

        public const int BadArguments = -8;

        public const int UnknownSession = -12;

        public const int NewConfigNoQuorum = -13;

        public const int ReconfigInProgress = -14;

        public const int ApiError = -100;

        public const int NoNode = -101;

        public const int NoAuth = -102;

        public const int BadVersion = -103;

        public const int NoChildrenForEphemerals = -108;

        public const int NodeExists = -110;

        public const int NotEmpty = -111;

        public const int SessionExpired = -112;

        public const int InvalidCallback = -113;

        public const int InvalidAcl = -114;

        public const int AuthFailed = -115;

        public const int EphemeralOnLocalSession = -120;
    }

    public enum Code
    {
        OK = CodeDeprecated.Ok,
        SYSTEMERROR = CodeDeprecated.SystemError,
        RUNTIMEINCONSISTENCY = CodeDeprecated.RuntimeInconsistency,
        DATAINCONSISTENCY = CodeDeprecated.DataInconsistency,
        CONNECTIONLOSS = CodeDeprecated.ConnectionLoss,
        MARSHALLINGERROR = CodeDeprecated.MarshallingError,
        UNIMPLEMENTED = CodeDeprecated.Unimplemented,
        OPERATIONTIMEOUT = CodeDeprecated.OperationTimeout,
        BADARGUMENTS = CodeDeprecated.BadArguments,
        NEWCONFIGNOQUORUM = CodeDeprecated.NewConfigNoQuorum,
        RECONFIGINPROGRESS = CodeDeprecated.ReconfigInProgress,
        UNKNOWNSESSION = CodeDeprecated.UnknownSession,
        APIERROR = CodeDeprecated.ApiError,
        NONODE = CodeDeprecated.NoNode,
        NOAUTH = CodeDeprecated.NoAuth,
        BADVERSION = CodeDeprecated.BadVersion,
        NOCHILDRENFOREPHEMERALS = CodeDeprecated.NoChildrenForEphemerals,
        NODEEXISTS = CodeDeprecated.NodeExists,
        NOTEMPTY = CodeDeprecated.NotEmpty,
        SESSIONEXPIRED = CodeDeprecated.SessionExpired,
        INVALIDCALLBACK = CodeDeprecated.InvalidCallback,
        INVALIDACL = CodeDeprecated.InvalidAcl,
        AUTHFAILED = CodeDeprecated.AuthFailed,
        SESSIONMOVED = -118,
        NOTREADONLY = -119,
        EPHEMERALONLOCALSESSION = CodeDeprecated.EphemeralOnLocalSession,
        NOWATCHER = -121,
        REQUESTTIMEOUT = -122,
        RECONFIGDISABLED = -123,
        SESSIONCLOSEDREQUIRESASLAUTH = -124,
        QUOTAEXCEEDED = -125,
        THROTTLEDOP = -127
    }

    public static string getCodeMessage(Code code) => code switch
    {
        Code.OK => "ok",
        Code.SYSTEMERROR => "SystemError",
        Code.RUNTIMEINCONSISTENCY => "RuntimeInconsistency",
        Code.DATAINCONSISTENCY => "DataInconsistency",
        Code.CONNECTIONLOSS => "ConnectionLoss",
        Code.MARSHALLINGERROR => "MarshallingError",
        Code.NEWCONFIGNOQUORUM => "NewConfigNoQuorum",
        Code.RECONFIGINPROGRESS => "ReconfigInProgress",
        Code.UNIMPLEMENTED => "Unimplemented",
        Code.OPERATIONTIMEOUT => "OperationTimeout",
        Code.BADARGUMENTS => "BadArguments",
        Code.APIERROR => "ApiError",
        Code.NONODE => "NoNode",
        Code.NOAUTH => "NoAuth",
        Code.BADVERSION => "BadVersion",
        Code.NOCHILDRENFOREPHEMERALS => "NoChildrenForEphemerals",
        Code.NODEEXISTS => "NodeExists",
        Code.INVALIDACL => "InvalidAcl",
        Code.AUTHFAILED => "AuthFailed",
        Code.NOTEMPTY => "Directory not empty",
        Code.SESSIONEXPIRED => "Session expired",
        Code.INVALIDCALLBACK => "Invalid callback",
        Code.SESSIONMOVED => "Session moved",
        Code.NOTREADONLY => "Not a read-only call",
        Code.EPHEMERALONLOCALSESSION => "Ephemeral node on local session",
        Code.NOWATCHER => "No such watcher",
        Code.RECONFIGDISABLED => "Reconfig is disabled",
        Code.SESSIONCLOSEDREQUIRESASLAUTH => "Session closed because client failed to authenticate",
        Code.QUOTAEXCEEDED => "Quota has exceeded",
        Code.THROTTLEDOP => "Op throttled due to high load",
        _ => $"Unknown error {code}"
    };

    private Code _code;

    private String path;

    public KeeperException(Code code) {
        this._code = code;
    }

    KeeperException(Code code, String path) {
        this._code = code;
        this.path = path;
    }

    /**
     * Read the error code for this exception
     * @return the error code for this exception
     * @deprecated deprecated in 3.1.0, use {@link #code()} instead
     */
    /*
    @Deprecated
    public int getCode() {
        return code.code;
    }
    */

    /**
     * Read the error Code for this exception
     * @return the error Code for this exception
     */
    public Code code() {
        return _code;
    }

    /**
     * Read the path for this exception
     * @return the path associated with this error, null if none
     */
    public String getPath() {
        return path;
    }

    //@Override
    public String getMessage() {
        if (path == null || path.isEmpty()) {
            return "KeeperErrorCode = " + getCodeMessage(_code);
        }
        return "KeeperErrorCode = " + getCodeMessage(_code) + " for " + path;
    }

    internal void setMultiResults(List<OpResult> results) {
        this.results = results;
    }

    /**
     * If this exception was thrown by a multi-request then the (partial) results
     * and error codes can be retrieved using this getter.
     * @return A copy of the list of results from the operations in the multi-request.
     *
     * @since 3.4.0
     *
     */
    public List<OpResult> getResults() {
        return results != null ? results.ToList() : null;
    }

    /**
     *  @see Code#APIERROR
     */
    public class APIErrorException : KeeperException {

        public APIErrorException()
            : base(Code.APIERROR)
        {
        }

    }

    /**
     *  @see Code#AUTHFAILED
     */
    public class AuthFailedException : KeeperException {

        public AuthFailedException()
            : base(Code.AUTHFAILED)
        {
        }

    }

    /**
     *  @see Code#BADARGUMENTS
     */
    public class BadArgumentsException : KeeperException {

        public BadArgumentsException()
            : base(Code.BADARGUMENTS)
        {
        }
        public BadArgumentsException(String path)
            : base(Code.BADARGUMENTS, path)
        {
        }

    }

    /**
     * @see Code#BADVERSION
     */
    public class BadVersionException : KeeperException {

        public BadVersionException()
            : base(Code.BADVERSION)
        {
        }
        public BadVersionException(String path)
            : base(Code.BADVERSION, path)
        {
        }

    }

    /**
     * @see Code#CONNECTIONLOSS
     */
    public class ConnectionLossException : KeeperException {

        public ConnectionLossException()
            : base(Code.CONNECTIONLOSS)
        {
        }

    }

    /**
     * @see Code#DATAINCONSISTENCY
     */
    public class DataInconsistencyException : KeeperException {

        public DataInconsistencyException()
            : base(Code.DATAINCONSISTENCY)
        {
        }

    }

    /**
     * @see Code#INVALIDACL
     */
    public class InvalidACLException : KeeperException {

        public InvalidACLException()
            : base(Code.INVALIDACL)
        {
        }
        public InvalidACLException(String path)
            : base(Code.INVALIDACL, path)
        {
        }

    }

    /**
     * @see Code#INVALIDCALLBACK
     */
    public class InvalidCallbackException : KeeperException {

        public InvalidCallbackException()
            : base(Code.INVALIDCALLBACK)
        {
        }

    }

    /**
     * @see Code#MARSHALLINGERROR
     */
    public class MarshallingErrorException : KeeperException {

        public MarshallingErrorException()
            : base(Code.MARSHALLINGERROR)
        {
        }

    }

    /**
     * @see Code#NOAUTH
     */
    public class NoAuthException : KeeperException {

        public NoAuthException()
            : base(Code.NOAUTH)
        {
        }

    }

    /**
     * @see Code#NEWCONFIGNOQUORUM
     */
    public class NewConfigNoQuorum : KeeperException {

        public NewConfigNoQuorum()
            : base(Code.NEWCONFIGNOQUORUM)
        {
        }

    }

    /**
     * @see Code#RECONFIGINPROGRESS
     */
    public class ReconfigInProgress : KeeperException {

        public ReconfigInProgress()
            : base(Code.RECONFIGINPROGRESS)
        {
        }

    }

    /**
     * @see Code#NOCHILDRENFOREPHEMERALS
     */
    public class NoChildrenForEphemeralsException : KeeperException {

        public NoChildrenForEphemeralsException()
            : base(Code.NOCHILDRENFOREPHEMERALS)
        {
        }
        public NoChildrenForEphemeralsException(String path)
            : base(Code.NOCHILDRENFOREPHEMERALS, path)
        {
        }

    }

    /**
     * @see Code#NODEEXISTS
     */
    public class NodeExistsException : KeeperException {

        public NodeExistsException()
            : base(Code.NODEEXISTS)
        {
        }
        public NodeExistsException(String path)
            : base(Code.NODEEXISTS, path)
        {
        }

    }

    /**
     * @see Code#NONODE
     */
    public class NoNodeException : KeeperException {

        public NoNodeException()
            : base(Code.NONODE)
        {
        }
        public NoNodeException(String path)
            : base(Code.NONODE, path)
        {
        }

    }

    /**
     * @see Code#NOTEMPTY
     */
    public class NotEmptyException : KeeperException {

        public NotEmptyException()
            : base(Code.NOTEMPTY)
        {
        }
        public NotEmptyException(String path)
            : base(Code.NOTEMPTY, path)
        {
        }

    }

    /**
     * @see Code#OPERATIONTIMEOUT
     */
    public class OperationTimeoutException : KeeperException {

        public OperationTimeoutException()
            : base(Code.OPERATIONTIMEOUT)
        {
        }

    }

    /**
     * @see Code#RUNTIMEINCONSISTENCY
     */
    public class RuntimeInconsistencyException : KeeperException {

        public RuntimeInconsistencyException()
            : base(Code.RUNTIMEINCONSISTENCY)
        {
        }

    }

    /**
     * @see Code#SESSIONEXPIRED
     */
    public class SessionExpiredException : KeeperException {

        public SessionExpiredException()
            : base(Code.SESSIONEXPIRED)
        {
        }

    }

    /**
     * @see Code#UNKNOWNSESSION
     */
    public class UnknownSessionException : KeeperException {

        public UnknownSessionException()
            : base(Code.UNKNOWNSESSION)
        {
        }

    }

    /**
     * @see Code#SESSIONMOVED
     */
    public class SessionMovedException : KeeperException {

        public SessionMovedException()
            : base(Code.SESSIONMOVED)
        {
        }

    }

    /**
     * @see Code#NOTREADONLY
     */
    public class NotReadOnlyException : KeeperException {

        public NotReadOnlyException()
            : base(Code.NOTREADONLY)
        {
        }

    }

    /**
     * @see Code#EPHEMERALONLOCALSESSION
     */
    public class EphemeralOnLocalSessionException : KeeperException {

        public EphemeralOnLocalSessionException()
            : base(Code.EPHEMERALONLOCALSESSION)
        {
        }

    }

    /**
     * @see Code#SYSTEMERROR
     */
    public class SystemErrorException : KeeperException {

        public SystemErrorException()
            : base(Code.SYSTEMERROR)
        {
        }

    }

    /**
     * @see Code#UNIMPLEMENTED
     */
    public class UnimplementedException : KeeperException {

        public UnimplementedException()
            : base(Code.UNIMPLEMENTED)
        {
        }

    }

    /**
     * @see Code#NOWATCHER
     */
    public class NoWatcherException : KeeperException {

        public NoWatcherException()
            : base(Code.NOWATCHER)
        {
        }

        public NoWatcherException(String path)
            : base(Code.NOWATCHER, path)
        {
        }

    }

    /**
     * @see Code#RECONFIGDISABLED
     */
    public class ReconfigDisabledException : KeeperException {

        public ReconfigDisabledException()
            : base(Code.RECONFIGDISABLED)
        {
        }
        public ReconfigDisabledException(String path)
            : base(Code.RECONFIGDISABLED, path)
        {
        }

    }

    /**
     * @see Code#SESSIONCLOSEDREQUIRESASLAUTH
     */
    public class SessionClosedRequireAuthException : KeeperException {

        public SessionClosedRequireAuthException()
            : base(Code.SESSIONCLOSEDREQUIRESASLAUTH)
        {
        }
        public SessionClosedRequireAuthException(String path)
            : base(Code.SESSIONCLOSEDREQUIRESASLAUTH, path)
        {
        }

    }

    /**
     * @see Code#REQUESTTIMEOUT
     */
    public class RequestTimeoutException : KeeperException {

        public RequestTimeoutException()
            : base(Code.REQUESTTIMEOUT)
        {
        }

    }

    /**
     * @see Code#QUOTAEXCEEDED
     */
    public class QuotaExceededException : KeeperException {
        public QuotaExceededException()
            : base(Code.QUOTAEXCEEDED)
        {
        }
        public QuotaExceededException(String path)
            : base(Code.QUOTAEXCEEDED, path)
        {
        }
    }

    /**
     * @see Code#THROTTLEDOP
     */
    public class ThrottledOpException : KeeperException {
        public ThrottledOpException()
            : base(Code.THROTTLEDOP)
        {
        }
    }
}


public static class KeeperExceptionCodeExtensions
{
    private static readonly Dictionary<int, KeeperException.Code> Lookup = new();

    static KeeperExceptionCodeExtensions()
    {
        foreach (KeeperException.Code code in Enum.GetValues(typeof(KeeperException.Code)))
        {
            Lookup[(int)code] = code;
        }
    }

    public static KeeperException.Code get(int code)
    {
        if (!Lookup.TryGetValue(code, out var codeValue))
        {
            throw new ArgumentException($"The current client version cannot lookup this code: {code}");
        }

        return codeValue;
    }

    public static int intValue(this KeeperException.Code code) => (int)code;
}
