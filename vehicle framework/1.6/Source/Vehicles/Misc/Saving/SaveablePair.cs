// Decompiled with JetBrains decompiler
// Type: Vehicles.SaveablePair`2
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Collections.Generic;
using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles;

public class SaveablePair<K, V> : SaveableField
{
  public K key;
  public V value;
  public string uniqueKey;
  public LookMode keyLookMode;
  public LookMode valueLookMode;
  protected System.Type keyType;
  protected System.Type valueType;

  public SaveablePair()
  {
  }

  public SaveablePair(
    VehicleDef def,
    FieldInfo field,
    string uniqueKey,
    KeyValuePair<K, V> keyValuePair,
    LookMode keyLookMode,
    LookMode valueLookMode)
    : base((Def) def, field)
  {
    if (keyLookMode == 4 || valueLookMode == 4)
    {
      Log.Warning("Cannot use LookMode.Def for SaveablePair. Would require resolving of Def post-loading. Consider using SaveableDefPair");
    }
    else
    {
      this.key = keyValuePair.Key;
      this.value = keyValuePair.Value;
      this.uniqueKey = uniqueKey;
      this.keyLookMode = keyLookMode;
      this.valueLookMode = valueLookMode;
      this.keyType = typeof (K);
      this.valueType = typeof (V);
    }
  }

  public override int GetHashCode()
  {
    return Gen.HashCombine<int>(base.GetHashCode(), Gen.HashCombine<string>(0, this.uniqueKey));
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<string>(ref this.uniqueKey, "uniqueKey", (string) null, false);
    Scribe_Values.Look<LookMode>(ref this.keyLookMode, "keyLookMode", (LookMode) 0, false);
    Scribe_Values.Look<LookMode>(ref this.valueLookMode, "valueLookMode", (LookMode) 0, false);
    if (this.keyLookMode == 4 || this.valueLookMode == 4)
    {
      Log.Error("Cannot save LookMode.Def for SaveablePair. Would require resolving of Def post-loading. Consider using SaveableDefPair");
    }
    else
    {
      Scribe_Universal.Look<K>(ref this.key, "key", this.keyLookMode, ref this.keyType);
      Scribe_Universal.Look<V>(ref this.value, "value", this.valueLookMode, ref this.valueType);
      if (Scribe.mode != 4)
        return;
      this.keyType = typeof (K);
      this.valueType = typeof (V);
    }
  }
}
