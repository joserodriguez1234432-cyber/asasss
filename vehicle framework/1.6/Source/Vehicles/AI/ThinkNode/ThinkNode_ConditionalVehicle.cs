// Decompiled with JetBrains decompiler
// Type: Vehicles.ThinkNode_ConditionalVehicle
// Assembly: Vehicles, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 81BFDC99-D8AD-42E1-8470-2F73E7836B4A
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\Vehicles.dll

using Verse;
using Verse.AI;

#nullable disable
namespace Vehicles;

public class ThinkNode_ConditionalVehicle : ThinkNode_Conditional
{
  protected virtual bool Satisfied(Pawn pawn) => pawn is VehiclePawn;
}
