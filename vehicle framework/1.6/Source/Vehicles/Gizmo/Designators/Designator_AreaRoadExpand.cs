// Decompiled with JetBrains decompiler
// Type: Vehicles.Designator_AreaRoadExpand
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Designator_AreaRoadExpand : Designator_AreaRoad
{
  public Designator_AreaRoadExpand()
    : base((DesignateMode) 0)
  {
    ((Command) this).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VF_RoadZoneExpand"));
    ((Command) this).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VF_RoadZoneExpandDesc"));
    ((Command) this).icon = (Texture) ContentFinder<Texture2D>.Get("UI/Designators/RoadAreaOn", true);
    ((Designator) this).soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
    ((Designator) this).soundDragChanged = SoundDefOf.Designate_DragZone_Changed;
    ((Designator) this).soundSucceeded = SoundDefOf.Designate_ZoneAdd_AllowedArea;
  }
}
