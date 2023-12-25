using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LoadingManager : MonoBehaviour
{
    /* Static 필드 */
    static string NextScene = "Lobby";

    /* Static 메서드 */
    public static void LoadScene(string sceneName) => LoadSceneTask(sceneName).Forget();

    static async UniTaskVoid LoadSceneTask(string sceneName)
    {
        // 씬 전환 전 고정 대기 시간
        await UniTask.Delay(TimeSpan.FromSeconds(2));

        // 전환할 씬 이름 기록
        NextScene = sceneName;

        // 로딩 씬 전환
        SceneManager.LoadScene("Loading");
    }

    /* 필드 */
    [FormerlySerializedAs("m_LoadingUI")]
    [Header("UI")]
    [SerializeField] LoadingBar m_LoadingBar;

    /* MonoBehaviour */
    void Start()
    {
        LoadNextScene();
    }

    /* 메서드 */
    void LoadNextScene() => LoadNextSceneTask().Forget();
    async UniTaskVoid LoadNextSceneTask()
    {
        // 다음 씬 비동기 로딩 시작
        var op = SceneManager.LoadSceneAsync(NextScene);

        // 자동 씬 전환 방지
        op.allowSceneActivation = false;

        while (!Mathf.Approximately(op.progress, 0.9f))
        {
            // 로딩율 갱신
            m_LoadingBar.Refresh(op.progress / 0.9f);

            await UniTask.Yield();
        }

        // 로딩 완료
        m_LoadingBar.Refresh(1);

        // 로딩 완료 후 고정 대기 시간
        await UniTask.Delay(TimeSpan.FromSeconds(2));

        // 씬 전환
        op.allowSceneActivation = true;
    }
}
