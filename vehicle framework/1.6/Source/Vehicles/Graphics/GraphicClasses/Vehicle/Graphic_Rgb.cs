// Decompiled with JetBrains decompiler
// Type: Vehicles.Graphic_Rgb
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using JetBrains.Annotations;
using SmashTools;
using SmashTools.Rendering;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

#nullable disable
namespace Vehicles;

[PublicAPI]
public class Graphic_Rgb : Graphic
{
  public const string MaskSuffix = "m";
  private static readonly string[] pathExtensions = new string[8]
  {
    "_north",
    "_east",
    "_south",
    "_west",
    "_northEast",
    "_southEast",
    "_southWest",
    "_northWest"
  };
  protected bool westFlipped;
  protected bool eastFlipped;
  protected bool eastRotated;
  protected bool southRotated;
  protected bool eastDiagonalRotated;
  protected bool westDiagonalRotated;
  protected float drawRotatedExtraAngleOffset;
  public Color colorThree = Color.white;
  public float tiles = 1f;
  public Vector2 displacement = Vector2.zero;
  protected Texture2D[] textures;
  protected Texture2D[] masks;
  public Material[] materials;
  public int[] patternPointers;

  public virtual int MatCount => 8;

  public virtual bool WestFlipped => this.westFlipped;

  public virtual bool EastFlipped => this.eastFlipped;

  public bool EastRotated => this.eastRotated;

  public bool SouthRotated => this.southRotated;

  public bool EastDiagonalRotated => this.eastDiagonalRotated;

  public bool WestDiagonalRotated => this.westDiagonalRotated;

  public bool DiagonalRotated => this.EastDiagonalRotated || this.WestDiagonalRotated;

  public virtual bool ShouldDrawRotated
  {
    get
    {
      if (this.data != null && !this.data.drawRotated)
        return false;
      return Object.op_Equality((Object) base.MatEast, (Object) base.MatNorth) || Object.op_Equality((Object) base.MatWest, (Object) base.MatNorth);
    }
  }

  public virtual float DrawRotatedExtraAngleOffset => this.drawRotatedExtraAngleOffset;

  public virtual Material MatSingle => base.MatNorth;

  public virtual Material MatNorth => this.materials[0];

  public virtual Material MatEast => this.materials[1];

  public virtual Material MatSouth => this.materials[2];

  public virtual Material MatWest => this.materials[3];

  public virtual GraphicDataRGB DataRgb
  {
    get
    {
      if (this.data is GraphicDataRGB data)
        return data;
      SmashLog.ErrorOnce($"Unable to retrieve <field>DataRGB</field> for <type>{((object) this).GetType()}</type>. GraphicData type = {this.data?.GetType().ToString() ?? "Null"}", ((object) this).GetHashCode());
      return (GraphicDataRGB) null;
    }
  }

  public virtual Texture2D TexAt(Rot8 rot) => this.textures[rot.AsInt];

  public virtual Texture2D MaskAt(Rot8 rot) => this.masks[rot.AsInt];

  public virtual Mesh MeshAt(Rot4 rot)
  {
    Vector2 vector2_1 = this.drawSize;
    if (((Rot4) ref rot).IsHorizontal && !base.ShouldDrawRotated)
      vector2_1 = Vector2Utility.Rotated(vector2_1);
    if (Rot4.op_Equality(rot, Rot4.West) && base.WestFlipped || Rot4.op_Equality(rot, Rot4.East) && base.EastFlipped)
    {
      if (this.EastRotated)
        vector2_1 = Vector2Utility.Rotated(vector2_1);
      return MeshPool.GridPlaneFlip(vector2_1);
    }
    if ((!this.EastRotated || !Rot4.op_Equality(rot, Rot4.East)) && (!this.SouthRotated || !Rot4.op_Equality(rot, Rot4.South)))
      return MeshPool.GridPlane(vector2_1);
    Vector2 vector2_2 = Vector2Utility.Rotated(vector2_1);
    return !base.EastFlipped ? MeshPool.GridPlane(vector2_2) : MeshPool.GridPlaneFlip(vector2_2);
  }

