// Decompiled with JetBrains decompiler
// Type: Vehicles.Rendering.VehicleGraphics
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles.Rendering;

public static class VehicleGraphics
{
  private static readonly VehicleGraphics.OverlayGUIRenderer overlayRenderer = new VehicleGraphics.OverlayGUIRenderer();

  public static Vector3 DrawOffsetFull(this Graphic graphic, Rot8 rot)
  {
    return graphic.data.DrawOffsetFull(rot);
  }

  public static Vector3 DrawOffsetFull(this GraphicData graphicData, Rot8 rot)
  {
    Vector2 vector2 = VehicleGraphics.VehicleDrawOffset(rot, graphicData.drawOffset.x, graphicData.drawOffset.y);
    return new Vector3(vector2.x, graphicData.drawOffset.y, vector2.y);
  }

  public static Vector2 VehicleDrawOffset(
    Rot8 rot,
    float offsetX,
    float offsetY,
    float additionalRotation = 0.0f)
  {
    return Ext_Math.RotatePointClockwise(offsetX, offsetY, rot.AsAngle + additionalRotation);
  }

  public static Rect AdjustRectToVehicleDef(VehicleDef vehicleDef, Rect rect, Rot8 rot)
  {
    Vector2 vector2_1 = vehicleDef.ScaleDrawRatio(((Rect) ref rect).size);
    bool flag = rot.IsHorizontal || rot.IsDiagonal;
    Vector2 vector2_2 = Vector2.op_Implicit(vehicleDef.drawProperties.DisplayOffsetForRot((Rot4) rot));
    float num1 = vector2_1.x;
    float num2 = vector2_1.y;
    if (flag)
    {
      num1 = vector2_1.y;
      num2 = vector2_1.x;
    }
    float num3 = (float) (((double) ((Rect) ref rect).width - (double) num1) / 2.0 + (double) vector2_2.x * (double) ((Rect) ref rect).width);
    float num4 = (float) (((double) ((Rect) ref rect).height - (double) num2) / 2.0 + (double) vector2_2.y * (double) ((Rect) ref rect).height);
    return new Rect(((Rect) ref rect).x + num3, ((Rect) ref rect).y + num4, num1, num2);
  }

  public static void DrawVehicle(
    Rect rect,
    VehiclePawn vehicle,
    Rot8? rot = null,
    List<GraphicOverlay> extraOverlays = null,
    List<VehicleTurret> extraTurrets = null,
    List<string> excludeTurrets = null)
  {
    VehicleGraphics.DrawVehicle(rect, vehicle, vehicle.patternData, rot, extraOverlays: extraOverlays, extraTurrets: extraTurrets, excludeTurrets: excludeTurrets);
  }

