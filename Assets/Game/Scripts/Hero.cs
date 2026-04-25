using System.Collections;
using UnityEngine;

public class Hero : MonoBehaviour
{
    [SerializeField] private Wallet _wallet;
    [SerializeField] private Animator _animator;
    [SerializeField] private Thrower _thrower;
    [SerializeField] private Transform _target;
    [SerializeField] private float _speed = 5f;
    [Space]
    [SerializeField] private float _attackHeight = 1.3f;
    [Space]
    [SerializeField] private float _lookDistance = 12;
    [SerializeField] private float _retreatDistance = 6;
    [SerializeField] private float _waypointDistance = 4;

    [SerializeField] private AudioClip _coinCollectSound;
    [SerializeField] private AudioClip _tapSound;

    private bool _breakAction;
    private float _lastMoveTime;

    public void MoveTo(Vector3 position)
    {
        _breakAction = true;
        _target.position = position;

        AudioSource.PlayClipAtPoint(_tapSound, position);
    }

    private void Start()
    {
        StartCoroutine(UpdateAgent());
    }

    private IEnumerator UpdateAgent()
    {
        while (true)
        {
            if (_breakAction)
            {
                _breakAction = false;

                var target = _target.transform.position.x;
                yield return MoveAction(target);
            }
            else if (TryDetectEnemy(out var foundEnemy))
            {
                var target = foundEnemy.transform.position.x;
                var m = target - transform.position.x;
                var s = Mathf.Sign(m);
                var d = Mathf.Abs(m);

                if (d < _retreatDistance &&
                    Time.time - _lastMoveTime > Random.Range(0.2f, 0.5f))
                {
                    var back = Mathf.Sign(m) * 4;
                    target = transform.position.x - back;
                    yield return MoveAction(target);
                }
                else
                {
                    Debug.DrawLine(transform.position, foundEnemy.transform.position, Color.red, 2);

                    UpdateDirection(target);
                    yield return AttackAction(foundEnemy);
                }
            }
            else if (TryDetectCoin(out var foundCoin) &&
                _lastMoveTime > Random.Range(0.25f, 1f))
            {
                var target = foundCoin.transform.position.x;
                yield return MoveAction(target);
            }
            else if (Time.time - _lastMoveTime > Random.Range(7f, 15f))
            {
                var range = Random.Range(-_waypointDistance, _waypointDistance);
                var target = _target.transform.position.x + range;

                yield return MoveAction(target);
            }
            else if (Mathf.Abs(_target.position.x - transform.position.x) > _waypointDistance * 1.5f &&
                Time.time - _lastMoveTime > 1)
            {
                yield return MoveAction(_target.position.x);
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

            if (TryDetectEnemy(out var foundEnemy, 
                _retreatDistance * -Mathf.Sign(m)))
            {
                Debug.DrawLine(transform.position, foundEnemy.transform.position, Color.blue, 2);

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

    private IEnumerator AttackAction(Enemy enemy)
    {
        yield return new WaitForSeconds(1f);

        if (enemy != null)
        {
            var height = Random.Range(_attackHeight * 0.5f, _attackHeight * 2f);
            _thrower.Throw(enemy.transform.position.x, enemy.Velocity, height);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private bool TryDetectEnemy(out Enemy foundEnemy, float look = 0)
    {
        var lookArea = new Vector2(_lookDistance * 2, 0.5f);
        var lookOffset = Vector3.zero;

        if (look != 0)
        {
            lookArea.x = Mathf.Abs(look);
            lookOffset.x = look / 2 * -1;
        }

        var hits = Physics2D.OverlapBoxAll(transform.position + lookOffset, lookArea, 0);

        System.Array.Sort(hits, (a, b) =>
        {
            var dA = Vector2.Distance(transform.position, a.transform.position);
            var dB = Vector2.Distance(transform.position, b.transform.position);

            return dA.CompareTo(dB);
        });

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out foundEnemy))
            {
                return true;
            }
        }

        foundEnemy = null;
        return false;
    }

    private bool TryDetectCoin(out Coin foundCoin)
    {
        var lookArea = new Vector2(_lookDistance * 2, 0.5f);
        var hits = Physics2D.OverlapBoxAll(transform.position, lookArea, 0);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out foundCoin))
            {
                return true;
            }
        }

        foundCoin = null;
        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<Coin>(out var _))
        {
            AudioSource.PlayClipAtPoint(_coinCollectSound, transform.position);

            _wallet.Add(1);
            Destroy(collision.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        var h = Vector3.right * _lookDistance;
        var v = Vector3.up * 0.25f;

        var p1 = transform.position + h + v;
        var p2 = transform.position - h + v;
        var p3 = transform.position - h - v;
        var p4 = transform.position + h - v;

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3); 
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }
}
