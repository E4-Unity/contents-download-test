using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DownloadManager
{
    /* 필드 */
    readonly IEnumerable<string> m_Labels;
    long m_PatchSize = 0;
    long m_DownloadedSize = 0;
    readonly List<string> m_TargetLabels = new List<string>(); // 업데이트가 필요한 라벨
    readonly Dictionary<string, long> m_PatchMap = new Dictionary<string, long>(); // 라벨 별로 다운로드

    /* 프로퍼티 */
    public long DownloadedSize => m_DownloadedSize;
    public long PatchSize => m_PatchSize;
    public float DownloadedRatio => m_PatchSize == 0 ? 0 : m_DownloadedSize / (float)m_PatchSize;

    /* 이벤트 */
    public event Action<float> OnUpdated; // m_DownloadedSize, m_PatchSize

    /* 생성자 */
    public DownloadManager(IEnumerable<string> labels)
    {
        // 필드 초기화
        m_Labels = labels;

        // Addressable 초기화
        Addressables.InitializeAsync().WaitForCompletion();
    }

    /* API */
    public void CheckUpdate() => CheckUpdateTask().Forget();

    public void Patch()
    {
        // 패치 진행
        foreach (var label in m_TargetLabels)
        {
            // 라벨 패치 진행
            PatchLabel(label);
        }
    }

    /* 메서드 */
    async void PatchLabel(string label)
    {
        // 패치 맵 초기화
        long downloadedBytes = 0;

        // 다운로드 시작
        var handle = Addressables.DownloadDependenciesAsync(label);

        // 다운로드 진행 상황 갱신
        while (!handle.IsDone)
        {
            // 다운로드 파일 크기가 갱신될 때까지 대기
            await UniTask.WaitUntil(() => downloadedBytes != handle.GetDownloadStatus().DownloadedBytes || handle.IsDone);

            // 다운로드 파일 크기 갱신
            m_DownloadedSize += handle.GetDownloadStatus().DownloadedBytes - downloadedBytes;
            downloadedBytes = handle.GetDownloadStatus().DownloadedBytes;

            // 이벤트 호출
            OnUpdated?.Invoke(DownloadedRatio);
        }

        // 다운로드 완료
        OnUpdated?.Invoke(1);
        Addressables.Release(handle);
    }

    async UniTaskVoid CheckUpdateTask()
    {
        // 업데이트 확인
        var capacity = m_Labels.Count();
        Dictionary<string, AsyncOperationHandle<long>> getDownloadSizeHandles = new Dictionary<string, AsyncOperationHandle<long>>(capacity); // Handle 목록
        List<Task> getDownloadSizeTasks = new List<Task>(capacity); // Task 목록

        // 라벨 별로 업데이트 크기 확인
        foreach (var label in m_Labels)
        {
            // 유효성 검사
            if(string.IsNullOrEmpty(label)) continue;

            // 업데이트 크기 확인
            var handle = Addressables.GetDownloadSizeAsync(label);
            getDownloadSizeHandles.Add(label, handle);
            getDownloadSizeTasks.Add(handle.Task);
        }

        // 모든 라벨들에 대한 업데이트 확인이 끝날 때까지 대기
        await Task.WhenAll(getDownloadSizeTasks);

        // 업데이트 크기 계산
        m_PatchSize = 0;
        m_TargetLabels.Capacity = getDownloadSizeHandles.Count;
        foreach (var (label, handle) in getDownloadSizeHandles)
        {
            // 업데이트 가능 여부 확인
            if(handle.Result == decimal.Zero) continue;

            // 업데이트가 필요한 라벨만 기록
            m_TargetLabels.Add(label);
            m_PatchSize += handle.Result;
        }
    }
}
