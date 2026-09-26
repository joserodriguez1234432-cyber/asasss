// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.CellRectUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

#nullable disable
namespace VehicleMapFramework;

public static class CellRectUtility
{
  public static CellRectUtility.CellRectReversible get_Reverse(CellRect cellRect)
  {
    return new CellRectUtility.CellRectReversible(cellRect, true);
  }

  public static CellRectUtility.CellRectReversible EdgeRectClockwise(
    this CellRect cellRect,
    Rot4 rot)
  {
    CellRect edgeRect = ((CellRect) ref cellRect).GetEdgeRect(rot);
    bool flag;
    switch (((Rot4) ref rot).AsInt)
    {
      case 1:
      case 2:
        flag = true;
        break;
      default:
        flag = false;
        break;
    }
    return !flag ? new CellRectUtility.CellRectReversible(edgeRect) : new CellRectUtility.CellRectReversible(edgeRect, true);
  }

  public readonly struct CellRectReversible(CellRect cellRect, bool reverse = false) : 
    IEnumerable<IntVec3>,
    IEnumerable
  {
    public CellRect InnerRect => cellRect;

    public CellRectUtility.CellRectReversible.Enumerator GetEnumerator()
    {
      return new CellRectUtility.CellRectReversible.Enumerator(cellRect, reverse);
    }

    IEnumerator<IntVec3> IEnumerable<IntVec3>.GetEnumerator()
    {
      return (IEnumerator<IntVec3>) this.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();

    public struct Enumerator : IEnumerator<IntVec3>, IDisposable, IEnumerator
    {
      private int x;
      private int z;

      public Enumerator(CellRect ir, bool reverse = false)
      {
        // ISSUE: reference to a compiler-generated field
        this.\u003Cir\u003EP = ir;
        // ISSUE: reference to a compiler-generated field
        this.\u003Creverse\u003EP = reverse;
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        this.x = this.\u003Creverse\u003EP ? this.\u003Cir\u003EP.maxX + 1 : this.\u003Cir\u003EP.minX - 1;
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        this.z = this.\u003Creverse\u003EP ? this.\u003Cir\u003EP.maxZ : this.\u003Cir\u003EP.minZ;
      }

      public IntVec3 Current => new IntVec3(this.x, 0, this.z);

      object IEnumerator.Current => (object) new IntVec3(this.x, 0, this.z);

      public bool MoveNext()
      {
        // ISSUE: reference to a compiler-generated field
        if (this.\u003Creverse\u003EP)
        {
          --this.x;
          // ISSUE: reference to a compiler-generated field
          if (this.x < this.\u003Cir\u003EP.minX)
          {
            // ISSUE: reference to a compiler-generated field
            this.x = this.\u003Cir\u003EP.maxX;
            --this.z;
          }
          // ISSUE: reference to a compiler-generated field
          return this.z >= this.\u003Cir\u003EP.minZ;
        }
        ++this.x;
        // ISSUE: reference to a compiler-generated field
        if (this.x > this.\u003Cir\u003EP.maxX)
        {
          // ISSUE: reference to a compiler-generated field
          this.x = this.\u003Cir\u003EP.minX;
          ++this.z;
        }
        // ISSUE: reference to a compiler-generated field
        return this.z <= this.\u003Cir\u003EP.maxZ;
      }

      public void Reset()
      {
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        this.x = this.\u003Creverse\u003EP ? this.\u003Cir\u003EP.maxX + 1 : this.\u003Cir\u003EP.minX - 1;
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        this.z = this.\u003Creverse\u003EP ? this.\u003Cir\u003EP.maxZ : this.\u003Cir\u003EP.minZ;
      }

      void IDisposable.Dispose()
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024155C14DD93420B3FB201F6D830124341
  {
    [ExtensionMarker("<M>$B500517AA1540DCCA341AA5DA8B78058")]
    public CellRectUtility.CellRectReversible Reverse
    {
      [ExtensionMarker("<M>$B500517AA1540DCCA341AA5DA8B78058")] get
      {
        throw new NotSupportedException();
      }
    }

    [ExtensionMarker("<M>$B500517AA1540DCCA341AA5DA8B78058")]
    public CellRectUtility.CellRectReversible EdgeRectClockwise(Rot4 rot)
    {
      throw new NotSupportedException();
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024B500517AA1540DCCA341AA5DA8B78058
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024(CellRect cellRect)
      {
      }
    }
  }
}
