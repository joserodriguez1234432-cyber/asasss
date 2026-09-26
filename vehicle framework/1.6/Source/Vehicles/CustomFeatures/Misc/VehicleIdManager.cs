// Decompiled with JetBrains decompiler
// Type: Vehicles.VehicleIdManager
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;

#nullable disable
namespace Vehicles;

public class VehicleIdManager : GameComponent
{
  private int nextUpgradeId;
  private int nextCannonId;
  private int nextVehicleHandlerId;
  private int nextReservationId;
  private int nextRequestCollectionId;
  private int nextAirDefenseId;

  public VehicleIdManager(Game game) => VehicleIdManager.Instance = this;

  public static VehicleIdManager Instance { get; private set; }

  public int GetNextRequestCollectionId() => this.GetNextId(ref this.nextRequestCollectionId);

  public int GetNextReservationId() => this.GetNextId(ref this.nextReservationId);

  public int GetNextUpgradeId() => this.GetNextId(ref this.nextUpgradeId);

  public int GetNextCannonId() => this.GetNextId(ref this.nextCannonId);

  public int GetNextHandlerId() => this.GetNextId(ref this.nextVehicleHandlerId);

  public int GetNextAirDefenseId() => this.GetNextId(ref this.nextAirDefenseId);

  private int GetNextId(ref int id)
  {
    ++id;
    return id;
  }

  public virtual void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<int>(ref this.nextUpgradeId, "nextUpgradeId", 0, false);
    Scribe_Values.Look<int>(ref this.nextCannonId, "nextCannonId", 0, false);
    Scribe_Values.Look<int>(ref this.nextVehicleHandlerId, "nextVehicleHandlerId", 0, false);
    Scribe_Values.Look<int>(ref this.nextReservationId, "nextReservationId", 0, false);
    Scribe_Values.Look<int>(ref this.nextRequestCollectionId, "nextRequestCollectionId", 0, false);
    Scribe_Values.Look<int>(ref this.nextAirDefenseId, "nextAirDefenseId", 0, false);
  }
}
