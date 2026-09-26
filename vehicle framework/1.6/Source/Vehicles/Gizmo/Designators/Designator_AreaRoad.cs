// Decompiled with JetBrains decompiler
// Type: Vehicles.Designator_AreaRoad
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[UsedImplicitly]
public abstract class Designator_AreaRoad : Designator_Cells
{
  private readonly DesignateMode mode;
  private static Designator_AreaRoad.RoadType roadType = Designator_AreaRoad.RoadType.Prioritize;

  protected Designator_AreaRoad(DesignateMode mode)
  {
    this.mode = mode;
    ((Designator) this).useMouseIcon = true;
  }

  public virtual bool DragDrawMeasurements => true;

  public virtual DrawStyleCategoryDef DrawStyleCategory => DrawStyleCategoryDefOf.Areas;

  public virtual void ProcessInput(Event ev)
  {
    if (!((Designator) this).CheckCanInteract())
      return;
    if (this.mode == null)
      Find.WindowStack.Add((Window) new FloatMenu(new List<FloatMenuOption>(2)
      {
        RoadTypeOption(TaggedString.op_Implicit(Translator.Translate("VF_RoadType_Prioritize")), Designator_AreaRoad.RoadType.Prioritize),
        RoadTypeOption(TaggedString.op_Implicit(Translator.Translate("VF_RoadType_Avoid")), Designator_AreaRoad.RoadType.Avoid)
      }));
    else
      ((Designator) this).ProcessInput(ev);

    FloatMenuOption RoadTypeOption(string label, Designator_AreaRoad.RoadType roadType)
    {
      return new FloatMenuOption(label, (Action) (() =>
      {
        Designator_AreaRoad.roadType = roadType;
        // ISSUE: reference to a compiler-generated method
        this.\u003C\u003En__0(ev);
      }), (MenuOptionPriority) 3, (Action<Rect>) null, (Thing) null, 0.0f, (Func<Rect, bool>) null, (WorldObject) null, true, 0);
    }
  }

  public virtual void DesignateSingleCell(IntVec3 cell)
  {
    if (this.mode == null)
    {
      switch (Designator_AreaRoad.roadType)
      {
        case Designator_AreaRoad.RoadType.Prioritize:
          ((Designator) this).Map.areaManager.Get<Area_Road>()[cell] = true;
          ((Designator) this).Map.areaManager.Get<Area_RoadAvoidal>()[cell] = false;
          break;
        case Designator_AreaRoad.RoadType.Avoid:
          ((Designator) this).Map.areaManager.Get<Area_Road>()[cell] = false;
          ((Designator) this).Map.areaManager.Get<Area_RoadAvoidal>()[cell] = true;
          break;
      }
    }
    else
    {
      ((Designator) this).Map.areaManager.Get<Area_Road>()[cell] = false;
      ((Designator) this).Map.areaManager.Get<Area_RoadAvoidal>()[cell] = false;
    }
  }

  public virtual AcceptanceReport CanDesignateCell(IntVec3 cell)
  {
    if (!GenGrid.InBounds(cell, ((Designator) this).Map))
      return AcceptanceReport.op_Implicit(false);
    bool flag1 = ((Designator) this).Map.areaManager.Get<Area_Road>()[cell];
    bool flag2 = ((Designator) this).Map.areaManager.Get<Area_RoadAvoidal>()[cell];
    if (this.mode != null)
      return AcceptanceReport.op_Implicit(flag1 | flag2);
    bool flag3;
    switch (Designator_AreaRoad.roadType)
    {
      case Designator_AreaRoad.RoadType.Prioritize:
        flag3 = !flag1;
        break;
      case Designator_AreaRoad.RoadType.Avoid:
        flag3 = !flag2;
        break;
      default:
        flag3 = true;
        break;
    }
    return AcceptanceReport.op_Implicit(flag3);
  }

  public virtual void SelectedUpdate()
  {
    GenUI.RenderMouseoverBracket();
    ((Designator) this).Map.areaManager.Get<Area_Road>().MarkForDraw();
    ((Designator) this).Map.areaManager.Get<Area_RoadAvoidal>().MarkForDraw();
  }

  public enum RoadType : byte
  {
    None,
    Prioritize,
    Avoid,
  }
}
