using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Arrow : MonoBehaviour
{
    [SerializeField] private Vector2 _tail = Vector2.left;
    [SerializeField] [Range(0, 1f)] private float _tailFriction = 1f;

    private Rigidbody2D _body;
    private Quaternion _lastRotation;
    private Vector3 _lastPosition;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        var tailPosition = transform.TransformPoint(_tail);
        var tailVelocity = _body.GetPointVelocity(tailPosition);

        var projection = Vector2.Dot(tailVelocity, transform.up) * transform.up;
        var tailForce = projection * -_tailFriction;

        _body.AddForceAtPosition(tailForce, tailPosition);
        Debug.DrawRay(tailPosition, tailForce, Color.red);

        _lastRotation = transform.rotation;
        _lastPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.OnDamage();
            Destroy(gameObject);
        }
        else
        {
            transform.rotation = _lastRotation;
            transform.position = _lastPosition;

            _body.Sleep();
            _body.freezeRotation = true;
            _body.simulated = false;
        }
    }
}
