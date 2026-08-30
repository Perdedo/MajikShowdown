using UnityEngine;

public class SpinningTimer : MonoBehaviour
{
    [SerializeField] private HordeController hordeController;

    [Header("Rotation")]
    [SerializeField] private float preparationSpeed;
    [SerializeField] private float hordeSpeed;

    private void Update()
    {
        if (hordeController == null || !hordeController.running) return;

        float rotationSpeed = 0f;
        if (hordeController.inPause)
        {
            rotationSpeed = preparationSpeed;
        }
        else if (hordeController.inHorde)
        {
            rotationSpeed = -hordeSpeed;
        }
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}