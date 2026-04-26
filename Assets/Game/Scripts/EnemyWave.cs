using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnemyWave : MonoBehaviour, IGameTask
{
    [SerializeField] private float _spawnMinInterval = 1f;
    [SerializeField] private float _spawnMaxInterval = 4f;
    [SerializeField] private Vector3 _spawnPosition = new Vector3(-51, 0, 0);
    [Space]
    [SerializeField] private float _startDelay = 5f;
    [SerializeField] private float _attackDuration = 30f;
    [SerializeField] private float _retreatDuration = 30f;
    [SerializeField] private float _endDelay = 5f;
    [Space]
    [SerializeField] private EnemyContainer[] _enemies;
    [Space]
    [SerializeField] private Transform _lightContainer;
    [SerializeField] private Color _attackColor = Color.red;
    [Space]
    [SerializeField] private AudioClip _attackSound;
    [SerializeField] private AudioClip _endSound;
    [SerializeField] private AudioClip _attackLoopSound;
    [SerializeField] private AudioClip _baseLoopSound;
    [Space]
    [SerializeField] private AudioSource _musicSource;

    [System.Serializable]
    public class EnemyContainer
    {
        public Enemy EnemyPrefab;
        public int Count;
    }

    private Light2D[] _lights;
    private Color _defaultColor;

    private void Awake()
    {
        _lights = _lightContainer.GetComponentsInChildren<Light2D>();
        _defaultColor = _lights.First().color;
    }

    public IEnumerator Running(EnemyTarget enemyTarget)
    {
        Debug.Log($"ENEMY WAVE | {name} | START");

        yield return new WaitForSeconds(_startDelay);
        _musicSource.Stop();
        yield return SetLightColor(_defaultColor, _attackColor, 1);
        AudioSource.PlayClipAtPoint(_attackSound, _spawnPosition);
        yield return new WaitForSeconds(3);

        _musicSource.clip = _attackLoopSound;
        _musicSource.Play();
        
        Debug.Log($"ENEMY WAVE | {name} | SPAWNING");

        var enemyPrefabs = _enemies
            .SelectMany(x => Enumerable.Range(0, x.Count).Select(y => x.EnemyPrefab))
            .OrderBy(x => Random.value)
            .ToArray();

        var enemies = new List<Enemy>(enemyPrefabs.Length);

        foreach (var enemyPrefab in enemyPrefabs)
        {
            var enemy = Instantiate(enemyPrefab, _spawnPosition, Quaternion.identity);
            enemy.AttackTo(enemyTarget);

            enemies.Add(enemy);

            var delay = Random.Range(_spawnMinInterval, _spawnMaxInterval);
            yield return new WaitForSeconds(delay);

            if (!enemyTarget.IsLive)
            {
                yield break;
            }
        }

        Debug.Log($"ENEMY WAVE | {name} | ATTACK");

        var breakTime = Time.time + _attackDuration;
        yield return new WaitUntil(() => enemies.Count(x => x != null) <= 0 || Time.time >= breakTime || !enemyTarget.IsLive);

        _musicSource.Stop();

        yield return SetLightColor(_attackColor, _defaultColor, 1);
        AudioSource.PlayClipAtPoint(_endSound, _spawnPosition);

        yield return new WaitForSeconds(3);

        _musicSource.clip = _baseLoopSound;
        _musicSource.Play();

        if (enemies.Any(x => x != null))
        {
            Debug.Log($"ENEMY WAVE | {name} | RETREAT");

            foreach (var enemy in enemies)
            {
                enemy.Retreat();
            }

            if (!enemyTarget.IsLive)
            {
                yield break;
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

    public IEnumerator SetLightColor(Color a, Color b, float duration = 1f)
    {
        for (var t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            foreach (var light in _lights)
            {
                light.color = Color.Lerp(a, b, t);
            }

            yield return null;
        }
    }
}