// Decompiled with JetBrains decompiler
// Type: Vehicles.SaveableField
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles;

public class SaveableField : IExposable
{
  public string name;
  public System.Type classType;
  private int defHashCode;

  public SaveableField()
  {
  }

  public SaveableField(Def def, FieldInfo field)
  {
    this.name = field.Name;
    this.classType = field.DeclaringType;
    this.defHashCode = def.GetHashCode();
  }

  public FieldInfo FieldInfo => this.classType.GetCachedField(this.name);

  public virtual void ResolveReferences()
  {
  }

  public static SaveableField SaveableFieldFor(Def def, FieldInfo field)
  {
    return new SaveableField(def, field);
  }

  public static implicit operator FieldInfo(SaveableField field) => field.FieldInfo;

  public override bool Equals(object obj)
  {
    return obj is SaveableField target && SaveableField.Equals(this, target);
  }

  public static bool Equals(SaveableField source, SaveableField target)
  {
    return source.GetHashCode() == target.GetHashCode();
  }

  public override int GetHashCode()
  {
    return Gen.HashCombine<int>(this.defHashCode, Gen.HashCombine<string>(Gen.HashCombine<string>(0, this.classType.FullName), this.name));
  }

  public virtual void ExposeData()
  {
    Scribe_Values.Look<string>(ref this.name, "name", (string) null, false);
    Scribe_Values.Look<System.Type>(ref this.classType, "classType", (System.Type) null, false);
    Scribe_Values.Look<int>(ref this.defHashCode, "defHashCode", -1, false);
  }
}
