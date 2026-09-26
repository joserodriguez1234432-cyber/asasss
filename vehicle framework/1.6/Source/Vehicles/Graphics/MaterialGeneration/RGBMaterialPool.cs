// Decompiled with JetBrains decompiler
// Type: Vehicles.RGBMaterialPool
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using HarmonyLib;
using LudeonTK;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[StaticConstructorOnStartup]
public static class RGBMaterialPool
{
  private const int MaxUiArraySize = 4;
  private static readonly Dictionary<IMaterialCacheTarget, Material[]> Cache = new Dictionary<IMaterialCacheTarget, Material[]>();
  private static readonly Dictionary<IMaterialCacheTarget, Material[]> UiCache = new Dictionary<IMaterialCacheTarget, Material[]>();
  private static readonly Dictionary<Shader, Shader> ShaderToUiShader = new Dictionary<Shader, Shader>();

  public static event Action<IMaterialCacheTarget> OnTargetCached;

  public static event Action<IMaterialCacheTarget> OnTargetRemoved;

  static RGBMaterialPool()
  {
    RGBMaterialPool.ShaderToUiShader[VehicleShaderTypeDefOf.CutoutComplexRGB.Shader] = ShaderDatabase.LoadShader("ShaderRGBUI");
    RGBMaterialPool.ShaderToUiShader[VehicleShaderTypeDefOf.CutoutComplexPattern.Shader] = ShaderDatabase.LoadShader("ShaderRGBPatternUI");
    RGBMaterialPool.ShaderToUiShader[VehicleShaderTypeDefOf.CutoutComplexSkin.Shader] = ShaderDatabase.LoadShader("ShaderRGBSkinUI");
  }

  public static int Count => RGBMaterialPool.Cache.Count;

  public static int TotalMaterials
  {
    get
    {
      return RGBMaterialPool.Cache.Values.Sum<Material[]>((Func<Material[], int>) (mats => mats.Length));
    }
  }

  internal static List<IMaterialCacheTarget> AllCacheTargets
  {
    get => RGBMaterialPool.Cache.Keys.ToList<IMaterialCacheTarget>();
  }

  public static bool TargetCached(IMaterialCacheTarget target)
  {
    return RGBMaterialPool.Cache.ContainsKey(target);
  }

  public static Material[] GetAll(IMaterialCacheTarget target)
  {
    return GenCollection.TryGetValue<IMaterialCacheTarget, Material[]>((IReadOnlyDictionary<IMaterialCacheTarget, Material[]>) RGBMaterialPool.Cache, target, (Material[]) null);
  }

  public static Material GetUi(IMaterialCacheTarget target, Rot4 rot)
  {
    Material[] materialArray = GenCollection.TryGetValue<IMaterialCacheTarget, Material[]>((IReadOnlyDictionary<IMaterialCacheTarget, Material[]>) RGBMaterialPool.UiCache, target, (Material[]) null);
    if (materialArray.Length == 0)
    {
      Trace.Fail("Trying to get ui material with 0 materials cached.");
      return (Material) null;
    }
    return materialArray.Length < 4 ? materialArray[0] : materialArray[((Rot4) ref rot).AsInt];
  }

  public static Material Get(IMaterialCacheTarget target, Rot8 rot)
  {
    Material[] materialArray;
    if (!RGBMaterialPool.Cache.TryGetValue(target, out materialArray))
      return (Material) null;
    if (rot.AsInt < materialArray.Length)
      return materialArray[rot.AsInt];
    Log.Error($"Attempting to fetch material out of bounds. Target={target} Rot8={rot}. " + $"Max count for {target} is {target.MaterialCount}");
    return (Material) null;
  }

  public static void CacheMaterialsFor(
    IMaterialCacheTarget target,
    int renderQueue = 0,
    List<ShaderParameter> shaderParameters = null)
  {
    RGBMaterialPool.CacheMaterialsFor(target, target.PatternDef, renderQueue, shaderParameters);
  }

