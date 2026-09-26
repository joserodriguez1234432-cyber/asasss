// Decompiled with JetBrains decompiler
// Type: Vehicles.SaveableDefPair`1
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles;

public class SaveableDefPair<K> : SaveableField where K : Def
{
  [Unsaved(false)]
  public K key;
  public string defName;
  public string uniqueKey;
  public LookMode valueLookMode;
  protected System.Type keyType;
  protected System.Type valueType;

  public SaveableDefPair()
  {
  }

  public SaveableDefPair(
    VehicleDef def,
    FieldInfo field,
    string uniqueKey,
    K key,
    LookMode valueLookMode)
    : base((Def) def, field)
  {
    this.key = key;
    this.defName = key.defName;
    this.uniqueKey = uniqueKey;
    this.valueLookMode = valueLookMode;
    this.keyType = typeof (K);
  }

  public override void ResolveReferences()
  {
    this.key = DefDatabase<K>.GetNamed(this.defName, true);
  }

  public override int GetHashCode()
  {
    return Gen.HashCombine<int>(base.GetHashCode(), Gen.HashCombine<string>(0, this.uniqueKey));
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<string>(ref this.uniqueKey, "uniqueKey", (string) null, false);
    Scribe_Values.Look<LookMode>(ref this.valueLookMode, "valueLookMode", (LookMode) 0, false);
    Scribe_Values.Look<string>(ref this.defName, "defName", (string) null, false);
    if (Scribe.mode != 4)
      return;
    this.keyType = typeof (K);
  }
}