  public virtual Mesh MeshAtFull(Rot8 rot)
  {
    if (!rot.IsDiagonal)
      return base.MeshAt((Rot4) rot);
    if (this.EastDiagonalRotated)
    {
      if (rot == Rot8.NorthEast)
        return base.MeshAt((Rot4) Rot8.North);
      if (rot == Rot8.SouthEast)
        return base.MeshAt((Rot4) Rot8.South);
    }
    if (this.WestDiagonalRotated)
    {
      if (rot == Rot8.NorthWest)
        return base.MeshAt((Rot4) Rot8.North);
      if (rot == Rot8.SouthWest)
        return base.MeshAt((Rot4) Rot8.South);
    }
    return base.MeshAt((Rot4) rot);
  }

  public Material MatAtFull(Rot8 rot)
  {
    return ((IList<Material>) this.materials).OutOfBounds<Material>(rot.AsInt) ? BaseContent.BadMat : this.materials[rot.AsInt];
  }

  public virtual void Init(GraphicRequest req)
  {
    this.masks = new Texture2D[this.MatCount];
    this.materials = new Material[this.MatCount];
    if (req.shader.SupportsRGBMaskTex())
      Log.Warning($"Calling Non-RGB Init with shader type that supports RGB Shaders. Req={req.path} ShaderType={req.graphicData.shaderType}");
    this.CopyData(req);
    this.GetTextures(req.path);
    if (ShaderUtility.SupportsMaskTex(req.shader) || req.shader.SupportsRGBMaskTex())
      this.GetMasks(req.path, req.shader);
    for (int index = 0; index < this.masks.Length; ++index)
    {
      MaterialRequest materialRequest = new MaterialRequest()
      {
        mainTex = (Texture) this.textures[index],
        maskTex = this.masks[index],
        shader = req.shader,
        color = req.color,
        colorTwo = req.colorTwo,
        renderQueue = req.renderQueue,
        shaderParameters = req.shaderParameters
      };
      this.materials[index] = MaterialPool.MatFrom(materialRequest);
    }
  }

  public virtual void Init(GraphicRequestRGB req)
  {
    if (!req.shader.SupportsRGBMaskTex())
      Log.Warning($"Calling RGB Init with unsupported shader type. Req={req}");
    this.masks = new Texture2D[this.MatCount];
    this.materials = new Material[this.MatCount];
    this.CopyData((GraphicRequest) req);
    this.colorThree = req.colorThree;
    this.tiles = req.tiles;
    this.displacement = req.displacement;
    this.GetTextures(req.path);
    if (!ShaderUtility.SupportsMaskTex(req.shader) && !req.shader.SupportsRGBMaskTex())
      return;
    this.GetMasks(req.path, req.shader);
  }

  private void CopyData(GraphicRequest req)
  {
    this.data = req.graphicData;
    this.path = req.path;
    this.maskPath = req.maskPath;
    this.color = req.color;
    this.colorTwo = req.colorTwo;
    this.drawSize = req.drawSize;
  }

