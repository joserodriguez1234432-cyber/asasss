// Decompiled with JetBrains decompiler
// Type: Vehicles.Upgrade
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using System.Collections.Generic;

#nullable disable
namespace Vehicles;

[PublicAPI]
public abstract class Upgrade
{
  protected UpgradeNode node;

  public abstract bool UnlockOnLoad { get; }

  public virtual bool HasGraphics => false;

  public virtual IEnumerable<string> ConfigErrors
  {
    get
    {
      yield break;
    }
  }

  public virtual IEnumerable<UpgradeTextEntry> UpgradeDescription(VehiclePawn vehicle)
  {
    yield break;
  }

  public abstract void Unlock(VehiclePawn vehicle, bool unlockingPostLoad);

  public abstract void Refund(VehiclePawn vehicle);

  public virtual void PostLoad()
  {
  }

  public virtual void Init(UpgradeNode node) => this.node = node;
}
