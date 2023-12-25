using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 값을 부드럽게 변경해주는 클래스
/// </summary>
public class SmoothSlider : MonoBehaviour
{
    /* 컴포넌트 */
    [Header("Smooth Slider")]
    [SerializeField] Slider m_Slider;

    protected Slider GetSlider() => m_Slider;

    /* 필드 */
    [SerializeField] float m_Interval = 1; // 목표값 도달 시간
    float m_TargetValue;
    bool m_IsActivated;

    /* 이벤트 */
    protected event Action<float> OnValueUpdated;

    /* MonoBehaviour */
    protected virtual void Awake()
    {
        // 컴포넌트 할당
        if (!m_Slider)
        {
            m_Slider = GetComponentInChildren<Slider>();
        }

        // 이벤트 바인딩
        OnValueUpdated += OnValueUpdated_Event;
    }

    /* API */
    public void Refresh(float downloadRatio) => SetTargetValue(downloadRatio);

    /* 메서드 */
    protected virtual void OnValueUpdated_Event(float value)
    {
        m_Slider.value = value;
    }

    protected void SetTargetValue(float targetValue)
    {
        // 유효성 검사
        if (targetValue < 0 || Mathf.Approximately(m_Slider.value, targetValue)) return;

        // 목표값 설정
        m_TargetValue = targetValue;

        // UI 갱신 작업 시작
        UpdateValueTask().Forget();
    }

    async UniTaskVoid UpdateValueTask()
    {
        // 중복 작업 방지
        if (m_IsActivated) return;
        m_IsActivated = true;

        // 초기화
        float timer = 0;
        var startValue = m_Slider.value;
        var currentValue = startValue;

        // 1초 내로 목표 값 도달
        while (!Mathf.Approximately(currentValue, m_TargetValue))
        {
            timer += Time.deltaTime;
            currentValue = Mathf.Lerp(startValue, m_TargetValue, timer / m_Interval);

            OnValueUpdated?.Invoke(currentValue);

            await UniTask.Yield();
        }

        OnValueUpdated?.Invoke(m_TargetValue);

        // 작업 종료
        m_IsActivated = false;
    }
}
