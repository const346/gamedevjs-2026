using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float _speed = 0.5f;
    [SerializeField] private GameObject _rewardPrefab;
    [SerializeField] private Vector2 _rewardOffset = Vector2.up;

    private bool _breakAction;
    private bool _isRetreat;
    private float _lastMoveTime;
    
    public void OnDamage()
    {
        SpawnReward();
        Destroy(gameObject);
    }

    private void SpawnReward()
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
    }

    public void Retreat()
    {
        _isRetreat = true;
    }

    private void Start()
    {
        StartCoroutine(UpdateAgent());
    }

    private IEnumerator UpdateAgent()
    {
        var motor = FindAnyObjectByType<Motor>();

        var damageArea = 2f;
        var damagePosition = motor.transform.position;
        var homePosition = new Vector3(-50, 0, 0);

        while (true)
        {
            if (!_isRetreat)
            {
                var target = damagePosition.x;
                target += UnityEngine.Random.Range(-damageArea, damageArea);

                yield return MoveAction(target);

                yield return new WaitForSeconds(2);
            }
            else
            {
                yield return MoveAction(homePosition.x);
            }

            yield return null;
        }
    }

    private IEnumerator MoveAction(float target)
    {
        while (true)
        {
            var m = target - transform.position.x;
            var d = Mathf.Abs(m);
            var v = Mathf.Sign(m) * _speed;

            if (d < 0.01f || _breakAction)
            {
                yield break;
            }

            UpdateDirection(target);

            var s = v * Time.deltaTime;
            s = Mathf.Clamp(s, -d, d);

            transform.position += Vector3.right * s;
            _lastMoveTime = Time.time;

            yield return null;
        }
    }

    private IEnumerator AttackAction(float target)
    {
        UpdateDirection(target);

        yield return new WaitForSeconds(0.6f);

        yield return new WaitForSeconds(0.6f);
    }

    private void UpdateDirection(float target)
    {
        if (target != 0 && _animator != null)
        {
            var direction = Mathf.Sign(target - transform.position.x);
            var scale = transform.localScale;
            scale.x = direction * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}
