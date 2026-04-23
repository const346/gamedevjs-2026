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
    private float _speedScaler = 1;
    private EnemyTarget _target;

    public float Velocity { get; private set; }

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
                var rand = Random.Range(-2f, 2f);
                body.AddForce(Vector2.up * 5 + Vector2.right * rand, ForceMode2D.Impulse);
            }

            Destroy(obj, 15f); // ...
        }
    }

    public void Retreat()
    {
        _isRetreat = true;
    }

    public void AttackTo(EnemyTarget target)
    {
        _target = target;
    }

    private void Start()
    {
        StartCoroutine(UpdateAgent());
    }

    private IEnumerator UpdateAgent()
    {
        var homePosition = transform.position;

        while (true)
        {
            if (!_isRetreat)
            {
                var targetPosition = _target.transform.position.x;
                targetPosition += Random.Range(-_target.DamageArea, _target.DamageArea);

                yield return MoveAction(targetPosition);
                yield return AttackAction(_target);
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
        _speedScaler = Random.Range(0.8f, 1.2f);

        while (true)
        {
            var m = target - transform.position.x;
            var d = Mathf.Abs(m);
            var v = Mathf.Sign(m) * _speed * _speedScaler;

            if (d < 0.01f || _breakAction)
            {
                yield break;
            }

            UpdateDirection(target);

            var s = v * Time.deltaTime;
            s = Mathf.Clamp(s, -d, d);

            transform.position += Vector3.right * s;

            Velocity = v;

            yield return null;
        }
    }

    private IEnumerator AttackAction(EnemyTarget target)
    {
        UpdateDirection(target.transform.position.x);

        yield return new WaitForSeconds(0.6f);

        target.OnDamage();

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
