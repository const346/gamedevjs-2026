using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _target;
    [SerializeField] private float _speed = 5f;

    [SerializeField] private Vector2 _visionArea = new Vector2(16, 0.2f);

    [SerializeField] private float _aimHeight = 1.3f;
    [SerializeField] private float _aimForce = 0.9f;

    [SerializeField] private float _retreatDistance = 6;
    [SerializeField] private float _waypointRadius = 4f;

    [Space]
    [SerializeField] private float _coins = 0f;

    private bool _breakAction;
    private float _lastMoveTime;

    public void MoveTo(Vector3 position)
    {
        _breakAction = true;
        _target.position = position;
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
            else if (TryLookAroundEnemy(out var foundEnemy))
            {
                Debug.DrawLine(transform.position, foundEnemy.transform.position, Color.red, 2);

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
                    yield return AttackAction(target);
                }
            }
            else if (TryLookAroundCoin(out var foundCoin) &&
                _lastMoveTime > Random.Range(0.25f, 1f))
            {
                var target = foundCoin.transform.position.x;
                yield return MoveAction(target);
            }
            else if (Time.time - _lastMoveTime > Random.Range(7f, 15f))
            {
                var range = Random.Range(-_waypointRadius, _waypointRadius);
                var target = _target.transform.position.x + range;

                yield return MoveAction(target);
            }
            else if (Mathf.Abs(_target.position.x - transform.position.x) > _waypointRadius * 1.5f &&
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

    private IEnumerator AttackAction(float target)
    {
        UpdateDirection(target);

        yield return new WaitForSeconds(0.6f);

        var thrower = GetComponent<Thrower>();
        var force = thrower.Aim(transform.position, new Vector2(target, 0), _aimHeight);
        thrower.Throw(force * _aimForce);

        yield return new WaitForSeconds(0.6f);
    }

    private bool TryLookAroundEnemy(out Enemy foundEnemy)
    {
        var hits = Physics2D.OverlapBoxAll(transform.position, _visionArea, 0);

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

    private bool TryLookAroundCoin(out Coin foundCoin)
    {
        var hits = Physics2D.OverlapBoxAll(transform.position, _visionArea, 0);

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
            _coins++;
            Destroy(collision.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        var h = Vector3.right * _visionArea.x / 2;
        var v = Vector3.up * _visionArea.y / 2;

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
