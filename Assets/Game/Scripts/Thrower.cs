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

    public Vector2 Aim(Vector2 start, Vector2 target, float height)
    {
        var g = Physics2D.gravity.y * -1;
        var apexY = Mathf.Max(start.y, target.y) + height;
        var vy = Mathf.Sqrt(2f * g * (apexY - start.y));

        var timeUp = vy / g;
        var timeDown = Mathf.Sqrt(2f * (apexY - target.y) / g);
        var totalTime = timeUp + timeDown;

        var vx = (target.x - start.x) / totalTime;

        return new Vector2(vx, vy);
    }
}
