// Decompiled with JetBrains decompiler
// Type: Vehicles.UpgradeInProgress
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;

#nullable disable
namespace Vehicles;

public class UpgradeInProgress : IExposable
{
  private string nodeKey;
  [Unsaved(false)]
  public UpgradeNode node;
  private VehiclePawn vehicle;
  private float workLeft;
  private bool removal;

  public UpgradeInProgress()
  {
  }

  public UpgradeInProgress(VehiclePawn vehicle, UpgradeNode node, bool removal)
  {
    this.nodeKey = node.key;
    this.node = node;
    this.vehicle = vehicle;
    this.WorkLeft = node.work;
    this.removal = removal;
  }

  public bool Removal => this.removal;

  public float WorkLeft
  {
    get => this.workLeft;
    set
    {
      if ((double) this.workLeft == (double) value)
        return;
      this.workLeft = value;
    }
  }

  public void ExposeData()
  {
    Scribe_Values.Look<string>(ref this.nodeKey, "nodeKey", (string) null, false);
    Scribe_References.Look<VehiclePawn>(ref this.vehicle, "vehicle", false);
    Scribe_Values.Look<float>(ref this.workLeft, "workLeft", 0.0f, false);
    if (Scribe.mode != 4)
      return;
    this.node = ((ThingWithComps) this.vehicle).GetComp<CompUpgradeTree>().Props.def.GetNode(this.nodeKey);
  }
}
