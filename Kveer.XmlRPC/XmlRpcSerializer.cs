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

// TODO: overriding default mapping action in a struct should not affect nested structs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace CookComputing.XmlRpc
{
	internal struct Fault
	{
		public int FaultCode;
		public string FaultString;
	}

	public class XmlRpcSerializer
	{
		// public properties

		public int Indentation { get; set; } = 2;

		public XmlRpcNonStandard NonStandard { get; set; } = XmlRpcNonStandard.None;

		public bool UseEmptyParamsTag { get; set; } = true;

		public bool UseIndentation { get; set; } = true;

		public bool UseIntTag { get; set; }

		public bool UseStringTag { get; set; } = true;

		public Encoding XmlEncoding { get; set; }

		// private properties
		private bool AllowInvalidHttpContent => (NonStandard & XmlRpcNonStandard.AllowInvalidHttpContent) != 0;

		private bool AllowStringFaultCode => (NonStandard & XmlRpcNonStandard.AllowStringFaultCode) != 0;

		private bool IgnoreDuplicateMembers => (NonStandard & XmlRpcNonStandard.IgnoreDuplicateMembers) != 0;

		private bool MapEmptyDateTimeToMinValue => (NonStandard & XmlRpcNonStandard.MapEmptyDateTimeToMinValue) != 0;

		private bool MapZerosDateTimeToMinValue => (NonStandard & XmlRpcNonStandard.MapZerosDateTimeToMinValue) != 0;

		public void SerializeRequest(Stream stm, XmlRpcRequest request)
		{
			var xtw = new XmlTextWriter(stm, XmlEncoding);
			ConfigureXmlFormat(xtw);
			xtw.WriteStartDocument();
			xtw.WriteStartElement("", "methodCall", "");
			{
				// TODO: use global action setting
				const MappingAction mappingAction = MappingAction.Error;
				xtw.WriteElementString("methodName", request.xmlRpcMethod ?? request.method);
				if (request.args.Length > 0 || UseEmptyParamsTag)
				{
					xtw.WriteStartElement("", "params", "");
					try
					{
						if (!IsStructParamsMethod(request.mi))
							SerializeParams(xtw, request, mappingAction);
						else
							SerializeStructParams(xtw, request, mappingAction);
					}
					catch (XmlRpcUnsupportedTypeException ex)
					{
						throw new XmlRpcUnsupportedTypeException(ex.UnsupportedType,
																 string.Format("A parameter is of, or contains an instance of, "
																			   + "type {0} which cannot be mapped to an XML-RPC type",
																			   ex.UnsupportedType));
					}

					xtw.WriteEndElement();
				}
			}
			xtw.WriteEndElement();
			xtw.Flush();
		}

		private void SerializeParams(XmlTextWriter xtw, XmlRpcRequest request,
									 MappingAction mappingAction)
		{
			ParameterInfo[] pis = null;
			if (request.mi != null) pis = request.mi.GetParameters();
			for (var i = 0; i < request.args.Length; i++)
			{
				if (pis != null)
				{
					if (i >= pis.Length)
						throw new XmlRpcInvalidParametersException("Number of request "
																   + "parameters greater than number of proxy method parameters.");
					if (i == pis.Length - 1
						&& Attribute.IsDefined(pis[i], typeof(ParamArrayAttribute)))
					{
						var ary = (Array)request.args[i];
						foreach (var o in ary)
						{
							if (o == null)
								throw new XmlRpcNullParameterException(
									"Null parameter in params array");
							xtw.WriteStartElement("", "param", "");
							Serialize(xtw, o, mappingAction);
							xtw.WriteEndElement();
						}

						break;
					}
				}

				if (request.args[i] == null)
					throw new XmlRpcNullParameterException(string.Format(
															   "Null method parameter #{0}", i + 1));
				xtw.WriteStartElement("", "param", "");
				Serialize(xtw, request.args[i], mappingAction);
				xtw.WriteEndElement();
			}
		}

		private void SerializeStructParams(XmlTextWriter xtw, XmlRpcRequest request,
										   MappingAction mappingAction)
		{
			var pis = request.mi.GetParameters();
			if (request.args.Length > pis.Length)
				throw new XmlRpcInvalidParametersException("Number of request "
														   + "parameters greater than number of proxy method parameters.");
			if (Attribute.IsDefined(pis[request.args.Length - 1],
									typeof(ParamArrayAttribute)))
				throw new XmlRpcInvalidParametersException("params parameter cannot "
														   + "be used with StructParams.");
			xtw.WriteStartElement("", "param", "");
			xtw.WriteStartElement("", "value", "");
			xtw.WriteStartElement("", "struct", "");
			for (var i = 0; i < request.args.Length; i++)
			{
				if (request.args[i] == null)
					throw new XmlRpcNullParameterException(string.Format(
															   "Null method parameter #{0}", i + 1));
				xtw.WriteStartElement("", "member", "");
				xtw.WriteElementString("name", pis[i].Name);
				Serialize(xtw, request.args[i], mappingAction);
				xtw.WriteEndElement();
			}

			xtw.WriteEndElement();
			xtw.WriteEndElement();
			xtw.WriteEndElement();
		}

		public XmlRpcResponse DeserializeResponse(Stream stm, Type svcType)
		{
			if (stm == null)
				throw new ArgumentNullException(nameof(stm),
												"XmlRpcSerializer.DeserializeResponse");
			if (AllowInvalidHttpContent)
			{
				Stream newStm = new MemoryStream();
				Util.CopyStream(stm, newStm);
				stm = newStm;
				stm.Position = 0;
				while (true)
				{
					// for now just strip off any leading CR-LF characters
					var byt = stm.ReadByte();
					if (byt == -1)
						throw new XmlRpcIllFormedXmlException(
							"Response from server does not contain valid XML.");
					if (byt != 0x0d && byt != 0x0a && byt != ' ' && byt != '\t')
					{
						stm.Position = stm.Position - 1;
						break;
					}
				}
			}

			var xdoc = new XmlDocument { PreserveWhitespace = true };
			try
			{
				using (var xmlRdr = new XmlTextReader(stm))
				{
#if (!COMPACT_FRAMEWORK)
					xmlRdr.DtdProcessing = DtdProcessing.Prohibit;
#endif
					xdoc.Load(xmlRdr);
				}
			}
			catch (Exception ex)
			{
				throw new XmlRpcIllFormedXmlException(
					"Response from server does not contain valid XML.", ex);
			}

			return DeserializeResponse(xdoc, svcType);
		}

		public XmlRpcResponse DeserializeResponse(TextReader txtrdr, Type svcType)
		{
			if (txtrdr == null)
				throw new ArgumentNullException(nameof(txtrdr),
												"XmlRpcSerializer.DeserializeResponse");
			var xdoc = new XmlDocument { PreserveWhitespace = true };
			try
			{
				using (var xmlRdr = new XmlTextReader(txtrdr))
				{
#if (!COMPACT_FRAMEWORK)
					xmlRdr.DtdProcessing = DtdProcessing.Prohibit;
#endif
					xdoc.Load(xmlRdr);
				}
			}
			catch (Exception ex)
			{
				throw new XmlRpcIllFormedXmlException(
					"Response from server does not contain valid XML.", ex);
			}

			return DeserializeResponse(xdoc, svcType);
		}

		public XmlRpcResponse DeserializeResponse(XmlDocument xdoc, Type returnType)
		{
			var response = new XmlRpcResponse();
			object retObj = null;
			var methodResponseNode = SelectSingleNode(xdoc, "methodResponse");
			if (methodResponseNode == null)
				throw new XmlRpcInvalidXmlRpcException(
					"Response XML not valid XML-RPC - missing methodResponse element.");
			// check for fault response
			var faultNode = SelectSingleNode(methodResponseNode, "fault");
			if (faultNode != null)
			{
				var parseStack = new ParseStack("fault response");
				// TODO: use global action setting
				const MappingAction mappingAction = MappingAction.Error;
				var faultEx = ParseFault(faultNode, parseStack,
										 mappingAction);
				throw faultEx;
			}

			var paramsNode = SelectSingleNode(methodResponseNode, "params");
			if (paramsNode == null && returnType != null)
				if (returnType == typeof(void))
					return new XmlRpcResponse(null);
				else
					throw new XmlRpcInvalidXmlRpcException(
						"Response XML not valid XML-RPC - missing params element.");
			var paramNode = SelectSingleNode(paramsNode, "param");
			if (paramNode == null && returnType != null)
				if (returnType == typeof(void))
					return new XmlRpcResponse(null);
				else
					throw new XmlRpcInvalidXmlRpcException(
						"Response XML not valid XML-RPC - missing params element.");
			var valueNode = SelectSingleNode(paramNode, "value");
			if (valueNode == null)
				throw new XmlRpcInvalidXmlRpcException(
					"Response XML not valid XML-RPC - missing value element.");
			if (returnType == typeof(void))
			{
				retObj = null;
			}
			else
			{
				var parseStack = new ParseStack("response");
				// TODO: use global action setting
				const MappingAction mappingAction = MappingAction.Error;
				var node = SelectValueNode(valueNode);
				retObj = ParseValue(node, returnType, parseStack, mappingAction);
			}

			response.retVal = retObj;
			return response;
		}

		//#if (DEBUG)
		public
			//#endif
			void Serialize(
				XmlTextWriter xtw,
				object o,
				MappingAction mappingAction)
		{
			Serialize(xtw, o, mappingAction, new ArrayList(16));
		}

		//#if (DEBUG)
		public
			//#endif
			void Serialize(
				XmlTextWriter xtw,
				object o,
				MappingAction mappingAction,
				ArrayList nestedObjs)
		{
			if (nestedObjs.Contains(o))
				throw new XmlRpcUnsupportedTypeException(nestedObjs[0].GetType(),
														 "Cannot serialize recursive data structure");
			nestedObjs.Add(o);
			try
			{
				xtw.WriteStartElement("", "value", "");
				var xType = XmlRpcServiceInfo.GetXmlRpcType(o.GetType());
				if (xType == XmlRpcType.tArray)
				{
					xtw.WriteStartElement("", "array", "");
					xtw.WriteStartElement("", "data", "");
					var a = (Array)o;
					foreach (var aobj in a)
					{
						if (aobj == null)
							throw new XmlRpcMappingSerializeException(string.Format(
																		  "Items in array cannot be null ({0}[]).",
																		  o.GetType().GetElementType()));
						Serialize(xtw, aobj, mappingAction, nestedObjs);
					}

					xtw.WriteEndElement();
					xtw.WriteEndElement();
				}
				else if (xType == XmlRpcType.tMultiDimArray)
				{
					var mda = (Array)o;
					var indices = new int[mda.Rank];
					BuildArrayXml(xtw, mda, 0, indices, mappingAction, nestedObjs);
				}
				else if (xType == XmlRpcType.tBase64)
				{
					var buf = (byte[])o;
					xtw.WriteStartElement("", "base64", "");
					xtw.WriteBase64(buf, 0, buf.Length);
					xtw.WriteEndElement();
				}
				else if (xType == XmlRpcType.tBoolean)
				{
					bool boolVal;
					if (o is bool b)
						boolVal = b;
					else
						boolVal = (bool)(XmlRpcBoolean)o;
					xtw.WriteElementString("boolean", boolVal ? "1" : "0");
				}
				else if (xType == XmlRpcType.tDateTime)
				{
					DateTime dt;
					if (o is DateTime time)
						dt = time;
					else
						dt = (XmlRpcDateTime)o;
					var sdt = dt.ToString("yyyyMMdd'T'HH':'mm':'ss",
										  DateTimeFormatInfo.InvariantInfo);
					xtw.WriteElementString("dateTime.iso8601", sdt);
				}
				else if (xType == XmlRpcType.tDouble)
				{
					double doubleVal;
					if (o is double d)
						doubleVal = d;
					else
						doubleVal = (XmlRpcDouble)o;
					xtw.WriteElementString("double", doubleVal.ToString(null,
																		CultureInfo.InvariantCulture));
				}
				else if (xType == XmlRpcType.tHashtable)
				{
					xtw.WriteStartElement("", "struct", "");
					var xrs = o as XmlRpcStruct;
					foreach (var obj in xrs.Keys)
					{
						var skey = obj as string;
						xtw.WriteStartElement("", "member", "");
						xtw.WriteElementString("name", skey);
						Serialize(xtw, xrs[skey], mappingAction, nestedObjs);
						xtw.WriteEndElement();
					}

					xtw.WriteEndElement();
				}
				else if (xType == XmlRpcType.tInt32)
				{
					xtw.WriteElementString(UseIntTag ? "int" : "i4", o.ToString());
				}
				else if (xType == XmlRpcType.tInt64)
				{
					xtw.WriteElementString("i8", o.ToString());
				}
				else if (xType == XmlRpcType.tString)
				{
					if (UseStringTag)
						xtw.WriteElementString("string", (string)o);
					else
						xtw.WriteString((string)o);
				}
				else if (xType == XmlRpcType.tStruct)
				{
					var structAction
						= StructMappingAction(o.GetType(), mappingAction);
					xtw.WriteStartElement("", "struct", "");
					var mis = o.GetType().GetMembers();
					foreach (var mi in mis)
					{
						if (Attribute.IsDefined(mi, typeof(NonSerializedAttribute)))
							continue;
						if (mi.MemberType == MemberTypes.Field)
						{
							var fi = (FieldInfo)mi;
							var member = fi.Name;
							var attrchk = Attribute.GetCustomAttribute(fi,
																	   typeof(XmlRpcMemberAttribute));
							if (attrchk is XmlRpcMemberAttribute)
							{
								var mmbr = ((XmlRpcMemberAttribute)attrchk).Member;
								if (mmbr != "")
									member = mmbr;
							}

							if (fi.GetValue(o) == null)
							{
								var memberAction = MemberMappingAction(o.GetType(),
																	   fi.Name, structAction);
								if (memberAction == MappingAction.Ignore)
									continue;
								throw new XmlRpcMappingSerializeException(@"Member """ + member +
																		  @""" of struct """ + o.GetType().Name + @""" cannot be null.");
							}

							xtw.WriteStartElement("", "member", "");
							xtw.WriteElementString("name", member);
							Serialize(xtw, fi.GetValue(o), mappingAction, nestedObjs);
							xtw.WriteEndElement();
						}
						else if (mi.MemberType == MemberTypes.Property)
						{
							var pi = (PropertyInfo)mi;
							var member = pi.Name;
							var attrchk = Attribute.GetCustomAttribute(pi,
																	   typeof(XmlRpcMemberAttribute));
							if (attrchk is XmlRpcMemberAttribute)
							{
								var mmbr = ((XmlRpcMemberAttribute)attrchk).Member;
								if (mmbr != "")
									member = mmbr;
							}

							if (pi.GetValue(o, null) == null)
							{
								var memberAction = MemberMappingAction(o.GetType(),
																	   pi.Name, structAction);
								if (memberAction == MappingAction.Ignore)
									continue;
							}

							xtw.WriteStartElement("", "member", "");
							xtw.WriteElementString("name", member);
							Serialize(xtw, pi.GetValue(o, null), mappingAction, nestedObjs);
							xtw.WriteEndElement();
						}
					}

					xtw.WriteEndElement();
				}
				else if (xType == XmlRpcType.tVoid)
				{
					xtw.WriteElementString("string", "");
				}
				else
				{
					throw new XmlRpcUnsupportedTypeException(o.GetType());
				}

				xtw.WriteEndElement();
			}
			catch (NullReferenceException)
			{
				throw new XmlRpcNullReferenceException("Attempt to serialize data "
													   + "containing null reference");
			}
			finally
			{
				nestedObjs.RemoveAt(nestedObjs.Count - 1);
			}
		}

		private void BuildArrayXml(
			XmlTextWriter xtw,
			Array ary,
			int curRank,
			int[] indices,
			MappingAction mappingAction,
			ArrayList nestedObjs)
		{
			xtw.WriteStartElement("", "array", "");
			xtw.WriteStartElement("", "data", "");
			if (curRank < ary.Rank - 1)
				for (var i = 0; i < ary.GetLength(curRank); i++)
				{
					indices[curRank] = i;
					xtw.WriteStartElement("", "value", "");
					BuildArrayXml(xtw, ary, curRank + 1, indices, mappingAction, nestedObjs);
					xtw.WriteEndElement();
				}
			else
				for (var i = 0; i < ary.GetLength(curRank); i++)
				{
					indices[curRank] = i;
					Serialize(xtw, ary.GetValue(indices), mappingAction, nestedObjs);
				}

			xtw.WriteEndElement();
			xtw.WriteEndElement();
		}

		private object ParseValue(
			XmlNode node,
			Type valueType,
			ParseStack parseStack,
			MappingAction mappingAction)
		{
			return ParseValue(node, valueType, parseStack, mappingAction,
							  out var parsedType, out var parsedArrayType);
		}

		//#if (DEBUG)
		public
			//#endif
			object ParseValue(
				XmlNode node,
				Type valueType,
				ParseStack parseStack,
				MappingAction mappingAction,
				out Type parsedType,
				out Type parsedArrayType)
		{
			parsedType = null;
			parsedArrayType = null;
			// if suppplied type is System.Object then ignore it because
			// if doesn't provide any useful information (parsing methods
			// expect null in this case)
			var valType = valueType;
			if (valType != null && valType.BaseType == null)
				valType = null;

			object retObj;
			if (node == null)
			{
				retObj = "";
			}
			else if (node is XmlText || node is XmlWhitespace)
			{
				if (valType != null && valType != typeof(string))
					throw new XmlRpcTypeMismatchException(parseStack.ParseType
														  + " contains implicit string value where "
														  + XmlRpcServiceInfo.GetXmlRpcTypeString(valType)
														  + " expected " + StackDump(parseStack));
				retObj = node.Value;
			}
			else
			{
				if (node.Name == "array")
				{
					retObj = ParseArray(node, valType, parseStack, mappingAction);
				}
				else if (node.Name == "base64")
				{
					retObj = ParseBase64(node, valType, parseStack, mappingAction);
				}
				else if (node.Name == "struct")
				{
					// if we don't know the expected struct type then we must
					// parse the XML-RPC struct as an instance of XmlRpcStruct
					if (valType != null && valType != typeof(XmlRpcStruct)
										&& !valType.IsSubclassOf(typeof(XmlRpcStruct)))
					{
						retObj = ParseStruct(node, valType, parseStack, mappingAction);
					}
					else
					{
						if (valType == null || valType == typeof(object))
							valType = typeof(XmlRpcStruct);
						// TODO: do we need to validate type here?
						retObj = ParseHashtable(node, valType, parseStack, mappingAction);
					}
				}
				else if (node.Name == "i4" // integer has two representations in XML-RPC spec
						 || node.Name == "int")
				{
					retObj = ParseInt(node, valType, parseStack, mappingAction);
					parsedType = typeof(int);
					parsedArrayType = typeof(int[]);
				}
				else if (node.Name == "i8")
				{
					retObj = ParseLong(node, valType, parseStack, mappingAction);
					parsedType = typeof(long);
					parsedArrayType = typeof(long[]);
				}
				else if (node.Name == "string")
				{
					retObj = ParseString(node, valType, parseStack, mappingAction);
					parsedType = typeof(string);
					parsedArrayType = typeof(string[]);
				}
				else if (node.Name == "boolean")
				{
					retObj = ParseBoolean(node, valType, parseStack, mappingAction);
					parsedType = typeof(bool);
					parsedArrayType = typeof(bool[]);
				}
				else if (node.Name == "double")
				{
					retObj = ParseDouble(node, valType, parseStack, mappingAction);
					parsedType = typeof(double);
					parsedArrayType = typeof(double[]);
				}
				else if (node.Name == "dateTime.iso8601")
				{
					retObj = ParseDateTime(node, valType, parseStack, mappingAction);
					parsedType = typeof(DateTime);
					parsedArrayType = typeof(DateTime[]);
				}
				else
				{
					throw new XmlRpcInvalidXmlRpcException(
						"Invalid value element: <" + node.Name + ">");
				}
			}

			return retObj;
		}

		private object ParseArray(
			XmlNode node,
			Type valueType,
			ParseStack parseStack,
			MappingAction mappingAction)
		{
			// required type must be an array
			if (valueType != null
				&& !(valueType.IsArray
					 || valueType == typeof(Array)
					 || valueType == typeof(object)))
				throw new XmlRpcTypeMismatchException(parseStack.ParseType
													  + " contains array value where "
													  + XmlRpcServiceInfo.GetXmlRpcTypeString(valueType)
													  + " expected " + StackDump(parseStack));
			if (valueType != null)
			{
				var xmlRpcType = XmlRpcServiceInfo.GetXmlRpcType(valueType);
				if (xmlRpcType == XmlRpcType.tMultiDimArray)
				{
					parseStack.Push("array mapped to type " + valueType.Name);
					var ret = ParseMultiDimArray(node, valueType, parseStack,
												 mappingAction);
					return ret;
				}

				parseStack.Push("array mapped to type " + valueType.Name);
			}
			else
			{
				parseStack.Push("array");
			}

			var dataNode = SelectSingleNode(node, "data");
			var childNodes = SelectNodes(dataNode, "value");
			var nodeCount = childNodes.Length;
			var elements = new object[nodeCount];
			// determine type of array elements
			Type elemType = null;
			if (valueType != null
				&& valueType != typeof(Array)
				&& valueType != typeof(object))
			{
#if (!COMPACT_FRAMEWORK)
				elemType = valueType.GetElementType();
#else
        string[] checkMultiDim = Regex.Split(ValueType.FullName, 
          "\\[\\]$");
        // determine assembly of array element type
        Assembly asmbly = ValueType.Assembly;
        string[] asmblyName = asmbly.FullName.Split(',');
        string elemTypeName = checkMultiDim[0] + ", " + asmblyName[0]; 
        elemType = Type.GetType(elemTypeName);
#endif
			}
			else
			{
				elemType = typeof(object);
			}

			var bGotType = false;
			Type useType = null;
			var i = 0;
			foreach (var vNode in childNodes)
			{
				parseStack.Push(string.Format("element {0}", i));
				var vvNode = SelectValueNode(vNode);
				Type parsedType;
				Type parsedArrayType;
				elements[i++] = ParseValue(vvNode, elemType, parseStack, mappingAction,
										   out parsedType, out parsedArrayType);
				if (bGotType == false)
				{
					useType = parsedArrayType;
					bGotType = true;
				}
				else
				{
					if (useType != parsedArrayType)
						useType = null;
				}

				parseStack.Pop();
			}

			var args = new object[1];
			args[0] = nodeCount;
			object retObj;
			if (valueType != null
				&& valueType != typeof(Array)
				&& valueType != typeof(object))
			{
				retObj = CreateArrayInstance(valueType, args);
			}
			else
			{
				retObj = CreateArrayInstance(useType == null ? typeof(object[]) : useType, args);
			}

			for (var j = 0; j < elements.Length; j++) ((Array)retObj).SetValue(elements[j], j);
			parseStack.Pop();
			return retObj;
		}

		private object ParseMultiDimArray(XmlNode node, Type ValueType,
										  ParseStack parseStack, MappingAction mappingAction)
		{
			// parse the type name to get element type and array rank
#if (!COMPACT_FRAMEWORK)
			var elemType = ValueType.GetElementType();
			var rank = ValueType.GetArrayRank();
#else
      string[] checkMultiDim = Regex.Split(ValueType.FullName, 
        "\\[,[,]*\\]$");
      Type elemType = Type.GetType(checkMultiDim[0]);
      string commas = ValueType.FullName.Substring(checkMultiDim[0].Length+1, 
        ValueType.FullName.Length-checkMultiDim[0].Length-2);
      int rank = commas.Length+1;
#endif
			// elements will be stored sequentially as nested arrays are parsed
			var elements = new ArrayList();
			// create array to store length of each dimension - initialize to 
			// all zeroes so that when parsing we can determine if an array for 
			// that dimension has been parsed already
			var dimLengths = new int[rank];
			dimLengths.Initialize();
			ParseMultiDimElements(node, rank, 0, elemType, elements, dimLengths,
								  parseStack, mappingAction);
			// build arguments to define array dimensions and create the array
			var args = new object[dimLengths.Length];
			for (var argi = 0; argi < dimLengths.Length; argi++) args[argi] = dimLengths[argi];
			var ret = (Array)CreateArrayInstance(ValueType, args);
			// copy elements into new multi-dim array
			//!! make more efficient
			var length = ret.Length;
			for (var e = 0; e < length; e++)
			{
				var indices = new int[dimLengths.Length];
				var div = 1;
				for (var f = indices.Length - 1; f >= 0; f--)
				{
					indices[f] = e / div % dimLengths[f];
					div *= dimLengths[f];
				}

				ret.SetValue(elements[e], indices);
			}

			return ret;
		}

		private void ParseMultiDimElements(XmlNode node, int rank, int CurRank,
										   Type elemType, IList elements, IList<int> dimLengths,
										   ParseStack parseStack, MappingAction mappingAction)
		{
			if (node.Name != "array")
				throw new XmlRpcTypeMismatchException(
					"param element does not contain array element.");
			var dataNode = SelectSingleNode(node, "data");
			var childNodes = SelectNodes(dataNode, "value");
			var nodeCount = childNodes.Length;
			//!! check that multi dim array is not jagged
			if (dimLengths[CurRank] != 0 && nodeCount != dimLengths[CurRank])
				throw new XmlRpcNonRegularArrayException(
					"Multi-dimensional array must not be jagged.");
			dimLengths[CurRank] = nodeCount; // in case first array at this rank
			if (CurRank < rank - 1)
				foreach (var vNode in childNodes)
				{
					var arrayNode = SelectSingleNode(vNode, "array");
					ParseMultiDimElements(arrayNode, rank, CurRank + 1, elemType,
										  elements, dimLengths, parseStack, mappingAction);
				}
			else
				foreach (var vNode in childNodes)
				{
					var vvNode = SelectValueNode(vNode);
					elements.Add(ParseValue(vvNode, elemType, parseStack,
											mappingAction));
				}
		}

		private object ParseStruct(
			XmlNode node,
			Type valueType,
			ParseStack parseStack,
			MappingAction mappingAction)
		{
			if (valueType.IsPrimitive)
				throw new XmlRpcTypeMismatchException(parseStack.ParseType
													  + " contains struct value where "
													  + XmlRpcServiceInfo.GetXmlRpcTypeString(valueType)
													  + " expected " + StackDump(parseStack));
#if !FX1_0
			if (valueType.IsGenericType
				&& valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
				valueType = valueType.GetGenericArguments()[0];
#endif
			object retObj;
			try
			{
				retObj = Activator.CreateInstance(valueType);
			}
			catch (Exception)
			{
				throw new XmlRpcTypeMismatchException(parseStack.ParseType
													  + " contains struct value where "
													  + XmlRpcServiceInfo.GetXmlRpcTypeString(valueType)
													  + " expected (as type " + valueType.Name + ") "
													  + StackDump(parseStack));
			}

			// Note: mapping action on a struct is only applied locally - it 
			// does not override the global mapping action when members of the 
			// struct are parsed
			var localAction = mappingAction;
			if (valueType != null)
			{
				parseStack.Push("struct mapped to type " + valueType.Name);
				localAction = StructMappingAction(valueType, mappingAction);
			}
			else
			{
				parseStack.Push("struct");
			}

			// create map of field names and remove each name from it as 
			// processed so we can determine which fields are missing
			// TODO: replace HashTable with lighter collection
			var names = new Hashtable();
			foreach (var fi in valueType.GetFields())
			{
				if (Attribute.IsDefined(fi, typeof(NonSerializedAttribute)))
					continue;
				names.Add(fi.Name, fi.Name);
			}

			foreach (var pi in valueType.GetProperties())
			{
				if (Attribute.IsDefined(pi, typeof(NonSerializedAttribute)))
					continue;
				names.Add(pi.Name, pi.Name);
			}

			var members = SelectNodes(node, "member");
			foreach (var member in members)
			{
				if (member.Name != "member")
					continue;
				SelectTwoNodes(member, "name", out var nameNode, out var dupName, "value",
							   out var valueNode, out var dupValue);
				if (nameNode?.FirstChild == null)
					throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
														   + " contains a member with missing name"
														   + " " + StackDump(parseStack));
				if (dupName)
					throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
														   + " contains member with more than one name element"
														   + " " + StackDump(parseStack));
				var name = nameNode.FirstChild.Value;
				if (valueNode == null)
					throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
														   + " contains struct member " + name + " with missing value "
														   + " " + StackDump(parseStack));
				if (dupValue)
					throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
														   + " contains member with more than one value element"
														   + " " + StackDump(parseStack));
				var structName = GetStructName(valueType, name);
				if (structName != null)
					name = structName;
				MemberInfo mi = valueType.GetField(name);
				if (mi == null)
					mi = valueType.GetProperty(name);
				if (mi == null)
					continue;
				if (names.Contains(name))
				{
					names.Remove(name);
				}
				else
				{
					if (Attribute.IsDefined(mi, typeof(NonSerializedAttribute)))
					{
						parseStack.Push(string.Format("member {0}", name));
						throw new XmlRpcNonSerializedMember("Cannot map XML-RPC struct "
															+ "member onto member marked as [NonSerialized]: "
															+ " " + StackDump(parseStack));
					}

					if (!IgnoreDuplicateMembers)
						throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
															   + " contains struct value with duplicate member "
															   + nameNode.FirstChild.Value
															   + " " + StackDump(parseStack));
					continue; // ignore duplicate member
				}

				object valObj = null;
				switch (mi.MemberType)
				{
					case MemberTypes.Field:
						var fi = (FieldInfo)mi;
						if (valueType == null)
							parseStack.Push(string.Format("member {0}", name));
						else
							parseStack.Push(string.Format("member {0} mapped to type {1}",
														  name, fi.FieldType.Name));
						try
						{
							var vvvNode = SelectValueNode(valueNode);
							valObj = ParseValue(vvvNode, fi.FieldType,
												parseStack, mappingAction);
						}
						catch (XmlRpcInvalidXmlRpcException)
						{
							if (valueType != null && localAction == MappingAction.Error)
							{
								var memberAction = MemberMappingAction(valueType,
																	   name, MappingAction.Error);
								if (memberAction == MappingAction.Error)
									throw;
							}
						}
						finally
						{
							parseStack.Pop();
						}

						fi.SetValue(retObj, valObj);
						break;
					case MemberTypes.Property:
						var pi = (PropertyInfo)mi;
						if (valueType == null)
							parseStack.Push(string.Format("member {0}", name));
						else

							parseStack.Push(string.Format("member {0} mapped to type {1}",
														  name, pi.PropertyType.Name));
						var vvNode = SelectValueNode(valueNode);
						valObj = ParseValue(vvNode, pi.PropertyType,
											parseStack, mappingAction);
						parseStack.Pop();

						pi.SetValue(retObj, valObj, null);
						break;
				}
			}

			if (localAction == MappingAction.Error && names.Count > 0)
				ReportMissingMembers(valueType, names, parseStack);
			parseStack.Pop();
			return retObj;
		}

		private void ReportMissingMembers(
			Type valueType,
			IDictionary names,
			ParseStack parseStack)
		{
			var sb = new StringBuilder();
			var errorCount = 0;
			var sep = "";
			foreach (string s in names.Keys)
			{
				var memberAction = MemberMappingAction(valueType, s,
													   MappingAction.Error);
				if (memberAction == MappingAction.Error)
				{
					sb.Append(sep);
					sb.Append(s);
					sep = " ";
					errorCount++;
				}
			}

			if (errorCount > 0)
			{
				var plural = "";
				if (errorCount > 1)
					plural = "s";
				throw new XmlRpcTypeMismatchException(parseStack.ParseType
													  + " contains struct value with missing non-optional member"
													  + plural + ": " + sb + " " + StackDump(parseStack));
			}
		}

		private static string GetStructName(Type valueType, string xmlRpcName)
		{
			// given a member name in an XML-RPC struct, check to see whether
			// a field has been associated with this XML-RPC member name, return
			// the field name if it has else return null
			if (valueType == null)
				return null;
			foreach (var fi in valueType.GetFields())
			{
				var attr = Attribute.GetCustomAttribute(fi,
														typeof(XmlRpcMemberAttribute));
				if (attr is XmlRpcMemberAttribute attribute && attribute.Member == xmlRpcName)
				{
					var ret = fi.Name;
					return ret;
				}
			}

			foreach (var pi in valueType.GetProperties())
			{
				var attr = Attribute.GetCustomAttribute(pi,
														typeof(XmlRpcMemberAttribute));
				if (attr is XmlRpcMemberAttribute attribute && attribute.Member == xmlRpcName)
				{
					var ret = pi.Name;
					return ret;
				}
			}

			return null;
		}

		private static MappingAction StructMappingAction(
			Type type,
			MappingAction currentAction)
		{
			// if struct member has mapping action attribute, override the current
			// mapping action else just return the current action
			if (type == null)
				return currentAction;
			var attr = Attribute.GetCustomAttribute(type,
													typeof(XmlRpcMissingMappingAttribute));
			if (attr != null)
				return ((XmlRpcMissingMappingAttribute)attr).Action;
			return currentAction;
		}

		private static MappingAction MemberMappingAction(
			Type type,
			string memberName,
			MappingAction currentAction)
		{
			// if struct member has mapping action attribute, override the current
			// mapping action else just return the current action
			if (type == null)
				return currentAction;
			Attribute attr;
			var fi = type.GetField(memberName);
			if (fi != null)
			{
				attr = Attribute.GetCustomAttribute(fi,
													typeof(XmlRpcMissingMappingAttribute));
			}
			else
			{
				var pi = type.GetProperty(memberName);
				attr = Attribute.GetCustomAttribute(pi,
													typeof(XmlRpcMissingMappingAttribute));
			}

			return attr != null ? ((XmlRpcMissingMappingAttribute)attr).Action : currentAction;
		}

		private object ParseHashtable(
			XmlNode node,
			Type valueType,
			ParseStack parseStack,
			MappingAction mappingAction)
		{
			var retObj = new XmlRpcStruct();
			parseStack.Push("struct mapped to XmlRpcStruct");
			try
			{
				var members = SelectNodes(node, "member");
				foreach (var member in members)
				{
					if (member.Name != "member")
						continue;
					SelectTwoNodes(member, "name", out var nameNode, out var dupName, "value",
								   out var valueNode, out var dupValue);
					if (nameNode?.FirstChild == null)
						throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
															   + " contains a member with missing name"
															   + " " + StackDump(parseStack));
					if (dupName)
						throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
															   + " contains member with more than one name element"
															   + " " + StackDump(parseStack));
					var rpcName = nameNode.FirstChild.Value;
					if (valueNode == null)
						throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
															   + " contains struct member " + rpcName + " with missing value "
															   + " " + StackDump(parseStack));
					if (dupValue)
						throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
															   + " contains member with more than one value element"
															   + " " + StackDump(parseStack));
					if (retObj.Contains(rpcName))
						if (!IgnoreDuplicateMembers)
							throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
																   + " contains struct value with duplicate member "
																   + nameNode.FirstChild.Value
																   + " " + StackDump(parseStack));
						else
							continue;
					object valObj;
					parseStack.Push(string.Format("member {0}", rpcName));
					try
					{
						var vvNode = SelectValueNode(valueNode);
						valObj = ParseValue(vvNode, null, parseStack,
											mappingAction);
					}
					finally
					{
						parseStack.Pop();
					}

					retObj.Add(rpcName, valObj);
				}
			}
			finally
			{
				parseStack.Pop();
			}

			return retObj;
		}

		private object ParseInt(
			XmlNode node,
			Type valueType,
			ParseStack parseStack,
			MappingAction mappingAction)
		{
			if (valueType != null && valueType != typeof(object)
								  && valueType != typeof(int)
#if !FX1_0
								  && valueType != typeof(int?)
#endif
								  && valueType != typeof(XmlRpcInt))
				throw new XmlRpcTypeMismatchException(parseStack.ParseType +
													  " contains int value where "
													  + XmlRpcServiceInfo.GetXmlRpcTypeString(valueType)
													  + " expected " + StackDump(parseStack));
			int retVal;
			parseStack.Push("integer");
			try
			{
				var valueNode = node.FirstChild;
				if (valueNode == null)
					throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
														   + " contains invalid int element " + StackDump(parseStack));
				try
				{
					var strValue = valueNode.Value;
					retVal = int.Parse(strValue);
				}
				catch (Exception)
				{
					throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
														   + " contains invalid int value " + StackDump(parseStack));
				}
			}
			finally
			{
				parseStack.Pop();
			}

			if (valueType == typeof(XmlRpcInt))
				return new XmlRpcInt(retVal);
			return retVal;
		}

		private object ParseLong(
			XmlNode node,
			Type valueType,
			ParseStack parseStack,
			MappingAction mappingAction)
		{
			if (valueType != null && valueType != typeof(object)
								  && valueType != typeof(long)
#if !FX1_0
								  && valueType != typeof(long?)
#endif
			)
				throw new XmlRpcTypeMismatchException(parseStack.ParseType +
													  " contains i8 value where "
													  + XmlRpcServiceInfo.GetXmlRpcTypeString(valueType)
													  + " expected " + StackDump(parseStack));
			long retVal;
			parseStack.Push("i8");
			try
			{
				var valueNode = node.FirstChild;
				if (valueNode == null)
					throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
														   + " contains invalid i8 element " + StackDump(parseStack));
				try
				{
					var strValue = valueNode.Value;
					retVal = long.Parse(strValue);
				}
				catch (Exception)
				{
					throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
														   + " contains invalid i8 value " + StackDump(parseStack));
				}
			}
			finally
			{
				parseStack.Pop();
			}

			return retVal;
		}

		private object ParseString(
			XmlNode node,
			Type valueType,
			ParseStack parseStack,
			MappingAction mappingAction)
		{
			if (valueType != null && valueType != typeof(string)
								  && valueType != typeof(object))
				throw new XmlRpcTypeMismatchException(parseStack.ParseType
													  + " contains string value where "
													  + XmlRpcServiceInfo.GetXmlRpcTypeString(valueType)
													  + " expected " + StackDump(parseStack));
			string ret;
			parseStack.Push("string");
			try
			{
				ret = node.FirstChild == null ? "" : node.FirstChild.Value;
			}
			finally
			{
				parseStack.Pop();
			}

			return ret;
		}

		private object ParseBoolean(
			XmlNode node,
			Type valueType,
			ParseStack parseStack,
			MappingAction mappingAction)
		{
			if (valueType != null && valueType != typeof(object)
								  && valueType != typeof(bool)
#if !FX1_0
								  && valueType != typeof(bool?)
#endif
								  && valueType != typeof(XmlRpcBoolean))
				throw new XmlRpcTypeMismatchException(parseStack.ParseType
													  + " contains boolean value where "
													  + XmlRpcServiceInfo.GetXmlRpcTypeString(valueType)
													  + " expected " + StackDump(parseStack));
			bool retVal;
			parseStack.Push("boolean");
			try
			{
				var s = node.FirstChild.Value;
				if (s == "1")
					retVal = true;
				else if (s == "0")
					retVal = false;
				else
					throw new XmlRpcInvalidXmlRpcException(
						"reponse contains invalid boolean value "
						+ StackDump(parseStack));
			}
			finally
			{
				parseStack.Pop();
			}

			if (valueType == typeof(XmlRpcBoolean))
				return new XmlRpcBoolean(retVal);
			return retVal;
		}

		private object ParseDouble(
			XmlNode node,
			Type valueType,
			ParseStack parseStack,
			MappingAction mappingAction)
		{
			if (valueType != null && valueType != typeof(object)
								  && valueType != typeof(double)
#if !FX1_0
								  && valueType != typeof(double?)
#endif
								  && valueType != typeof(XmlRpcDouble))
				throw new XmlRpcTypeMismatchException(parseStack.ParseType
													  + " contains double value where "
													  + XmlRpcServiceInfo.GetXmlRpcTypeString(valueType)
													  + " expected " + StackDump(parseStack));
			double retVal;
			parseStack.Push("double");
			try
			{
				retVal = double.Parse(node.FirstChild.Value,
									  CultureInfo.InvariantCulture.NumberFormat);
			}
			catch (Exception)
			{
				throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
													   + " contains invalid double value " + StackDump(parseStack));
			}
			finally
			{
				parseStack.Pop();
			}

			if (valueType == typeof(XmlRpcDouble))
				return new XmlRpcDouble(retVal);
			return retVal;
		}

		private object ParseDateTime(
			XmlNode node,
			Type valueType,
			ParseStack parseStack,
			MappingAction mappingAction)
		{
			if (valueType != null && valueType != typeof(object)
								  && valueType != typeof(DateTime)
#if !FX1_0
								  && valueType != typeof(DateTime?)
#endif
								  && valueType != typeof(XmlRpcDateTime))
				throw new XmlRpcTypeMismatchException(parseStack.ParseType
													  + " contains dateTime.iso8601 value where "
													  + XmlRpcServiceInfo.GetXmlRpcTypeString(valueType)
													  + " expected " + StackDump(parseStack));
			DateTime retVal;
			parseStack.Push("dateTime");
			try
			{
				var child = node.FirstChild;
				if (child == null)
					if (MapEmptyDateTimeToMinValue)
						return DateTime.MinValue;
					else
						throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
															   + " contains empty dateTime value "
															   + StackDump(parseStack));
				var s = child.Value;
				// Allow various iso8601 formats, e.g.
				//   XML-RPC spec yyyyMMddThh:mm:ss
				//   WordPress yyyyMMddThh:mm:ssZ
				//   TypePad yyyy-MM-ddThh:mm:ssZ
				//   other yyyy-MM-ddThh:mm:ss
				if (!DateTime8601.TryParseDateTime8601(s, out retVal))
					if (MapZerosDateTimeToMinValue && s.StartsWith("0000")
												   && (s == "00000000T00:00:00" || s == "0000-00-00T00:00:00Z"
																				|| s == "00000000T00:00:00Z" || s == "0000-00-00T00:00:00"))
						retVal = DateTime.MinValue;
					else
						throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
															   + " contains invalid dateTime value "
															   + StackDump(parseStack));
			}
			finally
			{
				parseStack.Pop();
			}

			if (valueType == typeof(XmlRpcDateTime))
				return new XmlRpcDateTime(retVal);
			return retVal;
		}

		private object ParseBase64(
			XmlNode node,
			Type valueType,
			ParseStack parseStack,
			MappingAction mappingAction)
		{
			if (valueType != null && valueType != typeof(byte[])
								  && valueType != typeof(object))
				throw new XmlRpcTypeMismatchException(parseStack.ParseType
													  + " contains base64 value where "
													  + XmlRpcServiceInfo.GetXmlRpcTypeString(valueType)
													  + " expected " + StackDump(parseStack));
			byte[] ret;
			parseStack.Push("base64");
			try
			{
				if (node.FirstChild == null)
				{
					ret = new byte[0];
				}
				else
				{
					var s = node.FirstChild.Value;
					try
					{
						ret = Convert.FromBase64String(s);
					}
					catch (Exception)
					{
						throw new XmlRpcInvalidXmlRpcException(parseStack.ParseType
															   + " contains invalid base64 value "
															   + StackDump(parseStack));
					}
				}
			}
			finally
			{
				parseStack.Pop();
			}

			return ret;
		}

		private XmlRpcFaultException ParseFault(
			XmlNode faultNode,
			ParseStack parseStack,
			MappingAction mappingAction)
		{
			var valueNode = SelectSingleNode(faultNode, "value");
			var structNode = SelectSingleNode(valueNode, "struct");
			if (structNode == null)
				throw new XmlRpcInvalidXmlRpcException(
					"struct element missing from fault response.");
			Fault fault;
			try
			{
				fault = (Fault)ParseValue(structNode, typeof(Fault), parseStack,
										   mappingAction);
			}
			catch (Exception ex)
			{
				// some servers incorrectly return fault code in a string
				if (AllowStringFaultCode) throw;

				FaultStructStringCode faultStrCode;
				try
				{
					faultStrCode = (FaultStructStringCode)ParseValue(structNode,
																	  typeof(FaultStructStringCode), parseStack, mappingAction);
					fault.FaultCode = Convert.ToInt32(faultStrCode.FaultCode);
					fault.FaultString = faultStrCode.FaultString;
				}
				catch (Exception)
				{
					// use exception from when attempting to parse code as integer
					throw ex;
				}
			}

			return new XmlRpcFaultException(fault.FaultCode, fault.FaultString);
		}

		public void SerializeFaultResponse(
			Stream stm,
			XmlRpcFaultException faultEx)
		{
			FaultStruct fs;
			fs.faultCode = faultEx.FaultCode;
			fs.faultString = faultEx.FaultString;

			var xtw = new XmlTextWriter(stm, XmlEncoding);
			ConfigureXmlFormat(xtw);
			xtw.WriteStartDocument();
			xtw.WriteStartElement("", "methodResponse", "");
			xtw.WriteStartElement("", "fault", "");
			Serialize(xtw, fs, MappingAction.Error);
			xtw.WriteEndElement();
			xtw.WriteEndElement();
			xtw.Flush();
		}

		private void ConfigureXmlFormat(
			XmlTextWriter xtw)
		{
			if (UseIndentation)
			{
				xtw.Formatting = Formatting.Indented;
				xtw.Indentation = Indentation;
			}
			else
			{
				xtw.Formatting = Formatting.None;
			}
		}

		private string StackDump(ParseStack parseStack)
		{
			var sb = new StringBuilder();
			foreach (string elem in parseStack)
			{
				sb.Insert(0, elem);
				sb.Insert(0, " : ");
			}

			sb.Insert(0, parseStack.ParseType);
			sb.Insert(0, "[");
			sb.Append("]");
			return sb.ToString();
		}

		private XmlNode SelectSingleNode(XmlNode node, string name)
		{
#if (COMPACT_FRAMEWORK)
      foreach (XmlNode selnode in node.ChildNodes)
      {
        // For "*" element else return null
        if ((name == "*") && !(selnode.Name.StartsWith("#")))
          return selnode;
        if (selnode.Name == name)
          return selnode;
      }
      return null;
#else
			return node.SelectSingleNode(name);
#endif
		}

		private XmlNode[] SelectNodes(XmlNode node, string name)
		{
			var list = new ArrayList();
			foreach (XmlNode selnode in node.ChildNodes)
				if (selnode.Name == name)
					list.Add(selnode);
			return (XmlNode[])list.ToArray(typeof(XmlNode));
		}

		private XmlNode SelectValueNode(XmlNode valueNode)
		{
			// an XML-RPC value is either held as the child node of a <value> element
			// or is just the text of the value node as an implicit string value
			var vvNode = SelectSingleNode(valueNode, "*");
			if (vvNode == null)
				vvNode = valueNode.FirstChild;
			return vvNode;
		}

		private void SelectTwoNodes(XmlNode node, string name1, out XmlNode node1,
									out bool dup1, string name2, out XmlNode node2, out bool dup2)
		{
			node1 = node2 = null;
			dup1 = dup2 = false;
			foreach (XmlNode selnode in node.ChildNodes)
				if (selnode.Name == name1)
					if (node1 == null)
						node1 = selnode;
					else
						dup1 = true;
				else if (selnode.Name == name2)
					if (node2 == null)
						node2 = selnode;
					else
						dup2 = true;
		}

		// TODO: following to return Array?
		private object CreateArrayInstance(Type type, object[] args)
		{
#if (!COMPACT_FRAMEWORK)
			return Activator.CreateInstance(type, args);
#else
		Object Arr = Array.CreateInstance(type.GetElementType(), (int)args[0]);
		return Arr;
#endif
		}

		private bool IsStructParamsMethod(MethodInfo mi)
		{
			if (mi == null)
				return false;
			var ret = false;
			var attr = Attribute.GetCustomAttribute(mi,
													typeof(XmlRpcMethodAttribute));
			if (attr != null)
			{
				var mattr = (XmlRpcMethodAttribute)attr;
				ret = mattr.StructParams;
			}

			return ret;
		}

		private struct FaultStruct
		{
			public int faultCode;
			public string faultString;
		}

		private struct FaultStructStringCode
		{
			public string FaultCode;
			public string FaultString;
		}

		//#if (DEBUG)
		public
			//#endif
			class ParseStack : Stack
		{
			public string m_parseType;

			public ParseStack(string parseType)
			{
				m_parseType = parseType;
			}

			public string ParseType => m_parseType;
		}

#if (!COMPACT_FRAMEWORK)
		public XmlRpcRequest DeserializeRequest(Stream stm, Type svcType)
		{
			if (stm == null)
				throw new ArgumentNullException(nameof(stm),
												"XmlRpcSerializer.DeserializeRequest");
			var xdoc = new XmlDocument { PreserveWhitespace = true };
			try
			{
				using (var xmlRdr = new XmlTextReader(stm))
				{
					xmlRdr.DtdProcessing = DtdProcessing.Prohibit;
					xdoc.Load(xmlRdr);
				}
			}
			catch (Exception ex)
			{
				throw new XmlRpcIllFormedXmlException(
					"Request from client does not contain valid XML.", ex);
			}

			return DeserializeRequest(xdoc, svcType);
		}

		public XmlRpcRequest DeserializeRequest(TextReader txtrdr, Type svcType)
		{
			if (txtrdr == null)
				throw new ArgumentNullException(nameof(txtrdr),
												"XmlRpcSerializer.DeserializeRequest");
			var xdoc = new XmlDocument { PreserveWhitespace = true };
			try
			{
				using (var xmlRdr = new XmlTextReader(txtrdr))
				{
					xmlRdr.DtdProcessing = DtdProcessing.Prohibit;
					xdoc.Load(xmlRdr);
				}
			}
			catch (Exception ex)
			{
				throw new XmlRpcIllFormedXmlException(
					"Request from client does not contain valid XML.", ex);
			}

			return DeserializeRequest(xdoc, svcType);
		}

		public XmlRpcRequest DeserializeRequest(XmlDocument xdoc, Type svcType)
		{
			var request = new XmlRpcRequest();
			var callNode = SelectSingleNode(xdoc, "methodCall");
			if (callNode == null)
				throw new XmlRpcInvalidXmlRpcException(
					"Request XML not valid XML-RPC - missing methodCall element.");
			var methodNode = SelectSingleNode(callNode, "methodName");
			if (methodNode == null)
				throw new XmlRpcInvalidXmlRpcException(
					"Request XML not valid XML-RPC - missing methodName element.");
			if (methodNode.FirstChild == null)
				throw new XmlRpcInvalidXmlRpcException(
					"Request XML not valid XML-RPC - missing methodName element.");
			request.method = methodNode.FirstChild.Value;
			if (request.method == "")
				throw new XmlRpcInvalidXmlRpcException(
					"Request XML not valid XML-RPC - empty methodName.");
			request.mi = null;
			var pis = new ParameterInfo[0];
			if (svcType != null)
			{
				// retrieve info for the method which handles this XML-RPC method
				var svcInfo
					= XmlRpcServiceInfo.CreateServiceInfo(svcType);
				request.mi = svcInfo.GetMethodInfo(request.method);
				// if a service type has been specified and we cannot find the requested
				// method then we must throw an exception
				if (request.mi == null)
				{
					var msg = string.Format("unsupported method called: {0}",
											request.method);
					throw new XmlRpcUnsupportedMethodException(msg);
				}

				// method must be marked with XmlRpcMethod attribute
				var attr = Attribute.GetCustomAttribute(request.mi,
														typeof(XmlRpcMethodAttribute));
				if (attr == null)
					throw new XmlRpcMethodAttributeException(
						"Method must be marked with the XmlRpcMethod attribute.");
				pis = request.mi.GetParameters();
			}

			var paramsNode = SelectSingleNode(callNode, "params");
			if (paramsNode == null)
				if (svcType != null)
				{
					if (pis.Length == 0)
					{
						request.args = new object[0];
						return request;
					}

					throw new XmlRpcInvalidParametersException(
						"Method takes parameters and params element is missing.");
				}
				else
				{
					request.args = new object[0];
					return request;
				}

			var paramNodes = SelectNodes(paramsNode, "param");
			var paramsPos = GetParamsPos(pis);
			var minParamCount = paramsPos == -1 ? pis.Length : paramsPos;
			if (svcType != null && paramNodes.Length < minParamCount)
				throw new XmlRpcInvalidParametersException(
					"Request contains too few param elements based on method signature.");
			if (svcType != null && paramsPos == -1 && paramNodes.Length > pis.Length)
				throw new XmlRpcInvalidParametersException(
					"Request contains too many param elements based on method signature.");
			var parseStack = new ParseStack("request");
			// TODO: use global action setting
			var mappingAction = MappingAction.Error;
			var paramObjCount = paramsPos == -1 ? paramNodes.Length : paramsPos + 1;
			var paramObjs = new object[paramObjCount];
			// parse ordinary parameters
			var ordinaryParams = paramsPos == -1 ? paramNodes.Length : paramsPos;
			for (var i = 0; i < ordinaryParams; i++)
			{
				var paramNode = paramNodes[i];
				var valueNode = SelectSingleNode(paramNode, "value");
				if (valueNode == null)
					throw new XmlRpcInvalidXmlRpcException("Missing value element.");
				var node = SelectValueNode(valueNode);
				if (svcType != null)
				{
					parseStack.Push(string.Format("parameter {0}", i + 1));
					// TODO: why following commented out?
					//          parseStack.Push(String.Format("parameter {0} mapped to type {1}", 
					//            i, pis[i].ParameterType.Name));
					paramObjs[i] = ParseValue(node, pis[i].ParameterType, parseStack,
											  mappingAction);
				}
				else
				{
					parseStack.Push(string.Format("parameter {0}", i));
					paramObjs[i] = ParseValue(node, null, parseStack, mappingAction);
				}

				parseStack.Pop();
			}

			// parse params parameters
			if (paramsPos != -1)
			{
				var paramsType = pis[paramsPos].ParameterType.GetElementType();
				var args = new object[1];
				args[0] = paramNodes.Length - paramsPos;
				var varargs = (Array)CreateArrayInstance(pis[paramsPos].ParameterType,
														  args);
				for (var i = 0; i < varargs.Length; i++)
				{
					var paramNode = paramNodes[i + paramsPos];
					var valueNode = SelectSingleNode(paramNode, "value");
					if (valueNode == null)
						throw new XmlRpcInvalidXmlRpcException("Missing value element.");
					var node = SelectValueNode(valueNode);
					parseStack.Push(string.Format("parameter {0}", i + 1 + paramsPos));
					varargs.SetValue(ParseValue(node, paramsType, parseStack,
												mappingAction), i);
					parseStack.Pop();
				}

				paramObjs[paramsPos] = varargs;
			}

			request.args = paramObjs;
			return request;
		}

		private int GetParamsPos(ParameterInfo[] pis)
		{
			if (pis.Length == 0)
				return -1;
			if (Attribute.IsDefined(pis[pis.Length - 1], typeof(ParamArrayAttribute)))
				return pis.Length - 1;
			return -1;
		}

		public void SerializeResponse(Stream stm, XmlRpcResponse response)
		{
			var ret = response.retVal;
			if (ret is XmlRpcFaultException)
			{
				SerializeFaultResponse(stm, (XmlRpcFaultException)ret);
				return;
			}

			var xtw = new XmlTextWriter(stm, XmlEncoding);
			ConfigureXmlFormat(xtw);
			xtw.WriteStartDocument();
			xtw.WriteStartElement("", "methodResponse", "");
			xtw.WriteStartElement("", "params", "");
			// "void" methods actually return an empty string value
			if (ret == null) ret = "";
			xtw.WriteStartElement("", "param", "");
			// TODO: use global action setting
			var mappingAction = MappingAction.Error;
			try
			{
				Serialize(xtw, ret, mappingAction);
			}
			catch (XmlRpcUnsupportedTypeException ex)
			{
				throw new XmlRpcInvalidReturnType(string.Format(
													  "Return value is of, or contains an instance of, type {0} which "
													  + "cannot be mapped to an XML-RPC type", ex.UnsupportedType));
			}

			xtw.WriteEndElement();
			xtw.WriteEndElement();
			xtw.WriteEndElement();
			xtw.Flush();
		}
#endif
	}
}