  public static void DrawVehicle(
    Rect rect,
    VehiclePawn vehicle,
    PatternData patternData,
    Rot8? rot = null,
    bool withoutTurrets = false,
    List<GraphicOverlay> extraOverlays = null,
    List<VehicleTurret> extraTurrets = null,
    List<string> excludeTurrets = null)
  {
    VehicleDef vehicleDef1 = vehicle.VehicleDef;
    try
    {
      VehicleGraphics.overlayRenderer.Clear();
      if (!Mathf.Approximately(((Rect) ref rect).width, ((Rect) ref rect).height))
        Log.WarningOnce("Drawing VehicleDef with non-uniform rect. VehicleDefs are best drawn in square rects which will then be adjusted to fit.", "DrawVehicleDef".GetHashCode());
      Rot8 rot1 = rot ?? vehicleDef1.drawProperties.displayRotation;
      Rect vehicleDef2 = VehicleGraphics.AdjustRectToVehicleDef(vehicleDef1, rect, rot1);
      Graphic_Vehicle graphic = vehicleDef1.graphicData.Graphic as Graphic_Vehicle;
      PatternData patternData1 = patternData ?? GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) vehicleDef1).defName, (PatternData) vehicleDef1.graphicData);
      float rotate;
      Texture2D texture = VehicleTex.VehicleTexture(vehicleDef1, (Rot4) rot1, out rotate);
      Material material = (Material) null;
      if (graphic.Shader.SupportsRGBMaskTex())
      {
        material = RGBMaterialPool.Get((IMaterialCacheTarget) vehicleDef1, rot1);
        RGBMaterialPool.SetProperties((IMaterialCacheTarget) vehicleDef1, patternData1, new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).TexAt), new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).MaskAt));
      }
      if (vehicle.CompVehicleTurrets != null && !withoutTurrets)
      {
        foreach (VehicleGraphics.RenderData graphicsProperty in VehicleGraphics.RetrieveAllTurretSettingsGraphicsProperties(rect, vehicleDef1, rot1, (IEnumerable<VehicleTurret>) vehicle.CompVehicleTurrets.Turrets.OrderBy<VehicleTurret, int>((Func<VehicleTurret, int>) (t => t.drawLayer)), patternData1, excludeTurrets))
          VehicleGraphics.overlayRenderer.Add(graphicsProperty);
        if (!GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) extraTurrets))
        {
          foreach (VehicleGraphics.RenderData graphicsProperty in VehicleGraphics.RetrieveAllTurretSettingsGraphicsProperties(rect, vehicleDef1, rot1, (IEnumerable<VehicleTurret>) extraTurrets.OrderBy<VehicleTurret, int>((Func<VehicleTurret, int>) (t => t.drawLayer)), patternData1, excludeTurrets))
            VehicleGraphics.overlayRenderer.Add(graphicsProperty);
        }
      }
      foreach (VehicleGraphics.RenderData graphicsProperty in VehicleGraphics.RetrieveAllOverlaySettingsGraphicsProperties(rect, vehicle, rot1, patternData1, extraOverlays))
        VehicleGraphics.overlayRenderer.Add(graphicsProperty);
      VehicleGraphics.overlayRenderer.FinalizeForRendering();
      VehicleGraphics.overlayRenderer.RenderLayer(VehicleGraphics.GUILayer.Lower);
      VehicleGraphics.DrawVehicleFitted(vehicleDef2, rotate, texture, material);
      VehicleGraphics.overlayRenderer.RenderLayer(VehicleGraphics.GUILayer.Upper);
    }
    catch (Exception ex)
    {
      SmashLog.Error($"Exception thrown while trying to draw Graphics <type>VehicleDef</type>=\"{((Def) vehicleDef1)?.defName ?? "Null"}\" Exception={ex}");
    }
    finally
    {
      VehicleGraphics.overlayRenderer.Clear();
    }
  }

  public static void DrawVehicleDef(
    Rect rect,
    VehicleDef vehicleDef,
    PatternData patternData = null,
    Rot8? rot = null,
    bool withoutTurrets = false,
    List<GraphicOverlay> extraOverlays = null,
    List<VehicleTurret> extraTurrets = null,
    List<string> excludeTurrets = null)
  {
    try
    {
      VehicleGraphics.overlayRenderer.Clear();
      if ((double) ((Rect) ref rect).width != (double) ((Rect) ref rect).height)
        Log.WarningOnce("Drawing VehicleDef with non-uniform rect. VehicleDefs are best drawn in square rects which will then be adjusted to fit.", nameof (DrawVehicleDef).GetHashCode());
      Rot8 rot1 = rot ?? vehicleDef.drawProperties.displayRotation;
      Rect vehicleDef1 = VehicleGraphics.AdjustRectToVehicleDef(vehicleDef, rect, rot1);
      Graphic_Vehicle graphic = vehicleDef.graphicData.Graphic as Graphic_Vehicle;
      PatternData patternData1 = patternData ?? GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) vehicleDef).defName, (PatternData) vehicleDef.graphicData);
      float rotate;
      Texture2D texture = VehicleTex.VehicleTexture(vehicleDef, (Rot4) rot1, out rotate);
      Material material = (Material) null;
      if (graphic.Shader.SupportsRGBMaskTex())
      {
        material = RGBMaterialPool.Get((IMaterialCacheTarget) vehicleDef, rot1);
        RGBMaterialPool.SetProperties((IMaterialCacheTarget) vehicleDef, patternData1, new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).TexAt), new Func<Rot8, Texture2D>(((Graphic_Rgb) graphic).MaskAt));
      }
      CompProperties_VehicleTurrets sortedCompProperties = vehicleDef.GetSortedCompProperties<CompProperties_VehicleTurrets>();
      if (sortedCompProperties != null && !withoutTurrets)
      {
        foreach (VehicleGraphics.RenderData graphicsProperty in VehicleGraphics.RetrieveAllTurretSettingsGraphicsProperties(rect, vehicleDef, rot1, (IEnumerable<VehicleTurret>) sortedCompProperties.turrets.OrderBy<VehicleTurret, int>((Func<VehicleTurret, int>) (t => t.drawLayer)), patternData1, excludeTurrets))
          VehicleGraphics.overlayRenderer.Add(graphicsProperty);
        if (!GenList.NullOrEmpty<VehicleTurret>((IList<VehicleTurret>) extraTurrets))
        {
          foreach (VehicleGraphics.RenderData graphicsProperty in VehicleGraphics.RetrieveAllTurretSettingsGraphicsProperties(rect, vehicleDef, rot1, (IEnumerable<VehicleTurret>) extraTurrets.OrderBy<VehicleTurret, int>((Func<VehicleTurret, int>) (t => t.drawLayer)), patternData1, excludeTurrets))
            VehicleGraphics.overlayRenderer.Add(graphicsProperty);
        }
      }
      foreach (VehicleGraphics.RenderData graphicsProperty in VehicleGraphics.RetrieveAllOverlaySettingsGraphicsProperties(rect, vehicleDef, rot1, patternData1, extraOverlays))
        VehicleGraphics.overlayRenderer.Add(graphicsProperty);
      VehicleGraphics.overlayRenderer.FinalizeForRendering();
      VehicleGraphics.overlayRenderer.RenderLayer(VehicleGraphics.GUILayer.Lower);
      VehicleGraphics.DrawVehicleFitted(vehicleDef1, rotate, texture, material);
      VehicleGraphics.overlayRenderer.RenderLayer(VehicleGraphics.GUILayer.Upper);
    }
    catch (Exception ex)
    {
      SmashLog.Error($"Exception thrown while trying to draw Graphics <type>VehicleDef</type>=\"{((Def) vehicleDef)?.defName ?? "Null"}\" Exception={ex}");
    }
    finally
    {
      VehicleGraphics.overlayRenderer.Clear();
    }
  }

  public static IEnumerable<VehicleGraphics.RenderData> RetrieveAllOverlaySettingsGraphicsProperties(
    Rect rect,
    VehiclePawn vehicle,
    Rot8 rot,
    PatternData pattern = null,
    List<GraphicOverlay> extraOverlays = null)
  {
    foreach (GraphicOverlay graphicOverlay in vehicle.DrawTracker.overlayRenderer.AllOverlaysListForReading)
    {
      if (graphicOverlay.data.renderUI)
        yield return VehicleGraphics.RetrieveOverlaySettingsGraphicsProperties(rect, vehicle.VehicleDef, rot, graphicOverlay, pattern);
    }
    if (!GenList.NullOrEmpty<GraphicOverlay>((IList<GraphicOverlay>) extraOverlays))
    {
      foreach (GraphicOverlay extraOverlay in extraOverlays)
      {
        if (extraOverlay.data.renderUI)
          yield return VehicleGraphics.RetrieveOverlaySettingsGraphicsProperties(rect, vehicle.VehicleDef, rot, extraOverlay, pattern);
      }
    }
  }

  public static IEnumerable<VehicleGraphics.RenderData> RetrieveAllOverlaySettingsGraphicsProperties(
    Rect rect,
    VehicleDef vehicleDef,
    Rot8 rot,
    PatternData pattern = null,
    List<GraphicOverlay> extraOverlays = null)
  {
    foreach (GraphicOverlay overlay in vehicleDef.drawProperties.overlays)
    {
      if (overlay.data.renderUI)
        yield return VehicleGraphics.RetrieveOverlaySettingsGraphicsProperties(rect, vehicleDef, rot, overlay, pattern);
    }
    if (!GenList.NullOrEmpty<GraphicOverlay>((IList<GraphicOverlay>) extraOverlays))
    {
      foreach (GraphicOverlay extraOverlay in extraOverlays)
      {
        if (extraOverlay.data.renderUI)
          yield return VehicleGraphics.RetrieveOverlaySettingsGraphicsProperties(rect, vehicleDef, rot, extraOverlay, pattern);
      }
    }
  }

  public static VehicleGraphics.RenderData RetrieveOverlaySettingsGraphicsProperties(
    Rect rect,
    VehicleDef vehicleDef,
    Rot8 rot,
    GraphicOverlay graphicOverlay,
    PatternData pattern)
  {
    Rect rect1 = VehicleGraphics.OverlayRect(rect, vehicleDef, graphicOverlay, rot);
    Graphic_Rgb graphic = graphicOverlay.data.graphicData.Graphic;
    Texture2D mainTex = graphic.TexAt(rot);
    Material material = (Material) null;
    if (graphic.Shader.SupportsRGBMaskTex())
    {
      material = RGBMaterialPool.Get((IMaterialCacheTarget) graphicOverlay, rot);
      RGBMaterialPool.SetProperties((IMaterialCacheTarget) graphicOverlay, pattern, new Func<Rot8, Texture2D>(graphic.TexAt), new Func<Rot8, Texture2D>(graphic.MaskAt));
    }
    else if (ShaderUtility.SupportsMaskTex(graphic.Shader))
      material = graphic.MatAt((Rot4) rot, (Thing) null);
    return new VehicleGraphics.RenderData(rect1, (Texture) mainTex, material, graphicOverlay.data.graphicData.DrawOffsetFull(rot).y, graphicOverlay.data.rotation);
  }

  public static IEnumerable<VehicleGraphics.RenderData> RetrieveAllTurretSettingsGraphicsProperties(
    Rect rect,
    VehicleDef vehicleDef,
    Rot8 rot,
    IEnumerable<VehicleTurret> turrets,
    PatternData patternData,
    List<string> excludeTurrets = null)
  {
    foreach (VehicleTurret turret1 in turrets)
    {
      VehicleTurret turret = turret1;
      VehicleTurret turretRef = turret.reference ?? turret;
      GenText.NullOrEmpty(turret.parentKey);
      if (!turret.NoGraphic)
        yield return VehicleGraphics.RetrieveTurretSettingsGraphicsProperties(rect, vehicleDef, rot, turretRef, patternData);
      if (excludeTurrets == null || !excludeTurrets.Contains(turret.key) && !excludeTurrets.Contains(turret.parentKey))
      {
        if (!GenList.NullOrEmpty<VehicleTurret.TurretDrawData>((IList<VehicleTurret.TurretDrawData>) turretRef.TurretGraphics))
        {
          List<VehicleTurret.TurretDrawData>.Enumerator enumerator = turretRef.TurretGraphics.GetEnumerator();
          while (enumerator.MoveNext())
          {
            VehicleTurret.TurretDrawData current = enumerator.Current;
            Rect rect1 = VehicleGraphics.TurretRect(rect, vehicleDef, turretRef, rot);
            Material material = (Material) null;
            if (patternData != null && current.graphic.Shader.SupportsRGBMaskTex())
            {
              material = RGBMaterialPool.Get((IMaterialCacheTarget) current, Rot8.North);
              RGBMaterialPool.SetProperties((IMaterialCacheTarget) current, patternData, new Func<Rot8, Texture2D>(((Graphic_Rgb) current.graphic).TexAt), new Func<Rot8, Texture2D>(((Graphic_Rgb) current.graphic).MaskAt));
            }
            else if (ShaderUtility.SupportsMaskTex(current.graphic.Shader))
              material = current.graphic.MatAt((Rot4) Rot8.North, (Thing) null);
            yield return new VehicleGraphics.RenderData(rect1, (Texture) current.graphic.TexAt(Rot8.North), material, current.graphicData.DrawOffsetFull(rot).y + turretRef.DrawLayerOffset, turretRef.defaultAngleRotated + rot.AsAngle);
          }
          enumerator = new List<VehicleTurret.TurretDrawData>.Enumerator();
        }
        turretRef = (VehicleTurret) null;
        turret = (VehicleTurret) null;
      }
    }
  }

  public static VehicleGraphics.RenderData RetrieveTurretSettingsGraphicsProperties(
    Rect rect,
    VehicleDef vehicleDef,
    Rot8 rot,
    VehicleTurret turret,
    PatternData patternData)
  {
    Rect rect1 = VehicleGraphics.TurretRect(rect, vehicleDef, turret, rot);
    Material material = (Material) null;
    if (patternData != null && turret.Graphic.Shader.SupportsRGBMaskTex())
    {
      material = RGBMaterialPool.Get((IMaterialCacheTarget) turret, Rot8.North);
      RGBMaterialPool.SetProperties((IMaterialCacheTarget) turret, patternData, new Func<Rot8, Texture2D>(((Graphic_Rgb) turret.Graphic).TexAt), new Func<Rot8, Texture2D>(((Graphic_Rgb) turret.Graphic).MaskAt));
    }
    else if (ShaderUtility.SupportsMaskTex(turret.Graphic.Shader))
      material = turret.Graphic.MatAt((Rot4) Rot8.North, (Thing) null);
    return new VehicleGraphics.RenderData(rect1, (Texture) turret.Texture, material, turret.GraphicData.DrawOffsetFull(rot).y + turret.DrawLayerOffset, turret.defaultAngleRotated + rot.AsAngle);
  }

  internal static Rect TurretRect(
    Rect rect,
    VehicleDef vehicleDef,
    VehicleTurret turret,
    Rot8 rot,
    float iconScale = 1f)
  {
    turret.ResolveGraphics(vehicleDef);
    return turret.ScaleUIRectFor(vehicleDef, rect, rot, iconScale);
  }

  internal static Rect OverlayRect(
    Rect rect,
    VehicleDef vehicleDef,
    GraphicOverlay graphicOverlay,
    Rot8 rot,
    float scale = 1f)
  {
    GraphicDataRGB graphicData = graphicOverlay.data.graphicData;
    Vector2 vector2_1 = vehicleDef.ScaleDrawRatio((GraphicData) graphicData, (Rot4) rot, ((Rect) ref rect).size, scale);
    Vector2 vector2_2 = Vector2.op_Addition(((Rect) ref rect).position, Vector2.op_Multiply(Vector2.op_Subtraction(((Rect) ref rect).size, vector2_1), 0.5f));
    Vector3 vector3 = graphicData.DrawOffsetForRot((Rot4) rot);
    Vector2 vector2_3;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_3).\u002Ector(graphicData.drawSize.x, graphicData.drawSize.y);
    Vector2 vector2_4;
    // ISSUE: explicit constructor call
    ((Vector2) ref vector2_4).\u002Ector(vector2_1.x / vector2_3.x, vector2_1.y / vector2_3.y);
    return new Rect(Vector2.op_Addition(vector2_2, new Vector2(vector3.x * vector2_4.x, -vector3.z * vector2_4.y)), vector2_1);
  }

  public static void DrawVehicleFitted(
    Rect rect,
    VehicleDef vehicleDef,
    Rot4 rot,
    Material material)
  {
    float rotate;
    Texture2D texture2D = VehicleTex.VehicleTexture(vehicleDef, rot, out rotate);
    Rect rect1;
    // ISSUE: explicit constructor call
    ((Rect) ref rect1).\u002Ector(0.0f, 0.0f, 1f, 1f);
    Vector2 drawSize = vehicleDef.graphicData.drawSize;
    if (((Rot4) ref rot).IsHorizontal)
    {
      float x = drawSize.x;
      drawSize.x = drawSize.y;
      drawSize.y = x;
    }
    Widgets.DrawTextureFitted(rect, (Texture) texture2D, GenUI.IconDrawScale((ThingDef) vehicleDef), drawSize, rect1, rotate, material, 1f);
  }

  public static void DrawVehicleFitted(
    Rect rect,
    float angle,
    Texture2D texture,
    Material material)
  {
    Widgets.DrawTextureFitted(rect, (Texture) texture, 1f, new Vector2((float) ((Texture) texture).width, (float) ((Texture) texture).height), new Rect(0.0f, 0.0f, 1f, 1f), angle, material, 1f);
  }

  private enum GUILayer
  {
    Lower,
    Upper,
  }

  public readonly struct RenderData(
    Rect rect,
    Texture mainTex,
    Material material,
    float layer,
    float angle) : IComparable<VehicleGraphics.RenderData>
  {
    public readonly Rect rect = rect;
    public readonly Texture mainTex = mainTex;
    public readonly Material material = material;
    public readonly float layer = layer;
    public readonly float angle = angle;

    public static VehicleGraphics.RenderData Invalid
    {
      get => new VehicleGraphics.RenderData(Rect.zero, (Texture) null, (Material) null, -1f, 0.0f);
    }

    int IComparable<VehicleGraphics.RenderData>.CompareTo(VehicleGraphics.RenderData other)
    {
      if ((double) this.layer < (double) other.layer)
        return -1;
      return (double) this.layer > (double) other.layer ? 1 : 0;
    }
  }

  private class OverlayGUIRenderer
  {
    private readonly List<VehicleGraphics.RenderData> renderDataLower = new List<VehicleGraphics.RenderData>();
    private readonly List<VehicleGraphics.RenderData> renderDataUpper = new List<VehicleGraphics.RenderData>();

    public void Add(VehicleGraphics.RenderData renderData)
    {
      if ((double) renderData.layer < 0.0)
        this.renderDataLower.Add(renderData);
      else
        this.renderDataUpper.Add(renderData);
    }

    public void Clear()
    {
      this.renderDataLower.Clear();
      this.renderDataUpper.Clear();
    }

    public void FinalizeForRendering()
    {
      this.renderDataLower.Sort();
      this.renderDataUpper.Sort();
    }

    public void RenderLayer(VehicleGraphics.GUILayer layer)
    {
      if (layer != VehicleGraphics.GUILayer.Lower)
      {
        if (layer != VehicleGraphics.GUILayer.Upper)
          throw new NotImplementedException("GUILayer");
        foreach (VehicleGraphics.RenderData renderData in this.renderDataUpper)
          UIElements.DrawTextureWithMaterialOnGUI(renderData.rect, renderData.mainTex, renderData.material, renderData.angle, new Rect());
      }
      else
      {
        foreach (VehicleGraphics.RenderData renderData in this.renderDataLower)
          UIElements.DrawTextureWithMaterialOnGUI(renderData.rect, renderData.mainTex, renderData.material, renderData.angle, new Rect());
      }
    }
  }
}
