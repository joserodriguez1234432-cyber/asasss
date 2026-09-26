// Decompiled with JetBrains decompiler
// Type: Vehicles.GridOwnerList`1
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Threading;
using Verse;

#nullable disable
namespace Vehicles;

[UsedImplicitly]
public abstract class GridOwnerList<T> where T : IPathConfig
{
  protected int[] piggyToOwner;
  protected T[] configs;
  protected object gridOwnerLock = new object();

  public event GridOwnerList<T>.OwnershipTransferred OnOwnershipTransfer;

  public bool AnyOwners { get; private set; }

  public VehicleDef[] AllOwners { get; private set; }

  public IEnumerable<VehicleDef> AllPiggies
  {
    get
    {
      foreach (VehicleDef vehicleDef in DefDatabase<VehicleDef>.AllDefsListForReading)
      {
        if (!this.IsOwner(vehicleDef))
          yield return vehicleDef;
      }
    }
  }

  internal virtual void Init()
  {
    if (this.piggyToOwner == null)
      this.piggyToOwner = new int[DefDatabase<VehicleDef>.DefCount];
    this.piggyToOwner.Populate<int>(-1);
    List<VehicleDef> owners = new List<VehicleDef>();
    this.GenerateConfigs();
    this.SeparateIntoGroups(owners);
    this.AllOwners = owners.ToArray();
    this.AnyOwners = owners.Count > 0;
  }

  protected abstract void GenerateConfigs();

  protected abstract bool CanTransferOwnershipTo(VehicleDef vehicleDef);

  protected void SeparateIntoGroups(List<VehicleDef> owners, bool compress = true)
  {
    foreach (VehicleDef vehicleDef in DefDatabase<VehicleDef>.AllDefsListForReading)
    {
      int ownerId;
      if (this.TryGetOwner(owners, vehicleDef, out ownerId) & compress)
      {
        this.piggyToOwner[vehicleDef.DefIndex] = ownerId;
      }
      else
      {
        this.piggyToOwner[vehicleDef.DefIndex] = vehicleDef.DefIndex;
        owners.Add(vehicleDef);
      }
    }
  }

  protected bool TryGetOwner(List<VehicleDef> owners, VehicleDef vehicleDef, out int ownerId)
  {
    T config1 = this.configs[vehicleDef.DefIndex];
    foreach (VehicleDef owner in owners)
    {
      ownerId = owner.DefIndex;
      if (config1.UsesRegions == this.configs[ownerId].UsesRegions)
      {
        ref T local = ref config1;
        T obj = default (T);
        if ((object) obj == null)
        {
          obj = local;
          local = ref obj;
        }
        // ISSUE: variable of a boxed type
        __Boxed<T> config2 = (object) this.configs[ownerId];
        if (local.MatchesReachability((IPathConfig) config2))
          return true;
      }
    }
    ownerId = -1;
    return false;
  }

  public bool TryForfeitOwnership(VehicleDef ownerDef)
  {
    foreach (VehicleDef piggy in this.GetPiggies(ownerDef))
    {
      if (this.CanTransferOwnershipTo(piggy))
      {
        this.TransferOwnership(piggy);
        return true;
      }
    }
    return false;
  }

  public void TransferOwnership(VehicleDef vehicleDef)
  {
    VehicleDef owner = this.GetOwner(vehicleDef);
    if (vehicleDef == owner)
      return;
    foreach (VehicleDef piggy in this.GetPiggies(owner))
      Interlocked.Exchange(ref this.piggyToOwner[piggy.DefIndex], vehicleDef.DefIndex);
    Interlocked.Exchange(ref this.piggyToOwner[owner.DefIndex], vehicleDef.DefIndex);
    lock (this.gridOwnerLock)
      Interlocked.Exchange<VehicleDef>(ref this.AllOwners[Array.IndexOf<VehicleDef>(this.AllOwners, owner)], vehicleDef);
    GridOwnerList<T>.OwnershipTransferred ownershipTransfer = this.OnOwnershipTransfer;
    if (ownershipTransfer == null)
      return;
    ownershipTransfer(owner, vehicleDef);
  }

  public bool IsOwner(VehicleDef vehicleDef) => this.IsOwner(vehicleDef.DefIndex);

  public bool IsOwner(int id) => this.piggyToOwner[id] == id;

  public VehicleDef GetOwner(VehicleDef vehicleDef)
  {
    return this.IsOwner(vehicleDef.DefIndex) ? vehicleDef : this.GetOwner(this.piggyToOwner[vehicleDef.DefIndex]);
  }

  private VehicleDef GetOwner(int ownerId)
  {
    foreach (VehicleDef allOwner in this.AllOwners)
    {
      if (allOwner.DefIndex == ownerId)
        return allOwner;
    }
    Log.Error($"Unable to find owner by id {ownerId}");
    return (VehicleDef) null;
  }

  public IEnumerable<VehicleDef> GetPiggies(VehicleDef vehicleDef)
  {
    foreach (VehicleDef allPiggy in this.AllPiggies)
    {
      if (this.GetOwner(allPiggy) == vehicleDef)
        yield return allPiggy;
    }
  }

  public delegate void OwnershipTransferred(VehicleDef fromVehicleDef, VehicleDef toVehicleDef) where T : IPathConfig;
}
