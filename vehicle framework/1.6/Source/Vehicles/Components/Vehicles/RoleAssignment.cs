// Decompiled with JetBrains decompiler
// Type: Vehicles.RoleAssignment
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace Vehicles;

public class RoleAssignment
{
  private readonly List<Pawn> drivers = new List<Pawn>();
  private readonly List<Pawn> turretOperators = new List<Pawn>();
  private readonly List<Pawn> passengers = new List<Pawn>();

  public bool IsEmpty
  {
    get => this.drivers.Count == 0 && this.turretOperators.Count == 0 && this.passengers.Count == 0;
  }

  public void Set(IEnumerable<Pawn> pawns)
  {
    this.drivers.Clear();
    this.turretOperators.Clear();
    this.passengers.Clear();
    foreach (Pawn pawn in pawns)
    {
      if (VehicleRoleHandler.CanOperateRole(pawn, HandlingType.Turret))
        this.turretOperators.Add(pawn);
      else if (VehicleRoleHandler.CanOperateRole(pawn, HandlingType.Movement))
        this.drivers.Add(pawn);
      else
        this.passengers.Add(pawn);
    }
    this.Resort();
  }

  public bool TryPull(VehicleRoleHandler handler, out Pawn pawn)
  {
    HandlingType handlingTypes = handler.role.HandlingTypes;
    pawn = (Pawn) null;
    if ((handlingTypes & HandlingType.Turret) != HandlingType.None)
    {
      if (pawn == null)
        pawn = TryPop(this.turretOperators);
    }
    else if ((handlingTypes & HandlingType.Movement) != HandlingType.None)
    {
      if (pawn == null)
        pawn = TryPop(this.drivers);
      if (pawn == null)
        pawn = TryPop(this.turretOperators);
    }
    else if (handlingTypes == HandlingType.None)
    {
      if (pawn == null)
        pawn = TryPop(this.turretOperators);
      if (pawn == null)
        pawn = TryPop(this.drivers);
      if (pawn == null)
        pawn = TryPop(this.passengers);
    }
    return pawn != null;

    static Pawn TryPop(List<Pawn> pawns)
    {
      return pawns.Count != 0 ? GenCollection.Pop<Pawn>(pawns) : (Pawn) null;
    }
  }

  private void Resort()
  {
    GenCollection.SortByDescending<Pawn, float>(this.drivers, (Func<Pawn, float>) (pawn => pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) * 2f + pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness)));
    GenCollection.SortByDescending<Pawn, int>(this.turretOperators, (Func<Pawn, int>) (pawn => pawn.skills?.GetSkill(SkillDefOf.Shooting)?.Aptitude ?? -1));
  }
}