  protected virtual void GetTextures(string path)
  {
    this.textures = new Texture2D[this.MatCount];
    for (int index = 0; index < this.MatCount; ++index)
      this.textures[index] = ContentFinder<Texture2D>.Get(path + Graphic_Rgb.pathExtensions[index], false);
    if (!Object.op_Implicit((Object) this.textures[0]))
      this.textures[0] = ContentFinder<Texture2D>.Get(path, false);
    if (this.MatCount >= 4)
    {
      if (!Object.op_Implicit((Object) this.textures[0]))
      {
        if (Object.op_Implicit((Object) this.textures[2]))
        {
          this.textures[0] = this.textures[2];
          this.drawRotatedExtraAngleOffset = 180f;
        }
        else if (Object.op_Implicit((Object) this.textures[1]))
        {
          this.textures[0] = this.textures[1];
          this.drawRotatedExtraAngleOffset = -90f;
        }
        else if (Object.op_Implicit((Object) this.textures[3]))
        {
          this.textures[0] = this.textures[3];
          this.drawRotatedExtraAngleOffset = 90f;
        }
      }
      if (!Object.op_Implicit((Object) this.textures[0]))
      {
        Log.Error($"Failed to find any textures at {path} while constructing {Gen.ToStringSafe<Graphic_Rgb>(this)}");
        return;
      }
      if (!Object.op_Implicit((Object) this.textures[2]))
      {
        this.textures[2] = this.textures[0];
        this.southRotated = this.DataAllowsFlip;
      }
      if (!Object.op_Implicit((Object) this.textures[1]))
      {
        if (Object.op_Implicit((Object) this.textures[3]))
        {
          this.textures[1] = this.textures[3];
          this.eastFlipped = this.DataAllowsFlip;
        }
        else
        {
          this.textures[1] = this.textures[0];
          this.eastRotated = this.DataAllowsFlip;
        }
      }
      if (!Object.op_Implicit((Object) this.textures[3]))
      {
        if (Object.op_Implicit((Object) this.textures[1]))
        {
          this.textures[3] = this.textures[1];
          this.westFlipped = this.DataAllowsFlip;
        }
        else
          this.textures[3] = this.textures[0];
      }
    }
    if (this.MatCount != 8)
      return;
    if (!Object.op_Implicit((Object) this.textures[4]))
    {
      this.textures[4] = !Object.op_Implicit((Object) this.textures[7]) ? this.textures[0] : this.textures[7];
      this.eastDiagonalRotated = this.DataAllowsFlip;
    }
    if (!Object.op_Implicit((Object) this.textures[5]))
    {
      this.textures[5] = !Object.op_Implicit((Object) this.textures[6]) ? this.textures[2] : this.textures[6];
      this.eastDiagonalRotated = this.DataAllowsFlip;
    }
    if (!Object.op_Implicit((Object) this.textures[6]))
    {
      this.textures[6] = !Object.op_Implicit((Object) this.textures[5]) ? this.textures[2] : this.textures[5];
      this.westDiagonalRotated = this.DataAllowsFlip;
    }
    if (Object.op_Implicit((Object) this.textures[7]))
      return;
    this.textures[7] = !Object.op_Implicit((Object) this.textures[4]) ? this.textures[0] : this.textures[4];
    this.westDiagonalRotated = this.DataAllowsFlip;
  }

  protected virtual void GetMasks(string path, Shader shader)
  {
    this.patternPointers = new int[8]
    {
      0,
      1,
      2,
      3,
      4,
      5,
      6,
      7
    };
    if (!shader.SupportsRGBMaskTex() && !ShaderUtility.SupportsMaskTex(shader))
      return;
    for (int index = 0; index < this.MatCount; ++index)
    {
      ref Texture2D local = ref this.masks[index];
      if (local == null)
        local = ContentFinder<Texture2D>.Get($"{path}{Graphic_Rgb.pathExtensions[index]}m", false);
    }
    if (!Object.op_Implicit((Object) this.masks[0]))
      this.masks[0] = ContentFinder<Texture2D>.Get(path + Graphic_Single.MaskSuffix, false);
    if (this.MatCount >= 4)
    {
      if (!Object.op_Implicit((Object) this.masks[0]))
      {
        if (Object.op_Implicit((Object) this.masks[2]))
        {
          this.masks[0] = this.masks[2];
          this.patternPointers[0] = 2;
        }
        else if (Object.op_Implicit((Object) this.masks[1]))
        {
          this.masks[0] = this.masks[1];
          this.patternPointers[0] = 1;
        }
        else if (Object.op_Implicit((Object) this.masks[3]))
        {
          this.masks[0] = this.masks[3];
          this.patternPointers[0] = 3;
        }
      }
      if (!Object.op_Implicit((Object) this.masks[0]))
        return;
      if (!Object.op_Implicit((Object) this.masks[2]))
      {
        this.masks[2] = this.masks[0];
        this.patternPointers[2] = 0;
      }
      if (!Object.op_Implicit((Object) this.masks[1]))
      {
        if (Object.op_Implicit((Object) this.masks[3]))
        {
          this.masks[1] = this.masks[3];
          this.patternPointers[1] = 3;
        }
        else
        {
          this.masks[1] = this.masks[0];
          this.patternPointers[1] = 0;
        }
      }
      if (!Object.op_Implicit((Object) this.masks[3]))
      {
        this.masks[3] = this.masks[1];
        this.patternPointers[3] = 1;
      }
    }
    if (this.MatCount != 8)
      return;
    if (!Object.op_Implicit((Object) this.masks[4]))
    {
      this.masks[4] = this.masks[0];
      this.patternPointers[4] = 0;
      this.eastDiagonalRotated = this.DataAllowsFlip;
    }
    if (!Object.op_Implicit((Object) this.masks[5]))
    {
      this.masks[5] = this.masks[2];
      this.patternPointers[5] = 2;
      this.eastDiagonalRotated = this.DataAllowsFlip;
    }
    if (!Object.op_Implicit((Object) this.masks[6]))
    {
      this.masks[6] = this.masks[2];
      this.patternPointers[6] = 2;
      this.westDiagonalRotated = this.DataAllowsFlip;
    }
    if (Object.op_Implicit((Object) this.masks[7]))
      return;
    this.masks[7] = this.masks[0];
    this.patternPointers[7] = 0;
    this.westDiagonalRotated = this.DataAllowsFlip;
  }

