// Decompiled with JetBrains decompiler
// Type: SmashTools.DynamicDelegateBase
// Assembly: SmashTools, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3B8999A7-6EA8-49BB-AA7F-3E2E898B5249
// Assembly location: D:\Programas\steamapps\common\RimWorld\Mods\3014915404\1.6\Assemblies\SmashTools.dll

using HarmonyLib;
using JetBrains.Annotations;
using SmashTools.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Verse;

#nullable disable
namespace SmashTools;

[PublicAPI]
public abstract class DynamicDelegateBase
{
  internal const string ExactParamsName = "ExactParams";
  public MethodInfo method;
  public object[] args;
  private int runtimeArgs = -1;

  public DynamicDelegateBase()
  {
  }

  protected DynamicDelegateBase(MethodInfo method)
  {
    this.method = method;
    this.RecacheRuntimeArgCount();
    this.RecacheInjectedCount();
    this.LoadDefaultArgs();
  }

  public int InjectedCount { get; private set; }

  public int RuntimeArguments
  {
    get
    {
      if (this.runtimeArgs < 0)
        this.RecacheRuntimeArgCount();
      return this.runtimeArgs;
    }
  }

  private void RecacheRuntimeArgCount()
  {
    Type type = this.GetType();
    this.runtimeArgs = type.IsGenericType ? type.GetGenericArguments().Length : 0;
  }

  private void RecacheInjectedCount()
  {
    this.InjectedCount = 0;
    ParameterInfo[] parameters = this.method.GetParameters();
    for (int runtimeArguments = this.RuntimeArguments; runtimeArguments < parameters.Length && parameters[runtimeArguments].Name.StartsWith("__"); ++runtimeArguments)
      ++this.InjectedCount;
  }

  private void LoadDefaultArgs()
  {
    ParameterInfo[] parameters = this.method.GetParameters();
    this.args = new object[parameters.Length];
    for (int index = this.RuntimeArguments + this.InjectedCount; index < parameters.Length; ++index)
      this.args[index] = parameters[index].ParameterType.GetDefaultValue();
  }

  public void LoadDataFromXmlCustom([NotNull] XmlNode xmlNode)
  {
    string innerText = xmlNode.InnerText;
    string[] enumerable = innerText.Split('(', StringSplitOptions.None);
    if (((IEnumerable<string>) enumerable).NullOrEmpty<string>())
    {
      Log.Error($"Unable to parse {this.GetType().Name} {xmlNode.Name}.\nText={innerText}");
    }
    else
    {
      try
      {
        string[] strArray = enumerable[0].Split('.', StringSplitOptions.None);
        if (strArray.Length < 2)
        {
          Log.Error("Unable to resolve method, too few arguments. Must include at least TypeName.MethodName");
        }
        else
        {
          string str = strArray[strArray.Length - 1];
          this.method = AccessTools.Method(GenTypes.GetTypeInAnyAssembly(string.Join(".", strArray, 0, strArray.Length - 1), (string) null), str, (Type[]) null, (Type[]) null);
          string[] list = enumerable[enumerable.Length - 1].Replace(")", "").Split(',', StringSplitOptions.None);
          ParameterInfo[] parameters = this.method.GetParameters();
          if (list.Length > parameters.Length)
          {
            Log.Error("Number of parameters is less than number of args passed in. Xml=" + innerText);
          }
          else
          {
            bool flag = false;
            XmlAttribute attribute = xmlNode.Attributes["ExactParams"];
            if (attribute != null)
              flag = bool.Parse(attribute.Value.ToLowerInvariant());
            this.RecacheRuntimeArgCount();
            this.RecacheInjectedCount();
            if (flag && list.Length + this.RuntimeArguments != parameters.Length)
              Log.Error("Number of parameters doesn't match number of args passed in. Xml=" + innerText);
            this.LoadDefaultArgs();
            for (int index1 = this.RuntimeArguments + this.InjectedCount; index1 < parameters.Length; ++index1)
            {
              int index2 = index1 - this.RuntimeArguments;
              if (((IList<string>) list).OutOfBounds<string>(index2))
              {
                this.args[index1] = Type.Missing;
              }
              else
              {
                string entry = list[index2];
                this.args[index1] = !(entry.ToUpperInvariant() == "NULL") || !parameters[index1].ParameterType.IsClass && !(Nullable.GetUnderlyingType(parameters[index1].ParameterType) != (Type) null) ? this.ParseArgument(parameters[index1].ParameterType, entry, index1) : (object) null;
              }
            }
          }
        }
      }
      catch (IndexOutOfRangeException ex)
      {
        Log.Error($"Formatting error in {innerText}. Unable to parse into resolved method.");
      }
    }
  }

  private object ParseArgument(Type type, string entry, int index)
  {
    if (Nullable.GetUnderlyingType(type) != (Type) null)
      return entry.NullOrEmpty<char>() ? (object) null : ParseHelper.FromString(entry, type);
    if (ParseHelper.HandlesType(type))
      return ParseHelper.FromString(entry, type);
    if (type.IsSubclassOf(typeof (Def)))
    {
      if (DelayedCrossRefResolver.Resolved)
        return (object) GenDefDatabase.GetDef(type, entry, true);
      DelayedCrossRefResolver.RegisterIndex((Array) this.args, type, entry, index);
      return (object) null;
    }
    Log.ErrorOnce($"Unhandled type {type.Name} in ResolvedMethod arguments.", type.GetHashCode());
    return type.GetDefaultValue();
  }

  public override string ToString()
  {
    if (this.method == (MethodInfo) null)
      return string.Empty;
    string str = $"{GenTypes.GetTypeNameWithoutIgnoredNamespaces(this.method.DeclaringType)}.{this.method.Name}";
    if (!((IEnumerable<object>) this.args).NullOrEmpty<object>())
      str = $"{str}({string.Join(",", ((IEnumerable<object>) this.args).Select<object, string>((Func<object, string>) (obj => obj?.ToString() ?? "NULL")))})";
    return str;
  }

  public string ToStringSignature()
  {
    string stringSignature = this.method.Name;
    if (!((IEnumerable<object>) this.args).NullOrEmpty<object>())
      stringSignature = $"{stringSignature}( {string.Join<Type>(", ", ((IEnumerable<object>) this.args).Select<object, Type>((Func<object, Type>) (obj => obj.GetType())))} )";
    return stringSignature;
  }

  protected void InjectArguments(object[] injectedArgs)
  {
    if (this.InjectedCount <= 0 || ((IEnumerable<object>) injectedArgs).NullOrEmpty<object>())
      return;
    for (int runtimeArguments = this.RuntimeArguments; runtimeArguments < injectedArgs.Length; ++runtimeArguments)
      this.args[runtimeArguments] = injectedArgs[runtimeArguments];
  }
}
