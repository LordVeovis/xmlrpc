/* 
XML-RPC.NET library
Copyright (c) 2001-2006, Charles Cook <charlescook@cookcomputing.com>

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, 
and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.
*/

using System.Collections.Generic;
using System.Linq;

namespace CookComputing.XmlRpc
{
	using System;
	using System.Collections;
	using System.Reflection;

	public enum XmlRpcType
	{
		tInvalid,
		tInt32,
		tInt64,
		tBoolean,
		tString,
		tDouble,
		tDateTime,
		tBase64,
		tStruct,
		tHashtable,
		tArray,
		tMultiDimArray,
		tVoid
	}

	public class XmlRpcServiceInfo
	{
		public static XmlRpcServiceInfo CreateServiceInfo(Type type)
		{
			var svcInfo = new XmlRpcServiceInfo();
			// extract service info
			var svcAttr = (XmlRpcServiceAttribute)
			  Attribute.GetCustomAttribute(type, typeof(XmlRpcServiceAttribute));
			if (svcAttr != null && svcAttr.Description != "")
				svcInfo.Doc = svcAttr.Description;
			if (svcAttr != null && svcAttr.Name != "")
				svcInfo.Name = svcAttr.Name;
			else
				svcInfo.Name = type.Name;
			// extract method info
			var methods = new Hashtable();

			foreach (var itf in type.GetInterfaces())
			{
				var itfAttr = (XmlRpcServiceAttribute)
				  Attribute.GetCustomAttribute(itf, typeof(XmlRpcServiceAttribute));
				if (itfAttr != null)
					svcInfo.Doc = itfAttr.Description;
#if (!COMPACT_FRAMEWORK)
				var imap = type.GetInterfaceMap(itf);
				foreach (var mi in imap.InterfaceMethods)
				{
					ExtractMethodInfo(methods, mi, itf);
				}
#else
        foreach (MethodInfo mi in itf.GetMethods())
        {
          ExtractMethodInfo(methods, mi, itf);
        }
#endif
			}

			foreach (var mi in type.GetMethods())
			{
				var mthds = new ArrayList
				{
					mi
				};
				var curMi = mi;
				while (true)
				{
					var baseMi = curMi.GetBaseDefinition();
					if (baseMi.DeclaringType == curMi.DeclaringType)
						break;
					mthds.Insert(0, baseMi);
					curMi = baseMi;
				}
				foreach (MethodInfo mthd in mthds)
				{
					ExtractMethodInfo(methods, mthd, type);
				}
			}
			svcInfo.Methods = new XmlRpcMethodInfo[methods.Count];
			methods.Values.CopyTo(svcInfo.Methods, 0);
			Array.Sort(svcInfo.Methods);
			return svcInfo;
		}

		private static void ExtractMethodInfo(Hashtable methods, MethodInfo mi, Type type)
		{
			var attr = (XmlRpcMethodAttribute)
			  Attribute.GetCustomAttribute(mi,
			  typeof(XmlRpcMethodAttribute));
			if (attr == null)
				return;
			var mthdInfo = new XmlRpcMethodInfo
			{
				MethodInfo = mi,
				XmlRpcName = GetXmlRpcMethodName(mi),
				MiName = mi.Name,
				Doc = attr.Description,
				IsHidden = attr.IntrospectionMethod | attr.Hidden
			};
			// extract parameters information
			var parmList = new List<XmlRpcParameterInfo>();
			var parms = mi.GetParameters();
			foreach (var parm in parms)
			{
				var parmInfo = new XmlRpcParameterInfo
				{
					Name = parm.Name,
					Type = parm.ParameterType,
					XmlRpcType = GetXmlRpcTypeString(parm.ParameterType),
					// retrieve optional attributed info
					Doc = ""
				};
				var pattr = (XmlRpcParameterAttribute)
				  Attribute.GetCustomAttribute(parm,
				  typeof(XmlRpcParameterAttribute));
				if (pattr != null)
				{
					parmInfo.Doc = pattr.Description;
					parmInfo.XmlRpcName = pattr.Name;
				}
				parmInfo.IsParams = Attribute.IsDefined(parm,
				  typeof(ParamArrayAttribute));
				parmList.Add(parmInfo);
			}
			mthdInfo.Parameters = parmList.ToArray();
			// extract return type information
			mthdInfo.ReturnType = mi.ReturnType;
			mthdInfo.ReturnXmlRpcType = GetXmlRpcTypeString(mi.ReturnType);
			var orattrs = mi.ReturnTypeCustomAttributes.GetCustomAttributes(
			  typeof(XmlRpcReturnValueAttribute), false);
			if (orattrs.Length > 0)
			{
				mthdInfo.ReturnDoc = ((XmlRpcReturnValueAttribute)orattrs[0]).Description;
			}

			if (methods[mthdInfo.XmlRpcName] != null)
			{
				throw new XmlRpcDupXmlRpcMethodNames(string.Format("Method "
				  + "{0} in type {1} has duplicate XmlRpc method name {2}",
				  mi.Name, type.Name, mthdInfo.XmlRpcName));
			}
			else
				methods.Add(mthdInfo.XmlRpcName, mthdInfo);
		}

