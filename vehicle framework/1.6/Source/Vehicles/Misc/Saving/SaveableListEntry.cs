// Decompiled with JetBrains decompiler
// Type: Vehicles.SaveableListEntry
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using System.Reflection;
using Verse;

#nullable disable
namespace Vehicles;

public class SaveableListEntry : SaveableField
{
  public int index;

  public SaveableListEntry()
  {
  }

  public SaveableListEntry(VehicleDef def, FieldInfo field, int index)
    : base((Def) def, field)
  {
    this.index = index;
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Values.Look<int>(ref this.index, "index", 0, false);
  }
}
