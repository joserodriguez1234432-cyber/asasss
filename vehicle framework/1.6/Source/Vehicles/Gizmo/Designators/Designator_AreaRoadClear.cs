// Decompiled with JetBrains decompiler
// Type: Vehicles.Designator_AreaRoadClear
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

public class Designator_AreaRoadClear : Designator_AreaRoad
{
  public Designator_AreaRoadClear()
    : base((DesignateMode) 1)
  {
    ((Command) this).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VF_RoadZoneClear"));
    ((Command) this).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VF_RoadZoneClearDesc"));
    ((Command) this).icon = (Texture) ContentFinder<Texture2D>.Get("UI/Designators/RoadAreaOff", true);
    ((Designator) this).soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
    ((Designator) this).soundDragChanged = (SoundDef) null;
    ((Designator) this).soundSucceeded = SoundDefOf.Designate_ZoneDelete;
  }
}
