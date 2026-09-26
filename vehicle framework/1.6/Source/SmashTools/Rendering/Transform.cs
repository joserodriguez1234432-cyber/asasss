// Decompiled with JetBrains decompiler
// Type: SmashTools.Rendering.Transform
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using SmashTools.Animations;
using UnityEngine;
using Verse;

#nullable disable
namespace SmashTools.Rendering;

public sealed class Transform : ITweakFields, IExposable
{
  private static readonly Vector3 DefaultScale = Vector3.one;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  [AnimationProperty(Name = "Position")]
  public Vector3 position;
  [TweakField(SettingsType = UISettingsType.SliderFloat)]
  [SliderValues(MinValue = 0.0f, MaxValue = 360f, Increment = 1f, RoundDecimalPlaces = 0)]
  [AnimationProperty(Name = "Rotation")]
  public float rotation;
  [TweakField(SettingsType = UISettingsType.FloatBox)]
  [AnimationProperty(Name = "Scale")]
  public Vector3 scale = Transform.DefaultScale;

  string ITweakFields.Category => (string) null;

  string ITweakFields.Label => nameof (Transform);

  void IExposable.ExposeData()
  {
    Scribe_Values.Look<Vector3>(ref this.position, "position", new Vector3(), false);
    Scribe_Values.Look<float>(ref this.rotation, "rotation", 0.0f, false);
    Scribe_Values.Look<Vector3>(ref this.scale, "scale", Transform.DefaultScale, false);
  }

  public void Reset()
  {
    this.position = Vector3.zero;
    this.rotation = 0.0f;
    this.scale = Transform.DefaultScale;
  }

  void ITweakFields.OnFieldChanged()
  {
  }
}
