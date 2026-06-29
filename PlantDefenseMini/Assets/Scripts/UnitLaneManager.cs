using System.Collections.Generic;
using UnityEngine;

public class UnitLaneManager : BaseSingleton<UnitLaneManager>
{
    private readonly Dictionary<int, List<UnitController>> unitsByLane = new Dictionary<int, List<UnitController>>();
    private static readonly IReadOnlyList<UnitController> EmptyUnits = new List<UnitController>();

    public void Register(UnitController unit, int laneIndex)
    {
        if (unit == null || laneIndex < 0)
        {
            return;
        }

        if (!unitsByLane.TryGetValue(laneIndex, out List<UnitController> units))
        {
            units = new List<UnitController>();
            unitsByLane.Add(laneIndex, units);
        }

        if (!units.Contains(unit))
        {
            units.Add(unit);
        }
    }

    public void Unregister(UnitController unit)
    {
        if (unit == null)
        {
            return;
        }

        foreach (List<UnitController> units in unitsByLane.Values)
        {
            units.Remove(unit);
        }
    }

    public IReadOnlyList<UnitController> GetUnitsInLane(int laneIndex)
    {
        if (unitsByLane.TryGetValue(laneIndex, out List<UnitController> units))
        {
            units.RemoveAll(unit => unit == null || unit.IsDead);
            return units;
        }

        return EmptyUnits;
    }
}
