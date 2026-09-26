// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.MoteThrownSinker
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using UnityEngine;
using Vehicles;
using Verse;

#nullable disable
namespace VehicleMapFramework;

[StaticConstructorOnStartup]
public class MoteThrownSinker : MoteThrown
{
  private Texture texture;
  private Quaternion texRotation;
  private Vector3 texDrawSize;
  private (int, int) texSize;
  private VehiclePawnWithMap vehicle;
  private GraphicOverlay overlay;
  private Color overlayColor;
  private SimpleCurve colorOverlayAlphaCurve;
  private bool disposeOnDespawn;

  public static Material TransparentMaterial { get; private set; }

  public static Material SilhouetteMaterial { get; private set; }

  public static MaterialPropertyBlock MaterialPropertyBlock { get; private set; }

  static MoteThrownSinker()
  {
    LongEventHandler.ExecuteWhenFinished((Action) (() =>
    {
      MoteThrownSinker.TransparentMaterial = MaterialPool.MatFrom(ShaderDatabase.Transparent);
      MoteThrownSinker.SilhouetteMaterial = MaterialPool.MatFrom(ShaderDatabase.Silhouette);
      MoteThrownSinker.MaterialPropertyBlock = new MaterialPropertyBlock();
    }));
  }

  public void SetParameters(
    RenderTexture _texture,
    Quaternion _texRotation,
    Vector3 _texDrawSize,
    Color _overlayColor,
    SimpleCurve _colorOverlayAlphaCurve)
  {
    this.texture = (Texture) _texture;
    this.texRotation = _texRotation;
    this.texDrawSize = _texDrawSize;
    this.overlayColor = _overlayColor;
    this.colorOverlayAlphaCurve = _colorOverlayAlphaCurve;
    this.disposeOnDespawn = true;
  }

  public virtual void SetParameters(
    Texture _texture,
    Quaternion _texRotation,
    Vector3 _texDrawSize,
    (int, int) _texSize,
    VehiclePawnWithMap _vehicle,
    GraphicOverlay _overlay,
    Color _overlayColor,
    SimpleCurve _colorOverlayAlphaCurve)
  {
    this.texture = _texture;
    this.texRotation = _texRotation;
    this.texDrawSize = _texDrawSize;
    this.texSize = _texSize;
    this.vehicle = _vehicle;
    this.overlay = _overlay;
    this.overlayColor = _overlayColor;
    this.colorOverlayAlphaCurve = _colorOverlayAlphaCurve;
  }

  protected virtual void TickInterval(int delta)
  {
    ((Entity) this).TickInterval(delta);
    this.MaintainTexture();
  }

  public virtual void DeSpawn(DestroyMode mode = 0)
  {
    ((Mote) this).DeSpawn(mode);
    if (!this.disposeOnDespawn || !(this.texture is RenderTexture texture))
      return;
    if (texture.IsCreated())
      texture.Release();
    Object.Destroy((Object) texture);
  }

  protected virtual void DrawAt(Vector3 drawLoc, bool flip = false)
  {
    if (this.texture == null)
      return;
    MaterialPropertyBlock materialPropertyBlock = MoteThrownSinker.MaterialPropertyBlock;
    materialPropertyBlock.Clear();
    materialPropertyBlock.SetTexture(AdditionalShaderPropertyIDs.MainTex, this.texture);
    Matrix4x4 matrix4x4 = Matrix4x4.TRS(Vector3Utility.WithY(((Mote) this).exactPosition, Altitudes.AltitudeFor(((BuildableDef) ((Thing) this).def).altitudeLayer) + ((Mote) this).yOffset), this.texRotation, this.texDrawSize);
    materialPropertyBlock.SetColor(ShaderPropertyIDs.Color, GenColor.WithAlpha(Color.white, ((Mote) this).Alpha));
    Graphics.DrawMesh(MeshPool.plane10, matrix4x4, MoteThrownSinker.TransparentMaterial, 0, (Camera) null, 0, MoteThrownSinker.MaterialPropertyBlock);
    materialPropertyBlock.SetColor(ShaderPropertyIDs.Color, GenColor.WithAlpha(this.overlayColor, this.colorOverlayAlphaCurve.Evaluate(((Mote) this).AgeSecs / ((Thing) this).def.mote.Lifespan)));
    Graphics.DrawMesh(MeshPool.plane10, matrix4x4, MoteThrownSinker.SilhouetteMaterial, 0, (Camera) null, 0, MoteThrownSinker.MaterialPropertyBlock);
  }

  private void MaintainTexture()
  {
    if (this.vehicle == null || this.overlay == null)
      return;
    this.texture = VehicleMapUIRenderer.GetOverlayWithVehicleMapTexture(this.vehicle, this.overlay, Rot4.North, this.texSize, CellRect.Empty);
  }
}
