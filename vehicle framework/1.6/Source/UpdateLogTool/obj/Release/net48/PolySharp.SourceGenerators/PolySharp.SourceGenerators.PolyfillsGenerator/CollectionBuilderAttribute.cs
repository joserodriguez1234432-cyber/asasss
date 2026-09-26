// Decompiled with JetBrains decompiler
// Type: System.Runtime.CompilerServices.CollectionBuilderAttribute
// Assembly: UpdateLogTool, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C0B0D1B9-DF61-4E60-8ED1-86C03A6FDDFD
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\UpdateLogTool.dll

using System.Diagnostics.CodeAnalysis;

#nullable enable
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
[ExcludeFromCodeCoverage]
internal sealed class CollectionBuilderAttribute : Attribute
{
  public CollectionBuilderAttribute(Type builderType, string methodName)
  {
    this.BuilderType = builderType;
    this.MethodName = methodName;
  }

  public Type BuilderType { get; }

  public string MethodName { get; }
}
