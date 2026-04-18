using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CircleCollider2D))]
public class Gear : MonoBehaviour, IDraggable
{
    [Range(4, 24)]
    [SerializeField] private int _numberOfTeeth = 4;
    [SerializeField] private float _quality = 0.99f;
    [SerializeField] private float _guarantee = 360f;
    [SerializeField] private bool _isDraggable = true;
    [SerializeField] private float _toothWidth = 0.6f;
    [SerializeField] private float _toothHeight = 0.5f;

    [Space]
    public UnityEvent<float> OnSimulate;

    [Space]
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Transform _baseScalable;
    [SerializeField] private Transform _baseContainer;
    [SerializeField] private Transform _toothContainer;
    [SerializeField] private Sprite[] _toothSprites;

    public bool IsDraggable => _isDraggable;

    private CircleCollider2D _collider;
    private List<Gear> _contacts = new List<Gear>();
    private long _simulateFrame;
    private float _accumulated;

    private void Awake()
    {
        Initialize();
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null)
            {
                Initialize();
            }
        };
#endif
    }

    public void Simulate(Gear parent, float torgue)
    {
        var direction = Vector2.up;
        var isEdgeContact = parent != null && !IsAxialContact(parent);

        if (isEdgeContact)
        {
            direction = parent.transform.position - transform.position;
            torgue = torgue * -1 * parent._numberOfTeeth / _numberOfTeeth;
        }

        if (_simulateFrame == Time.frameCount)
        {
            BreakTooth(direction);
            return;
        }
        else if (_accumulated >= _guarantee && isEdgeContact)
        {
            var breakChance = (1 - _quality) * Mathf.Abs(torgue) / (Mathf.PI * 2);
            if (UnityEngine.Random.value < breakChance)
            {
                BreakTooth(direction);
            }
        }

        _simulateFrame = Time.frameCount;
        _accumulated += Mathf.Abs(torgue);

        transform.rotation *= Quaternion.Euler(0, 0, torgue);
        OnSimulate?.Invoke(torgue);
        
        foreach (var contact in _contacts)
        {
            if (contact != parent)
            {
                contact.Simulate(this, torgue);
            }
        }


        //{ // debug only
        //    for (var i = 0; i < _toothContainer.childCount; i++)
        //    {
        //        var tooth = _toothContainer.GetChild(i);
        //        if (tooth.TryGetComponent<SpriteRenderer>(out var spriteRendererX))
        //        {
        //            spriteRendererX.color = Color.white;
        //        }
        //    }

        //    if (parent != null)
        //    {
        //        var toothX = GetTooth((Vector2)(parent.transform.position - transform.position));
        //        if (toothX.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        //        {
        //            spriteRenderer.color = Color.grey;
        //        }
        //    }
        //}
    }

    private bool IsAxialContact(Gear gear)
    {
        return _contacts.Contains(gear) && 
            gear.transform.position.x == transform.position.x && 
            gear.transform.position.y == transform.position.y;
    }

    public void AddContact(Gear gear)
    {
        if (!_contacts.Contains(gear))
        {
            _contacts.Add(gear);
        }
    }

    public void RemoveContact(Gear gear)
    {
        _contacts.Remove(gear);
    }

    public Transform GetTooth(Vector2 direction)
    {
        var localDirection = Quaternion.Inverse(transform.rotation) * direction.normalized;
        var alpha = Mathf.Atan2(localDirection.y, localDirection.x) * Mathf.Rad2Deg;
        alpha = (alpha + 360 - 90) % 360;
    
        var toothAngle = 360f / _numberOfTeeth;
        var toothIndex = Mathf.RoundToInt(alpha / toothAngle) % _numberOfTeeth;
        return _toothContainer.GetChild(toothIndex);
    }

    public void BreakTooth(Vector2 direction)
    {
        var tooth = GetTooth(direction);
        if (tooth.gameObject.activeSelf)
        {
            tooth.gameObject.SetActive(false);

            // effect broken tooth
            var cloneTooth = Instantiate(tooth, tooth.position, tooth.rotation, null);
            cloneTooth.gameObject.SetActive(true);
            Destroy(cloneTooth.gameObject, 5f);

            var body = cloneTooth.gameObject.AddComponent<Rigidbody2D>();
            var forceDirection = new Vector2(direction.y, -direction.x).normalized;
            var forcePosition = cloneTooth.position + 
                (Vector3) UnityEngine.Random.insideUnitCircle * 0.2f;
            var force = forceDirection * UnityEngine.Random.Range(8, 12);

            body.AddForceAtPosition(force, forcePosition, ForceMode2D.Impulse);
        }
    }

    public void Drag(Vector2 position)
    {
        // clear contacts and find new ones
        foreach (var contact in _contacts)
        {
            contact.RemoveContact(this);
        }

        _contacts.Clear();

        // find new contacts
        var result = (Vector3)position;
        var pairGear = default(Gear);

        if (TryFindNearGear(position, out var nearGear))
        {
            var p = (Vector2)nearGear.transform.position;
            var m = p - position;
            var d = m.magnitude;

            if (d < nearGear._collider.radius / 2f)
            {
                AddContact(nearGear);
                nearGear.AddContact(this);

                result = nearGear.transform.position + Vector3.back;
            }
            else
            {
                pairGear = nearGear;

                AddContact(nearGear);
                nearGear.AddContact(this);

                var distance = _collider.radius + nearGear._collider.radius -
                    Mathf.Max(_toothHeight, nearGear._toothHeight);

                result = p - m.normalized * distance;
            }
        }

        if (pairGear != null && TryFindNearGear(result, out var foundGear, g => g == pairGear))
        {
            var cA = foundGear.transform.position;
            var rA = foundGear._collider.radius;
            var cB = pairGear.transform.position;
            var rB = pairGear._collider.radius;
            var r = _collider.radius - Mathf.Max(_toothHeight, foundGear._toothHeight);

            if (SolveCircleV3(cA, rA, cB, rB, r, out var rp1, out var rp2))
            {
                AddContact(foundGear);
                foundGear.AddContact(this);

                var d1 = ((Vector2)result - rp1).sqrMagnitude;
                var d2 = ((Vector2)result - rp2).sqrMagnitude;

                result = d1 < d2 ? rp1 : rp2;
            }
        }

        // move to position
        transform.position = result;
    }

    private bool TryFindNearGear(Vector2 position, out Gear result, Func<Gear, bool> skip = null)
    {
        var hits = Physics2D.OverlapCircleAll(position, _collider.radius);

        Array.Sort(hits, (a, b) =>
        {
            if (a is CircleCollider2D cA && b is CircleCollider2D cB)
            {
                var dA = ((Vector2)a.transform.position - position).sqrMagnitude - cA.radius * cA.radius;
                var dB = ((Vector2)b.transform.position - position).sqrMagnitude - cB.radius * cB.radius;

                return dA.CompareTo(dB);
            }

            return 0;
        });

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject != gameObject &&
                hits[i].TryGetComponent<Gear>(out var gear))
            {
                if (skip != null && skip(gear))
                {
                    continue;
                }

                result = gear;
                return true;
            }
        }

        result = default;
        return false;
    }

    private void OnDrawGizmos()
    {
        if (_collider == null)
        {
            _collider = GetComponent<CircleCollider2D>();
        }

        Gizmos.color = Color.green;
        foreach (var gear in _contacts)
        {
            if (gear != null)
            {
                Gizmos.DrawLine(transform.position, gear.transform.position);
            }
        }
        Gizmos.color = Color.aliceBlue;
        Gizmos.DrawWireSphere(transform.position, _collider.radius - _toothHeight); 
        Gizmos.color = Color.antiqueWhite;
        Gizmos.DrawWireSphere(transform.position, _collider.radius);
    }

    private void Initialize()
    {
        _collider = GetComponent<CircleCollider2D>();
        _collider.radius = (_toothWidth * _numberOfTeeth) / Mathf.PI + _toothHeight;

        if (_baseScalable != null)
        {
            _baseScalable.localScale = Vector3.one * 2 * (_toothWidth * _numberOfTeeth) / Mathf.PI;
        }


        if (_baseContainer != null)
        {
            for (var i = 0; i < _baseContainer.childCount; i++)
            {
                _baseContainer.GetChild(i)
                    .gameObject.SetActive(false);
            }

            var currentBase = _baseContainer.GetChild(_numberOfTeeth - 1);

            if (currentBase.TryGetComponent<SpriteRenderer>(out var spriteRenderer) &&
                spriteRenderer.drawMode == SpriteDrawMode.Sliced)
            {
                spriteRenderer.size = Vector3.one * 2 * (_toothWidth * _numberOfTeeth) / Mathf.PI;
            }

            currentBase.gameObject.SetActive(true);
        }

        if (_toothContainer != null)
        {
            for (var i = 0; i < _toothContainer.childCount; i++)
            {
                var tooth = _toothContainer.GetChild(i);
                tooth.gameObject.SetActive(false);
            }

            for (var i = 0; i < _numberOfTeeth; i++)
            {
                var tooth = _toothContainer.GetChild(i);
                tooth.gameObject.SetActive(true);

                var angle = (360f / _numberOfTeeth) * i;

                var radius = _collider.radius - _toothHeight;
                var localPosition = Quaternion.Euler(0, 0, angle) * Vector3.up * radius;
                localPosition.z = tooth.localPosition.z;

                tooth.localPosition = localPosition;

                tooth.localRotation = Quaternion.Euler(0, 0, angle + 90);
                //tooth.localScale = new Vector3(_toothWidth, _toothHeight, 1);
            }
        }

        if (_toothSprites != null && _toothSprites.Length > 0)
        {
            for (var i = 0; i < _toothContainer.childCount; i++)
            {
                var tooth = _toothContainer.GetChild(i);
                if (tooth.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                {
                    var index = UnityEngine.Random.Range(0, _toothSprites.Length);
                    spriteRenderer.sprite = _toothSprites[index];
                }
            }
        }

        if (_text != null)
        {
            _text.text = _numberOfTeeth.ToString();
        }
    }

    private static bool SolveCircleV3(Vector2 cA, float rA, Vector2 cB, float rB, float rC, out Vector2 p1, out Vector2 p2)
    {
        var dA = rA + rC;
        var dB = rB + rC;

        var AB = cB - cA;
        var d = AB.magnitude;
        if (d <= 0.0001f)
        {
            p1 = p2 = Vector2.zero;
            return false;
        }

        var a = (dA * dA - dB * dB + d * d) / (2f * d);
        var hSq = dA * dA - a * a;

        if (hSq < 0)
        {
            p1 = p2 = Vector2.zero;
            return false;
        }

        var h = Mathf.Sqrt(hSq);
        var P = cA + a * AB / d;
        var perp = new Vector2(-AB.y, AB.x) / d;

        p1 = P + perp * h;
        p2 = P - perp * h;

        return true;
    }
}
