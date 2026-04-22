using UnityEngine;

public class Thrower : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _spawnAnchor;
    [SerializeField] private float _destroyDelay = 10f;

    public void Throw(Vector2 force)
    {
        var bullet = Instantiate(_bulletPrefab, _spawnAnchor.position, Quaternion.identity);
        Destroy(bullet, _destroyDelay);

        if (bullet.TryGetComponent<Rigidbody2D>(out var body))
        {
            body.AddForce(force, ForceMode2D.Impulse);
        }

        var angle = Vector2.Angle(force, Vector2.right);
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void Throw(float target, float velocity, float height)
    {
        var force = Aim(transform.position.x, target, velocity, height);
        Throw(force);
    }

    public static Vector2 Aim(float start, float target, float velocity, float height)
    {
        var g = -Physics2D.gravity.y;
        var vy = Mathf.Sqrt(2f * g * height);
        var time = (vy / g) * 2f;

        for (int i = 0; i < 3; i++)
        {
            var futureX = target + velocity * time;
            var vx = (futureX - start) / time;

            var newTime = (futureX - start) / vx;
            time = newTime;
        }

        var finalFutureX = target + velocity * time;
        var finalVx = (finalFutureX - start) / time;

        return new Vector2(finalVx, vy);
    }
}
