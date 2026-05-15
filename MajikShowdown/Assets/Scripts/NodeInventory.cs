using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NodeInventory : MonoBehaviour, IDropZone
{
    //[Header("Prefab Nodes")]
    //public List<SpellNodeInterface> nodePrefabs = new List<SpellNodeInterface>();
    private List<SpellNodeInterface> activeNodes = new List<SpellNodeInterface>();
    public ContentSizeFitter contentSizeFitter;
    public LayoutGroup layoutGroup;
    public SpellNodeDescription nodeDescription;
    public TMP_Dropdown typeDropdown;
    public Toggle onlyUnusedToggle;
    public Toggle recentOrderToggle;
    int currentOrder = 0;
    private NodeFilter currentFilter = new NodeFilter();
    private Dictionary<DraggableNode, int> usageCount = new();
    public SpellCaster caster;
    private Dictionary<SpellNode, SpellNodeInterface> nodeMap = new();
    void Start()
    {
        ShowNodeInventory();
        typeDropdown.ClearOptions();
        typeDropdown.AddOptions(new List<string> {
            "Show All Runes",
            "Show Effect Runes",
            "Show Stat Runes",
            "Show Trajectory Runes",
            "Show Trigger Runes",
            "Show Core Runes"
        });
        onlyUnusedToggle.SetIsOnWithoutNotify(false);
        recentOrderToggle.SetIsOnWithoutNotify(false);
        typeDropdown.onValueChanged.RemoveAllListeners();
        onlyUnusedToggle.onValueChanged.RemoveAllListeners();
        recentOrderToggle.onValueChanged.RemoveAllListeners();
        typeDropdown.onValueChanged.AddListener(OnFilterChanged);
        onlyUnusedToggle.onValueChanged.AddListener(_ => OnFilterChanged(0));
        recentOrderToggle.onValueChanged.AddListener(_ => OnFilterChanged(0));
        ApplyFilter();
    }
    public bool CanReceive(DraggableNode node) => true;
    public void Release(DraggableNode node) { }
    public void Receive(DraggableNode node)
    {
        node.SetOriginZone(this as IDropZone);
        var spellNode = node.GetComponent<SpellNodeInterface>();
        if (spellNode != null) AddNodeToInventory(spellNode);
        ApplyFilter();
    }
    public void Freeze()
    {
        if (contentSizeFitter != null) contentSizeFitter.enabled = false;
        if (layoutGroup != null) layoutGroup.enabled = false;
    }
    public void Unfreeze()
    {
        if (layoutGroup != null) layoutGroup.enabled = true;
        if (contentSizeFitter != null) contentSizeFitter.enabled = true;
    }
    public void ShowNodeInventory()
    {
        foreach (var nodeData in caster.runtimeNodes)
        {
            ShowNode(nodeData);
        }
    }

    public void ShowNode(SpellNode nodeData)
    {
        SpellNodeInterface instance = Instantiate(caster.genericNodePrefab, transform);
        instance.Setup(nodeData);
        instance.acquisitionOrder = activeNodes.Count;
        instance.linkedDescription = nodeDescription;
        RectTransform rect = instance.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        nodeMap[nodeData] = instance;
        activeNodes.Add(instance);
        var draggable = instance.GetComponent<DraggableNode>();
        if (draggable != null)
        {
            draggable.SetOriginZone(this as IDropZone);
        }
    }

    public void RefreshNodeState(SpellNode nodeData)
    {
        if (nodeMap.TryGetValue(nodeData, out var visual))
        {
            visual.SetUsed(nodeData.IsInUse);
        }
    }

    public void AddNodeToInventory(SpellNodeInterface node)
    {
        node.linkedDescription = nodeDescription;
        if (!activeNodes.Contains(node))
        {
            activeNodes.Add(node);
        }
        node.transform.SetParent(transform, false);
        RectTransform rect = node.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }
    public void RemoveNodeFromInventory(SpellNodeInterface node)
    {
        if (activeNodes.Contains(node))
        {
            activeNodes.Remove(node);
        }
    }
    void OnFilterChanged(int _)
    {
        currentFilter.category = (NodeCategory)typeDropdown.value;
        currentFilter.onlyUnused = onlyUnusedToggle.isOn;
        currentFilter.orderByRecent = recentOrderToggle.isOn;
        ApplyFilter();
    }
    void ApplyFilter()
    {
        IEnumerable<SpellNodeInterface> query = activeNodes;

        if (currentFilter.category != NodeCategory.All)
        {
            query = query.Where(n => n.GetCategory() == currentFilter.category);
        }

        if (currentFilter.onlyUnused)
        {
            query = query.Where(n => !n.IsUsed());
        }

        if (currentFilter.orderByRecent)
        {
            query = query.OrderByDescending(n => n.acquisitionOrder);
        }

        foreach (var node in activeNodes)
        {
            node.gameObject.SetActive(false);
        }

        int index = 0;
        foreach (var node in query)
        {
            node.gameObject.SetActive(true);
            node.transform.SetSiblingIndex(index++);
        }
    }

    public void SetNodeInUse(DraggableNode node, bool inUse)
    {
        if (!usageCount.ContainsKey(node))
        {
            usageCount[node] = 0;
        }

        usageCount[node] += inUse ? 1 : -1;
        if (usageCount[node] < 0) usageCount[node] = 0;

        var spellNode = node.GetComponent<SpellNodeInterface>();
        spellNode?.SetUsed(usageCount[node] > 0);
    }

    public int GetNodeIndex(SpellNodeInterface node)
    {
        return activeNodes.IndexOf(node);
    }

    public void InsertNodeAt(SpellNodeInterface node, int index)
    {
        if (activeNodes.Contains(node))
        {
            activeNodes.Remove(node);
        }
        index = Mathf.Clamp(index, 0, activeNodes.Count);
        activeNodes.Insert(index, node);
        ApplyFilter();
    }
}