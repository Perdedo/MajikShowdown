
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class MenuCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private Transform[] cameraPoints;

    [Header("Rotation")]
    [SerializeField] private float horizontalAmplitude = 10f;
    [SerializeField] private float oscillationSpeed = 0.25f;

    [Header("Timing")]
    [SerializeField] private float minViewDuration = 8f;
    [SerializeField] private float maxViewDuration = 15f;

    [Header("Randomization")]
    [SerializeField] private int recentHistorySize = 3;

    private readonly List<int> shuffleBag = new List<int>();
    private readonly Queue<int> recentPoints = new Queue<int>();

    private int currentPointIndex;
    private Coroutine cameraRoutine;

    private void Start()
    {
        if (virtualCamera == null || cameraPoints == null || cameraPoints.Length == 0)
        {
            Debug.LogError("[MenuCameraController] Missing camera or camera points!");
            enabled = false;
            return;
        }

        for (int i = 0; i < cameraPoints.Length; i++)
        {
            if (cameraPoints[i] == null)
            {
                Debug.LogError("[MenuCameraController] Camera point at index " + i + " is missing!");
                enabled = false;
                return;
            }
        }

        currentPointIndex = 0;
        recentPoints.Enqueue(currentPointIndex);

        RefillShuffleBag(true);
        SetCameraToPoint(currentPointIndex);

        cameraRoutine = StartCoroutine(CameraRoutine());
    }

    private IEnumerator CameraRoutine()
    {
        while (true)
        {
            float viewDuration = UnityEngine.Random.Range(minViewDuration, maxViewDuration);
            float elapsed = 0f;

            while (elapsed < viewDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float horizontal = Mathf.PingPong(elapsed * oscillationSpeed, horizontalAmplitude * 2f) - horizontalAmplitude;

                virtualCamera.transform.rotation = GetPointRotation(currentPointIndex, horizontal);

                yield return null;
            }

            currentPointIndex = GetNextPoint();
            SetCameraToPoint(currentPointIndex);
        }
    }

    private void SetCameraToPoint(int index)
    {
        Vector3 position = cameraPoints[index].position;
        Quaternion rotation = GetPointRotation(index, -horizontalAmplitude);

        virtualCamera.transform.SetPositionAndRotation(position, rotation);
    }

    private int GetNextPoint()
    {
        if (shuffleBag.Count == 0)
            RefillShuffleBag(false);

        int selectedBagIndex = -1;

        for (int i = 0; i < shuffleBag.Count; i++)
        {
            if (!recentPoints.Contains(shuffleBag[i]))
            {
                selectedBagIndex = i;
                break;
            }
        }

        if (selectedBagIndex == -1)
        {
            RefillShuffleBag(false);

            for (int i = 0; i < shuffleBag.Count; i++)
            {
                if (!recentPoints.Contains(shuffleBag[i]))
                {
                    selectedBagIndex = i;
                    break;
                }
            }
        }

        int selected = shuffleBag[selectedBagIndex];
        shuffleBag.RemoveAt(selectedBagIndex);

        recentPoints.Enqueue(selected);

        int historyLimit = Mathf.Clamp(recentHistorySize, 0, cameraPoints.Length - 1);

        while (recentPoints.Count > historyLimit)
            recentPoints.Dequeue();

        return selected;
    }

    private void RefillShuffleBag(bool skipFirstPoint)
    {
        shuffleBag.Clear();

        int startIndex = skipFirstPoint ? 1 : 0;

        for (int i = startIndex; i < cameraPoints.Length; i++)
            shuffleBag.Add(i);

        for (int i = shuffleBag.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (shuffleBag[i], shuffleBag[randomIndex]) = (shuffleBag[randomIndex], shuffleBag[i]);
        }
    }

    private Quaternion GetPointRotation(int index, float horizontal)
    {
        return cameraPoints[index].rotation * Quaternion.Euler(0f, horizontal, 0f);
    }

    private void OnDisable()
    {
        if (cameraRoutine != null)
        {
            StopCoroutine(cameraRoutine);
            cameraRoutine = null;
        }
    }
}