  public static void CacheMaterialsFor(
    IMaterialCacheTarget target,
    PatternDef patternDef,
    int renderQueue = 0,
    List<ShaderParameter> shaderParameters = null)
  {
    if (RGBMaterialPool.Cache.ContainsKey(target) || patternDef == null)
      return;
    Material[] materialArray1 = new Material[target.MaterialCount];
    Material[] materialArray2 = new Material[Mathf.Min(4, target.MaterialCount)];
    for (int newRot = 0; newRot < materialArray1.Length; ++newRot)
    {
      Rot8 rot8 = new Rot8(newRot);
      Material material1 = new Material(patternDef.ShaderTypeDef.Shader);
      ((Object) material1).name = target.Name + rot8.ToStringNamed();
      material1.mainTexture = (Texture) null;
      material1.color = Color.clear;
      Material material2 = material1;
      Material material3 = new Material(patternDef.ShaderTypeDef.Shader);
      ((Object) material3).name = target.Name + rot8.ToStringNamed();
      material3.mainTexture = (Texture) null;
      material3.color = Color.clear;
      Material material4 = material3;
      if (renderQueue != 0)
      {
        material2.renderQueue = renderQueue;
        material4.renderQueue = renderQueue;
      }
      if (!GenList.NullOrEmpty<ShaderParameter>((IList<ShaderParameter>) shaderParameters))
      {
        foreach (ShaderParameter shaderParameter in shaderParameters)
        {
          shaderParameter.Apply(material2);
          shaderParameter.Apply(material4);
        }
      }
      materialArray1[newRot] = material2;
      if (newRot < 4)
        materialArray2[newRot] = material4;
    }
    RGBMaterialPool.Cache.Add(target, materialArray1);
    RGBMaterialPool.UiCache.Add(target, materialArray2);
    Action<IMaterialCacheTarget> onTargetCached = RGBMaterialPool.OnTargetCached;
    if (onTargetCached == null)
      return;
    onTargetCached(target);
  }

  public static void SetPropertyBlock(
    IMaterialCacheTarget target,
    PatternData patternData,
    Texture2D mainTex,
    Texture2D maskTex,
    Rot8 rot)
  {
    if (!RGBMaterialPool.Cache.ContainsKey(target))
    {
      Log.Error($"Materials for {target} have not been created. Out of sequence material editing.");
    }
    else
    {
      MaterialPropertyBlock propertyBlock = target.PropertyBlock;
      propertyBlock.Clear();
      propertyBlock.SetTexture(AdditionalShaderPropertyIDs.MainTex, (Texture) mainTex);
      if (patternData.patternDef != PatternDefOf.Default)
      {
        float tiles = patternData.tiles;
        float num1;
        if (patternData.patternDef.properties.tiles.TryGetValue("All", out num1))
          tiles *= num1;
        if (!Mathf.Approximately(tiles, 0.0f))
          propertyBlock.SetFloat(AdditionalShaderPropertyIDs.TileNum, tiles);
        if (patternData.patternDef.properties.equalize)
        {
          float num2 = 1f;
          float num3 = 1f;
          if (((Texture) mainTex).width > ((Texture) mainTex).height)
            num3 = (float) ((Texture) mainTex).height / (float) ((Texture) mainTex).width;
          else
            num2 = (float) ((Texture) mainTex).width / (float) ((Texture) mainTex).height;
          propertyBlock.SetFloat(AdditionalShaderPropertyIDs.ScaleX, num2);
          propertyBlock.SetFloat(AdditionalShaderPropertyIDs.ScaleY, num3);
        }
        if (patternData.patternDef.properties.dynamicTiling)
        {
          propertyBlock.SetFloat(AdditionalShaderPropertyIDs.DisplacementX, patternData.displacement.x);
          propertyBlock.SetFloat(AdditionalShaderPropertyIDs.DisplacementY, patternData.displacement.y);
        }
      }
      Texture2D texture2D = patternData.patternDef[rot];
      if (patternData.patternDef.ShaderTypeDef == VehicleShaderTypeDefOf.CutoutComplexSkin)
        target.PropertyBlock.SetTexture(AdditionalShaderPropertyIDs.SkinTex, (Texture) texture2D);
      else if (patternData.patternDef.ShaderTypeDef == VehicleShaderTypeDefOf.CutoutComplexPattern)
        target.PropertyBlock.SetTexture(AdditionalShaderPropertyIDs.PatternTex, (Texture) texture2D);
      if (Object.op_Inequality((Object) maskTex, (Object) null))
        target.PropertyBlock.SetTexture(ShaderPropertyIDs.MaskTex, (Texture) maskTex);
      target.PropertyBlock.SetColor(AdditionalShaderPropertyIDs.ColorOne, patternData.color);
      target.PropertyBlock.SetColor(ShaderPropertyIDs.ColorTwo, patternData.colorTwo);
      target.PropertyBlock.SetColor(AdditionalShaderPropertyIDs.ColorThree, patternData.colorThree);
    }
  }

