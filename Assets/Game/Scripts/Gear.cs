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
    [SerializeField] private float _quality = 1.0f;
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

    private void Awake()
    {
        Initialize();
    }
    private void OnValidate()
    {
        Initialize();
    }

    public void Simulate(Gear parent, float step)
    {
        if (_simulateFrame == Time.frameCount) /// TODO: broke tooth
        {
            return;
        }

        _simulateFrame = Time.frameCount;

        var k = IsAxialContact(parent) ? step :
            step * -1 * parent._numberOfTeeth / _numberOfTeeth;

        transform.rotation *= Quaternion.Euler(0, 0, k);

        OnSimulate?.Invoke(k);

        foreach (var contact in _contacts)
        {
            if (contact != parent)
            {
                contact.Simulate(this, k);
            }
        }
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

    public void Drag(Vector2 position)
    {
        // clear contacts and find new ones
        foreach (var contact in _contacts)
        {
            contact.RemoveContact(this);
        }

        _contacts.Clear();

        // find new contacts
        var hits = Physics2D.OverlapCircleAll(position, _collider.radius);
        Array.Sort(hits, (a, b) =>
        {
            var d1 = Vector2.Distance(position, a.transform.position);
            var d2 = Vector2.Distance(position, b.transform.position);
            return d1.CompareTo(d2);
        });

        var result = (Vector3)position;
        var pairGear = default(Gear);

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (hit.gameObject != gameObject &&
                hit.TryGetComponent<Gear>(out var gear))
            {
                var p2 = (Vector2)gear.transform.position;
                var m = p2 - position;
                var d = m.magnitude;

                if (pairGear != null)
                {
                    var g1 = gear;
                    var g2 = pairGear;

                    var cA = g1.transform.position;
                    var rA = g1._collider.radius;
                    var cB = g2.transform.position;
                    var rB = g2._collider.radius;

                    var r = _collider.radius - Mathf.Max(_toothHeight, gear._toothHeight);

                    if (SolveCircleV3(cA, rA, cB, rB, r, out var rp1, out var rp2))
                    {
                        AddContact(gear);
                        gear.AddContact(this);

                        var d1 = Vector2.Distance(result, rp1);
                        var d2 = Vector2.Distance(result, rp2);

                        result = d1 < d2 ? rp1 : rp2;
                    }

                    break;
                }
                else if (d < gear._collider.radius / 2f)
                {
                    AddContact(gear);
                    gear.AddContact(this);

                    result = gear.transform.position + Vector3.back;
                    break;
                }
                else
                {
                    pairGear = gear;

                    AddContact(gear);
                    gear.AddContact(this);

                    var distance = _collider.radius + gear._collider.radius -
                        Mathf.Max(_toothHeight, gear._toothHeight);

                    var direction = m.normalized;
                    result = p2 - direction * distance;

                    //// sync rotation
                    //var alpha = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    //var thetaA = gear.transform.eulerAngles.z;
                    //var teethA = gear._numberOfTeeth;
                    //var teethB = _numberOfTeeth;

                    //var thetaB = alpha + 90f - (180f / teethB) + (alpha - thetaA - 90f) * (teethA / teethB);
                    //transform.rotation = Quaternion.Euler(0, 0, thetaB);
                }
            }
        }

        // move to position
        transform.position = result;
    }

    private void OnDrawGizmos()
    {
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
            currentBase.gameObject.SetActive(true);

            if (currentBase.TryGetComponent<SpriteRenderer>(out var spriteRenderer) && 
                spriteRenderer.drawMode == SpriteDrawMode.Sliced)
            {
                spriteRenderer.size = Vector3.one * 2 * (_toothWidth * _numberOfTeeth) / Mathf.PI;
            }
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
