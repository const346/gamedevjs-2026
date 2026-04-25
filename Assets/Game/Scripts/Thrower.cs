using UnityEngine;

public class Thrower : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _bulletPrefab;
    [SerializeField] private Transform _spawnAnchor;
    [SerializeField] private AudioClip _throwSound;
    [Space]
    [SerializeField] private bool _faceThrowDirection = true;

    public void Throw(Vector2 force)
    {
        var bullet = Instantiate(_bulletPrefab, _spawnAnchor.position, Quaternion.identity);
        bullet.AddForce(force, ForceMode2D.Impulse);

        if (_faceThrowDirection)
        {
            var angle = Vector2.Angle(force, Vector2.right);
            bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        AudioSource.PlayClipAtPoint(_throwSound, _spawnAnchor.position);
    }

    public void Throw(float target, float velocity, float height)
    {
        var gravity = -Physics2D.gravity.y;
        var start = transform.position.x;
        var force = MathTool.Aim(start, target, velocity, gravity, height);

        Throw(force);
    }
}
