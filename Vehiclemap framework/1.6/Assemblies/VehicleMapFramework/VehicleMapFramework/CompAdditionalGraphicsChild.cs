// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompAdditionalGraphicsChild
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using RimWorld;
using System.Collections.Generic;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompAdditionalGraphicsChild : ThingComp
{
  [UsedImplicitly]
  public ThingWithComps parentThing;

  private CompProperties_DrawAdditionalGraphics Props
  {
    get => (CompProperties_DrawAdditionalGraphics) this.props;
  }

  public virtual List<GraphicData> Graphics => this.Props.graphics;

  public virtual void PostSpawnSetup(bool respawningAfterLoad)
  {
    if (respawningAfterLoad)
      return;
    this.parentThing = GridsUtility.GetFirstThingWithComp<CompDrawAdditionalGraphicsOpacity>(((Thing) this.parent).Position, ((Thing) this.parent).Map);
    this.parentThing?.GetComp<CompDrawAdditionalGraphicsOpacity>()?.children.Add(this.parent);
  }

  public virtual void PostDeSpawn(Map map, DestroyMode mode = 0)
  {
    this.parentThing?.GetComp<CompDrawAdditionalGraphicsOpacity>()?.children.Remove(this.parent);
  }

  public virtual void PostExposeData()
  {
    base.PostExposeData();
    Scribe_References.Look<ThingWithComps>(ref this.parentThing, "parentThing", false);
  }
}
