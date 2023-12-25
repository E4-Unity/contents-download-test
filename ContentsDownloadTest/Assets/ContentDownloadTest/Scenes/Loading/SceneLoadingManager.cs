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