  public static void SetProperties(
    IMaterialCacheTarget target,
    PatternData patternData,
    Func<Rot8, Texture2D> mainTexGetter = null,
    Func<Rot8, Texture2D> maskTexGetter = null)
  {
    Material[] materialArray1;
    Material[] materialArray2;
    if (!RGBMaterialPool.Cache.TryGetValue(target, out materialArray1) || !RGBMaterialPool.UiCache.TryGetValue(target, out materialArray2))
    {
      Log.Error($"Materials for {target} have not been created. Out of sequence material editing.");
    }
    else
    {
      for (int newRot = 0; newRot < materialArray1.Length; ++newRot)
      {
        Rot8 rot = new Rot8(newRot);
        Material material1 = materialArray1[newRot];
        material1.SetColor(AdditionalShaderPropertyIDs.ColorOne, patternData.color);
        material1.SetColor(ShaderPropertyIDs.ColorTwo, patternData.colorTwo);
        material1.SetColor(AdditionalShaderPropertyIDs.ColorThree, patternData.colorThree);
        Texture2D texture2D1 = material1.mainTexture as Texture2D;
        if (mainTexGetter != null)
          texture2D1 = mainTexGetter(rot);
        material1.mainTexture = (Texture) texture2D1;
        if (!Object.op_Implicit((Object) texture2D1))
        {
          Trace.Fail("Trying to set material properties with no main tex");
        }
        else
        {
          Texture2D texture2D2 = maskTexGetter != null ? maskTexGetter(rot) : (Texture2D) null;
          if (Object.op_Implicit((Object) texture2D2))
            material1.SetTexture(ShaderPropertyIDs.MaskTex, (Texture) texture2D2);
          if (patternData.patternDef != PatternDefOf.Default)
          {
            float tiles = patternData.tiles;
            float num1;
            if (patternData.patternDef.properties.tiles.TryGetValue("All", out num1))
              tiles *= num1;
            if (!Mathf.Approximately(tiles, 0.0f))
              material1.SetFloat(AdditionalShaderPropertyIDs.TileNum, tiles);
            if (patternData.patternDef.properties.equalize)
            {
              float num2 = 1f;
              float num3 = 1f;
              if (((Texture) texture2D1).width > ((Texture) texture2D1).height)
                num3 = (float) ((Texture) texture2D1).height / (float) ((Texture) texture2D1).width;
              else
                num2 = (float) ((Texture) texture2D1).width / (float) ((Texture) texture2D1).height;
              material1.SetFloat(AdditionalShaderPropertyIDs.ScaleX, num2);
              material1.SetFloat(AdditionalShaderPropertyIDs.ScaleY, num3);
            }
            if (patternData.patternDef.properties.dynamicTiling)
            {
              material1.SetFloat(AdditionalShaderPropertyIDs.DisplacementX, patternData.displacement.x);
              material1.SetFloat(AdditionalShaderPropertyIDs.DisplacementY, patternData.displacement.y);
            }
          }
          if (Object.op_Inequality((Object) patternData.patternDef.ShaderTypeDef.Shader, (Object) material1.shader))
            material1.shader = patternData.patternDef.ShaderTypeDef.Shader;
          Texture2D texture2D3 = patternData.patternDef[rot];
          if (patternData.patternDef.ShaderTypeDef == VehicleShaderTypeDefOf.CutoutComplexSkin)
            material1.SetTexture(AdditionalShaderPropertyIDs.SkinTex, (Texture) texture2D3);
          else if (patternData.patternDef.ShaderTypeDef == VehicleShaderTypeDefOf.CutoutComplexPattern)
            material1.SetTexture(AdditionalShaderPropertyIDs.PatternTex, (Texture) texture2D3);
          if (newRot < 4)
          {
            Material material2 = materialArray2[newRot];
            material2.shader = material1.shader;
            Shader shader;
            if (Object.op_Implicit((Object) material2.shader) && RGBMaterialPool.ShaderToUiShader.TryGetValue(material2.shader, out shader))
              material2.shader = shader;
            material2.CopyPropertiesFromMaterial(material1);
          }
        }
      }
    }
  }

