// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.ShaderUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class ShaderUtility
{
  public static bool SupportsOpacity(this Shader shader)
  {
    return Object.op_Equality((Object) shader, (Object) VMF_DefOf.VMF_CutoutComplexRGBOpacity.Shader) || Object.op_Equality((Object) shader, (Object) VMF_DefOf.VMF_CutoutComplexPatternOpacity.Shader) || Object.op_Equality((Object) shader, (Object) VMF_DefOf.VMF_CutoutComplexSkinOpacity.Shader);
  }

  public static ShaderTypeDef OpacityShaderTypeDefCorrespond(this ShaderTypeDef shaderTypeDef)
  {
    if (shaderTypeDef == VehicleShaderTypeDefOf.CutoutComplexRGB)
      return VMF_DefOf.VMF_CutoutComplexRGBOpacity;
    if (shaderTypeDef == VehicleShaderTypeDefOf.CutoutComplexPattern)
      return VMF_DefOf.VMF_CutoutComplexPatternOpacity;
    return shaderTypeDef == VehicleShaderTypeDefOf.CutoutComplexSkin ? VMF_DefOf.VMF_CutoutComplexSkinOpacity : shaderTypeDef;
  }

  public static Shader OpacityShaderCorrespond(this Shader shader)
  {
    if (Object.op_Equality((Object) shader, (Object) VehicleShaderTypeDefOf.CutoutComplexRGB.Shader))
      return VMF_DefOf.VMF_CutoutComplexRGBOpacity.Shader;
    if (Object.op_Equality((Object) shader, (Object) VehicleShaderTypeDefOf.CutoutComplexPattern.Shader))
      return VMF_DefOf.VMF_CutoutComplexPatternOpacity.Shader;
    return Object.op_Equality((Object) shader, (Object) VehicleShaderTypeDefOf.CutoutComplexSkin.Shader) ? VMF_DefOf.VMF_CutoutComplexSkinOpacity.Shader : shader;
  }
}
