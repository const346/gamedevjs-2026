using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggable
{
    int DragPriority { get; }
    bool IsDraggable { get; }
    void Drag(Vector2 position);
}

public class DragController : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IDragHandler
{
    [SerializeField] private Camera _camera;

    private bool _isDragging;
    private Vector3 _dragOffset;
    private IDraggable _dragObject;

    private void DragCamera(Vector2 screenPosition, Vector2 delta)
    {
        var prevScreenPosition = screenPosition - delta;
        var z = Mathf.Abs(_camera.transform.position.z);

        var prevWorld = _camera.ScreenToWorldPoint(new Vector3(prevScreenPosition.x, prevScreenPosition.y, z));
        var currentWorld = _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, z));
        var move = prevWorld - currentWorld;

        move.y = 0; // Lock vertical movement

        _camera.transform.position += move;

        // Clamp camera position to bounds
        var bottomLeft = _camera.ViewportToWorldPoint(new Vector3(0, 0, z));
        var topRight = _camera.ViewportToWorldPoint(new Vector3(1, 1, z));

        var camHalfWidth = (topRight.x - bottomLeft.x) * 0.5f;
        var camHalfHeight = (topRight.y - bottomLeft.y) * 0.5f;

        var minBounds = new Vector2(-50, -100);
        var maxBounds = new Vector2(50, 100); 

        var pos = _camera.transform.position;

        pos.x = Mathf.Clamp(pos.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
        pos.y = Mathf.Clamp(pos.y, minBounds.y + camHalfHeight, maxBounds.y - camHalfHeight);

        _camera.transform.position = pos;
    }

    public void OnDrag(PointerEventData data)
    {
        if (_isDragging)
        {
            var worldPosition = ScreenToWorld(data.position);
            _dragObject.Drag(worldPosition + _dragOffset);
        }
        else
        {
            DragCamera(data.position, data.delta);
        }
    }

    public void OnPointerDown(PointerEventData data)
    {
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

                    _isDragging = true;
                    _dragOffset = hit.transform.position - worldPosition;
                    _dragObject = draggable;
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData data)
    {
        _isDragging = false;
        _dragObject = null;
    }

    private Vector3 ScreenToWorld(Vector2 position)
    {
        var cameraZ = Mathf.Abs(_camera.transform.position.z);
        var screenPosition = new Vector3(position.x, position.y, cameraZ);
        var worldPosition = _camera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }
}