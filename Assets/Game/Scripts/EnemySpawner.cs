using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private float _spawnInterval = 5f;
    [SerializeField] private float _spawnRadius = 2f;

    private float _spawnTimer;

    private void Update()
    {
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= _spawnInterval)
        {
            _spawnTimer -= _spawnInterval;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        var randomOffset = Random.Range(-_spawnRadius, _spawnRadius);
        var spawnPosition = (Vector2)transform.position + Vector2.right * randomOffset;
        Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
    }
}
