using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float _speed = 0.5f;
    [SerializeField] private GameObject _rewardPrefab;
    [SerializeField] private Vector2 _rewardOffset = Vector2.up;

    private void Update()
    {
        transform.position += Vector3.right * _speed * Time.deltaTime;
    }

    public void OnDamage()
    {
        if (_rewardPrefab != null)
        {
            var position = transform.position + (Vector3)_rewardOffset;
            var obj = Instantiate(_rewardPrefab, position, Quaternion.identity);

            if (obj.TryGetComponent<Rigidbody2D>(out var body))
            {
                var rand = UnityEngine.Random.Range(-2f, 2f);
                body.AddForce(Vector2.up * 5 + Vector2.right * rand, ForceMode2D.Impulse);
            }

            Destroy(obj, 15f); // ...
        }

        Destroy(gameObject);
    }
}
