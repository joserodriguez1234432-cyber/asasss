// Decompiled with JetBrains decompiler
// Type: Vehicles.StatUpgradeCategoryDef
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;

#nullable disable
namespace Vehicles;

public class StatUpgradeCategoryDef : Def
{
  public ToStringStyle toStringStyle = (ToStringStyle) 2;
  public ToStringNumberSense toStringNumberSense = (ToStringNumberSense) 1;
  [MustTranslate]
  public string formatString;
  public UpgradeEffectType upgradeEffectType;
}
