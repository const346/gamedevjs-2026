using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class Gear : MonoBehaviour, IDraggable
{
    [SerializeField] [Range(4, 22)] private int _numberOfTeeth = 4;
    [SerializeField] [Range(0f, 1f)] private float _brokenTeethRatio;
    [SerializeField] private float _quality = 0.99f;
    [SerializeField] private float _guarantee = 360f;
    [SerializeField] private float _toothWidth = 0.6f;
    [SerializeField] private float _toothHeight = 0.5f;
    [SerializeField] private int _seek;
    [SerializeField] private bool _isDraggable = true;
    [SerializeField] private LayerMask _placementMask;
    [SerializeField] private bool IgnoreJoints;

    [Space]
    public UnityEvent<float> OnSimulate;

    [Space]
    [SerializeField] private Rigidbody2D _body;
    [SerializeField] private CircleCollider2D _collider;
    [SerializeField] private SortingGroup _sortingGroup;
    [SerializeField] private Transform _baseContainer;
    [SerializeField] private Transform _toothContainer;
    [Space]
    [SerializeField] private Sprite[] _toothSprites;

    public bool IsDraggable
    {
        get => _isDraggable;
        set => _isDraggable = value;
    }

    public int DragPriority => _sortingGroup.sortingOrder;
    public int NumberOfTeeth => _numberOfTeeth;
    public float OuterRadius => _collider.radius;
    public float InnerRadius => OuterRadius - _toothHeight;
    public Vector2 Position => transform.position;
    public float Rotation => transform.rotation.eulerAngles.z;

    private static readonly GearGraph _gearGraph = new();

    private long _sortFrame;
    private long _simulateFrame;
    private float _accumulated;

    private void Awake()
    {
        Rebuild();
    }

    private void OnEnable()
    {
        _gearGraph.Register(this);
    }

    private void OnDisable()
    {
        _gearGraph.Unregister(this);
    }

    public void Simulate(Gear parent, float torgue, bool isAxial = false)
    {
        //var direction = Vector2.up;
        //var isEdgeContact = parent != null && !IsAxialContact(parent);

        //if (isEdgeContact)
        //{
        //    direction = parent.transform.position - transform.position;
        //    torgue = torgue * -1 * parent._numberOfTeeth / _numberOfTeeth;
        //}

        //if (_simulateFrame == Time.frameCount)
        //{
        //    BreakTooth(direction);
        //    return;
        //}
        //else if (_accumulated >= _guarantee && isEdgeContact)
        //{
        //    var breakChance = (1 - _quality) * Mathf.Abs(torgue) / (Mathf.PI * 2);
        //    if (UnityEngine.Random.value < breakChance)
        //    {
        //        BreakTooth(direction);
        //    }
        //}

        //
        //_accumulated += Mathf.Abs(torgue);

        if (_simulateFrame == Time.frameCount)
        {
            //BreakTooth(direction);
            return;
        }

        _simulateFrame = Time.frameCount;

        if (parent != null && !isAxial)
        {
            torgue = torgue * -1 * parent.NumberOfTeeth / NumberOfTeeth;
        }

        transform.rotation *= Quaternion.Euler(0, 0, torgue);
        OnSimulate?.Invoke(torgue);

        foreach (var joints in _gearGraph.GetJoints(this))
        {
            if (joints != parent)
            {
                joints.Simulate(this, torgue);
            }
        }

        if (!isAxial)
        {
            foreach (var gear in _gearGraph.Get(Position))
            {
                if (gear != this)
                {
                    gear.Simulate(this, torgue, true);
                }
            }
        }
    }

    private void Sort(Gear parent, int sortOffset, bool isAxial = false)
    {
        if (_sortFrame == Time.frameCount)
        {
            return;
        }

        _sortFrame = Time.frameCount;

        _sortingGroup.sortingOrder = sortOffset;

        foreach (var joints in _gearGraph.GetJoints(this))
        {
            if (joints != parent)
            {
                joints.Sort(this, sortOffset);
            }
        }

        if (!isAxial)
        {
            foreach (var gear in _gearGraph.Get(Position))
            {
                if (gear != this)
                {
                    gear.Sort(this, sortOffset + (NumberOfTeeth - gear.NumberOfTeeth), true);
                }
            }
        }
    }

    //private Transform GetTooth(Vector2 direction)
    //{
    //    var localDirection = Quaternion.Inverse(transform.rotation) * direction.normalized;
    //    var alpha = Mathf.Atan2(localDirection.y, localDirection.x) * Mathf.Rad2Deg;
    //    alpha = (alpha + 360 - 90) % 360;

    //    var toothAngle = 360f / _numberOfTeeth;
    //    var toothIndex = Mathf.RoundToInt(alpha / toothAngle) % _numberOfTeeth;
    //    return _toothContainer.GetChild(toothIndex);
    //}

    //private void BreakTooth(Vector2 direction)
    //{
    //    var tooth = GetTooth(direction);
    //    if (tooth.gameObject.activeSelf)
    //    {
    //        tooth.gameObject.SetActive(false);

    //        // effect broken tooth
    //        var cloneTooth = Instantiate(tooth, tooth.position, tooth.rotation, null);
    //        cloneTooth.gameObject.SetActive(true);
    //        Destroy(cloneTooth.gameObject, 5f);

    //        var body = cloneTooth.gameObject.AddComponent<Rigidbody2D>();
    //        var forceDirection = new Vector2(direction.y, -direction.x).normalized;
    //        var forcePosition = cloneTooth.position + 
    //            (Vector3) UnityEngine.Random.insideUnitCircle * 0.2f;
    //        var force = forceDirection * UnityEngine.Random.Range(8, 12);

    //        body.AddForceAtPosition(force, forcePosition, ForceMode2D.Impulse);
    //    }
    //}

    public void Drag(Vector2 position)
    {
        var resultPosition = position;
        var resultRotation = Rotation;
        var angleCorrection = 0f;
        var sortOffset = 0;

        _gearGraph.ClearJoints(this);

        var isAxialFound = false;
        foreach (var gearAxialGroup in FindNearGears(resultPosition).GroupBy(x => x.Position))
        {
            var minGear = gearAxialGroup.Where(x => x != this)
                .DefaultIfEmpty()
                .Aggregate((m, n) => n.NumberOfTeeth < m.NumberOfTeeth ? n : m);

            var m = gearAxialGroup.Key - resultPosition;
            var d = m.magnitude;
            var r = InnerRadius * 0.75f + minGear.InnerRadius * 0.5f; // TODO: ???

            Debug.DrawRay(minGear.Position, m, Color.green, 0.05f);
            Debug.DrawRay(minGear.Position, m.normalized * r, Color.red, 0.05f);

            if (d < r && _gearGraph.Get(minGear.Position)
                .All(g => g == this || g.NumberOfTeeth != _numberOfTeeth))
            {
                resultPosition = minGear.Position;
                resultRotation = minGear.Rotation;

                sortOffset = minGear._sortingGroup.sortingOrder + (minGear.NumberOfTeeth - NumberOfTeeth);

                isAxialFound = true;
                break;
            }
        }

        if (!isAxialFound && TryFindNearGear(resultPosition, out var firstGear, g => g.IgnoreJoints))
        {
            _gearGraph.CreateJoint(this, firstGear);

            sortOffset = firstGear._sortingGroup.sortingOrder;

            var direction = (firstGear.Position - resultPosition).normalized;
            var distance = _collider.radius + firstGear._collider.radius -
                Mathf.Max(_toothHeight, firstGear._toothHeight);

            resultPosition = firstGear.Position - direction * distance;

            angleCorrection = MathTool.GetGearAngleCorrection
                (firstGear.NumberOfTeeth, NumberOfTeeth, firstGear.Rotation, Rotation, direction);

            if (TryFindNearGear(resultPosition, out var secondGear, g => g.Position == firstGear.Position || g.IgnoreJoints))
            {
                var cA = secondGear.transform.position;
                var rA = secondGear._collider.radius;
                var cB = firstGear.transform.position;
                var rB = firstGear._collider.radius;
                var rC = _collider.radius - Mathf.Max(_toothHeight, secondGear._toothHeight);

                if (MathTool.SolveCircle(cA, rA, cB, rB, rC, out var rp1, out var rp2))
                {
                    var d1 = (resultPosition - rp1).sqrMagnitude;
                    var d2 = (resultPosition - rp2).sqrMagnitude;

                    resultPosition = d1 < d2 ? rp1 : rp2;

                    direction = (firstGear.Position - resultPosition).normalized;
                    angleCorrection = MathTool.GetGearAngleCorrection
                        (firstGear.NumberOfTeeth, NumberOfTeeth, firstGear.Rotation, Rotation, direction);

                    direction = (secondGear.Position - resultPosition).normalized;
                    var forceAngleCorrection = MathTool.GetGearAngleCorrection
                        (secondGear.NumberOfTeeth, NumberOfTeeth, secondGear.Rotation, resultRotation + angleCorrection, direction);

                    secondGear.Simulate(this, -forceAngleCorrection);

                    _gearGraph.CreateJoint(this, secondGear);
                }
            }
        }

        //foreach (var nearGear in FindNearGears(resultPosition))
        //{
        //    var d = Vector2.Distance(nearGear.Position, resultPosition);
        //    var h = Mathf.Max(_toothHeight, nearGear._toothHeight);
        //    var q = (nearGear.OuterRadius + OuterRadius) - h;

        //    if (Mathf.Abs(d - q) < h * 0.2f)
        //    {
        //        _gearGraph.CreateJoint(this, nearGear);
        //    }

        //    // check second and others contact (invalid position or looping)
        //    // ...

        //    // нельзя ловить контакт с N шестернями на одной втулке
        //}

        // apply result
        transform.position = resultPosition;
        transform.rotation = Quaternion.Euler(0, 0, resultRotation + angleCorrection);

        // ...
        Sort(this, sortOffset);
    }

    public void DragStart()
    {
        _body.simulated = false;
        _body.bodyType = RigidbodyType2D.Static;
    }

    public void DragEnd()
    {
        if ((_placementMask.value == 0 ||
            Physics2D.OverlapPoint(transform.position, _placementMask) == null) &&
            _gearGraph.Get(Position).Count() < 2)
        {
            foreach (var gear in _gearGraph.All())
            {
                if (gear._body.bodyType == RigidbodyType2D.Dynamic)
                {
                    var offset = _sortingGroup.sortingOrder < gear._sortingGroup.sortingOrder ? 1 : -1;
                    gear._sortingGroup.sortingOrder += offset;
                }
            }

            _body.bodyType = RigidbodyType2D.Dynamic;
            _gearGraph.ClearJoints(this);
        }

        _body.simulated = true;
    }

    private IEnumerable<Gear> FindNearGears(Vector2 position)
    {
        var hits = Physics2D.OverlapCircleAll(position, _collider.radius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject != gameObject &&
                hits[i].isTrigger &&
                hits[i].attachedRigidbody != null &&
                hits[i].attachedRigidbody.bodyType != RigidbodyType2D.Dynamic &&
                hits[i].TryGetComponent<Gear>(out var gear))
            {
                yield return gear;
            }
        }
    }

    private bool TryFindNearGear(Vector2 position, out Gear result, Func<Gear, bool> skip = null)
    {
        var hits = Physics2D.OverlapCircleAll(position, _collider.radius);

        Array.Sort(hits, (a, b) =>
        {
            if (a is CircleCollider2D cA && b is CircleCollider2D cB)
            {
                var qA = (Vector2)a.transform.position - position;
                var qB = (Vector2)b.transform.position - position;

                var dA = Mathf.Abs(qA.magnitude - (cA.radius + InnerRadius));
                var dB = Mathf.Abs(qB.magnitude - (cB.radius + InnerRadius));

                return dA.CompareTo(dB);
            }

            return 0;
        });

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].gameObject != gameObject &&
                hits[i].isTrigger &&
                hits[i].attachedRigidbody != null &&
                hits[i].attachedRigidbody.bodyType != RigidbodyType2D.Dynamic &&
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

    public void Rebuild(int seek, int numberOfTeeth, float brokenTeethRatio)
    {
        _seek = seek;
        _numberOfTeeth = numberOfTeeth;
        _brokenTeethRatio = brokenTeethRatio;

        Rebuild();
    }

    private void Rebuild()
    {
        if (_collider != null)
        {
            _collider.radius = (_toothWidth * _numberOfTeeth) / Mathf.PI + _toothHeight;
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

        if (_toothContainer != null && _collider != null)
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
            }
        }

        if (_toothSprites != null && _toothSprites.Length > 0)
        {
            var rng = new System.Random(_seek + _numberOfTeeth);
            for (var i = 0; i < _toothContainer.childCount; i++)
            {
                var tooth = _toothContainer.GetChild(i);
                if (tooth.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                {
                    var index = rng.Next(0, _toothSprites.Length);
                    spriteRenderer.sprite = _toothSprites[index];
                }
            }
        }

        if (_brokenTeethRatio > 0)
        {
            var brokenTeethCount = (int)Mathf.Floor(_brokenTeethRatio * _numberOfTeeth);
            if (brokenTeethCount > 0)
            {
                var rng = new System.Random(_seek + _numberOfTeeth + brokenTeethCount);
                var m = Enumerable.Range(0, _numberOfTeeth)
                    .Select((i, v) => i < brokenTeethCount)
                    .OrderBy(x => rng.Next())
                    .ToArray();

                for (var i = 0; i < _numberOfTeeth; i++)
                {
                    var tooth = _toothContainer.GetChild(i);
                    tooth.gameObject.SetActive(!m[i]);
                }
            }
        }

        if (IgnoreJoints)
        {
            for (var i = 0; i < _numberOfTeeth; i++)
            {
                var tooth = _toothContainer.GetChild(i);
                tooth.gameObject.SetActive(false);
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null)
            {
                Rebuild();
            }
        };
    }

    private void OnDrawGizmos()
    {
        if (_collider == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        foreach (var gear in _gearGraph.GetJoints(this))
        {
            if (gear != null)
            {
                Gizmos.DrawLine(transform.position, gear.transform.position);
            }
        }

        Vector3? first = null;
        Vector3? prev = null;
        for (int i = 0; i < _numberOfTeeth; i++)
        {
            var angle = (360f / _numberOfTeeth) * i;
            var direction = transform.rotation * Quaternion.Euler(0, 0, angle) * Vector3.up;

            var p1 = transform.position + direction * (_collider.radius - _toothHeight);
            var p2 = transform.position + direction * _collider.radius;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(p1, p2);

            if (prev != null)
            {
                Gizmos.color = Color.whiteSmoke;
                Gizmos.DrawLine(prev.Value, p1);
            }
            else
            {
                first = p1;
            }

            prev = p1;
        }

        if (first != null && prev != null)
        {
            Gizmos.color = Color.whiteSmoke;
            Gizmos.DrawLine(first.Value, prev.Value);
        }
    }
#endif
}
