using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggable
{
    int DragPriority { get; }
    bool IsDraggable { get; }
    void Drag(Vector2 position);
    void DragStart();
    void DragEnd();
}

public class DragController : MonoBehaviour, /// TODO: rename InputController
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler,
    IPointerClickHandler
{
    [SerializeField] private Camera _camera;
    [SerializeField] private CameraController2D _cameraController;
    [SerializeField] private Hero _player;

    private bool _ignoreClick;
    private bool _isDragging;
    private Vector3 _dragOffset;
    private IDraggable _dragObject;
    private Vector2 _lastPosition;
    private Vector2 _lastDelta;

    public void OnPointerClick(PointerEventData data)
    {
        if (_ignoreClick)
        {
            return; 
        }

        if (Vector2.Distance(data.pressPosition, data.position) > 10f)
        {
            return;
        }

        var worldPosition = ScreenToWorld(data.position);
        worldPosition.y = 0;
        worldPosition.z = 0;

        _player.MoveTo(worldPosition);
    }

    public void OnDrag(PointerEventData data)
    {
        _lastPosition = data.position;
        _lastDelta = data.delta;

        if (!_isDragging)
        {
            _cameraController.Drag(_lastPosition, _lastDelta);
        }
    }

    public void OnPointerDown(PointerEventData data)
    {
        _lastPosition = data.position;
        _lastDelta = data.delta;

        var worldPosition = ScreenToWorld(data.position);
        var hits = Physics2D.OverlapPointAll(worldPosition);
        if (hits != null && hits.Length > 0)
        {
            var priority = int.MinValue;
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IDraggable>(out var draggable) && 
                    draggable.IsDraggable && 
                    draggable.DragPriority > priority)
                {
                    priority = draggable.DragPriority;

                    _dragOffset = hit.transform.position - worldPosition;
                    _dragObject = draggable;
                    _isDragging = true;
                }
            }

            if (_dragObject != null)
            {
                _dragObject.DragStart();
            }
        }

        _ignoreClick = Mathf.Abs(_cameraController.Velocity.x) > 10f;

        _cameraController.IsDragging = true;
        _cameraController.ClearInertia();
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (_dragObject != null)
        {
            _dragObject.DragEnd();
            _ignoreClick = true;
        }

        _isDragging = false;
        _dragObject = null;

        _cameraController.IsDragging = false;
    }

    private Vector3 ScreenToWorld(Vector2 position)
    {
        var cameraZ = Mathf.Abs(_camera.transform.position.z);
        var screenPosition = new Vector3(position.x, position.y, cameraZ);
        var worldPosition = _camera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

    private void Update()
    {
        if (_isDragging)
        {
            var screenSize = new Vector2(Screen.width, Screen.height);
            var viewportPosition = _lastPosition / screenSize;

            if (viewportPosition.x < 0.1f)
            {
                var delta = Mathf.InverseLerp(0.1f, 0f, viewportPosition.x) * -1;
                _cameraController.Move(delta);
            }
            else if (viewportPosition.x > 0.9f)
            {
                var delta = Mathf.InverseLerp(0.9f, 1f, viewportPosition.x) * 1;
                _cameraController.Move(delta);
            }

            var worldPosition = ScreenToWorld(_lastPosition);
            _dragObject.Drag(worldPosition + _dragOffset);
        }
    }
}