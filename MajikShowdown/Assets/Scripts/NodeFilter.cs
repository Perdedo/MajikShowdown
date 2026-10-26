public enum NodeSortMode
{
    AcquisitionOrder,
    Category
}

public class NodeFilter
{
    public NodeCategory category;
    public int quality = -1;
    public NodeSortMode sortMode;
    public bool hideUsed;
    public bool reverseSort;
    public bool favoritesOnly;
}