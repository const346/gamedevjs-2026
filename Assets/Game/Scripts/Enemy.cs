using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private AudioClip _deathSound;
    [SerializeField] private float _speed = 0.5f; 
    [SerializeField] private float _deathDuration = 2f;
    [SerializeField] private GameObject _rewardPrefab;
    [SerializeField] private Vector2 _rewardOffset = Vector2.up;

    private bool _breakAction;
    private bool _isRetreat; 
    private bool _isLive = true;
    private float _speedScaler = 1;
    private EnemyTarget _target;

    public float Velocity { get; private set; }

    public void OnDamage()
    {
        if (_isLive)
        {
            _animator.SetTrigger("Death");

            Destroy(_collider);
            Destroy(gameObject, _deathDuration);

            SpawnReward();

            AudioSource.PlayClipAtPoint(_deathSound, transform.position);

            _isLive = false;
        }
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
                var attackPosition = _target.GetRandDamagePosition();
                yield return MoveAction(attackPosition.x);

                if (_target.IsInsideDamageZone(transform.position))
                {
                    yield return AttackAction(_target);
                }
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

            if (d < 0.01f || _breakAction || !_isLive)
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
        _animator.SetTrigger("Attack");
        UpdateDirection(target.transform.position.x);

        yield return new WaitForSeconds(0.5f);

        target.OnDamage();

        yield return new WaitForSeconds(1.5f);
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
