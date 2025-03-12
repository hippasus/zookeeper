using ApacheZooKeeper.Jute;
using ApacheZooKeeper.Proto;

namespace ApacheZooKeeper.Compact;

using ApacheZooKeeper.Common;
using ApacheZooKeeper.Proto;

public class ProtocolManager
{
    private VolatileBoolean _isReadonlyAvailable = null;

    public bool isReadonlyAvailable() {
        return _isReadonlyAvailable != null && _isReadonlyAvailable.Value;
    }

    /**
     * Deserializing {@link ConnectRequest} should be specially handled for request from client
     * version before and including ZooKeeper 3.3 which doesn't understand readOnly field.
     */
    public ConnectRequest deserializeConnectRequest(IInputArchive inputArchive) {
        if (_isReadonlyAvailable != null) {
            if (_isReadonlyAvailable.Value) {
                return deserializeConnectRequestWithReadonly(inputArchive);
            } else {
                return deserializeConnectRequestWithoutReadonly(inputArchive);
            }
        }

        ConnectRequest request = deserializeConnectRequestWithoutReadonly(inputArchive);
        try {
            request.setReadOnly(inputArchive.readBool("readOnly"));
            this._isReadonlyAvailable ??= new(false);
            this._isReadonlyAvailable.Value = true;
        } catch (Exception e) {
            request.setReadOnly(false); // old version doesn't have readonly concept
            this._isReadonlyAvailable ??= new(false);
            this._isReadonlyAvailable.Value = false;
        }
        return request;
    }

    private ConnectRequest deserializeConnectRequestWithReadonly(IInputArchive inputArchive) {
        ConnectRequest request = new ConnectRequest();
        request.deserialize(inputArchive, "connect");
        return request;
    }

    private ConnectRequest deserializeConnectRequestWithoutReadonly(InputArchive inputArchive) {
        ConnectRequest request = new ConnectRequest();
        inputArchive.startRecord("connect");
        request.setProtocolVersion(inputArchive.readInt("protocolVersion"));
        request.setLastZxidSeen(inputArchive.readLong("lastZxidSeen"));
        request.setTimeOut(inputArchive.readInt("timeOut"));
        request.setSessionId(inputArchive.readLong("sessionId"));
        request.setPasswd(inputArchive.readBuffer("passwd"));
        inputArchive.endRecord("connect");
        return request;
    }

    /**
     * Deserializing {@link ConnectResponse} should be specially handled for response from server
     * version before and including ZooKeeper 3.3 which doesn't understand readOnly field.
     */
    public ConnectResponse deserializeConnectResponse(InputArchive inputArchive) {
        if (_isReadonlyAvailable != null) {
            if (_isReadonlyAvailable.Value) {
                return deserializeConnectResponseWithReadonly(inputArchive);
            } else {
                return deserializeConnectResponseWithoutReadonly(inputArchive);
            }
        }

        ConnectResponse response = deserializeConnectResponseWithoutReadonly(inputArchive);
        try {
            response.setReadOnly(inputArchive.readBool("readOnly"));
            this._isReadonlyAvailable ??= new(false);
            this._isReadonlyAvailable.Value = true;
        } catch (Exception e) {
            response.setReadOnly(false); // old version doesn't have readonly concept
            this._isReadonlyAvailable ??= new(false);
            this._isReadonlyAvailable.Value = false;
        }
        return response;
    }

    private ConnectResponse deserializeConnectResponseWithReadonly(InputArchive inputArchive) {
        ConnectResponse response = new ConnectResponse();
        response.deserialize(inputArchive, "connect");
        return response;
    }

    private ConnectResponse deserializeConnectResponseWithoutReadonly(InputArchive inputArchive) {
        ConnectResponse response = new ConnectResponse();
        inputArchive.startRecord("connect");
        response.setProtocolVersion(inputArchive.readInt("protocolVersion"));
        response.setTimeOut(inputArchive.readInt("timeOut"));
        response.setSessionId(inputArchive.readLong("sessionId"));
        response.setPasswd(inputArchive.readBuffer("passwd"));
        inputArchive.endRecord("connect");
        return response;
    }

    /**
     * The serialization of {@link ConnectResponse} has to be handled
     * specially as clients earlier than 3.5 might not expect the
     * {@code readOnly} flag.
     *
     * @param response the response to serialize
     * @param outputArchive the serialization destination
     * @see #deserializeConnectRequest(InputArchive)
     */
    public void serializeConnectResponse(ConnectResponse response, OutputArchive outputArchive) {
        serializeConnectResponse(response, outputArchive, isReadonlyAvailable());
    }

    private static void serializeConnectResponse(ConnectResponse response, OutputArchive outputArchive, bool withReadonly) {
        if (withReadonly) {
            serializeConnectResponseWithReadonly(response, outputArchive);
        } else {
            serializeConnectResponseWithoutReadonly(response, outputArchive);
        }
    }

    private static void serializeConnectResponseWithReadonly(ConnectResponse response, OutputArchive outputArchive) {
        response.serialize(outputArchive, "connect");
    }

    private static void serializeConnectResponseWithoutReadonly(ConnectResponse response, OutputArchive outputArchive) {
        outputArchive.startRecord(response, "connect");
        outputArchive.writeInt(response.getProtocolVersion(), "protocolVersion");
        outputArchive.writeInt(response.getTimeOut(), "timeOut");
        outputArchive.writeLong(response.getSessionId(), "sessionId");
        outputArchive.writeBuffer(response.getPasswd(), "passwd");
        outputArchive.endRecord(response, "connect");
    }
}
