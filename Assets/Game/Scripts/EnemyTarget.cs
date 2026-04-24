using UnityEngine;

public class EnemyTarget : MonoBehaviour
{
    [SerializeField] private int _health = 20; 
    [SerializeField] private BoxCollider2D[] _damageZones;
    [SerializeField] private float _damageArea = 2f;

    public float DamageArea => _damageArea;
    public bool IsLive => _health > 0;

    public void OnDamage()
    {
        _health = Mathf.Max(0, _health - 1);
    }

    public bool IsInsideDamageZone(Vector2 position)
    {
        foreach (var zone in _damageZones)
        {
            if (zone.OverlapPoint(position))
            {
                return true;
            }
        }

        return false;
    }

    public Vector2 GetRandDamagePosition()
    {
        var index = Random.Range(0, _damageZones.Length);
        var zone = _damageZones[index];
        var local = new Vector2(
            Random.Range(-0.5f, 0.5f) * zone.size.x,
            Random.Range(-0.5f, 0.5f) * zone.size.y
        );

        local += zone.offset;
        return zone.transform.TransformPoint(local);
    }
}