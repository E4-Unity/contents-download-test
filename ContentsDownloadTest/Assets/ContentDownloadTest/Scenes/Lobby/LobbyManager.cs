using UnityEngine;
using UnityEngine.AddressableAssets;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] AssetReferenceGameObject m_Target;

    void Start()
    {
        // 어드레서블 에셋 스폰
        var target = Addressables.InstantiateAsync(m_Target);
    }
}
