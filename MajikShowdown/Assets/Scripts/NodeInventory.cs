using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class NodeInventory : MonoBehaviour, IDropZone
{
    [Header("Prefab Nodes")] public List<SpellNodeInterface> nodePrefabs = new List<SpellNodeInterface>();
    private List<SpellNodeInterface> activeNodes = new List<SpellNodeInterface>();
    public ContentSizeFitter contentSizeFitter;
    public LayoutGroup layoutGroup;
    public SpellNodeDescription nodeDescription;
    public TMP_Dropdown typeDropdown;
    public Toggle onlyUnusedToggle;
    public Toggle recentOrderToggle;
    int currentOrder = 0;
    private NodeFilter currentFilter = new NodeFilter();
    void Start()
    {
        GenerateInventory();
        typeDropdown.ClearOptions();
        typeDropdown.AddOptions(new List<string> {
            "All Runes",
            "Effect Runes Only",
            "Stat Runes Only",
            "Trajectory Runes Only",
            "Trigger Runes Only",
            "Core Runes Only"
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
    public void GenerateInventory()
    {
        foreach (var prefab in nodePrefabs)
        {
            var instance = Instantiate(prefab, transform);
            instance.acquisitionOrder = currentOrder++;
            instance.linkedDescription = nodeDescription;
            activeNodes.Add(instance);
            var draggable = instance.GetComponent<DraggableNode>();
            if (draggable != null)
            {
                draggable.SetOriginZone(this as IDropZone);
            }
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
}