using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
public class NodeInventory : NetworkBehaviour, IDropZone
{
    //[Header("Prefab Nodes")]
    //public List<SpellNodeInterface> nodePrefabs = new List<SpellNodeInterface>();
    public List<SpellNodeInterface> activeNodes = new List<SpellNodeInterface>();
    public ContentSizeFitter contentSizeFitter;
    public LayoutGroup layoutGroup;
    public UICommandController commander;
    public SpellNodeDescription nodeDescription;
    public TMP_Dropdown typeDropdown;
    public Toggle onlyUnusedToggle;
    public Toggle recentOrderToggle;
    int currentOrder = 0;
    private NodeFilter currentFilter = new NodeFilter();
    private Dictionary<DraggableNode, int> usageCount = new();
    public SpellCaster caster;
    private Dictionary<SpellNode, SpellNodeInterface> nodeMap = new();
    public bool network = true;

    void Start()
    {
        /*if(isLocalPlayer)
        {
            GenerateInventory();
            if(!isServer)
            {
                CMDGenerateInventory();
            }
        }*/
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
        if(!isServer && network)
        {
            if(NetworkClient.ready)
            {
                CMDInitialize();
            }
            else
            {
                StartCoroutine(WaitInitialize());
            }
        }
    }
    IEnumerator WaitInitialize()
    {
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDInitialize();
    }
    [Command]
    public void CMDInitialize()
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
        if (spellNode != null)
            AddNodeToInventory(spellNode);
        ApplyFilter();
        if(!isServer && network)
        {
            /*if(NetworkClient.ready)
            {
                CMDReceive(commander.drags.IndexOf(node));
            }
            else
            {
                StartCoroutine(WaitReceive(node));
            }*/
            StartCoroutine(WaitReceive(node));
        }
    }

    IEnumerator WaitReceive(DraggableNode node)
    {
        //yield return new WaitUntil(() => commander.drags.Contains(node));
        yield return new WaitUntil(() => commander.drags.Exists(d => d.acquisitionOrder == node.acquisitionOrder));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDReceive(node.acquisitionOrder);
        //CMDReceive(commander.drags.IndexOf(node));
    }

    [Command]
    public void CMDReceive(int index)
    {
        //DraggableNode node = commander.drags[index];
        DraggableNode node = commander.drags.Find(d => d.acquisitionOrder == index);
        node.SetOriginZone(this as IDropZone);
        var spellNode = node.GetComponent<SpellNodeInterface>();
        if (spellNode != null)
            AddNodeToInventory(spellNode);
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
        commander.drags.Add(instance.GetComponent<DraggableNode>());
        commander.interfaces.Add(instance);
        var draggable = instance.GetComponent<DraggableNode>();
        if (draggable != null)
        {
            draggable.SetOriginZone(this as IDropZone);
            draggable.acquisitionOrder = activeNodes.Count - 1;
        }
    }

    public void RefreshNodeState(SpellNode nodeData)
    {
        if (nodeMap.TryGetValue(nodeData, out var visual))
        {
            visual.SetUsed(nodeData.IsInUse);
        }
    }

    /*public void GenerateInventory()
    {
        foreach (var prefab in nodePrefabs)
        {
            var instance = Instantiate(prefab, transform);
            activeNodes.Add(instance);
            instance.GetComponent<SpellNodeInterface>().inventory = this;
            var draggable = instance.GetComponent<DraggableNode>();
            if (draggable != null)
                draggable.SetOriginZone(this as IDropZone);
        }
    }

    [Command]
    public void CMDGenerateInventory()
    {
        foreach (var prefab in nodePrefabs)
        {
            var instance = Instantiate(prefab, transform);
            activeNodes.Add(instance);
            instance.GetComponent<SpellNodeInterface>().inventory = this;
            var draggable = instance.GetComponent<DraggableNode>();
            if (draggable != null)
                draggable.SetOriginZone(this as IDropZone);
        }
    }*/

    public void AddNodeToInventory(SpellNodeInterface node)
    {
        node.linkedDescription = nodeDescription;
        if (!activeNodes.Contains(node))
            activeNodes.Add(node);

        node.transform.SetParent(transform, false);

        RectTransform rect = node.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        if(!isServer && network)
        {
            /*if(NetworkClient.ready)
            {
                CMDAddNodeToInventory(commander.interfaces.IndexOf(node));
            }
            else
            {
                StartCoroutine(WaitAddNodeToInventory(node));
            }*/
            StartCoroutine(WaitAddNodeToInventory(node));
        }
    }

    IEnumerator WaitAddNodeToInventory(SpellNodeInterface node)
    {
        //yield return new WaitUntil(() => commander.interfaces.Contains(node));
        yield return new WaitUntil(() => commander.interfaces.Exists(i => i.acquisitionOrder == node.acquisitionOrder));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDAddNodeToInventory(node.acquisitionOrder);
        //CMDAddNodeToInventory(commander.interfaces.IndexOf(node));
    }

    [Command]
    public void CMDAddNodeToInventory(int index)
    {
        //SpellNodeInterface node = commander.interfaces[index];
        SpellNodeInterface node = commander.interfaces.Find(i => i.acquisitionOrder == index);
        node.linkedDescription = nodeDescription;
        if (!activeNodes.Contains(node))
            activeNodes.Add(node);

        node.transform.SetParent(transform, false);

        RectTransform rect = node.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    public void RemoveNodeFromInventory(SpellNodeInterface node)
    {
        if (activeNodes.Contains(node))
        {
            if(!isServer && network)
            {
                /*if(NetworkClient.ready)
                {
                    CMDRemoveNodeFromInventory(activeNodes.IndexOf(node));
                }
                else
                {
                    StartCoroutine(WaitRemoveNodeFromInventory(node));
                }*/
                StartCoroutine(WaitRemoveNodeFromInventory(node));
            }
            activeNodes.Remove(node);
        }
    }
    IEnumerator WaitRemoveNodeFromInventory(SpellNodeInterface node)
    {
        //yield return new WaitUntil(() => commander.interfaces.Contains(node));
        yield return new WaitUntil(() => commander.interfaces.Exists(i => i.acquisitionOrder == node.acquisitionOrder));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDRemoveNodeFromInventory(node.acquisitionOrder);
    }

    [Command]
    public void CMDRemoveNodeFromInventory(int ind)
    {
        SpellNodeInterface node = commander.interfaces.Find(i => i.acquisitionOrder == ind);
        activeNodes.Remove(node);
    }

    void OnFilterChanged(int _)
    {
        currentFilter.category = (NodeCategory)typeDropdown.value;
        currentFilter.onlyUnused = onlyUnusedToggle.isOn;
        currentFilter.orderByRecent = recentOrderToggle.isOn;
        ApplyFilter();
        if(!isServer && network)
        {
            if(NetworkClient.ready)
            {
                CMDOnFilterChanged(_);
            }
            else
            {
                StartCoroutine(WaitOnFilterChanged(_));
            }
        }
    }
    IEnumerator WaitOnFilterChanged(int _)
    {
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDOnFilterChanged(_);
    }

    [Command]
    void CMDOnFilterChanged(int _)
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
        if(!isServer && network)
        {
            /*if(NetworkClient.ready)
            {
                CMDSetNodeInUse(commander.drags.IndexOf(node), inUse);
            }
            else
            {
                StartCoroutine(WaitSetNodeInUse(node, inUse));
            }*/
            StartCoroutine(WaitSetNodeInUse(node, inUse));
        }
    }
    IEnumerator WaitSetNodeInUse(DraggableNode node, bool inUse)
    {
        //yield return new WaitUntil(() => commander.drags.Contains(node));
        yield return new WaitUntil(() => commander.drags.Exists(d => d.acquisitionOrder == node.acquisitionOrder));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDSetNodeInUse(node.acquisitionOrder, inUse);
        //CMDSetNodeInUse(commander.drags.IndexOf(node), inUse);
    }

    [Command]
    public void CMDSetNodeInUse(int index, bool inUse)
    {
        //DraggableNode node = commander.drags[index];
        DraggableNode node = commander.drags.Find(d => d.acquisitionOrder == index);
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
        if(!isServer && network)
        {
            /*if(NetworkClient.ready)
            {
                CMDInsertNodeAt(commander.interfaces.IndexOf(node), index);
            }
            else
            {
                StartCoroutine(WaitInsertNodeAt(node,index));
            }*/
            StartCoroutine(WaitInsertNodeAt(node,index));
        }
    }
    IEnumerator WaitInsertNodeAt(SpellNodeInterface node, int index)
    {
        //yield return new WaitUntil(() => commander.interfaces.Contains(node));
        yield return new WaitUntil(() => commander.interfaces.Exists(i => i.acquisitionOrder == node.acquisitionOrder));
        yield return new WaitUntil(() => NetworkClient.ready);
        CMDInsertNodeAt(node.acquisitionOrder, index);
    }

    [Command]
    public void CMDInsertNodeAt(int nodeInd, int index)
    {
        SpellNodeInterface node = commander.interfaces.Find(i => i.acquisitionOrder == nodeInd);
        //SpellNodeInterface node = commander.interfaces[nodeInd];
        if (activeNodes.Contains(node))
        {
            activeNodes.Remove(node);
        }
        index = Mathf.Clamp(index, 0, activeNodes.Count);
        activeNodes.Insert(index, node);
        ApplyFilter();
    }
}