  public Vehicles.Rendering.PreRenderResults ParallelGetPreRenderResults(
    [RequiresLocation, In] ref TransformData transformData,
    bool forceDraw = false,
    Thing thing = null,
    float extraRotation = 0.0f)
  {
    Vehicles.Rendering.PreRenderResults preRenderResults = new Vehicles.Rendering.PreRenderResults()
    {
      valid = true,
      draw = true,
      mesh = this.MeshAtFull(transformData.orientation),
      material = this.MatAtFull(transformData.orientation)
    };
    float rotAngle = transformData.orientation.AsRotationAngle + transformData.rotation;
    float rotation = rotAngle + extraRotation;
    this.AdjustAngle(transformData.orientation, ref rotation, ref rotAngle);
    Quaternion quaternion = Quaternion.AngleAxis(rotation, Vector3.up);
    GraphicData data = this.data;
    if (data != null && data.addTopAltitudeBias)
      quaternion = Quaternion.op_Multiply(quaternion, Quaternion.Euler(Vector3.op_Multiply(Vector3.left, 2f)));
    Vector3 position = transformData.position;
    if (thing != null && thing.Spawned)
    {
      AltitudeLayer? altLayerSpawned = this.DataRgb.altLayerSpawned;
      if (altLayerSpawned.HasValue)
      {
        AltitudeLayer valueOrDefault = altLayerSpawned.GetValueOrDefault();
        position.y = Altitudes.AltitudeFor(valueOrDefault);
      }
    }
    Vector3 vector3_1 = this.DrawOffset((Rot4) transformData.orientation);
    Vector3 vector3_2 = Quaternion.op_Multiply(Quaternion.AngleAxis(rotAngle, Vector3.up), vector3_1);
    Vector3 vector3_3 = Vector3.op_Addition(position, vector3_2);
    preRenderResults.position = vector3_3;
    preRenderResults.quaternion = quaternion;
    return preRenderResults;
  }

  private void AdjustAngle(Rot8 orientation, ref float rotation, ref float rotAngle)
  {
    switch (orientation.AsInt)
    {
      case 1:
        if (!this.EastRotated)
          break;
        rotation += orientation.AsAngle;
        break;
      case 2:
        if (!this.SouthRotated)
          break;
        rotation += orientation.AsAngle;
        break;
      case 3:
        if (!base.WestFlipped)
          break;
        if (this.EastRotated)
        {
          rotation += orientation.AsAngle;
          break;
        }
        rotation *= -1f;
        break;
      case 4:
      case 5:
        if (!this.EastDiagonalRotated)
          break;
        rotation *= -1f;
        rotAngle += Rot8.East.AsAngle;
        break;
      case 6:
      case 7:
        if (!this.WestDiagonalRotated)
          break;
        rotation *= -1f;
        rotAngle += Rot8.West.AsAngle;
        break;
    }
  }

  public virtual string ToString()
  {
    return $"{((object) this).GetType()} (initPath={this.path}, color={this.color}, colorTwo={this.colorTwo}, colorThree={this.colorThree})";
  }

  public virtual int GetHashCode()
  {
    return Gen.HashCombineStruct<Color>(Gen.HashCombineStruct<Color>(Gen.HashCombineStruct<Color>(Gen.HashCombine<string>(0, this.path), this.color), this.colorTwo), this.colorThree);
  }
}
