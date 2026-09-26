// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CompDrawAdditionalGraphicsOpacity
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using VehicleMapFramework.VMF_HarmonyPatches;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public class CompDrawAdditionalGraphicsOpacity : CompDrawAdditionalGraphics
{
  public List<ThingWithComps> children = new List<ThingWithComps>();
  private float opacity = 1f;
  private MaterialPropertyBlock propertyBlock;

  public CompDrawAdditionalGraphicsOpacity()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() => this.propertyBlock = new MaterialPropertyBlock()));
  }

  public float Opacity
  {
    get => this.opacity;
    set => this.opacity = value;
  }

  private CompProperties_DrawAdditionalGraphics Props
  {
    get => (CompProperties_DrawAdditionalGraphics) ((ThingComp) this).props;
  }

  public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
  {
    CompDrawAdditionalGraphicsOpacity additionalGraphicsOpacity = this;
    // ISSUE: reference to a compiler-generated method
    foreach (Gizmo gizmo in additionalGraphicsOpacity.\u003C\u003En__0())
      yield return gizmo;
    if (!GenList.NullOrEmpty<GraphicData>((IList<GraphicData>) additionalGraphicsOpacity.Props.graphics))
    {
      Texture mainTexture = additionalGraphicsOpacity.Props.graphics[0].Graphic.MatSouth.mainTexture;
      Vector2 vector2 = mainTexture.height > mainTexture.width ? new Vector2((float) mainTexture.height / (float) mainTexture.height, 1f) : new Vector2(1f, (float) mainTexture.height / (float) mainTexture.width);
      Command_Action commandAction = new Command_Action();
      ((Command) commandAction).defaultLabel = Translator.TranslateSimple("VMF_ChangeOpacity");
      ((Command) commandAction).icon = mainTexture;
      ((Command) commandAction).iconProportions = vector2;
      // ISSUE: reference to a compiler-generated method
      commandAction.action = new Action(additionalGraphicsOpacity.\u003CCompGetGizmosExtra\u003Eb__9_0);
      yield return (Gizmo) commandAction;
    }
  }

  public virtual void PostDraw()
  {
    if ((double) this.opacity == 0.0)
      return;
    foreach (GraphicData graphic in this.Props.graphics)
      Draw(graphic.Graphic);
    foreach (ThingWithComps child in this.children)
    {
      CompAdditionalGraphicsChild additionalGraphicsChild;
      if (ThingCompUtility.TryGetComp<CompAdditionalGraphicsChild>(child, ref additionalGraphicsChild))
      {
        foreach (GraphicData graphic in additionalGraphicsChild.Graphics)
          Draw(graphic.Graphic);
      }
    }

    void Draw(Graphic graphic)
    {
      Vector3 loc = ((Thing) ((ThingComp) this).parent).DrawPos;
      Rot4 rot = ((Thing) ((ThingComp) this).parent).BaseRotationVehicleDraw();
      float extraRotation = 0.0f;
      Patch_Graphic_Draw.Prefix(ref loc, ref rot, (Thing) ((ThingComp) this).parent, ref extraRotation, graphic is Graphic_Appearances graphicAppearances ? graphicAppearances.SubGraphicFor((Thing) ((ThingComp) this).parent) : graphic);
      VehiclePawnWithMap vehicle;
      if (((Thing) ((ThingComp) this).parent).IsOnVehicleMapOf(out vehicle))
      {
        float extraAngle = VehicleMapUtility.get_ExtraAngle((VehiclePawn) vehicle);
        extraRotation += extraAngle;
        Vector3 vector3_1 = graphic.DrawOffset(rot);
        Vector3 vector3_2 = Vector3Utility.RotatedBy(vector3_1, extraAngle);
        loc = Vector3.op_Addition(loc, new Vector3(vector3_2.x - vector3_1.x, 0.0f, vector3_2.z - vector3_1.z));
      }
      Mesh mesh = graphic.MeshAt(rot);
      Quaternion quaternion1 = graphic.QuatFromRot(rot);
      if ((double) extraRotation != 0.0)
        quaternion1 = Quaternion.op_Multiply(quaternion1, Quaternion.Euler(Vector3.op_Multiply(Vector3.up, extraRotation)));
      GraphicData data = graphic.data;
      if (data != null && data.addTopAltitudeBias)
        quaternion1 = Quaternion.op_Multiply(quaternion1, Quaternion.Euler(Vector3.op_Multiply(Vector3.left, 2f)));
      loc = Vector3.op_Addition(loc, graphic.DrawOffset(rot));
      Material material1 = graphic.MatAt(rot, (Thing) ((ThingComp) this).parent);
      loc.y += 0.11f;
      loc.y -= loc.z * 1E-05f;
      loc.y -= loc.x * 1E-06f;
      Color color = GenColor.WithAlpha(((Thing) ((ThingComp) this).parent).DrawColor, this.opacity);
      this.propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
      this.propertyBlock.SetColor(AdditionalShaderPropertyIDs.ColorOne, color);
      this.propertyBlock.SetFloat(Graphic_VehicleOpacity.OpacityID, this.opacity);
      Vector3 vector3 = loc;
      Quaternion quaternion2 = quaternion1;
      Material material2 = material1;
      MaterialPropertyBlock propertyBlock = this.propertyBlock;
      Graphics.DrawMesh(mesh, vector3, quaternion2, material2, 0, (Camera) null, 0, propertyBlock);
      ((Graphic) graphic.ShadowGraphic)?.DrawWorker(loc, rot, ((Thing) ((ThingComp) this).parent).def, (Thing) ((ThingComp) this).parent, extraRotation);
    }
  }

  public virtual void PostDeSpawn(Map map, DestroyMode mode = 0)
  {
    ((ThingComp) this).PostDeSpawn(map, mode);
    for (int index = this.children.Count - 1; index >= 0; --index)
    {
      ThingWithComps child = this.children[index];
      if (((Thing) child).Spawned)
        ((Entity) child).DeSpawn(mode);
    }
  }

  public virtual void PostExposeData()
  {
    ((ThingComp) this).PostExposeData();
    Scribe_Values.Look<float>(ref this.opacity, "opacity", 0.0f, false);
    Scribe_Collections.Look<ThingWithComps>(ref this.children, "children", (LookMode) 3, Array.Empty<object>());
  }
}
