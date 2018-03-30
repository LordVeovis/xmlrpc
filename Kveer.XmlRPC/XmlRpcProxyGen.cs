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

namespace CookComputing.XmlRpc
{
	using System;
	using System.Linq;
	using System.Reflection;
	using System.Reflection.Emit;

	public class XmlRpcProxyGen
	{
		static readonly IDictionary<Type, Type> s_types = new Dictionary<Type,Type>();

#if (!FX1_0)
		public static T Create<T>()
		{
			return (T)Create(typeof(T));
		}
#endif

		public static object Create(Type itf)
		{
			// create transient assembly
			Type proxyType;
			lock (typeof(XmlRpcProxyGen))
			{
				proxyType = s_types[itf];
				if (proxyType == null)
				{
					Guid guid = Guid.NewGuid();
					string assemblyName = "XmlRpcProxy" + guid;
					string typeName = "XmlRpcProxy" + guid;
					AssemblyBuilder assBldr = BuildAssembly(itf, assemblyName,
					   typeName, AssemblyBuilderAccess.Run);
					proxyType = assBldr.GetType(typeName);
					s_types.Add(itf, proxyType);
				}
			}
			var ret = Activator.CreateInstance(proxyType);
			return ret;
		}

		public static object CreateAssembly(
		  Type itf,
		  string typeName,
		  string assemblyName
		  )
		{
			// create persistable assembly
			if (assemblyName.IndexOf(".dll") == (assemblyName.Length - 4))
				assemblyName = assemblyName.Substring(0, assemblyName.Length - 4);
			AssemblyBuilder assBldr = BuildAssembly(itf, assemblyName,
			   typeName, AssemblyBuilderAccess.Run);
			Type proxyType = assBldr.GetType(typeName);
			object ret = Activator.CreateInstance(proxyType);
			//assBldr.Save(moduleName);
			return ret;
		}

		private static AssemblyBuilder BuildAssembly(
		  Type itf,
		  string assemblyName,
		  string typeName,
		  AssemblyBuilderAccess access)
		{
			string urlString = GetXmlRpcUrl(itf);
			var methods = GetXmlRpcMethods(itf);
			var beginMethods = GetXmlRpcBeginMethods(itf);
			var endMethods = GetXmlRpcEndMethods(itf);
			AssemblyName assName = new AssemblyName { Name = assemblyName };
			if (access == AssemblyBuilderAccess.Run)
				assName.Version = itf.Assembly.GetName().Version;
			AssemblyBuilder assBldr = AssemblyBuilder.DefineDynamicAssembly(assName, access);
			ModuleBuilder modBldr = assBldr.DefineDynamicModule(assName.Name);
			TypeBuilder typeBldr = modBldr.DefineType(
			  typeName,
			  TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public,
			  typeof(XmlRpcClientProtocol),
			  new [] { itf });
			BuildConstructor(typeBldr, typeof(XmlRpcClientProtocol), urlString);
			BuildMethods(typeBldr, methods);
			BuildBeginMethods(typeBldr, beginMethods);
			BuildEndMethods(typeBldr, endMethods);
			typeBldr.CreateType();
			return assBldr;
		}

		private static void BuildMethods(TypeBuilder tb, IEnumerable<MethodData> methods)
		{
			foreach (MethodData mthdData in methods)
			{
				MethodInfo mi = mthdData.mi;
				Type[] argTypes = new Type[mi.GetParameters().Length];
				string[] paramNames = new string[mi.GetParameters().Length];
				for (int i = 0; i < mi.GetParameters().Length; i++)
				{
					argTypes[i] = mi.GetParameters()[i].ParameterType;
					paramNames[i] = mi.GetParameters()[i].Name;
				}
				XmlRpcMethodAttribute mattr = (XmlRpcMethodAttribute)
				  Attribute.GetCustomAttribute(mi, typeof(XmlRpcMethodAttribute));
				BuildMethod(tb, mi.Name, mthdData.xmlRpcName, paramNames, argTypes,
				  mthdData.paramsMethod, mi.ReturnType, mattr.StructParams);
			}
		}

