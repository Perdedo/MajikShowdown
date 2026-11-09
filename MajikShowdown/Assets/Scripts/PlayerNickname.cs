using UnityEngine;

public class PlayerNickname : MonoBehaviour
{
    private Camera targetCamera;

    private void LateUpdate()
    {
        UpdateBillboard();
    }
    private void UpdateBillboard()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null) return;

        Vector3 direction = transform.position - targetCamera.transform.position;

        if (direction.sqrMagnitude <= 0.001f) return;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}
