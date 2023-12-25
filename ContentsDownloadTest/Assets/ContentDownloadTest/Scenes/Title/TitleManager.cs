using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    /* const */
    const float GigaBytes = 1073741824;
    const float MegaBytes = 1048576;
    const float KiloBytes = 1024;

    /* 컴포넌트 */
    DownloadManager m_DownloadManager;

    /* 레퍼런스 */
    [Header("Check Update")]
    [SerializeField] GameObject m_CheckingUpdatePanel;
    [SerializeField] TextMeshProUGUI m_CheckingUpdateText;

    [Header("Process Update")]
    [SerializeField] GameObject m_ProcessUpdatePanel;
    [SerializeField] Button m_DownloadButton;
    [SerializeField] TextMeshProUGUI m_FileSizeText;

    [Header("UI")]
    [SerializeField] DownloadBar m_DownloadBar;

    /* 필드 */
    [Header("Asset Label")]
    [SerializeField] AssetLabelReference[] m_Labels; // 업데이트 확인이 필요한 모든 라벨

    /* MonoBehaviour */
    void Awake()
    {
        // 라벨 유효성 검사
        var validLabels = new List<string>(m_Labels.Length);
        foreach (var label in m_Labels)
        {
            if(string.IsNullOrEmpty(label.labelString)) continue;
            validLabels.Add(label.labelString);
        }

        // Download Manager 초기화
        m_DownloadManager = new DownloadManager(validLabels);
        m_DownloadManager.OnUpdated += OnUpdated_Event;

        // 다운로드 버튼 이벤트 바인딩
        m_DownloadButton.onClick.AddListener(OnDownloadButtonClick_Event);

        // 초기화
        Init();
    }

    void Start()
    {
        // 업데이트 확인
        m_DownloadManager.CheckUpdate();
        UpdateUI();
    }

    /* 이벤트 함수 */
    void OnDownloadButtonClick_Event()
    {
        // 패치 진행
        m_DownloadManager.Patch();

        // 다운로드 버튼 비활성화
        m_DownloadButton.gameObject.SetActive(false);

        // 다운로드 게이지 바 활성화
        m_DownloadBar.gameObject.SetActive(true);
    }

    /* 메서드 */
    void Init()
    {
        // 패널 초기화
        m_CheckingUpdatePanel.SetActive(true);
        m_ProcessUpdatePanel.SetActive(false);
    }

    void UpdateUI()
    {
        // 업데이트 여부 확인
        if (m_DownloadManager.PatchSize > decimal.Zero)
        {
            // 업데이트 패널 활성화
            m_ProcessUpdatePanel.SetActive(true);

            // 패치 파일 총 크기 표시
            m_FileSizeText.text = GetFileSize(m_DownloadManager.PatchSize);
        }
        else
        {
            // 업데이트 없음
            m_CheckingUpdateText.text = "No Available Update";

            // 로비 씬 전환
            ChangeLobbyScene();
        }
    }

    void ChangeLobbyScene() => SceneLoadingManager.LoadScene(BuildScene.Lobby);

    void OnUpdated_Event(float downloadedRatio)
    {
        // UI 갱신
        m_DownloadBar.Refresh(downloadedRatio);

        // 다운로드 완료 여부 확인
        if (Mathf.Approximately(downloadedRatio, 1))
        {
            // 로비 씬 전환
            ChangeLobbyScene();
        }
    }

    string GetFileSize(long bytes)
    {
        // 초기화
        string size = "0 Bytes";

        // 유효성 검사
        if (bytes <= 0) return size;

        // 단위 변환
        if (bytes >= GigaBytes)
        {
            size = string.Format("{0:##.##}", bytes / GigaBytes) + " GB";
        }
        else if (bytes >= MegaBytes)
        {
            size = string.Format("{0:##.##}", bytes / MegaBytes) + " MB";
        }
        else if (bytes >= KiloBytes)
        {
            size = string.Format("{0:##.##}", bytes / KiloBytes) + " KB";
        }
        else
        {
            size = bytes + "Bytes";
        }

        return size;
    }
}