  public static void Release(IMaterialCacheTarget target)
  {
    Material[] materialArray;
    if (!RGBMaterialPool.Cache.TryGetValue(target, out materialArray))
      return;
    foreach (Object @object in materialArray)
      Object.Destroy(@object);
    RGBMaterialPool.Cache.Remove(target);
    RGBMaterialPool.UiCache.Remove(target);
    Action<IMaterialCacheTarget> onTargetRemoved = RGBMaterialPool.OnTargetRemoved;
    if (onTargetRemoved != null)
      onTargetRemoved(target);
    GraphicDatabaseRGB.Remove(target);
    Debug.Message($"Removed {target} from RGBMaterialPool and cleared all entries.");
  }

  public static void DestroyAll()
  {
    foreach (KeyValuePair<IMaterialCacheTarget, Material[]> keyValuePair in RGBMaterialPool.Cache)
    {
      IMaterialCacheTarget materialCacheTarget;
      Material[] materialArray;
      keyValuePair.Deconstruct(ref materialCacheTarget, ref materialArray);
      foreach (Object @object in materialArray)
        Object.Destroy(@object);
    }
    RGBMaterialPool.Cache.Clear();
    RGBMaterialPool.UiCache.Clear();
  }

  [DebugOutput("Vehicle Framework", false)]
  internal static void LogAllMaterials()
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine($"----- Outputting Cache (Targets={RGBMaterialPool.Cache.Count} " + $"Total={RGBMaterialPool.Cache.Values.Sum<Material[]>((Func<Material[], int>) (arr => arr.Length))}) -----");
    stringBuilder.AppendLine("Vanilla Material Count: " + $"{((Dictionary<Material, MaterialRequest>) AccessTools.Field(typeof (MaterialPool), "matDictionaryReverse").GetValue((object) null)).Count}");
    foreach (KeyValuePair<IMaterialCacheTarget, Material[]> keyValuePair in RGBMaterialPool.Cache)
    {
      IMaterialCacheTarget materialCacheTarget1;
      Material[] materialArray;
      keyValuePair.Deconstruct(ref materialCacheTarget1, ref materialArray);
      IMaterialCacheTarget materialCacheTarget2 = materialCacheTarget1;
      Material[] source = materialArray;
      stringBuilder.AppendLine($"Target={materialCacheTarget2} Materials=\n" + string.Join("\n", ((IEnumerable<Material>) source).Select<Material, string>((Func<Material, string>) (material => ((Object) material).name))));
    }
    stringBuilder.AppendLine("----- End of Cache Output -----");
    Log.Message(stringBuilder.ToString());
  }
}
