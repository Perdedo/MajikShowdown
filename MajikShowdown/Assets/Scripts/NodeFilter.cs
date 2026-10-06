public enum NodeSortMode
{
    AcquisitionOrder,
    Category
}

public class NodeFilter
{
    public NodeCategory category;
    public int quality = -1;
    public bool hideUsed;
    public NodeSortMode sortMode;
    public bool reverseSort;
}