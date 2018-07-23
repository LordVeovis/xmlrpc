using System;
using CookComputing.XmlRpc;
using NUnit.Framework;

namespace ntest
{
	[TestFixture]
	public class ServiceInfoTest
	{
		private struct struct1
		{
			public int mi;
		}

		private enum Fooe
		{
			one,
			two
		}

		private struct struct2
		{
			public int  mi;
			public Fooe mf;
		}


		public struct struct3
		{
			public int TestProperty => 12345;
		}


		private class Example
		{
			public Example childExample;
			public Example ChildExample { get; set; }
		}

		private class ExampleWithArray
		{
			public ExampleWithArray[] childExamples;
			public ExampleWithArray[] ChildExamples { get; set; }
		}

		[Test]
		public void Array()
		{
			var type    = typeof(Array);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tArray, rpcType,
							"Array doesn't map to XmlRpcType.tArray");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "array", "Array doesn't map to 'array'");
		}

		[Test]
		public void Base64()
		{
			var type    = typeof(byte[]);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tBase64, rpcType,
							"Byte[] doesn't map to XmlRpcType.tBase64");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "base64", "Byte[] doesn't map to 'base64'");
		}

		[Test]
		public void Boolean()
		{
			var type    = typeof(bool);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tBoolean, rpcType,
							"Boolean doesn't map to XmlRpcType.tBoolean");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "boolean",
							"Boolean doesn't map to 'boolean'");
		}

		[Test]
		public void DateTime()
		{
			var type    = typeof(DateTime);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tDateTime, rpcType,
							"DateTime doesn't map to XmlRpcType.tDateTime");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "dateTime",
							"DateTime doesn't map to 'dateTime'");
		}

		[Test]
		public void DBNull()
		{
			var value   = System.DBNull.Value;
			var type    = value.GetType();
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tStruct, rpcType,
							"GetXmlRpcType return DBNull as tStruct");
		}

		[Test]
		public void DerivedInterfaces()
		{
			var svcinfo = XmlRpcServiceInfo.CreateServiceInfo(
				typeof(FooBar));
			Assert.AreEqual(2, svcinfo.Methods.Length);
		}

		[Test]
		public void Double()
		{
			var type    = typeof(double);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tDouble, rpcType,
							"Double doesn't map to XmlRpcType.tDouble");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "double", "Double doesn't map to 'double'");
		}

		[Test]
		public void DupXmlRpcNames()
		{
			Assert.That(() => XmlRpcServiceInfo.CreateServiceInfo(
				typeof(IDupXmlRpcNames)), Throws.TypeOf<XmlRpcDupXmlRpcMethodNames>());
		}

		[Test]
		public void Int32()
		{
			var type    = typeof(int);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tInt32, rpcType,
							"Int32 doesn't map to XmlRpcType.tInt32");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "integer", "Int32 doesn't map to 'integer'");
		}

		[Test]
		public void Int64()
		{
			var type    = typeof(long);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tInt64, rpcType,
							"Int64 doesn't map to XmlRpcType.tInt64");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "i8", "Int64 doesn't map to 'i8'");
		}

		[Test]
		public void IntArray()
		{
			var type    = typeof(int[]);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tArray, rpcType,
							"Int32[] doesn't map to XmlRpcType.tArray");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "array", "Int32[] doesn't map to 'array'");
		}


		[Test]
		public void JaggedIntArray()
		{
			var type    = typeof(int[][]);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tArray, rpcType,
							"Int32[] doesn't map to XmlRpcType.tArray");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "array", "Int32[] doesn't map to 'array'");
		}

		[Test]
		public void MultiDimIntArray()
		{
			var type    = typeof(int[,]);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tMultiDimArray, rpcType,
							"Int32[] doesn't map to XmlRpcType.tMultiDimArray");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "array", "Int32['] doesn't map to 'array'");
		}

		[Test]
		public void PropertyMember()
		{
			var info = XmlRpcServiceInfo.CreateServiceInfo(
				typeof(struct3));
		}

		[Test]
		public void RecursiveArrayClass()
		{
			var type    = typeof(ExampleWithArray);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tStruct, rpcType);
		}

		[Test]
		public void RecursiveClass()
		{
			var type    = typeof(Example);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tStruct, rpcType);
		}

		[Test]
		public void String()
		{
			var type    = typeof(string);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tString, rpcType,
							"String doesn't map to XmlRpcType.tString");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "string", "String doesn't map to 'string'");
		}

		[Test]
		public void Struct()
		{
			var type    = typeof(struct1);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tStruct, rpcType,
							"struct doesn't map to XmlRpcType.tStruct");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "struct",
							"struct doesn't map to 'struct'");
		}

		[Test]
		public void StructWithEnum()
		{
			var type    = typeof(struct2);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tInvalid, rpcType,
							"struct doesn't map to XmlRpcType.tInvalid");
		}

		[Test]
		public void Void()
		{
			var type    = typeof(void);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tVoid, rpcType,
							"void doesn't map to XmlRpcType.tVoid");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "void", "void doesn't map to 'void'");
		}

		[Test]
		public void XmlRpcBoolean()
		{
			var type    = typeof(XmlRpcBoolean);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tBoolean, rpcType,
							"XmlRpcBoolean doesn't map to XmlRpcType.tBoolean");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "boolean",
							"XmlRpcBoolean doesn't map to 'boolean'");
		}

		[Test]
		public void XmlRpcDateTime()
		{
			var type    = typeof(XmlRpcDateTime);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tDateTime, rpcType,
							"XmlRpcDateTime doesn't map to XmlRpcType.tDateTime");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "dateTime",
							"XmlRpcDateTime doesn't map to 'dateTime'");
		}

		[Test]
		public void XmlRpcDouble()
		{
			var type    = typeof(XmlRpcDouble);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tDouble, rpcType,
							"XmlRpcDouble doesn't map to XmlRpcType.tDouble");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "double",
							"XmlRpcDouble doesn't map to 'double'");
		}

		[Test]
		public void XmlRpcInt()
		{
			var type    = typeof(XmlRpcInt);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tInt32, rpcType,
							"XmlRpcInt doesn't map to XmlRpcType.tInt32");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "integer",
							"XmlRpcInt doesn't map to 'integer'");
		}

		[Test]
		public void XmlRpcStruct()
		{
			var type    = typeof(XmlRpcStruct);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tHashtable, rpcType,
							"XmlRpcStruct doesn't map to XmlRpcType.tHashtable");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "struct",
							"XmlRpcStruct doesn't map to 'struct'");
		}

