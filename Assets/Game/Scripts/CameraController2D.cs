using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController2D : MonoBehaviour
{
    [SerializeField] private Vector2 _minBounds = new Vector2(-50, -100);
    [SerializeField] private Vector2 _maxBounds = new Vector2(50, 100);
    [SerializeField] private float _velocityScaler = 0.25f;


    private float _lastDragTime;
    private Vector3 _velocity; 
    private Camera _camera;

    public bool IsDragging { get; set; }
    public Vector3 Velocity => _velocity;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (IsDragging)
        {
            if (Time.time - _lastDragTime > 0.1f)
            {
                ClearInertia();
            }

            return;
        }

        if (_velocity.sqrMagnitude < 0.0001f)
        {
            _velocity = Vector3.zero;
            return;
        }

        _camera.transform.position += _velocity * Time.deltaTime;
        _velocity *= Mathf.Pow(0.135f, Time.deltaTime);

        ClampCamera();
    }

    public void Drag(Vector2 position, Vector2 delta)
    {
        var prevPosition = position - delta;
        var z = Mathf.Abs(_camera.transform.position.z);

        var prevWorld = _camera.ScreenToWorldPoint(new Vector3(prevPosition.x, prevPosition.y, z));
        var currentWorld = _camera.ScreenToWorldPoint(new Vector3(position.x, position.y, z));
        var move = prevWorld - currentWorld;
        move.y = 0; // Lock vertical movement

        _camera.transform.position += move;

        _velocity = move / Time.deltaTime * _velocityScaler;
        _velocity.y = 0;
        _velocity.z = 0;

        _lastDragTime = Time.time;

        ClampCamera();
    }

    public void ClearInertia()
    {
        _velocity = Vector3.zero;
    }

    private void ClampCamera()
    {
        var z = Mathf.Abs(_camera.transform.position.z);

        var bottomLeft = _camera.ViewportToWorldPoint(new Vector3(0, 0, z));
        var topRight = _camera.ViewportToWorldPoint(new Vector3(1, 1, z));

        var camHalfWidth = (topRight.x - bottomLeft.x) * 0.5f;
        var camHalfHeight = (topRight.y - bottomLeft.y) * 0.5f;

        var pos = _camera.transform.position;

        pos.x = Mathf.Clamp(pos.x, _minBounds.x + camHalfWidth, _maxBounds.x - camHalfWidth);
        pos.y = Mathf.Clamp(pos.y, _minBounds.y + camHalfHeight, _maxBounds.y - camHalfHeight);

        _camera.transform.position = pos;
    }
}
