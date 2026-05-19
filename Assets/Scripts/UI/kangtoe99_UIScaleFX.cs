using UnityEngine;
using UnityEngine.EventSystems;

// UI 스케일 효과 통합 컴포넌트 — 등장 보잉 + 호버 확대를 하나의 컴포넌트에서 처리.
// 단일 source of truth: baseScale. 모든 트윈은 baseScale 을 기준으로 계산되며
// LateUpdate 에서 transform.localScale 을 매 프레임 set 하여 외부/다른 컴포넌트가
// transform 을 건드려도 항상 이 컴포넌트의 의도가 적용된다.
//
// 상태머신: Idle → Boing(OnEnable) → (호버 중이면) HoverIn → Hovering → HoverOut → Idle
// 보잉 도중 호버 enter/exit 는 hovering 플래그만 갱신, 보잉이 끝나면 자동 전이.
// → 두 효과 충돌·visual jump 없음. 보잉 안 쓰는 UI 도 useBoing 끄면 hover 만 동작.
//
// Time.timeScale=0 환경(레벨업/일시정지 패널)에서도 작동 — unscaledDeltaTime 사용.
[DisallowMultipleComponent]
public class kangtoe99_UIScaleFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("모든 효과의 기준 스케일. 외부에서 transform.localScale 을 건드리지 말 것 — 이 컴포넌트가 매 LateUpdate 에 덮어쓴다.")]
    [SerializeField] private Vector3 baseScale = Vector3.one;

    [Header("Boing (등장)")]
    [Tooltip("OnEnable 에 보잉 효과를 자동 재생.")]
    [SerializeField] private bool playBoingOnEnable = true;
    [SerializeField, Min(0.05f)] private float boingDuration = 0.45f;
    [Tooltip("보잉 시작 시 baseScale 에 곱할 배수 (0.5 = 작게 솟아남, 1.2 = 부풀어 튐).")]
    [SerializeField, Min(0.01f)] private float boingStartScale = 0.5f;

    [Header("Hover")]
    [SerializeField] private bool useHover = true;
    [Tooltip("호버 시 baseScale 에 곱할 배수.")]
    [SerializeField, Min(1f)] private float hoverScale = 1.08f;
    [SerializeField, Min(0.01f)] private float hoverDuration = 0.12f;

    private enum State { Idle, Boing, HoverIn, Hovering, HoverOut }
    private State state = State.Idle;
    private float tweenT;
    private bool hovering;
    private Vector3 tweenStart;

    private void OnEnable()
    {
        hovering = false;
        if (playBoingOnEnable)
        {
            state = State.Boing;
            tweenT = 0f;
            transform.localScale = baseScale * boingStartScale;
        }
        else
        {
            state = State.Idle;
            transform.localScale = baseScale;
        }
    }

    private void OnDisable()
    {
        state = State.Idle;
        hovering = false;
        transform.localScale = baseScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!useHover) return;
        hovering = true;
        // 보잉 중에는 hovering 만 기록 — 보잉 끝나면 LateUpdate 가 자동으로 HoverIn 으로 전이한다.
        if (state == State.Boing) return;
        BeginHoverIn();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!useHover) return;
        hovering = false;
        if (state == State.Boing) return;
        BeginHoverOut();
    }

    private void BeginHoverIn()
    {
        tweenStart = transform.localScale; // 현재값에서 출발 → visual jump 없음
        state = State.HoverIn;
        tweenT = 0f;
    }

    private void BeginHoverOut()
    {
        tweenStart = transform.localScale;
        state = State.HoverOut;
        tweenT = 0f;
    }

    private void LateUpdate()
    {
        switch (state)
        {
            case State.Idle:
                transform.localScale = baseScale;
                break;

            case State.Boing:
                tweenT += Time.unscaledDeltaTime;
                float bp = Mathf.Clamp01(tweenT / boingDuration);
                float be = EaseOutElastic(bp);
                float bs = Mathf.LerpUnclamped(boingStartScale, 1f, be);
                transform.localScale = baseScale * bs;
                if (bp >= 1f)
                {
                    if (hovering) BeginHoverIn();
                    else { state = State.Idle; transform.localScale = baseScale; }
                }
                break;

            case State.HoverIn:
                tweenT += Time.unscaledDeltaTime;
                float hip = Mathf.Clamp01(tweenT / hoverDuration);
                float hie = 1f - Mathf.Pow(1f - hip, 3f);
                transform.localScale = Vector3.LerpUnclamped(tweenStart, baseScale * hoverScale, hie);
                if (hip >= 1f) state = State.Hovering;
                break;

            case State.Hovering:
                transform.localScale = baseScale * hoverScale;
                break;

            case State.HoverOut:
                tweenT += Time.unscaledDeltaTime;
                float hop = Mathf.Clamp01(tweenT / hoverDuration);
                float hoe = 1f - Mathf.Pow(1f - hop, 3f);
                transform.localScale = Vector3.LerpUnclamped(tweenStart, baseScale, hoe);
                if (hop >= 1f) { state = State.Idle; transform.localScale = baseScale; }
                break;
        }
    }

    private static float EaseOutElastic(float t)
    {
        if (t <= 0f) return 0f;
        if (t >= 1f) return 1f;
        const float c4 = 2f * Mathf.PI / 3f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }
}