		public MethodInfo GetMethodInfo(string xmlRpcMethodName)
		{
			return (from xmi in Methods where xmlRpcMethodName == xmi.XmlRpcName select xmi.MethodInfo).FirstOrDefault();
		}

		public static string GetXmlRpcMethodName(MethodInfo mi)
		{
			var attr = (XmlRpcMethodAttribute)
			  Attribute.GetCustomAttribute(mi,
			  typeof(XmlRpcMethodAttribute));
			// ReSharper disable once PossibleNullReferenceException
			return !string.IsNullOrEmpty(attr?.Method) ? attr.Method : mi.Name;
		}

		public string GetMethodName(string xmlRpcMethodName)
		{
			return Methods
				   .Where(mi => mi.XmlRpcName == xmlRpcMethodName)
				   .Select(mi => mi.MiName)
				   .FirstOrDefault();
		}

		public string Doc { get; set; }

		public string Name { get; set; }

		public XmlRpcMethodInfo[] Methods { get; private set; }

		public XmlRpcMethodInfo GetMethod(
		  string methodName)
		{
			return Methods.FirstOrDefault(mthdInfo => mthdInfo.XmlRpcName == methodName);
		}

		private XmlRpcServiceInfo()
		{
		}

		public static XmlRpcType GetXmlRpcType(Type t)
		{
			return GetXmlRpcType(t, new Stack());
		}