		static void BuildMethod(
		  TypeBuilder tb,
		  string methodName,
		  string rpcMethodName,
		  IReadOnlyList<string> paramNames,
		  Type[] argTypes,
		  bool paramsMethod,
		  Type returnType,
		  bool structParams)
		{
			var mthdBldr = tb.DefineMethod(
			  methodName,
			  MethodAttributes.Public | MethodAttributes.Virtual,
			  returnType, argTypes);
			// add attribute to method
			Type[] oneString = new Type[1] { typeof(string) };
			Type methodAttr = typeof(XmlRpcMethodAttribute);
			ConstructorInfo ci = methodAttr.GetConstructor(oneString);
			var pis
			  = new[] { methodAttr.GetProperty("StructParams") };
			var structParam = new object[] { structParams };
			CustomAttributeBuilder cab =
			  new CustomAttributeBuilder(ci, new object[] { rpcMethodName },
				pis, structParam);
			mthdBldr.SetCustomAttribute(cab);
			for (int i = 0; i < paramNames.Count; i++)
			{
				ParameterBuilder paramBldr = mthdBldr.DefineParameter(i + 1,
				  ParameterAttributes.In, paramNames[i]);
				// possibly add ParamArrayAttribute to final parameter
				if (i == paramNames.Count - 1 && paramsMethod)
				{
					ConstructorInfo ctorInfo = typeof(ParamArrayAttribute).GetConstructor(
					  new Type[0]);
					CustomAttributeBuilder attrBldr =
					  new CustomAttributeBuilder(ctorInfo, new object[0]);
					paramBldr.SetCustomAttribute(attrBldr);
				}
			}
			// generate IL
			ILGenerator ilgen = mthdBldr.GetILGenerator();
			// if non-void return, declared locals for processing return value
			LocalBuilder retVal = null;
			LocalBuilder tempRetVal = null;
			if (typeof(void) != returnType)
			{
				tempRetVal = ilgen.DeclareLocal(typeof(object));
				retVal = ilgen.DeclareLocal(returnType);
			}
			// declare variable to store method args and emit code to populate ut
			LocalBuilder argValues = ilgen.DeclareLocal(typeof(object[]));
			ilgen.Emit(OpCodes.Ldc_I4, argTypes.Length);
			ilgen.Emit(OpCodes.Newarr, typeof(object));
			ilgen.Emit(OpCodes.Stloc, argValues);
			for (int argLoad = 0; argLoad < argTypes.Length; argLoad++)
			{
				ilgen.Emit(OpCodes.Ldloc, argValues);
				ilgen.Emit(OpCodes.Ldc_I4, argLoad);
				ilgen.Emit(OpCodes.Ldarg, argLoad + 1);
				if (argTypes[argLoad].IsValueType)
				{
					ilgen.Emit(OpCodes.Box, argTypes[argLoad]);
				}
				ilgen.Emit(OpCodes.Stelem_Ref);
			}
			// call Invoke on base class
			var invokeTypes = new [] { typeof(MethodInfo), typeof(object[]) };
			MethodInfo invokeMethod
			  = typeof(XmlRpcClientProtocol).GetMethod("Invoke", invokeTypes);
			ilgen.Emit(OpCodes.Ldarg_0);
			ilgen.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetCurrentMethod"));
			ilgen.Emit(OpCodes.Castclass, typeof(MethodInfo));
			ilgen.Emit(OpCodes.Ldloc, argValues);
			ilgen.Emit(OpCodes.Call, invokeMethod);
			//  if non-void return prepare return value, otherwise pop to discard 
			if (typeof(void) != returnType)
			{
				// if return value is null, don't cast it to required type
				Label retIsNull = ilgen.DefineLabel();
				ilgen.Emit(OpCodes.Stloc, tempRetVal);
				ilgen.Emit(OpCodes.Ldloc, tempRetVal);
				ilgen.Emit(OpCodes.Brfalse, retIsNull);
				ilgen.Emit(OpCodes.Ldloc, tempRetVal);
				if (returnType.IsValueType)
				{
					ilgen.Emit(OpCodes.Unbox, returnType);
					ilgen.Emit(OpCodes.Ldobj, returnType);
				}
				else
				{
					ilgen.Emit(OpCodes.Castclass, returnType);
				}
				ilgen.Emit(OpCodes.Stloc, retVal);
				ilgen.MarkLabel(retIsNull);
				ilgen.Emit(OpCodes.Ldloc, retVal);
			}
			else
			{
				ilgen.Emit(OpCodes.Pop);
			}
			ilgen.Emit(OpCodes.Ret);
		}

