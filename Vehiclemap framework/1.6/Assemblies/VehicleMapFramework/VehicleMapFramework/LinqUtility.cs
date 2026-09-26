// Decompiled with JetBrains decompiler
// Type: VehicleMapFramework.LinqUtility
// Assembly: VehicleMapFramework, Version=1.6.562.0, Culture=neutral, PublicKeyToken=null
// MVID: 10A61882-945F-4CFC-9B06-CA8EEF5ADB36
// Assembly location: D:\Programas\steamapps\workshop\content\294100\3426502333\1.6\Assemblies\VehicleMapFramework.dll

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

#nullable disable
namespace VehicleMapFramework;

public static class LinqUtility
{
  public static IEnumerable<T> get_NonNull<T>([ItemCanBeNull] IEnumerable<T> enumerable) where T : class
  {
    return enumerable.Where<T>((Func<T, bool>) (x => (object) x != null));
  }

  public static IEnumerable<T> get_NonNull<T>([ItemCanBeNull] IEnumerable<T?> enumerable) where T : struct
  {
    return enumerable.OfType<T>();
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u00241BD4FEAF00BE0B98A77E2F634853CD38<\u0024T0> where \u0024T0 : class
  {
    [ExtensionMarker("<M>$848F81E28E81814754C308C036E1606C")]
    [ItemNotNull]
    public IEnumerable<\u0024T0> NonNull
    {
      [ExtensionMarker("<M>$848F81E28E81814754C308C036E1606C")] get
      {
        throw new NotSupportedException();
      }
    }

    [SpecialName]
    public static class \u003CM\u003E\u0024848F81E28E81814754C308C036E1606C
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024([ItemCanBeNull] IEnumerable<T> enumerable)
      {
      }
    }
  }

  [SpecialName]
  public sealed class \u003CG\u003E\u0024B8B443F2F4C405CA4A4DCB7A189C80F8<\u0024T0> where \u0024T0 : struct
  {
    [ExtensionMarker("<M>$3AD07B2C3D9FA07B172F14E79CD57F5F")]
    public IEnumerable<\u0024T0> NonNull
    {
      [ExtensionMarker("<M>$3AD07B2C3D9FA07B172F14E79CD57F5F")] get
      {
        throw new NotSupportedException();
      }
    }

    [SpecialName]
    public static class \u003CM\u003E\u00243AD07B2C3D9FA07B172F14E79CD57F5F
    {
      [CompilerGenerated]
      [SpecialName]
      public static void \u003CExtension\u003E\u0024([ItemCanBeNull] IEnumerable<T?> enumerable)
      {
      }
    }
  }
}
