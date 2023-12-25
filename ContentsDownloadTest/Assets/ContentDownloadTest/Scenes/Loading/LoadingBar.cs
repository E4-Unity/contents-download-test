public class LoadingBar : SmoothSlider
{
    /* MonoBehaviour */
    protected override void Awake()
    {
        base.Awake();

        InitValue(0);

        SceneLoadingManagerBase.OnProgressUpdated += Refresh;
    }

    void OnDestroy()
    {
        SceneLoadingManagerBase.OnProgressUpdated -= Refresh;
    }
}
