using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyWave : MonoBehaviour, IGameTask
{
    [SerializeField] private float _spawnMinInterval = 1f;
    [SerializeField] private float _spawnMaxInterval = 4f;

    [Space]
    [SerializeField] private float _startDelay = 5f;
    [SerializeField] private float _attackDuration = 30f;
    [SerializeField] private float _retreatDuration = 30f;
    [SerializeField] private float _endDelay = 5f;

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
        Debug.Log($"ENEMY WAVE | {name} | START");

        yield return new WaitForSeconds(_startDelay);

        Debug.Log($"ENEMY WAVE | {name} | SPAWNING");

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

        Debug.Log($"ENEMY WAVE | {name} | ATTACK");

        var breakTime = Time.time + _attackDuration;
        yield return new WaitUntil(() => enemies.Count(x => x != null) <= 0 || Time.time >= breakTime);

        if (enemies.Any(x => x != null))
        {
            Debug.Log($"ENEMY WAVE | {name} | RETREAT");

            foreach (var enemy in enemies)
            {
                enemy.Retreat();
            }

            breakTime = Time.time + _retreatDuration;
            yield return new WaitUntil(() => !enemies.Any(x => x != null) || Time.time >= breakTime);

            foreach (var enemy in enemies.Where(x => x != null))
            {
                Destroy(enemy.gameObject);
            }
        }

        Debug.Log($"ENEMY WAVE | {name} | END");
        yield return new WaitForSeconds(_endDelay);
    }
}