		private static void BuildBeginMethods(TypeBuilder tb, IEnumerable<MethodData> methods)
		{
			foreach (MethodData mthdData in methods)
			{
				MethodInfo mi = mthdData.mi;
				// assume method has already been validated for required signature   
				int paramCount = mi.GetParameters().Length;
				// argCount counts of params before optional AsyncCallback param
				int argCount = paramCount;
				Type[] argTypes = new Type[paramCount];
				for (int i = 0; i < mi.GetParameters().Length; i++)
				{
					argTypes[i] = mi.GetParameters()[i].ParameterType;
					if (argTypes[i] == typeof(AsyncCallback))
						argCount = i;
				}
				MethodBuilder mthdBldr = tb.DefineMethod(
				  mi.Name,
				  MethodAttributes.Public | MethodAttributes.Virtual,
				  mi.ReturnType,
				  argTypes);
				// add attribute to method
				var oneString = new [] { typeof(string) };
				Type methodAttr = typeof(XmlRpcBeginAttribute);
				ConstructorInfo ci = methodAttr.GetConstructor(oneString);
				CustomAttributeBuilder cab =
				  new CustomAttributeBuilder(ci, new object[] { mthdData.xmlRpcName });
				mthdBldr.SetCustomAttribute(cab);
				// start generating IL
				ILGenerator ilgen = mthdBldr.GetILGenerator();
				// declare variable to store method args and emit code to populate it
				LocalBuilder argValues = ilgen.DeclareLocal(typeof(object[]));
				ilgen.Emit(OpCodes.Ldc_I4, argCount);
				ilgen.Emit(OpCodes.Newarr, typeof(object));
				ilgen.Emit(OpCodes.Stloc, argValues);
				for (int argLoad = 0; argLoad < argCount; argLoad++)
				{
					ilgen.Emit(OpCodes.Ldloc, argValues);
					ilgen.Emit(OpCodes.Ldc_I4, argLoad);
					ilgen.Emit(OpCodes.Ldarg, argLoad + 1);
					ParameterInfo pi = mi.GetParameters()[argLoad];
					string paramTypeName = pi.ParameterType.AssemblyQualifiedName;
					paramTypeName = paramTypeName.Replace("&", "");
					Type paramType = Type.GetType(paramTypeName);
					if (paramType.IsValueType)
					{
						ilgen.Emit(OpCodes.Box, paramType);
					}
					ilgen.Emit(OpCodes.Stelem_Ref);
				}
				// emit code to store AsyncCallback parameter, defaulting to null 
				// if not in method signature
				LocalBuilder acbValue = ilgen.DeclareLocal(typeof(AsyncCallback));
				if (argCount < paramCount)
				{
					ilgen.Emit(OpCodes.Ldarg, argCount + 1);
					ilgen.Emit(OpCodes.Stloc, acbValue);
				}
				// emit code to store async state parameter, defaulting to null 
				// if not in method signature
				LocalBuilder objValue = ilgen.DeclareLocal(typeof(object));
				if (argCount < (paramCount - 1))
				{
					ilgen.Emit(OpCodes.Ldarg, argCount + 2);
					ilgen.Emit(OpCodes.Stloc, objValue);
				}
				// emit code to call BeginInvoke on base class
				var invokeTypes = new []
			  {
		typeof(MethodInfo),
		typeof(object[]),
		typeof(object),
		typeof(AsyncCallback),
		typeof(object)
			  };
				MethodInfo invokeMethod
				  = typeof(XmlRpcClientProtocol).GetMethod("BeginInvoke", invokeTypes);
				ilgen.Emit(OpCodes.Ldarg_0);
				ilgen.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetCurrentMethod"));
				ilgen.Emit(OpCodes.Castclass, typeof(MethodInfo));
				ilgen.Emit(OpCodes.Ldloc, argValues);
				ilgen.Emit(OpCodes.Ldarg_0);
				ilgen.Emit(OpCodes.Ldloc, acbValue);
				ilgen.Emit(OpCodes.Ldloc, objValue);
				ilgen.Emit(OpCodes.Call, invokeMethod);
				// BeginInvoke will leave IAsyncResult on stack - leave it there
				// for return value from method being built
				ilgen.Emit(OpCodes.Ret);
			}
		}

