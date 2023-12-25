using TMPro;
using UnityEngine;

public class DownloadBar : SmoothSlider
{
    /* 컴포넌트 */
    [SerializeField] TextMeshProUGUI m_DownloadRatioText;

    /* MonoBehaviour */
    protected override void Awake()
    {
        base.Awake();

        GetSlider().value = 0;
    }

    /* 메서드 */
    protected override void OnValueUpdated_Event(float value)
    {
        base.OnValueUpdated_Event(value);

        // 다운로드 진행률
        m_DownloadRatioText.text = (int)(value * 100) + " %";
    }
}
