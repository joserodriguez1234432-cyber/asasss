// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.Designator_RemoveVehicleSegment
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class Designator_RemoveVehicleSegment : Designator_Deconstruct
{
  protected virtual DesignationDef Designation => VMF_DefOf.VMF_RemoveSegment;

  public Designator_RemoveVehicleSegment()
  {
    ((Command) this).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_RemoveSegment"));
    ((Command) this).defaultDesc = TaggedString.op_Implicit(Translator.Translate("VMF_RemoveSegmentDesc"));
    ((Command) this).icon = (Texture) ContentFinder<Texture2D>.Get("UI/Designators/RemoveBridge", true);
    ((Designator) this).soundSucceeded = SoundDefOf.Designate_RemoveBridge;
    ((Command) this).hotKey = KeyBindingDefOf.Misc5;
  }

  public virtual AcceptanceReport CanDesignateThing(Thing t)
  {
    return AcceptanceReport.op_Implicit(ThingCompUtility.HasComp<CompMapExpander>(t) && ((Designator) this).Map.designationManager.DesignationOn(t, ((Designator) this).Designation) == null);
  }

  public virtual void DesignateThing(Thing t)
  {
    Thing.allowDestroyNonDestroyable = true;
    base.DesignateThing(t);
    Thing.allowDestroyNonDestroyable = false;
  }
}
