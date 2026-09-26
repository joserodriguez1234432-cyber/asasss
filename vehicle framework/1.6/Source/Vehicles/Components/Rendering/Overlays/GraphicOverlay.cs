// Decompiled with JetBrains decompiler
// Type: Vehicles.GraphicOverlay
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using RimWorld;
using SmashTools;
using SmashTools.Animations;
using SmashTools.Rendering;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Vehicles.Rendering;
using Verse;

#nullable disable
namespace Vehicles;

public class GraphicOverlay : 
  IAnimationObject,
  IMaterialCacheTarget,
  IParallelRenderer,
  IBlitTarget,
  ITransformable,
  ITweakFields
{
  public GraphicDataOverlay data;
  private readonly VehiclePawn vehicle;
  private readonly VehicleDef vehicleDef;
  private Vehicles.Rendering.PreRenderResults results;
  [TweakField]
  private Graphic graphic;
  private Graphic_DynamicShadow graphicShadow;
  [TweakField]
  [AnimationProperty(Name = "Transform")]
  private readonly Transform transform = new Transform();
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  [AnimationProperty(Name = "Propeller Acceleration")]
  internal float acceleration;

  private GraphicOverlay(GraphicDataOverlay graphicDataOverlay, VehicleDef vehicleDef)
  {
    this.data = graphicDataOverlay;
    this.vehicleDef = vehicleDef;
  }

  private GraphicOverlay(GraphicDataOverlay graphicDataOverlay, VehiclePawn vehicle)
  {
    this.data = graphicDataOverlay;
    this.vehicle = vehicle;
    this.vehicleDef = vehicle.VehicleDef;
    this.vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.Destroyed, new Action(this.Destroy));
    if (!this.data.dynamicShadows)
      return;
    ShadowData shadowData = new ShadowData()
    {
      volume = new Vector3(this.data.graphicData.drawSize.x, 0.0f, this.data.graphicData.drawSize.y),
      offset = new Vector3(this.data.graphicData.drawOffset.x, 0.0f, this.data.graphicData.drawOffset.z + 5f)
    };
    this.graphicShadow = new Graphic_DynamicShadow(this.data.graphicData.Graphic.TexAt(Rot8.North), shadowData);
  }

  public VehiclePawn Vehicle => this.vehicle;

  public VehicleDef VehicleDef => this.vehicleDef;

  public int MaterialCount
  {
    get
    {
      VehiclePawn vehicle = this.vehicle;
      return vehicle == null ? this.vehicleDef.MaterialCount : __nonvirtual (vehicle.MaterialCount);
    }
  }

  public PatternDef PatternDef => PatternDefOf.Default;

  public string Name => $"{this.vehicleDef.Name}_{this.data.graphicData.texPath}";

  public MaterialPropertyBlock PropertyBlock { get; private set; }

  string IAnimationObject.ObjectId => this.data.identifier ?? nameof (GraphicOverlay);

  public Graphic_DynamicShadow ShadowGraphic => this.graphicShadow;

  public Transform Transform => this.transform;

  string ITweakFields.Category => "Graphic Overlay";

  string ITweakFields.Label => this.data.identifier ?? this.data.graphicData.texPath;

  bool IParallelRenderer.IsDirty { get; set; }

  public Graphic Graphic
  {
    get
    {
      if (this.graphic == null)
      {
        if (this.PropertyBlock == null)
        {
          MaterialPropertyBlock materialPropertyBlock;
          this.PropertyBlock = materialPropertyBlock = new MaterialPropertyBlock();
        }
        VehiclePawn vehicle = this.vehicle;
        if (vehicle != null && ((Thing) vehicle).Destroyed && !GenList.NullOrEmpty<Material>((IList<Material>) RGBMaterialPool.GetAll((IMaterialCacheTarget) this)))
        {
          Log.Error($"Reinitializing RGB Materials but {this} has already been destroyed and the cache " + "was not cleared for this entry. This may result in a memory leak.");
          RGBMaterialPool.Release((IMaterialCacheTarget) this);
        }
        PatternData patternData = this.vehicle?.patternData ?? GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) this.vehicleDef).defName, new PatternData(this.vehicleDef.graphicData));
        GraphicDataRGB graphicDataRgb = new GraphicDataRGB();
        graphicDataRgb.CopyFrom((GraphicDataLayered) this.data.graphicData);
        if (GenTypes.SameOrSubclassOf(graphicDataRgb.graphicClass, typeof (Graphic_Rgb)) && graphicDataRgb.shaderType.Shader.SupportsRGBMaskTex())
        {
          graphicDataRgb.color = patternData.color;
          graphicDataRgb.colorTwo = patternData.colorTwo;
          graphicDataRgb.colorThree = patternData.colorThree;
          graphicDataRgb.tiles = patternData.tiles;
          graphicDataRgb.displacement = patternData.displacement;
          graphicDataRgb.pattern = patternData.patternDef;
          RGBMaterialPool.CacheMaterialsFor((IMaterialCacheTarget) this);
          graphicDataRgb.Init((IMaterialCacheTarget) this);
          this.graphic = (Graphic) graphicDataRgb.Graphic;
          Graphic_Rgb graphic = (Graphic_Rgb) this.graphic;
          RGBMaterialPool.SetProperties((IMaterialCacheTarget) this, patternData, new Func<Rot8, Texture2D>(graphic.TexAt), new Func<Rot8, Texture2D>(graphic.MaskAt));
        }
        else
          this.graphic = ((GraphicData) graphicDataRgb).Graphic;
      }
      return this.graphic;
    }
  }

  public void DynamicDrawPhaseAt(DrawPhase phase, in TransformData transformData, bool forceDraw = false)
  {
    switch ((int) phase)
    {
      case 0:
        for (int index = 0; index < 4; ++index)
          this.Graphic.MeshAt(new Rot4(index));
        break;
      case 1:
        this.results = this.ParallelGetPreRenderResults(ref transformData, forceDraw);
        break;
      case 2:
        if (!this.results.valid)
          this.results = this.ParallelGetPreRenderResults(ref transformData, forceDraw);
        this.Draw(ref transformData);
        this.results = new Vehicles.Rendering.PreRenderResults();
        break;
      default:
        throw new NotImplementedException();
    }
  }

  private Vehicles.Rendering.PreRenderResults ParallelGetPreRenderResults(
    [RequiresLocation, In] ref TransformData transformData,
    bool forceDraw = false)
  {
    ComponentRequirement component = this.data.component;
    if (component != null && !component.MeetsRequirements)
      return new Vehicles.Rendering.PreRenderResults()
      {
        valid = true,
        draw = false
      };
    if (this.Graphic is Graphic_Rgb graphic)
    {
      float extraRotation = this.transform.rotation + this.data.rotation;
      Vehicles.Rendering.PreRenderResults preRenderResults = graphic.ParallelGetPreRenderResults(ref transformData, forceDraw, (Thing) this.vehicle, extraRotation);
      ref Vector3 local = ref preRenderResults.position;
      local = Vector3.op_Addition(local, this.transform.position);
      return preRenderResults;
    }
    return new Vehicles.Rendering.PreRenderResults()
    {
      valid = true,
      draw = true
    };
  }

  private void Draw([RequiresLocation, In] ref TransformData transformData)
  {
    if (!this.results.draw)
      return;
    if (this.Graphic is Graphic_Rgb)
      Graphics.DrawMesh(this.results.mesh, this.results.position, this.results.quaternion, this.results.material, 0);
    else
      this.Graphic.DrawWorker(transformData.position, (Rot4) transformData.orientation, (ThingDef) null, (Thing) null, transformData.rotation);
  }

  private void Notify_ColorChanged()
  {
    if (!this.data.graphicData.shaderType.Shader.SupportsRGBMaskTex())
      return;
    RGBMaterialPool.SetProperties((IMaterialCacheTarget) this, this.vehicle?.patternData ?? GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) this.vehicleDef).defName, new PatternData(this.vehicleDef.graphicData)));
  }

  public void Destroy()
  {
    this.vehicle.RemoveEvent<VehicleEventDef>(VehicleEventDefOf.ColorChanged, new Action(this.Notify_ColorChanged));
    RGBMaterialPool.Release((IMaterialCacheTarget) this);
  }

  public static GraphicOverlay Create(GraphicDataOverlay graphicDataOverlay, VehiclePawn vehicle)
  {
    if (!UnityData.IsInMainThread)
    {
      Log.Error("Trying to create GraphicOverlay outside of the main thread.");
      return (GraphicOverlay) null;
    }
    GraphicOverlay target = new GraphicOverlay(graphicDataOverlay, vehicle);
    GraphicDataRGB graphicData = graphicDataOverlay.graphicData;
    if (graphicData.shaderType == null)
      graphicData.shaderType = ShaderTypeDefOf.Cutout;
    if (!VehicleMod.settings.main.useCustomShaders)
      graphicDataOverlay.graphicData.shaderType = graphicDataOverlay.graphicData.shaderType.Shader.SupportsRGBMaskTex(true) ? ShaderTypeDefOf.CutoutComplex : graphicDataOverlay.graphicData.shaderType;
    if (graphicDataOverlay.graphicData.shaderType.Shader.SupportsRGBMaskTex())
    {
      RGBMaterialPool.CacheMaterialsFor((IMaterialCacheTarget) target);
      graphicDataOverlay.graphicData.Init((IMaterialCacheTarget) target);
      PatternData patternData = vehicle.patternData;
      Graphic_Rgb graphic = (Graphic_Rgb) target.Graphic;
      RGBMaterialPool.SetProperties((IMaterialCacheTarget) target, patternData, new Func<Rot8, Texture2D>(graphic.TexAt), new Func<Rot8, Texture2D>(graphic.MaskAt));
    }
    else
    {
      Graphic_Rgb graphic1 = graphicDataOverlay.graphicData.Graphic;
    }
    vehicle.AddEvent<VehicleEventDef>(VehicleEventDefOf.ColorChanged, new Action(target.Notify_ColorChanged));
    return target;
  }

  public static GraphicOverlay Create(GraphicDataOverlay graphicDataOverlay, VehicleDef vehicleDef)
  {
    GraphicOverlay target = new GraphicOverlay(graphicDataOverlay, vehicleDef);
    GraphicDataRGB graphicData = graphicDataOverlay.graphicData;
    if (graphicData.shaderType == null)
      graphicData.shaderType = ShaderTypeDefOf.Cutout;
    if (!VehicleMod.settings.main.useCustomShaders)
      graphicDataOverlay.graphicData.shaderType = graphicDataOverlay.graphicData.shaderType.Shader.SupportsRGBMaskTex(true) ? ShaderTypeDefOf.CutoutComplex : graphicDataOverlay.graphicData.shaderType;
    if (graphicDataOverlay.graphicData.shaderType.Shader.SupportsRGBMaskTex())
    {
      RGBMaterialPool.CacheMaterialsFor((IMaterialCacheTarget) target);
      graphicDataOverlay.graphicData.Init((IMaterialCacheTarget) target);
      PatternData patternData = GenCollection.TryGetValue<string, PatternData>((IReadOnlyDictionary<string, PatternData>) VehicleMod.settings.vehicles.defaultGraphics, ((Def) vehicleDef).defName, new PatternData(vehicleDef.graphicData));
      Graphic_Rgb graphic = (Graphic_Rgb) target.Graphic;
      RGBMaterialPool.SetProperties((IMaterialCacheTarget) target, patternData, new Func<Rot8, Texture2D>(graphic.TexAt), new Func<Rot8, Texture2D>(graphic.MaskAt));
    }
    else
    {
      Graphic_Rgb graphic1 = graphicDataOverlay.graphicData.Graphic;
    }
    return target;
  }

  (int width, int height) IBlitTarget.TextureSize(in BlitRequest request)
  {
    Material material = this.Graphic.MatAt((Rot4) request.rot, (Thing) null);
    Texture mainTexture = Object.op_Implicit((Object) material) ? material.mainTexture : (Texture) null;
    return !Object.op_Inequality((Object) mainTexture, (Object) null) ? (0, 0) : (mainTexture.width, mainTexture.height);
  }

  IEnumerable<SmashTools.Rendering.RenderData> IBlitTarget.GetRenderData(
    Rect rect,
    BlitRequest request)
  {
    GraphicOverlay graphicOverlay = this;
    Rect rect1 = VehicleGraphics.OverlayRect(rect, graphicOverlay.vehicleDef, graphicOverlay, request.rot);
    Material material = (Material) null;
    Texture2D mainTexture = graphicOverlay.Graphic.MatAt((Rot4) request.rot, (Thing) null).mainTexture as Texture2D;
    if (graphicOverlay.Graphic.Shader.SupportsRGBMaskTex())
    {
      material = graphicOverlay.Graphic.MatAt((Rot4) request.rot, (Thing) null);
      if (graphicOverlay.Graphic is Graphic_Rgb graphic)
      {
        if (graphicOverlay.Graphic.Shader.SupportsRGBMaskTex())
          material = RGBMaterialPool.GetUi((IMaterialCacheTarget) graphicOverlay, (Rot4) request.rot);
        RGBMaterialPool.SetProperties((IMaterialCacheTarget) graphicOverlay, request.patternData, new Func<Rot8, Texture2D>(graphic.TexAt), new Func<Rot8, Texture2D>(graphic.MaskAt));
      }
      else
      {
        // ISSUE: reference to a compiler-generated method
        // ISSUE: reference to a compiler-generated method
        RGBMaterialPool.SetProperties((IMaterialCacheTarget) graphicOverlay, request.patternData, new Func<Rot8, Texture2D>(graphicOverlay.\u003CVehicles\u002ERendering\u002EIBlitTarget\u002EGetRenderData\u003Eb__48_0), new Func<Rot8, Texture2D>(graphicOverlay.\u003CVehicles\u002ERendering\u002EIBlitTarget\u002EGetRenderData\u003Eb__48_1));
      }
    }
    yield return new SmashTools.Rendering.RenderData(rect1, (Texture) mainTexture, material, graphicOverlay.vehicleDef.PropertyBlock, graphicOverlay.data.graphicData.DrawOffsetFull(request.rot).y, graphicOverlay.data.rotation);
  }

  void ITweakFields.OnFieldChanged() => this.SetDirty();

  void IParallelRenderer.DynamicDrawPhaseAt(
    DrawPhase phase,
    in TransformData transformData,
    bool forceDraw = false)
  {
    this.DynamicDrawPhaseAt(phase, in transformData, forceDraw);
  }
}
