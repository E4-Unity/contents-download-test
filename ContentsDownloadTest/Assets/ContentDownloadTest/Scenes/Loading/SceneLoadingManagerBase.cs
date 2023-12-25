using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class SceneLoadingManagerBase<TEnum> : SceneLoadingManagerBase where TEnum : Enum
{
    /* Static */
    // API
    public static void LoadScene(TEnum nextScene, TEnum loadingScene) =>
        LoadScene(Convert.ToInt32(nextScene), Convert.ToInt32(loadingScene));
}

public abstract class SceneLoadingManagerBase
{
    /* Static */
    // 필드
    static int NextScene;

    // 이벤트
    public static event Action<float> OnProgressUpdated;

    // API
    public static void LoadScene(int nextScene, int loadingScene) => LoadSceneTask(nextScene, loadingScene).Forget();

    public static void LoadNextScene()
    {
        // 현재 활성화된 씬으로 전환 금지
        if (SceneManager.GetActiveScene().buildIndex == NextScene) return;

        // 씬 전환
        LoadNextSceneTask().Forget();
    }

    public static void ReloadActiveScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    // 메서드
    static async UniTaskVoid LoadSceneTask(int nextScene, int loadingScene)
    {
        // 씬 전환 전 고정 대기 시간
        await UniTask.Delay(TimeSpan.FromSeconds(2));

        // 전환할 씬 이름 기록
        NextScene = nextScene;

        // 로딩 씬 전환
        SceneManager.LoadScene(loadingScene);
    }

    static async UniTaskVoid LoadNextSceneTask()
    {
        // 다음 씬 비동기 로딩 시작
        var op = SceneManager.LoadSceneAsync(NextScene);

        // 자동 씬 전환 방지
        op.allowSceneActivation = false;

        while (!Mathf.Approximately(op.progress, 0.9f))
        {
            // 로딩율 갱신
            OnProgressUpdated?.Invoke(op.progress / 0.9f);

            await UniTask.Yield();
        }

        // 로딩 완료
        OnProgressUpdated?.Invoke(1);

        // 로딩 완료 후 고정 대기 시간
        await UniTask.Delay(TimeSpan.FromSeconds(2));

        // 씬 전환
        op.allowSceneActivation = true;
    }
}
