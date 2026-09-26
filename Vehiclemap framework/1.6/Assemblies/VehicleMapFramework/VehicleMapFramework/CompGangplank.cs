// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompGangplank
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompGangplank : CompVehicleEnterSpot
{
  private const int RateTicks = 30;
  private int ticks;
  private Vector3 pairDrawPos;

  public CompProperties_Gangplank Props => (CompProperties_Gangplank) this.props;

  protected override bool Available => this.Pair != null;

  public override bool ShouldOffsetOnEdge => true;

  protected Thing Pair { get; set; }

  protected override TargetInfo AccessSpot
  {
    get
    {
      Thing pair = this.Pair;
      return pair == null ? TargetInfo.Invalid : TargetInfo.op_Implicit(pair);
    }
  }

  public override float MovePerTick(Pawn pawn) => 0.5f / pawn.TicksPerMoveCardinal;

  public virtual void CompTickInterval(int delta)
  {
    this.ticks += delta;
    if (this.ticks < 30)
      return;
    this.ticks = 0;
    VehiclePawnWithMap vehicle1;
    if (!((Thing) this.parent).IsOnVehicleMapOf(out vehicle1))
      return;
    if (this.Pair == null)
    {
      Vector3 drawPos = ((Thing) this.parent).DrawPos;
      Rot4 rotation = ((Thing) this.parent).Rotation;
      Vector3 vector3 = Vector3Utility.RotatedBy(Vector2Utility.ToVector3(((Rot4) ref rotation).AsVector2), VehicleMapUtility.get_FullAngle((VehiclePawn) vehicle1));
      Map groundMap = VehicleMapUtility.get_GroundMap((Thing) this.parent);
      for (int index = 1; index <= this.Props.length; ++index)
      {
        Vector3 original = Vector3.op_Addition(drawPos, Vector3.op_Multiply(vector3, (float) index));
        VehiclePawnWithMap vehicle2;
        if (original.TryGetVehicleMap(groundMap, out vehicle2, VehicleMapFlag.None) && vehicle1 != vehicle2)
        {
          Thing thing = GenSpawn.Spawn(VMF_DefOf.VMF_GangplankAnchor, IntVec3Utility.ToIntVec3(original.ToVehicleMapCoord(vehicle2)), vehicle2.VehicleMap, (WipeMode) 0);
          this.Pair = thing;
          CompGangplank comp = ThingCompUtility.TryGetComp<CompGangplank>(thing);
          if (comp != null)
            comp.Pair = (Thing) this.parent;
          this.pairDrawPos = thing.DrawPos;
          break;
        }
      }
    }
    else
    {
      if ((double) GenGeo.MagnitudeHorizontalSquared(Vector3.op_Subtraction(this.Pair.DrawPos, this.pairDrawPos)) <= 1.0)
        return;
      if (!this.Pair.Destroyed)
        this.Pair.Destroy((DestroyMode) 0);
      this.Pair = (Thing) null;
    }
  }
}
