public enum NodeSortMode
{
    AcquisitionOrder,
    Category
    //Rarity
}

public class NodeFilter
{
    public NodeCategory category;
    public bool hideUsed;

    public NodeSortMode sortMode;
    public bool reverseSort;
}