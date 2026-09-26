// Decompiled with JetBrains decompiler
// Type: SmashTools.TweakFieldAttribute
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using System;

#nullable disable
namespace SmashTools;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
public class TweakFieldAttribute : Attribute
{
  public string Category { get; set; }

  public string SubCategory { get; set; }

  public UISettingsType SettingsType { get; set; }
}
