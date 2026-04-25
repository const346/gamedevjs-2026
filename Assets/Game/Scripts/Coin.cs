using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float _lifetime = 20f;

    private void Awake()
    {
        Destroy(gameObject, _lifetime);
    }
}
