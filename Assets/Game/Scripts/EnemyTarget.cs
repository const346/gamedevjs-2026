using UnityEngine;

public class EnemyTarget : MonoBehaviour
{
    [SerializeField] private int _health = 20;
    [SerializeField] private float _damageArea = 2f;

    public float DamageArea => _damageArea;
    public bool IsLive => _health > 0;

    public void OnDamage()
    {
        _health = Mathf.Max(0, _health - 1);
    }
}