using UnityEngine;

public class kangtoe99_MouseRotationInput : MonoBehaviour, kangtoe99_IRotationInput
{
    [SerializeField] private Camera targetCamera;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    public Vector2 GetTargetDirection(Vector2 playerWorldPosition)
    {
        if (targetCamera == null) return Vector2.up;

        Vector3 mouseWorld = targetCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 diff = (Vector2)mouseWorld - playerWorldPosition;
        return diff.sqrMagnitude < 0.0001f ? Vector2.up : diff.normalized;
    }
}
