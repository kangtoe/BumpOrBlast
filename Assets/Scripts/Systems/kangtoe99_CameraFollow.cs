using UnityEngine;

public class kangtoe99_CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private Vector2 offset = Vector2.zero;

    private bool warnedMissingTarget;

    private void LateUpdate()
    {
        if (target == null)
        {
            var player = FindFirstObjectByType<kangtoe99_Player>();
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                if (!warnedMissingTarget)
                {
                    Debug.LogWarning("[kangtoe99_CameraFollow] 추적 대상(kangtoe99_Player)을 찾지 못했습니다.");
                    warnedMissingTarget = true;
                }
                return;
            }
        }

        transform.position = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
