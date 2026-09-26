// Decompiled with JetBrains decompiler
// Type: Vehicles.FloatMenuMulti
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class FloatMenuMulti : FloatMenu
{
  public const int RevalidateEveryFrame = 3;
  private Vector3 clickPos;
  private List<Pawn> selPawns;
  private Pawn clickedPawn;

  public FloatMenuMulti(
    List<FloatMenuOption> options,
    List<Pawn> selPawns,
    Pawn clickedPawn,
    string title,
    Vector3 clickPos)
    : base(options, title, false)
  {
    this.clickPos = clickPos;
    this.selPawns = selPawns;
    this.clickedPawn = clickedPawn;
  }

  public virtual void DoWindowContents(Rect rect)
  {
    if (this.selPawns == null || this.selPawns.Count < 1)
    {
      Find.WindowStack.TryRemove((Window) this, true);
    }
    else
    {
      if (Time.frameCount % 3 == 0)
      {
        for (int index = 0; index < this.options.Count; ++index)
        {
          if (!this.options[index].Disabled && !FloatMenuMulti.StillValid(this.options[index], this.selPawns, this.clickedPawn))
            this.options[index].Disabled = true;
        }
      }
      base.DoWindowContents(rect);
    }
  }

  private static bool StillValid(FloatMenuOption opt, List<Pawn> pawns, Pawn ship)
  {
    return ((Thing) ship).Spawned && !ship.Dead && !ship.Downed && !(ship as VehiclePawn).vehiclePather.Moving && pawns.All<Pawn>((Func<Pawn, bool>) (x => !x.Dead && !x.Downed && !x.InMentalState));
  }
}