		private static void BuildEndMethods(TypeBuilder tb, IEnumerable<MethodData> methods)
		{
			LocalBuilder retVal = null;
			LocalBuilder tempRetVal = null;
			foreach (MethodData mthdData in methods)
			{
				var mi = mthdData.mi;
				Type[] argTypes = { typeof(IAsyncResult) };
				var mthdBldr = tb.DefineMethod(mi.Name,
				  MethodAttributes.Public | MethodAttributes.Virtual,
				  mi.ReturnType, argTypes);
				// start generating IL
				var ilgen = mthdBldr.GetILGenerator();
				// if non-void return, declared locals for processing return value
				if (typeof(void) != mi.ReturnType)
				{
					tempRetVal = ilgen.DeclareLocal(typeof(object));
					retVal = ilgen.DeclareLocal(mi.ReturnType);
				}
				// call EndInvoke on base class
				var invokeTypes
				  = new[] { typeof(IAsyncResult), typeof(Type) };
				var invokeMethod
				  = typeof(XmlRpcClientProtocol).GetMethod("EndInvoke", invokeTypes);
				var getTypeTypes
				  = new[] { typeof(string) };
				var getTypeMethod
				  = typeof(Type).GetMethod("GetType", getTypeTypes);
				ilgen.Emit(OpCodes.Ldarg_0);  // "this"
				ilgen.Emit(OpCodes.Ldarg_1);  // IAsyncResult parameter
				ilgen.Emit(OpCodes.Ldstr, mi.ReturnType.AssemblyQualifiedName);
				ilgen.Emit(OpCodes.Call, getTypeMethod);
				ilgen.Emit(OpCodes.Call, invokeMethod);
				//  if non-void return prepare return value otherwise pop to discard 
				if (typeof(void) != mi.ReturnType)
				{
					// if return value is null, don't cast it to required type
					var retIsNull = ilgen.DefineLabel();
					ilgen.Emit(OpCodes.Stloc, tempRetVal);
					ilgen.Emit(OpCodes.Ldloc, tempRetVal);
					ilgen.Emit(OpCodes.Brfalse, retIsNull);
					ilgen.Emit(OpCodes.Ldloc, tempRetVal);
					if (mi.ReturnType.IsValueType)
					{
						ilgen.Emit(OpCodes.Unbox, mi.ReturnType);
						ilgen.Emit(OpCodes.Ldobj, mi.ReturnType);
					}
					else
					{
						ilgen.Emit(OpCodes.Castclass, mi.ReturnType);
					}
					ilgen.Emit(OpCodes.Stloc, retVal);
					ilgen.MarkLabel(retIsNull);
					ilgen.Emit(OpCodes.Ldloc, retVal);
				}
				else
				{
					// void method so throw away result from EndInvoke
					ilgen.Emit(OpCodes.Pop);
				}
				ilgen.Emit(OpCodes.Ret);
			}
		}

		private static void BuildConstructor(
		  TypeBuilder typeBldr,
		  Type baseType,
		  string urlStr)
		{
			var ctorBldr = typeBldr.DefineConstructor(
			  MethodAttributes.Public | MethodAttributes.SpecialName |
			  MethodAttributes.RTSpecialName | MethodAttributes.HideBySig,
			  CallingConventions.Standard,
			  Type.EmptyTypes);
			if (!string.IsNullOrEmpty(urlStr))
			{
				var urlAttr = typeof(XmlRpcUrlAttribute);
				var oneString = new[] { typeof(string) };
				var ci = urlAttr.GetConstructor(oneString);
				var cab =
				  new CustomAttributeBuilder(ci, new object[] { urlStr });
				typeBldr.SetCustomAttribute(cab);
			}
			var ilgen = ctorBldr.GetILGenerator();
			//  Call the base constructor.
			ilgen.Emit(OpCodes.Ldarg_0);
			var ctorInfo = baseType.GetConstructor(Type.EmptyTypes);
			ilgen.Emit(OpCodes.Call, ctorInfo);
			ilgen.Emit(OpCodes.Ret);
		}

		private static string GetXmlRpcUrl(MemberInfo itf)
		{
			var attr = Attribute.GetCustomAttribute(itf,
			  typeof(XmlRpcUrlAttribute));
			if (attr == null)
				return null;
			var xruAttr = attr as XmlRpcUrlAttribute;
			var url = xruAttr.Uri;
			return url;
		}

