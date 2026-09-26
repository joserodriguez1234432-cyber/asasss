// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompPipeConnector
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompPipeConnector : ThingComp
{
  public const int TicksInterval = 30;
  protected bool connectReq;
  public IPipeConnector selectedComp;

  public CompProperties_PipeConnector Props => (CompProperties_PipeConnector) this.props;

  public CompPipeConnector Pair { get; set; }

  public List<IPipeConnector> ConnectorComps
  {
    get
    {
      if (this.\u003CConnectorComps\u003Ek__BackingField == null)
        this.\u003CConnectorComps\u003Ek__BackingField = this.parent.AllComps.OfType<IPipeConnector>().ToList<IPipeConnector>();
      return this.\u003CConnectorComps\u003Ek__BackingField;
    }
  }

  private Material PipeMat
  {
    get
    {
      if (Object.op_Equality((Object) this.\u003CPipeMat\u003Ek__BackingField, (Object) null))
        this.\u003CPipeMat\u003Ek__BackingField = MaterialPool.MatFrom("VehicleMapFramework/Things/PipeConnector/Pipe", ShaderDatabase.Cutout);
      return this.\u003CPipeMat\u003Ek__BackingField;
    }
  }

  private Graphic PipeEndGraphic
  {
    get
    {
      if (this.\u003CPipeEndGraphic\u003Ek__BackingField == null)
        this.\u003CPipeEndGraphic\u003Ek__BackingField = GraphicDatabase.Get<Graphic_Single>("VehicleMapFramework/Things/PipeConnector/PipeEnd", ShaderDatabase.CutoutComplex);
      return this.\u003CPipeEndGraphic\u003Ek__BackingField.GetColoredVersion(ShaderDatabase.CutoutComplex, ((Thing) this.parent).DrawColor, ((Thing) this.parent).DrawColorTwo);
    }
  }

  public virtual void CompTick()
  {
    base.CompTick();
    if (!((Thing) this.parent).Spawned || Find.TickManager.TicksGame % 30 != 0 || this.selectedComp == null)
      return;
    if (VehicleMapUtility.get_IsOnVehicleMap((Thing) this.parent))
    {
      if (this.Pair != null && !this.connectReq)
      {
        this.selectedComp.DisconnectedAction();
        this.Pair = (CompPipeConnector) null;
      }
      this.connectReq = false;
    }
    else
    {
      bool flag = false;
      int num = GenRadial.NumCellsInRadius(this.Props.radius);
      IntVec3 position = ((Thing) this.parent).Position;
      for (int index = 0; index < num; ++index)
      {
        IntVec3 intVec3 = IntVec3.op_Addition(position, GenRadial.RadialPattern[index]);
        VehiclePawnWithMap vehicle;
        if (GenGrid.InBounds(intVec3, ((Thing) this.parent).Map) && intVec3.TryGetVehicleMap(((Thing) this.parent).Map, out vehicle))
        {
          IntVec3 vehicleMapCoord = intVec3.ToVehicleMapCoord(vehicle);
          if (GenGrid.InBounds(vehicleMapCoord, vehicle.VehicleMap))
          {
            ThingWithComps firstThingWithComp = GridsUtility.GetFirstThingWithComp<CompPipeConnector>(vehicleMapCoord, vehicle.VehicleMap);
            if (firstThingWithComp != null)
            {
              CompPipeConnector comp = firstThingWithComp.GetComp<CompPipeConnector>();
              CompPipeConnector.PipeMod? mod1 = comp.selectedComp?.Mod;
              CompPipeConnector.PipeMod mod2 = this.selectedComp.Mod;
              if (mod1.GetValueOrDefault() == mod2 & mod1.HasValue && this.selectedComp.ConnectCondition(comp))
              {
                comp.connectReq = true;
                if (this.Pair != comp || comp.Pair != this)
                {
                  this.Pair = comp;
                  comp.Pair = this;
                }
                this.selectedComp.ConnectedTickAction();
                flag = true;
                break;
              }
            }
          }
        }
      }
      if (flag)
        return;
      this.Pair = (CompPipeConnector) null;
    }
  }

  public virtual void PostDraw()
  {
    base.PostDraw();
    if (this.Pair == null || !((Thing) this.parent).IsOnVehicleMapOf(out VehiclePawnWithMap _))
      return;
    float num1 = Altitudes.AltitudeFor((AltitudeLayer) 32 /*0x20*/) - 1f / 1000f;
    Vector3 vector3_1 = Vector3Utility.WithY(((Thing) this.parent).DrawPos, num1);
    Vector3 vector3_2 = Vector3Utility.WithY(((Thing) this.Pair.parent).DrawPos, num1);
    Graphic pipeEndGraphic = this.PipeEndGraphic;
    float num2 = Vector3Utility.AngleFlat(Vector3.op_Subtraction(vector3_2, vector3_1));
    Graphics.DrawMesh(MeshPool.plane10, vector3_1, Quaternion.AngleAxis(num2, Vector3.up), pipeEndGraphic.MatSingle, 0);
    Graphics.DrawMesh(MeshPool.plane10, vector3_2, Quaternion.AngleAxis(num2 + 180f, Vector3.up), pipeEndGraphic.MatSingle, 0);
    GenDraw.DrawLineBetween(vector3_1, vector3_2, -1f / 1000f, this.PipeMat, 1f);
  }

  public virtual void PostDrawExtraSelectionOverlays()
  {
    base.PostDrawExtraSelectionOverlays();
    GenDraw.DrawRadiusRing(VehicleMapUtility.get_PositionOnBaseMap((Thing) this.parent), this.Props.radius);
  }

  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    CompPipeConnector compPipeConnector = this;
    Command_Action commandAction = new Command_Action();
    ((Command) commandAction).defaultLabel = TaggedString.op_Implicit(Translator.Translate("VMF_AssignPipeNet"));
    IPipeConnector selectedComp = compPipeConnector.selectedComp;
    ((Command) commandAction).icon = (Texture) ((selectedComp != null ? (object) selectedComp.GizmoIcon : (object) null) ?? (object) BaseContent.ClearTex);
    // ISSUE: reference to a compiler-generated method
    commandAction.action = new Action(compPipeConnector.\u003CCompGetGizmosExtra\u003Eb__22_0);
    yield return (Gizmo) commandAction;
  }

  public virtual string CompInspectStringExtra()
  {
    return !(this.selectedComp is ThingComp selectedComp) ? (string) null : selectedComp.CompInspectStringExtra();
  }

  public virtual void PostExposeData()
  {
    CompPipeConnector.PipeMod? mod = this.selectedComp?.Mod;
    Scribe_Values.Look<CompPipeConnector.PipeMod?>(ref mod, "selectedComp", new CompPipeConnector.PipeMod?(), false);
    this.selectedComp = GenCollection.FirstOrDefault<IPipeConnector>(this.ConnectorComps, (Predicate<IPipeConnector>) (c =>
    {
      int mod1 = (int) c.Mod;
      CompPipeConnector.PipeMod? nullable = mod;
      int valueOrDefault = (int) nullable.GetValueOrDefault();
      return mod1 == valueOrDefault & nullable.HasValue;
    }));
  }

  public enum PipeMod
  {
    VanillaExpandedFramework,
    DubsBadHygiene,
    Rimefeller,
  }
}
