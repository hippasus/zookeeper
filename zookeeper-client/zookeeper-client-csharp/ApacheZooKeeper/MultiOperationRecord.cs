namespace ApacheZooKeeper;

using ApacheZooKeeper.Jute;
using ApacheZooKeeper.Proto;

using System.Collections;

public class MultiOperationRecord : IRecord, IEnumerable<Op> {

    private List<Op> ops = new();
    private Op.OpKind? opKind = null;

    public MultiOperationRecord() {
    }

    public MultiOperationRecord(IEnumerable<Op> ops) {
        foreach (Op op in ops) {
            setOrCheckOpKind(op.getKind());
            add(op);
        }
    }

    /*
    @Override
    public Iterator<Op> iterator() {
        return ops.iterator();
    }
    */

    public IEnumerator<Op> GetEnumerator() => ops.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void add(Op op) {
        setOrCheckOpKind(op.getKind());
        ops.add(op);
    }

    public int size() {
        return ops.size();
    }

    /**
     * Returns the kind of the operations contained by the record.
     * @return The OpKind value of all the elements in the record.
     */
    public Op.OpKind getOpKind() {
        return opKind ?? default;
    }

    private void setOrCheckOpKind(Op.OpKind ok) {
        if (opKind == null) {
            opKind = ok;
        } else if (ok != opKind) {
            throw new ArgumentException("Mixing read and write operations (transactions)"
                                               + " is not allowed in a multi request.");
        }
    }

    public void Serialize(OutputArchive archive, String tag) {
        archive.startRecord(this, tag);
        foreach (Op op in ops) {
            MultiHeader h = new MultiHeader(op.getType(), false, -1);
            h.serialize(archive, tag);
            switch ((ZooDefs.OpCode)op.getType()) {
            case ZooDefs.OpCode.create:
            case ZooDefs.OpCode.create2:
            case ZooDefs.OpCode.createTTL:
            case ZooDefs.OpCode.createContainer:
            case ZooDefs.OpCode.delete:
            case ZooDefs.OpCode.setData:
            case ZooDefs.OpCode.check:
            case ZooDefs.OpCode.getChildren:
            case ZooDefs.OpCode.getData:
                op.toRequestRecord().serialize(archive, tag);
                break;
            default:
                throw new IOException("Invalid type of op");
            }
        }
        new MultiHeader(-1, true, -1).serialize(archive, tag);
        archive.endRecord(this, tag);
    }

    public void Deserialize(InputArchive archive, String tag) {
        archive.startRecord(tag);
        MultiHeader h = new MultiHeader();
        h.deserialize(archive, tag);
        try {
            while (!h.getDone()) {
                switch ((ZooDefs.OpCode)h.getType()) {
                case ZooDefs.OpCode.create:
                case ZooDefs.OpCode.create2:
                case ZooDefs.OpCode.createContainer:
                    CreateRequest cr = new CreateRequest();
                    cr.deserialize(archive, tag);
                    CreateMode? createMode = CreateModeExtensions.fromFlag(cr.getFlags(), null);
                    if (createMode == null) {
                        throw new IOException("invalid flag " + cr.getFlags() + " for create mode");
                    }
                    CreateOptions options = CreateOptions.newBuilder(cr.getAcl(), createMode.Value).build();
                    add(Op.create(cr.getPath(), cr.getData(), options, (ZooDefs.OpCode)h.getType()));
                    break;
                case ZooDefs.OpCode.createTTL:
                    CreateTTLRequest crTtl = new CreateTTLRequest();
                    crTtl.deserialize(archive, tag);
                    add(Op.create(crTtl.getPath(), crTtl.getData(), crTtl.getAcl(), crTtl.getFlags(), crTtl.getTtl()));
                    break;
                case ZooDefs.OpCode.delete:
                    DeleteRequest dr = new DeleteRequest();
                    dr.deserialize(archive, tag);
                    add(Op.delete(dr.getPath(), dr.getVersion()));
                    break;
                case ZooDefs.OpCode.setData:
                    SetDataRequest sdr = new SetDataRequest();
                    sdr.deserialize(archive, tag);
                    add(Op.setData(sdr.getPath(), sdr.getData(), sdr.getVersion()));
                    break;
                case ZooDefs.OpCode.check:
                    CheckVersionRequest cvr = new CheckVersionRequest();
                    cvr.deserialize(archive, tag);
                    add(Op.check(cvr.getPath(), cvr.getVersion()));
                    break;
                case ZooDefs.OpCode.getChildren:
                    GetChildrenRequest gcr = new GetChildrenRequest();
                    gcr.deserialize(archive, tag);
                    add(Op.getChildren(gcr.getPath()));
                    break;
                case ZooDefs.OpCode.getData:
                    GetDataRequest gdr = new GetDataRequest();
                    gdr.deserialize(archive, tag);
                    add(Op.getData(gdr.getPath()));
                    break;
                default:
                    throw new IOException("Invalid type of op");
                }
                h.deserialize(archive, tag);
            }
        } catch (ArgumentException e) {
            throw new IOException("Mixing different kind of ops");
        }
        archive.endRecord(tag);
    }

    /*
    @Override
    public boolean equals(Object o) {
        if (this == o) {
            return true;
        }
        if (!(o instanceof MultiOperationRecord)) {
            return false;
        }

        MultiOperationRecord that = (MultiOperationRecord) o;

        if (ops != null) {
            Iterator<Op> other = that.ops.iterator();
            for (Op op : ops) {
                boolean hasMoreData = other.hasNext();
                if (!hasMoreData) {
                    return false;
                }
                Op otherOp = other.next();
                if (!op.equals(otherOp)) {
                    return false;
                }
            }
            return !other.hasNext();
        } else {
            return that.ops == null;
        }

    }

    @Override
    public int hashCode() {
        int h = 1023;
        for (Op op : ops) {
            h = h * 25 + op.hashCode();
        }
        return h;
    }
    */

}
