using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    /* 레퍼런스 */
    [Header("Check Update")]
    [SerializeField] GameObject m_CheckingUpdatePanel;
    [SerializeField] TextMeshProUGUI m_CheckingUpdateText;

    [Header("Process Update")]
    [SerializeField] GameObject m_ProcessUpdatePanel;
    [SerializeField] Button m_DownloadButton;
    [SerializeField] Slider m_DownloadBar;
    [SerializeField] TextMeshProUGUI m_FileSizeText;
    [SerializeField] TextMeshProUGUI m_DownloadRatioText;

    /* 필드 */
    [Header("Asset Label")]
    [SerializeField] AssetLabelReference[] m_Labels;

    long m_TotalPatchSize;
    readonly Dictionary<string, long> m_PatchMap = new Dictionary<string, long>(); // 라벨 별로 다운로드

    /* MonoBehaviour */
    void Awake()
    {
        // 다운로드 버튼 이벤트 바인딩
        m_DownloadButton.onClick.AddListener(OnDownloadButtonClick_Event);
    }

    void Start()
    {
        // 패널 초기화
        m_CheckingUpdatePanel.SetActive(true);
        m_ProcessUpdatePanel.SetActive(false);

        // Addressable 초기화
        InitAddressable();

        // 업데이트 확인
        CheckUpdate();
    }

    /* 이벤트 함수 */
    public void OnDownloadButtonClick_Event()
    {
        // 패치 진행
        Patch();

        // 다운로드 버튼 비활성화
        m_DownloadButton.gameObject.SetActive(false);

        // 다운로드 게이지 바 활성화
        m_DownloadBar.gameObject.SetActive(true);
    }

    /* 메서드 */
    void InitAddressable() => Addressables.InitializeAsync().WaitForCompletion();
    void CheckUpdate() => StartCoroutine(CheckUpdateSequence());
    void Patch()
    {
        // 패치 진행
        StartCoroutine(PatchSequence());

        // 패치 진행 상황 확인
        CheckPatchProgress();
    }

    void CheckPatchProgress() => StartCoroutine(CheckPatchProgressSequence());
    void ChangeLobbyScene() => StartCoroutine(ChangeLobbySceneSequence());

    IEnumerator ChangeLobbySceneSequence()
    {
        // 씬 전환 전 고정 대기 시간
        yield return new WaitForSeconds(2f);

        // 로비 씬 전환
        LoadingManager.LoadScene("Lobby");
    }

    IEnumerator CheckPatchProgressSequence()
    {
        m_DownloadRatioText.text = "0 %";

        while (true)
        {
            // 총 다운로드된 용량 계산
            long totalDownloadSize = m_PatchMap.Sum(pair => pair.Value);

            // UI 갱신
            var downloadRatio = totalDownloadSize / (float)m_TotalPatchSize;
            m_DownloadBar.value = downloadRatio;
            m_DownloadRatioText.text = (int)(downloadRatio * 100) + " %";

            // 다운로드 완료 여부 확인
            if (totalDownloadSize == m_TotalPatchSize)
            {
                // 로비 씬 전환
                ChangeLobbyScene();
                break;
            }

            yield return null;
        }
    }

    IEnumerator PatchSequence()
    {
        // 총 다운로드 파일 사이즈 계산
        foreach (var label in m_Labels)
        {
            // 해당 라벨 파일 패치 확인
            var handle = Addressables.GetDownloadSizeAsync(label.labelString);
            yield return handle;

            // 패치 파일 용량 기록
            if (handle.Result != decimal.Zero)
            {
                // 라벨 패치 진행
                StartCoroutine(DownloadLabelSequence(label.labelString));
            }
        }
    }

    IEnumerator DownloadLabelSequence(string label)
    {
        // 패치 맵 초기화
        m_PatchMap.Add(label, 0);

        // 다운로드 시작
        var handle = Addressables.DownloadDependenciesAsync(label, false);

        // 다운로드 진행 상황 갱신
        while (!handle.IsDone)
        {
            m_PatchMap[label] = handle.GetDownloadStatus().DownloadedBytes;
            yield return null;
        }

        // 다운로드 완료
        m_PatchMap[label] = handle.GetDownloadStatus().TotalBytes;
        Addressables.Release(handle);
    }

    IEnumerator CheckUpdateSequence()
    {
        // 총 다운로드 파일 사이즈 계산
        foreach (var label in m_Labels)
        {
            // 해당 라벨 파일 패치 확인
            var handle = Addressables.GetDownloadSizeAsync(label);
            yield return handle;

            // 패치 파일 용량 기록
            m_TotalPatchSize += handle.Result;
        }

        // 업데이트 여부 확인
        if (m_TotalPatchSize > decimal.Zero)
        {
            // 업데이트 패널 활성화
            m_ProcessUpdatePanel.SetActive(true);

            // 패치 파일 총 크기 표시
            m_FileSizeText.text = GetFileSize(m_TotalPatchSize);
        }
        else
        {
            // 업데이트 없음
            m_CheckingUpdateText.text = "No Available Update";

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