		private static XmlRpcType GetXmlRpcType(Type t, Stack typeStack)
		{
			XmlRpcType ret;
			if (t == typeof(int))
				ret = XmlRpcType.tInt32;
			else if (t == typeof(XmlRpcInt))
				ret = XmlRpcType.tInt32;
			else if (t == typeof(long))
				ret = XmlRpcType.tInt64;
			else if (t == typeof(bool))
				ret = XmlRpcType.tBoolean;
			else if (t == typeof(XmlRpcBoolean))
				ret = XmlRpcType.tBoolean;
			else if (t == typeof(string))
				ret = XmlRpcType.tString;
			else if (t == typeof(double))
				ret = XmlRpcType.tDouble;
			else if (t == typeof(XmlRpcDouble))
				ret = XmlRpcType.tDouble;
			else if (t == typeof(DateTime))
				ret = XmlRpcType.tDateTime;
			else if (t == typeof(XmlRpcDateTime))
				ret = XmlRpcType.tDateTime;
			else if (t == typeof(byte[]))
				ret = XmlRpcType.tBase64;
			else if (t == typeof(XmlRpcStruct))
			{
				ret = XmlRpcType.tHashtable;
			}
			else if (t == typeof(Array))
				ret = XmlRpcType.tArray;
			else if (t.IsArray)
			{
#if (!COMPACT_FRAMEWORK)
				var elemType = t.GetElementType();
				if (elemType != typeof(object)
				  && GetXmlRpcType(elemType, typeStack) == XmlRpcType.tInvalid)
				{
					ret = XmlRpcType.tInvalid;
				}
				else
				{
					// single dim array
					ret = t.GetArrayRank() == 1 ? XmlRpcType.tArray : XmlRpcType.tMultiDimArray;
				}
#else
        //!! check types of array elements if not Object[]
        Type elemType = null;
        string[] checkSingleDim = Regex.Split(t.FullName, "\\[\\]$");
        if (checkSingleDim.Length > 1)  // single dim array
        {
          elemType = Type.GetType(checkSingleDim[0]);
          ret = XmlRpcType.tArray;
        }
        else
        {
          string[] checkMultiDim = Regex.Split(t.FullName, "\\[,[,]*\\]$");
          if (checkMultiDim.Length > 1)
          {
            elemType = Type.GetType(checkMultiDim[0]);
            ret = XmlRpcType.tMultiDimArray;
          }
          else
            ret = XmlRpcType.tInvalid;
        }
        if (elemType != null)
        {
          if (elemType != typeof(Object) 
            && GetXmlRpcType(elemType, typeStack) == XmlRpcType.tInvalid)
          {
            ret = XmlRpcType.tInvalid;
          }
        }
#endif

			}
#if !FX1_0
			else if (t == typeof(int?))
				ret = XmlRpcType.tInt32;
			else if (t == typeof(long?))
				ret = XmlRpcType.tInt64;
			else if (t == typeof(bool?))
				ret = XmlRpcType.tBoolean;
			else if (t == typeof(double?))
				ret = XmlRpcType.tDouble;
			else if (t == typeof(DateTime?))
				ret = XmlRpcType.tDateTime;
#endif
			else if (t == typeof(void))
			{
				ret = XmlRpcType.tVoid;
			}
			else if ((t.IsValueType && !t.IsPrimitive && !t.IsEnum)
			  || t.IsClass)
			{
				// if type is struct or class its only valid for XML-RPC mapping if all 
				// its members have a valid mapping or are of type object which
				// maps to any XML-RPC type
				var mis = t.GetMembers();
				foreach (var mi in mis)
				{
					if (mi.MemberType == MemberTypes.Field)
					{
						var fi = (FieldInfo)mi;
						if (typeStack.Contains(fi.FieldType))
							continue;
						try
						{
							typeStack.Push(fi.FieldType);
							if ((fi.FieldType != typeof(object)
							  && GetXmlRpcType(fi.FieldType, typeStack) == XmlRpcType.tInvalid))
							{
								return XmlRpcType.tInvalid;
							}
						}
						finally
						{
							typeStack.Pop();
						}
					}
					else if (mi.MemberType == MemberTypes.Property)
					{
						PropertyInfo pi = (PropertyInfo)mi;
						if (typeStack.Contains(pi.PropertyType))
							continue;
						try
						{
							typeStack.Push(pi.PropertyType);
							if ((pi.PropertyType != typeof(Object)
							  && GetXmlRpcType(pi.PropertyType, typeStack) == XmlRpcType.tInvalid))
							{
								return XmlRpcType.tInvalid;
							}
						}
						finally
						{
							typeStack.Pop();
						}
					}
				}
				ret = XmlRpcType.tStruct;
			}
			else
				ret = XmlRpcType.tInvalid;
			return ret;
		}

		public static string GetXmlRpcTypeString(Type t)
		{
			var rpcType = GetXmlRpcType(t);
			return GetXmlRpcTypeString(rpcType);
		}

		public static string GetXmlRpcTypeString(XmlRpcType t)
		{
			string ret;
			switch (t) {
				case XmlRpcType.tInt32:
					ret = "integer";
					break;
				case XmlRpcType.tInt64:
					ret = "i8";
					break;
				case XmlRpcType.tBoolean:
					ret = "boolean";
					break;
				case XmlRpcType.tString:
					ret = "string";
					break;
				case XmlRpcType.tDouble:
					ret = "double";
					break;
				case XmlRpcType.tDateTime:
					ret = "dateTime";
					break;
				case XmlRpcType.tBase64:
					ret = "base64";
					break;
				case XmlRpcType.tStruct:
					ret = "struct";
					break;
				case XmlRpcType.tHashtable:
					ret = "struct";
					break;
				case XmlRpcType.tArray:
					ret = "array";
					break;
				case XmlRpcType.tMultiDimArray:
					ret = "array";
					break;
				case XmlRpcType.tVoid:
					ret = "void";
					break;
				default:
					ret = null;
					break;
			}
			return ret;
		}
	}
}