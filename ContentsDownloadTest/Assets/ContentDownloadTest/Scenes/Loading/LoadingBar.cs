public class LoadingBar : SmoothSlider
{
    /* MonoBehaviour */
    protected override void Awake()
    {
        base.Awake();

        GetSlider().value = 0;
    }
}