#if !FX1_0
		[Test]
		public void NullableInt()
		{
			var type    = typeof(int?);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tInt32, rpcType,
							"int? doesn't map to XmlRpcType.tInt32");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "integer",
							"int? doesn't map to 'integer'");
		}

		[Test]
		public void NullableIn64()
		{
			var type    = typeof(long?);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tInt64, rpcType,
							"long? doesn't map to XmlRpcType.tInt64");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "i8",
							"long? doesn't map to 'i8'");
		}

		[Test]
		public void NullableBool()
		{
			var type    = typeof(bool?);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tBoolean, rpcType,
							"bool? doesn't map to XmlRpcType.tBoolean");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "boolean",
							"bool? doesn't map to 'boolean'");
		}

		[Test]
		public void NullableDouble()
		{
			var type    = typeof(double?);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tDouble, rpcType,
							"double? doesn't map to XmlRpcType.tDouble");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "double",
							"double? doesn't map to 'double'");
		}

		[Test]
		public void NullableDateTime()
		{
			var type    = typeof(DateTime?);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tDateTime, rpcType,
							"DateTime? doesn't map to XmlRpcType.tDateTime");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "dateTime",
							"DateTime? doesn't map to 'dateTime'");
		}

		private struct TestStruct
		{
			public int x;
		}

		[Test]
		public void NullableStruct()
		{
			var type    = typeof(TestStruct?);
			var rpcType = XmlRpcServiceInfo.GetXmlRpcType(type);
			Assert.AreEqual(XmlRpcType.tStruct, rpcType,
							"TestStruct? doesn't map to XmlRpcType.tStruct");
			var rpcString = XmlRpcServiceInfo.GetXmlRpcTypeString(type);
			Assert.AreEqual(rpcString, "struct",
							"TestStruct? doesn't map to 'struct'");
		}
#endif
	}

	internal interface IFoo
	{
		[XmlRpcMethod("IFoo.Foo", Description = "IFoo")]
		int Foo(int x);
	}

	internal interface IBar
	{
		[XmlRpcMethod("IBar.Foo", Description = "IFooBar")]
		int Foo(int x);
	}

	internal class FooBar : IFoo, IBar
	{
		int IBar.Foo(int x)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		int IFoo.Foo(int x)
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	internal interface IDupXmlRpcNames
	{
		[XmlRpcMethod("bad.Foo")]
		int Foo1(int x);

		[XmlRpcMethod("bad.Foo")]
		int Foo2(int x);
	}
}