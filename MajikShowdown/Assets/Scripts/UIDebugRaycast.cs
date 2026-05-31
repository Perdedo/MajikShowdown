using UnityEngine;
using UnityEngine.EventSystems;


public class UIDebugRaycast : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Clicked: {gameObject.name}");
    }
}
