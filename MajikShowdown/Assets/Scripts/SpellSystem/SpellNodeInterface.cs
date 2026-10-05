using Mirror.BouncyCastle.Asn1.Mozilla;
using Mirror.BouncyCastle.Math.Field;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpellNodeInterface : MonoBehaviour
{
    //COLOCAR CONECXÕES NESSE SCRIPT
    [HideInInspector] public RectTransform rect;
    public HexGridNode hexGridNode;
    //public SpellNode PrefabNode;
    public SpellNode Node;
    //public NodeConection.Conections[] ConectionPorts = new NodeConection.Conections[6];
    public NodeConection[] conections;
    public SpellNodeInfos info;
    public NodeInventory inventory;
    [SerializeField] private Image mainImage;
    [SerializeField] private Image nodeSymbol;
    [SerializeField] private Image selectedOutline;
    public GameObject usedNodeImg;
    public Image borderImg;
    [HideInInspector] public int acquisitionOrder;
    [HideInInspector] public SpellNodeDescription linkedDescription;
    public int CriticalConections = 0;
    private Tween selectionTween;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        //Initialize();
        /*Node = Instantiate(PrefabNode);
        Node.Interface = this;
        Node.Initialize();
        InitializeConections();
        this.GetComponent<Image>().color *= PrefabNode.color;
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        rect = GetComponent<RectTransform>();
        borderImg = transform.GetChild(0).GetComponent<Image>();*/
    }

    public void Setup(SpellNode nodeData)
    {
        rect = GetComponent<RectTransform>();
        Node = nodeData;
        Node.Interface = this;

        InitializeConections();
        SetupMainVisual();
        SetupBorder();
        SetupSelectionOutline();
        SetupBackground();
        SetupUsedState();
    }

    private void SetupSelectionOutline()
    {
        if (selectedOutline == null) return;

        selectedOutline.sprite = borderImg.sprite;
        selectedOutline.color = Color.white;
        selectedOutline.gameObject.SetActive(false);
    }

    private void SetupMainVisual()
    {
        mainImage.color = Node.color;
        mainImage.alphaHitTestMinimumThreshold = 0.1f;
    }

    private void SetupBorder()
    {
        SetNodeBorder(borderImg);
        borderImg.alphaHitTestMinimumThreshold = 0.1f;
    }

    private void SetupBackground()
    {
        bool hasBackground = Node.nodeSymbolSprite != null;
        nodeSymbol.gameObject.SetActive(hasBackground);

        if (!hasBackground) return;

        nodeSymbol.sprite = Node.nodeSymbolSprite;
        nodeSymbol.color = Node.symbolColor;
        nodeSymbol.alphaHitTestMinimumThreshold = 0.1f;

        SetInventoryVisual();
    }

    private void SetupUsedState()
    {
        usedNodeImg.SetActive(Node.IsInUse);
    }
    /*private void Start()
    {
        SetNodeBorder(borderImg);
    }*/

    [ContextMenu("Initialize")]

    /*public void Initialize()
    {
        rect = GetComponent<RectTransform>();

        borderImg = transform.GetChild(0).GetComponent<Image>();

        usedNodeImg = transform.GetChild(1).gameObject;
        GameManager.Instance.uiController.playerUI.caster.commander.InitializeSNI(this);
    }*/
    public void InitializeConections()
    {
        conections = new NodeConection[] { new(Node, 0), new(Node, 1), new(Node, 2), new(Node, 3), new(Node, 4), new(Node, 5) };
        UpdateConectionPorts();
    }

    public bool TryConectNode(SpellNodeInterface con, int index)
    {
        int mirrorIndex = (index + 3) % 6;
        if (index < conections.Length)
        {
            if (conections[index].TryConect(con.conections[mirrorIndex]))
            {
                //Node.ConectedNodes[index] = con.Node;
                //con.Node.ConectedNodes[mirrorIndex] = Node;
                UpdateConected();
                con.UpdateConected();
                return true;
            }

        }
        return false;
    }
    public bool CheckConectNode(SpellNodeInterface con, int index)
    {
        int mirrorIndex = (index + 3) % 6;
        if (index < conections.Length)
        {
            if (conections[index].CheckConection(con.conections[mirrorIndex]))
            {
                /*if(critical)
                {
                    CriticalConections++;
                    con.CriticalConections++;
                }*/
                return true;
            }

        }
        return false;
    }
    public void BreakConection(int index, bool RemoveNeighbor = true)
    {
        if (index < 0 || index >= conections.Length)
            return;

        NodeConection connection = conections[index];

        if (connection != null && (connection.conectedNode != null || connection.neighborNode != null))
        {
            /*if (connection.conectionType != NodeConection.Conections.None)
                CriticalConections--;
                connection.conectedNode.Interface.CriticalConections--;*/
            connection.RemoveConection(RemoveNeighbor);

            UpdateConected();

            var spell = Node.OwnerSpell;

            if (spell != null)
            {
                spell.UpdateSpell();
            }
        }

        GameManager.Instance.uiController.playerUI.caster.commander.BreakSNIConnection(this, index, RemoveNeighbor);
    }
    public void UpdateConected()
    {
        for (int i = 0; i < conections.Length; i++)
        {
            if (conections[i] != null)
            {
                Node.ConectedNodes[i] = conections[i].conectedNode;
            }
            else
            {
                Node.ConectedNodes[i] = null;
            }
        }

        if (hexGridNode != null)
        {
            hexGridNode.grid.ConfigurateSpell();
        }

        if (GameManager.Instance == null ||
            GameManager.Instance.uiController == null ||
            GameManager.Instance.uiController.playerUI == null ||
            GameManager.Instance.uiController.playerUI.caster == null ||
            GameManager.Instance.uiController.playerUI.caster.commander == null)
        {
            return;
        }

        GameManager.Instance.uiController.playerUI.caster.commander.UpdateSNIConnected(this);
    }
    public void UpdateConectionPorts()
    {
        for (int i = 0; i < conections.Length; i++)
        {
            if (conections[i] != null)
            {
                conections[i].conectionType = Node.ConectionPorts[i];
            }
        }
        //inventory.commander.UpdateSNIConnectionPorts(this);
        //GameManager.Instance.uiController.playerUI.caster.commander.UpdateSNIConnectionPorts(this);
    }

    public void SelectNode()
    {
        var description = linkedDescription ?? GameManager.Instance.uiController.playerUI.spellNodeDescription;
        var ui = GameManager.Instance.uiController.playerUI;

        if (ui.selectedNode == this)
        {
            SetSelectedVisual(false);

            description.HideAll();
            ui.selectedNode = null;
        }
        else
        {
            if (ui.selectedNode != null)
            {
                ui.selectedNode.SetSelectedVisual(false);
            }

            ui.selectedNode = this;
            SetSelectedVisual(true);

            description.ShowDescription(Node);
        }
    }

    public void SelectOnly()
    {
        var ui = GameManager.Instance.uiController.playerUI;

        if (ui.selectedNode == this)
        {
            SetSelectedVisual(true);
            return;
        }

        if (ui.selectedNode != null)
        {
            ui.selectedNode.SetSelectedVisual(false);
        }

        ui.selectedNode = this;
        SetSelectedVisual(true);

        var description = linkedDescription ?? ui.spellNodeDescription;
        description.ShowDescription(Node);
    }

    public void SetNodeBorder(Image img)
    {
        switch (GetCategory())
        {
            case NodeCategory.Core:
                img.sprite = info.core.borderSprite;
                break;

            case NodeCategory.Effect:
                img.sprite = info.effect.borderSprite;
                break;

            case NodeCategory.Trajectory:
                img.sprite = info.trajectory.borderSprite;
                break;

            case NodeCategory.Stat:
                img.sprite = info.stat.borderSprite;
                break;

            case NodeCategory.Trigger:
                img.sprite = info.trigger.borderSprite;
                break;
            case NodeCategory.CastingPoint:
                img.sprite = info.castingPoint.borderSprite;
                break;
        }
    }
    public bool IsUsed()
    {
        return Node.IsInUse;
    }

    public void SetUsed(bool used)
    {
        Node.IsInUse = used;
        ApplyUsedVisual(used);

        if (GameManager.Instance == null ||
            GameManager.Instance.uiController == null ||
            GameManager.Instance.uiController.playerUI == null ||
            GameManager.Instance.uiController.playerUI.caster == null)
        {
            return;
        }

        var caster = GameManager.Instance.uiController.playerUI.caster;

        if (caster.commander != null)
        {
            caster.commander.SetUsedSNI(this, used);
        }

        caster.SetNodeInUse(Node, used);
    }

    public void ApplyUsedVisual(bool used)
    {
        if (usedNodeImg != null)
        {
            usedNodeImg.SetActive(used);
        }
        if (inventory != null)
        {
            inventory.ApplyFilter();
        }
    }

    public NodeCategory GetCategory()
    {
        return Node.GetCategory();
    }

    public void SetSymbolAlpha(byte alpha)
    {
        if (nodeSymbol == null) return;

        Color color = nodeSymbol.color;
        color.a = alpha / 255f;
        nodeSymbol.color = color;
    }

    public void SetInventoryVisual()
    {
        SetSymbolAlpha(80);
    }

    public void SetGridValidVisual()
    {
        SetSymbolAlpha(255);
    }

    public void SetGridInvalidVisual()
    {
        SetSymbolAlpha(35);
    }

    public void SetSelectedVisual(bool selected)
    {
        if (selectedOutline == null) return;

        selectionTween?.Kill();
        selectionTween = null;
        if (!selected)
        {
            selectedOutline.gameObject.SetActive(false);
            return;
        }
        selectedOutline.gameObject.SetActive(true);
        selectedOutline.color = Color.white;
        selectionTween = selectedOutline.DOColor(new Color32(75, 75, 75, 255), 0.25f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        //selectedOutline.transform.DOScale(1.05f, 0.25f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        selectionTween?.Kill();
    }
}
