using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyWave : MonoBehaviour, IGameTask
{
    [SerializeField] private float _spawnMinInterval = 1f;
    [SerializeField] private float _spawnMaxInterval = 4f;

    [Space]
    [SerializeField] private float _prepareDelay = 5f;
    [SerializeField] private float _attackDuration = 30f;
    [SerializeField] private float _retreatDuration = 30f;

    [Space]
    [SerializeField] private EnemyContainer[] _enemies;

    [System.Serializable]
    public class EnemyContainer
    {
        public Enemy EnemyPrefab;
        public int Count;
    }

    public IEnumerator Running()
    {
        yield return new WaitForSeconds(_prepareDelay);

        var enemyPrefabs = _enemies
            .SelectMany(x => Enumerable.Range(0, x.Count).Select(y => x.EnemyPrefab))
            .OrderBy(x => Random.value)
            .ToArray();

        var enemies = new List<Enemy>(enemyPrefabs.Length);

        foreach (var enemyPrefab in enemyPrefabs)
        {
            var enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
            enemies.Add(enemy);

            var delay = Random.Range(_spawnMinInterval, _spawnMaxInterval);
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(_attackDuration);

        foreach (var enemy in enemies)
        {
            enemy.Retreat();
        }

        yield return new WaitForSeconds(_retreatDuration);
    }
}