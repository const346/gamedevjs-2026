using System.Collections;
using UnityEngine;

public class Thrower : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _bulletPrefab;
    [SerializeField] private Transform _gunAnchor;
    [SerializeField] private AudioClip _throwSound;
    [Space]
    [SerializeField] private bool _faceThrowDirection = true;

    private float _defaultAngle;
    private float _lastThrowTime;
    private bool _isThrowing;

    private void Start()
    {
        _defaultAngle = _gunAnchor.localRotation.eulerAngles.z;
    }

    public void Throw(Vector2 force)
    {
        var bullet = Instantiate(_bulletPrefab, _gunAnchor.position, Quaternion.identity);
        bullet.AddForce(force, ForceMode2D.Impulse);

        if (_faceThrowDirection)
        {
            var angle = Vector2.Angle(force, Vector2.right);
            bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            var localForce = transform.localScale * force;
            var visualAngle = Vector2.Angle(localForce, Vector2.right);
            _gunAnchor.transform.localRotation = Quaternion.Euler(0f, 0f, visualAngle);
        }

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
        if (!_isThrowing && Time.time - _lastThrowTime > 2f)
        {
            var currentAngle = _gunAnchor.localRotation.eulerAngles.z;
            var angle = Mathf.LerpAngle(currentAngle, _defaultAngle, Time.deltaTime);

            _gunAnchor.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
