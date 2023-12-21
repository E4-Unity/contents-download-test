using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] AssetReferenceGameObject m_Player;

    void Start()
    {
        // Player 스폰
        var player = Addressables.InstantiateAsync(m_Player).WaitForCompletion();
    }
}
