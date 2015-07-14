public class OctreeNeighbour<T>
{
    private readonly OctreeNode<T> _node;

    public OctreeNeighbour(OctreeNode<T> node)
    {
        _node = node;
    }

    public bool IsEmpty()
    {
        return _node == null;
    }
}