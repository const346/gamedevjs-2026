using System.Collections;
using UnityEngine;

public class Thrower : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _bulletPrefab;
    [SerializeField] private Transform _gunAnchor;
    [SerializeField] private AudioClip _throwSound;
    [Space]
    [SerializeField] private bool _faceThrowDirection = true;
    [SerializeField] private float _returnDelay = 2;
    [SerializeField] private Vector2 _bulletOffset;
    [SerializeField] private float _bulletOffsetRotation;

    private float _defaultAngle;
    private float _lastThrowTime;
    private bool _isThrowing;

    private void Start()
    {
        _defaultAngle = _gunAnchor.localRotation.eulerAngles.z;
    }

    public void Throw(Vector2 force)
    {
        var bullet = Instantiate(_bulletPrefab, _gunAnchor.transform.position, Quaternion.identity);
       
        if (_faceThrowDirection)
        {
            var angle = Vector2.Angle(force, Vector2.right);
            bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle );
            bullet.transform.position = _gunAnchor.TransformPoint(_bulletOffset);

            var localForce = transform.localScale * force;
            var visualAngle = Vector2.Angle(localForce, Vector2.right);
            _gunAnchor.transform.localRotation = Quaternion.Euler(0f, 0f, visualAngle - _bulletOffsetRotation);
        }

        bullet.AddForce(force, ForceMode2D.Impulse);

        _lastThrowTime = Time.time;
        _isThrowing = false;

        AudioSource.PlayClipAtPoint(_throwSound, _gunAnchor.position);
    }

    public void Throw(float target, float velocity, float height)
    {
        var gravity = -Physics2D.gravity.y;
        var start = transform.position.x;
        var force = MathTool.Aim(start, target, velocity, gravity, height);

        Throw(force);
    }

    public IEnumerator Throw(Enemy enemy, float height, float duration, float delayAfter)
    {
        _isThrowing = true;

        var gravity = -Physics2D.gravity.y;
        var start = transform.position.x;
        var force = Vector2.zero;

        for (var t = 0f; t < 1f; t += Time.deltaTime / duration)
        {
            if (enemy == null)
            {
                _isThrowing = false;
                yield break;
            }

            var startAngle = _gunAnchor.localRotation.eulerAngles.z;

            var target = enemy.transform.position.x;
            var velocity = enemy.Velocity;
            force = MathTool.Aim(start, target, velocity, gravity, height);
            Debug.DrawRay(_gunAnchor.position, force.normalized * 2f, Color.red);

            var localForce =  transform.localScale * force;
            var visualAngle = Vector2.Angle(localForce, Vector2.right);
            var angle = Mathf.LerpAngle(startAngle, visualAngle, t);

            _gunAnchor.localRotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        Throw(force);



        yield return new WaitForSeconds(delayAfter);

    }

    private void Update()
    {
        if (!_isThrowing && Time.time - _lastThrowTime > _returnDelay)
        {
            var currentAngle = _gunAnchor.localRotation.eulerAngles.z;
            var angle = Mathf.LerpAngle(currentAngle, _defaultAngle, Time.deltaTime);

            _gunAnchor.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnDrawGizmos()
    {
        if (_gunAnchor != null)
        {
            var p1 = _gunAnchor.transform.position;
            var p2 = _gunAnchor.TransformPoint(_bulletOffset);

            Gizmos.DrawLine(p1, p2);

            var rOffset = Quaternion.Euler(0f, 0f, _bulletOffsetRotation);
            var p3 = _gunAnchor.TransformDirection(rOffset * Vector2.right);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(p2, p3);
        }
    }
}
