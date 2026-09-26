// Decompiled with JetBrains decompiler
// Type: Vehicles.PatternDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using System.Collections.Generic;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[UsedImplicitly]
public class PatternDef : Def, IMaterialCacheTarget
{
  public string path;
  public PatternProperties properties;
  private TextureWrapMode wrapMode;
  public List<VehicleDef> exclusiveFor;
  private Texture2D[] patterns;

  private bool IsDefault => this == PatternDefOf.Default;

  public virtual ShaderTypeDef ShaderTypeDef => VehicleShaderTypeDefOf.CutoutComplexPattern;

  public int MaterialCount => 4;

  PatternDef IMaterialCacheTarget.PatternDef => this;

  public string Name => $"{this.modContentPack.Name}_{this.defName}";

  public MaterialPropertyBlock PropertyBlock { get; private set; }

  public Texture2D this[Rot8 rot]
  {
    get
    {
      if (this.patterns == null)
        this.RecacheTextures();
      return this.patterns[rot.AsInt];
    }
  }

  public bool ValidFor(VehicleDef def)
  {
    return GenList.NullOrEmpty<VehicleDef>((IList<VehicleDef>) this.exclusiveFor) || this.exclusiveFor.Contains(def);
  }

  private void RecacheTextures()
  {
    this.patterns = new Texture2D[8];
    string[] strArray = new string[8]
    {
      this.path + "_north",
      this.path + "_east",
      this.path + "_south",
      this.path + "_west",
      this.path + "_northEast",
      this.path + "_southEast",
      this.path + "_southWest",
      this.path + "_northWest"
    };
    this.patterns[0] = ContentFinder<Texture2D>.Get(this.path, false);
    ref Texture2D local = ref this.patterns[0];
    if (local == null)
      local = ContentFinder<Texture2D>.Get(strArray[0], false);
    if (this.patterns[0] == null)
      SmashLog.Error($"Unable to find Texture2D for <field>path</field> at {this.path}.");
    else if (this.IsDefault)
    {
      this.patterns[1] = this.patterns[0];
      this.patterns[2] = this.patterns[0];
      this.patterns[3] = this.patterns[0];
      this.patterns[4] = this.patterns[0];
      this.patterns[5] = this.patterns[0];
      this.patterns[6] = this.patterns[0];
      this.patterns[7] = this.patterns[0];
    }
    else
    {
      for (int index = 1; index < 8; ++index)
        this.patterns[index] = ContentFinder<Texture2D>.Get(strArray[index], false);
      if (!Object.op_Implicit((Object) this.patterns[1]))
        this.patterns[1] = this.patterns[0].Rotate(270f);
      if (!Object.op_Implicit((Object) this.patterns[2]))
        this.patterns[2] = this.patterns[0].Rotate(180f);
      if (!Object.op_Implicit((Object) this.patterns[3]))
        this.patterns[3] = this.patterns[0].Rotate(90f);
      if (!Object.op_Implicit((Object) this.patterns[4]))
        this.patterns[4] = this.patterns[0];
      if (!Object.op_Implicit((Object) this.patterns[5]))
        this.patterns[5] = this.patterns[2];
      if (!Object.op_Implicit((Object) this.patterns[6]))
        this.patterns[6] = this.patterns[2];
      if (!Object.op_Implicit((Object) this.patterns[7]))
        this.patterns[7] = this.patterns[2];
      for (int index = 0; index < this.patterns.Length; ++index)
      {
        Texture2D pattern = this.patterns[index];
        if (((Texture) pattern).wrapMode != this.wrapMode)
        {
          this.patterns[index] = Ext_Texture.WrapTexture(pattern, this.wrapMode);
          if (Ext_Texture.TryReplaceInContentFinder<Texture2D>(strArray[index], this.patterns[index]))
            Debug.Message($"[{this.modContentPack.Name}] Wrapping and destroying {strArray[index]}");
        }
      }
    }
  }

  public virtual IEnumerable<string> ConfigErrors()
  {
    foreach (string configError in base.ConfigErrors())
      yield return configError;
    if (GenText.NullOrEmpty(this.path))
      yield return "<field>path</field> must be a valid texture path.".ConvertRichText();
    if (this.properties?.tiles != null)
    {
      foreach (KeyValuePair<string, float> tile in this.properties.tiles)
      {
        if ((double) tile.Value == 0.0)
          yield return $"key <field>{tile.Key}</field> in <field>tiles</field> should not be set to 0. This will result in odd coloring of the pattern.".ConvertRichText();
      }
    }
  }

  public virtual void ResolveReferences()
  {
    if (this.properties == null)
      this.properties = new PatternProperties();
    PatternProperties properties = this.properties;
    if (properties.tiles == null)
      properties.tiles = new Dictionary<string, float>();
    if (!this.IsDefault)
      return;
    this.properties.IsDefault = true;
  }

  internal static void GenerateMaterials()
  {
    if (VehicleMod.settings.main.useCustomShaders)
    {
      foreach (PatternDef target in DefDatabase<PatternDef>.AllDefsListForReading)
      {
        target.RecacheTextures();
        PatternDef patternDef = target;
        if (patternDef.PropertyBlock == null)
        {
          MaterialPropertyBlock materialPropertyBlock;
          patternDef.PropertyBlock = materialPropertyBlock = new MaterialPropertyBlock();
        }
        RGBMaterialPool.CacheMaterialsFor((IMaterialCacheTarget) target);
      }
    }
    else
      PatternDefOf.Default.RecacheTextures();
  }
}
