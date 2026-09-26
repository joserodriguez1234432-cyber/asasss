// Decompiled with JetBrains decompiler
// Type: SmashTools.AnimatorObject
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System.Reflection;

#nullable disable
namespace SmashTools;

public class AnimatorObject
{
  public object parent;
  public FieldInfo fieldInfo;
  public string category;
  public string prefix;

  public AnimatorObject(object parent, FieldInfo fieldInfo, string category, string prefix)
  {
    this.parent = parent;
    this.fieldInfo = fieldInfo;
    this.category = category;
    this.prefix = prefix;
  }

  public string DisplayName
  {
    get
    {
      return !this.prefix.NullOrEmpty<char>() ? $"{this.prefix}.{this.fieldInfo.Name}" : this.fieldInfo.Name;
    }
  }

  public LinearCurve Curve
  {
    get
    {
      return this.parent == null ? (LinearCurve) null : this.fieldInfo.GetValue(this.parent) as LinearCurve;
    }
  }

  public void SetCurve(LinearCurve curve) => this.fieldInfo.SetValue(this.parent, (object) curve);
}
