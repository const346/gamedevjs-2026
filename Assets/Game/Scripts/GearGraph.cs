using System.Collections.Generic;
using UnityEngine;

public class GearGraph
{
    private readonly List<Gear> _gears = new();
    private readonly List<(Gear A, Gear B)> _joints = new();

    public void Register(Gear gear)
    {
        if (!_gears.Contains(gear))
        {
            _gears.Add(gear);
        }
    }

    public void Unregister(Gear gear)
    {
        ClearJoints(gear);
        _gears.Remove(gear);
    }

    public IEnumerable<Gear> Get(Vector2 position)
    {
        foreach (var gear in _gears)
        {
            if (gear.transform.position.x == position.x &&
                gear.transform.position.y == position.y)
            {
                yield return gear;
            }
        }
    }

    public IEnumerable<Gear> All()
    {
        foreach (var gear in _gears)
        {
            yield return gear;
        }
    }

    public IEnumerable<Gear> GetJoints(Gear gear)
    {
        foreach (var (a, b) in _joints)
        {
            if (gear == a)
            {
                yield return b;
            }
            else if (gear == b)
            {
                yield return a;
            }
        }
    }

    public void CreateJoint(Gear a, Gear b)
    {
        if (_joints.Contains((a, b)) ||
            _joints.Contains((b, a)))
        {
            return;
        }

        _joints.Add((a, b));
    }

    public void ClearJoints(Gear gear)
    {
        for (var i = _joints.Count - 1; i >= 0; i--)
        {
            if (gear == _joints[i].A ||
                gear == _joints[i].B)
            {
                _joints.RemoveAt(i);
            }
        }
    }

    private readonly HashSet<Gear> _visited = new();
    private readonly Queue<Gear> _queue = new();

    public bool HasPath(Gear a, Gear b)
    {
        //var visited = new HashSet<Gear>();
        //var queue = new Queue<Gear>();

        _visited.Clear();
        _queue.Clear();

        _queue.Enqueue(a);
        _visited.Add(a);

        while (_queue.Count > 0)
        {
            var node = _queue.Dequeue();
            if (node == b)
            {
                return true;
            }

            foreach (var next in GetJoints(node))
            {
                if (_visited.Add(next))
                {
                    _queue.Enqueue(next);
                }
            }

            foreach (var next in Get(node.Position))
            {
                if (_visited.Add(next))
                {
                    _queue.Enqueue(next);
                }
            }
        }

        return false;
    }
}