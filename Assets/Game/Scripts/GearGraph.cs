using System.Collections.Generic;
using UnityEngine;

public class GearGraph
{
    private List<Gear> _gears = new();
    private List<(Gear A, Gear B)> _joints = new();

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
}