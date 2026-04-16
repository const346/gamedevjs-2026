using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggable
{
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

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDragging)
        {
            var worldPosition = _camera.ScreenToWorldPoint(eventData.position);
            _dragObject.Drag(worldPosition + _dragOffset);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var worldPosition = _camera.ScreenToWorldPoint(eventData.position);
        var hit = Physics2D.OverlapPoint(worldPosition);

        if (hit != null && 
            hit.TryGetComponent<IDraggable>(out var draggable) && 
            draggable.IsDraggable)
        {
            _isDragging = true;
            _dragOffset = hit.transform.position - worldPosition;
            _dragObject = draggable;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
        _dragObject = null;
    }
}