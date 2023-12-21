using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    /* Static 필드 */
    public static string NextScene;

    /* Static 메서드 */
    public static void LoadScene(string sceneName)
    {
        // 전환할 씬 이름 기록
        NextScene = sceneName;

        // 로딩 씬 전환
        SceneManager.LoadScene("Loading");
    }

    /* 필드 */
    [SerializeField] Slider m_LoadingBar;

    /* MonoBehaviour */
    void Start()
    {
        StartCoroutine(LoadingSequence());
    }

    /* 메서드 */
    IEnumerator LoadingSequence()
    {
        yield return null;

        // 다음 씬 비동기 로딩 시작
        var op = SceneManager.LoadSceneAsync(NextScene);

        // 자동 씬 전환 방지
        op.allowSceneActivation = false;

        float timer = 0f;

        while (!op.isDone)
        {
            yield return null;

            timer += Time.deltaTime;

            // 로딩율 갱신
            if (op.progress < 0.9f)
            {
                // 로딩율 증가를 부드럽게 표현
                m_LoadingBar.value = Mathf.Lerp(m_LoadingBar.value, op.progress / 0.9f, timer);
            }
            else
            {
                // 로딩 완료
                m_LoadingBar.value = 1;

                // 로딩 완료 후 고정 대기 시간
                yield return new WaitForSeconds(2f);

                // 씬 전환
                op.allowSceneActivation = true;
            }
        }
    }
}
