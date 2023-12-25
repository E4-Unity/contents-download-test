# Contents Download Test

## 개요

컨텐츠 다운로드 기능 구현 테스트 프로젝트

## 구현 기능

1. 컨텐츠 다운로드
    - `Addressables` 패키지 사용
    - GitHub Pages 를 Asset Host Server 로 활용
2. 씬 전환 및 로딩
    - `SceneLoadingManagerBase`
    - `SceneLoadingManagerBase<TEnum>`
    - 로딩 씬이 여러 종류일 경우에도 사용 가능
3. SmoothSlider
    - 슬라이더 값을 부드럽게 갱신
    - `UpdateValue` 메서드를 오버라이드하여 다른 UI 값 갱신 역시 가능


## 시연 영상

![Animation](https://github.com/E4-Unity/contents-download-test/assets/59055049/e6792679-94b3-4207-9fa0-b3679778a641)

## 사용 예시

### SceneLoadingManagerBase

1. 프로젝트에 빌드된 씬 정보와 매핑된 `enum` 을 작성한 다음, 해당 `enum` 을 기반으로 `SceneLoadingManager` 클래스를 작성합니다.

    ```csharp
    // SceneLoadingManager.cs

    public enum BuildScene
    {
        Title,
        Loading,
        Lobby
    }

    public abstract class SceneLoadingManager : SceneLoadingManagerBase<BuildScene>
    {
        /* API */
        public static void LoadScene(BuildScene nextScene) => LoadScene(nextScene, BuildScene.Loading);
    }
    ```

2. 자동으로 다음 씬을 로딩하고 싶은 경우 아래 스크립트를 `로딩 씬`의 오브젝트에 부착합니다.

    ```csharp
    // LoadingManager.cs

    using UnityEngine;

    public class LoadingManager : MonoBehaviour
    {
        void Start()
        {
            SceneLoadingManagerBase.LoadNextScene();
        }
    }
    ```