		/// <summary>
		/// Type.GetMethods() does not return methods that a derived interface
		/// inherits from its base interfaces; this method does.
		/// </summary>
		private static IEnumerable<MethodInfo> GetMethods(Type type)
		{
			var methods = type.GetMethods();
			if (!type.IsInterface)
			{
				return methods;
			}

			var interfaces = type.GetInterfaces();
			if (interfaces.Length == 0)
			{
				return methods;
			}

			var result = (IEnumerable<MethodInfo>) methods;
			foreach (var itf in type.GetInterfaces())
			{
				result = result.Concat(itf.GetMethods());
			}

			return result;
		}

		private static IList<MethodData> GetXmlRpcMethods(Type itf)
		{
			var ret = new List<MethodData>();
			if (!itf.IsInterface)
				throw new Exception("type not interface");
			foreach (MethodInfo mi in GetMethods(itf))
			{
				string xmlRpcName = GetXmlRpcMethodName(mi);
				if (xmlRpcName == null)
					continue;
				ParameterInfo[] pis = mi.GetParameters();
				bool paramsMethod = pis.Length > 0 && Attribute.IsDefined(
				  pis[pis.Length - 1], typeof(ParamArrayAttribute));
				ret.Add(new MethodData(mi, xmlRpcName, paramsMethod));
			}
			return ret;
		}

		private static string GetXmlRpcMethodName(MethodInfo mi)
		{
			var attr = Attribute.GetCustomAttribute(mi,
			  typeof(XmlRpcMethodAttribute));
			if (attr == null)
				return null;
			var xrmAttr = attr as XmlRpcMethodAttribute;
			var rpcMethod = xrmAttr.Method;
			if (rpcMethod == "")
			{
				rpcMethod = mi.Name;
			}
			return rpcMethod;
		}

		private class MethodData
		{
			public MethodData(MethodInfo mi, string xmlRpcName, bool paramsMethod)
			{
				this.mi = mi;
				this.xmlRpcName = xmlRpcName;
				this.paramsMethod = paramsMethod;
			}

			public readonly MethodInfo mi;
			public readonly string xmlRpcName;
			public readonly bool paramsMethod;
		}

		private static IEnumerable<MethodData> GetXmlRpcBeginMethods(Type itf)
		{
			var ret = new List<MethodData>();
			if (!itf.IsInterface)
				throw new Exception("type not interface");
			foreach (MethodInfo mi in itf.GetMethods())
			{
				Attribute attr = Attribute.GetCustomAttribute(mi,
				  typeof(XmlRpcBeginAttribute));
				if (attr == null)
					continue;
				string rpcMethod = ((XmlRpcBeginAttribute)attr).Method;
				if (rpcMethod == "")
				{
					if (!mi.Name.StartsWith("Begin") || mi.Name.Length <= 5)
						throw new Exception(string.Format(
						  "method {0} has invalid signature for begin method",
						  mi.Name));
					rpcMethod = mi.Name.Substring(5);
				}
				int paramCount = mi.GetParameters().Length;
				int i;
				for (i = 0; i < paramCount; i++)
				{
					Type paramType = mi.GetParameters()[0].ParameterType;
					if (paramType == typeof(AsyncCallback))
						break;
				}
				if (paramCount > 1)
				{
					if (i < paramCount - 2)
						throw new Exception(string.Format(
						  "method {0} has invalid signature for begin method", mi.Name));
					if (i == (paramCount - 2))
					{
						Type paramType = mi.GetParameters()[i + 1].ParameterType;
						if (paramType != typeof(Object))
							throw new Exception(string.Format(
							  "method {0} has invalid signature for begin method",
							  mi.Name));
					}
				}
				ret.Add(new MethodData(mi, rpcMethod, false));
			}
			return ret;
		}

		private static IEnumerable<MethodData> GetXmlRpcEndMethods(Type itf)
		{
			var ret = new List<MethodData>();
			if (!itf.IsInterface)
				throw new Exception("type not interface");
			foreach (var mi in itf.GetMethods())
			{
				var attr = Attribute.GetCustomAttribute(mi,
				  typeof(XmlRpcEndAttribute));
				if (attr == null)
					continue;
				var pis = mi.GetParameters();
				if (pis.Length != 1)
					throw new Exception(string.Format(
					  "method {0} has invalid signature for end method", mi.Name));
				var paramType = pis[0].ParameterType;
				if (paramType != typeof(IAsyncResult))
					throw new Exception(string.Format(
					  "method {0} has invalid signature for end method", mi.Name));
				ret.Add(new MethodData(mi, "", false));
			}
			return ret;
		}
	}
}