using System;
using CookComputing.XmlRpc;
using NUnit.Framework;

// TODO: parse array
// TODO: parse struct
// TODO: parse XmlRpcStruct
// TODO: parse XmlRpcStruct derived
// TODO: array of base64

namespace ntest
{
	[TestFixture]
	public class ParseTest
	{
		private struct Struct2
		{
			public int            mi;
			public string         ms;
			public bool           mb;
			public double         md;
			public DateTime       mdt;
			public byte[]         mb64;
			public int[]          ma;
			public XmlRpcInt      xi;
			public XmlRpcBoolean  xb;
			public XmlRpcDouble   xd;
			public XmlRpcDateTime xdt;
			public XmlRpcStruct   xstr;
		}

		//---------------------- base64 ----------------------------------------// 
		private readonly byte[] testb =
		{
			121, 111, 117, 32, 99, 97, 110, 39, 116, 32, 114, 101, 97, 100,
			32, 116, 104, 105, 115, 33
		};

		//------------------------------------------------------------------------// 
		private struct Struct3
		{
			[XmlRpcMember("IntField")] public int intOne;

			[XmlRpcMember("IntProperty")]
			public int intTwo
			{
				get => _intTwo;
				set => _intTwo = value;
			}

			private int _intTwo;
		}


		private struct Struct4
		{
			[NonSerialized] public int x;
			public                 int y;
		}


		private struct Struct5
		{
			public int x;
		}


		private struct Struct6
		{
			[NonSerialized] public decimal x;
			public                 int     y;
		}

		private class RecursiveMember
		{
			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public RecursiveMember childExample;

			public string Level;
		}

		private class RecursiveArrayMember
		{
			public RecursiveArrayMember[] childExamples;
			public string                 Level;
		}

		[Test]
		public void Base64_Base64Type()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><base64>eW91IGNhbid0IHJlYWQgdGhpcyE=</base64></value>";
			var obj = Utils.Parse(xml, typeof(byte[]), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is byte[], "result is array of byte");
			var ret = obj as byte[];
			Assert.IsTrue(ret.Length == testb.Length);
			for (var i = 0; i < testb.Length; i++)
				Assert.IsTrue(testb[i] == ret[i]);
		}

		[Test]
		public void Base64_NullType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><base64>eW91IGNhbid0IHJlYWQgdGhpcyE=</base64></value>";
			var obj = Utils.Parse(xml, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is byte[], "result is array of byte");
			var ret = obj as byte[];
			Assert.IsTrue(ret.Length == testb.Length);
			for (var i = 0; i < testb.Length; i++)
				Assert.IsTrue(testb[i] == ret[i]);
		}

		[Test]
		public void Base64_ObjectType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><base64>eW91IGNhbid0IHJlYWQgdGhpcyE=</base64></value>";
			var obj = Utils.Parse(xml, typeof(object), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is byte[], "result is array of byte");
			var ret = obj as byte[];
			Assert.IsTrue(ret.Length == testb.Length);
			for (var i = 0; i < testb.Length; i++)
				Assert.IsTrue(testb[i] == ret[i]);
		}

		[Test]
		public void Boolean_BooleanType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><boolean>1</boolean></value>";
			var obj = Utils.Parse(xml, typeof(bool), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(true, (bool) obj);
		}

		//---------------------- boolean ---------------------------------------// 
		[Test]
		public void Boolean_NullType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><boolean>1</boolean></value>";
			var obj = Utils.Parse(xml, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(true, (bool) obj);
		}

		[Test]
		public void Boolean_ObjectType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><boolean>1</boolean></value>";
			var obj = Utils.Parse(xml, typeof(object), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(true, (bool) obj);
		}

		[Test]
		public void DateTime_allZeros1()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>0000-00-00T00:00:00</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.MapZerosDateTimeToMinValue;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error, serializer,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(DateTime.MinValue, (DateTime) obj);
		}

		[Test]
		public void DateTime_allZeros2()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>0000-00-00T00:00:00Z</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.MapZerosDateTimeToMinValue;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error, serializer,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(DateTime.MinValue, (DateTime) obj);
		}

		[Test]
		public void DateTime_allZeros3()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>00000000T00:00:00Z</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.MapZerosDateTimeToMinValue;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error, serializer,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(DateTime.MinValue, (DateTime) obj);
		}

		[Test]
		public void DateTime_allZeros4()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>0000-00-00T00:00:00</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.MapZerosDateTimeToMinValue;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error, serializer,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(DateTime.MinValue, (DateTime) obj);
		}

		[Test]
		public void DateTime_DateTimeType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37</dateTime.iso8601></value>";
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_Empty_NonStandard()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601></dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			Assert.That(() => Utils.Parse(xml, typeof(DateTime), MappingAction.Error, serializer,
										  out parsedType, out parsedArrayType),
						Throws.TypeOf<XmlRpcInvalidXmlRpcException>());
		}

		[Test]
		public void DateTime_Empty_Standard()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601></dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.MapEmptyDateTimeToMinValue;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error, serializer,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(DateTime.MinValue, (DateTime) obj);
		}

		//---------------------- dateTime ------------------------------------// 
		[Test]
		public void DateTime_NullType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37</dateTime.iso8601></value>";
			var obj = Utils.Parse(xml, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_ObjectTimeType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37</dateTime.iso8601></value>";
			var obj = Utils.Parse(xml, typeof(object), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_ROCA()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>2002-07-06T11:25:37</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error, serializer,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_TypePad()
		{
			// yyyy-MM-ddThh:mm:ssZ
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>2002-07-06T11:25:37Z</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  serializer, out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_TZMinus00()
		{
			// yyyyMMddThh:mm:ssZ+00
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37-00</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  serializer, out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_TZMinus0000()
		{
			// yyyyMMddThh:mm:ssZ+00
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37-0000</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  serializer, out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_TZMinus01()
		{
			// yyyyMMddThh:mm:ssZ+00
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T10:25:37-01</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  serializer, out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_TZMinus0130()
		{
			// yyyyMMddThh:mm:ssZ+00
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T09:55:37-0130</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  serializer, out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_TZMinus01Colon30()
		{
			// yyyyMMddThh:mm:ssZ+00
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T09:55:37-01:30</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  serializer, out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_TZPlus00()
		{
			// yyyyMMddThh:mm:ssZ+00
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37+00</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  serializer, out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_TZPlus0000()
		{
			// yyyyMMddThh:mm:ssZ+00
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37+0000</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  serializer, out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_TZPlus01()
		{
			// yyyyMMddThh:mm:ssZ+00
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T12:25:37+01</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  serializer, out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_TZPlus0130()
		{
			// yyyyMMddThh:mm:ssZ+00
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T12:55:37+0130</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  serializer, out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_TZPlus01Colon30()
		{
			// yyyyMMddThh:mm:ssZ+00
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T12:55:37+01:30</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  serializer, out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void DateTime_WordPress()
		{
			// yyyyMMddThh:mm:ssZ
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37Z</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  serializer, out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37),
							(DateTime) obj);
		}

		[Test]
		public void Default1EmptyString_StringType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value></value>";
			var obj = Utils.Parse(xml, typeof(string), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual("", (string) obj);
		}

		[Test]
		public void Default2EmptyString_StringType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value/>";
			var obj = Utils.Parse(xml, typeof(string), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual("", (string) obj);
		}

		[Test]
		public void DefaultString_NullType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value>astring</value>";
			var obj = Utils.Parse(xml, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual("astring", (string) obj);
		}

		[Test]
		public void DefaultString_StringType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value>astring</value>";
			var obj = Utils.Parse(xml, typeof(string), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual("astring", (string) obj);
		}

		[Test]
		public void Double_DoubleType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><double>543.21</double></value>";
			var obj = Utils.Parse(xml, typeof(double), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(543.21, (double) obj);
		}

		//---------------------- double ----------------------------------------// 
		[Test]
		public void Double_NullType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><double>543.21</double></value>";
			var obj = Utils.Parse(xml, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(543.21, (double) obj);
		}

		[Test]
		public void Double_ObjectType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><double>543.21</double></value>";
			var obj = Utils.Parse(xml, typeof(double), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(543.21, (double) obj);
		}

		[Test]
		public void Empty1String_StringType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><string></string></value>";
			var obj = Utils.Parse(xml, typeof(string), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual("", (string) obj);
		}

		[Test]
		public void Empty2String_StringType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><string/></value>";
			var obj = Utils.Parse(xml, typeof(string), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual("", (string) obj);
		}

		[Test]
		public void HomogArray_IntArrayType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><i4>13</i4></value>
      <value><i4>14</i4></value>
    </data>
  </array>
</value>";
			var obj = Utils.Parse(xml, typeof(int[]), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is int[], "result is array of int");
			var ret = obj as int[];
			Assert.AreEqual(12, ret[0]);
			Assert.AreEqual(13, ret[1]);
			Assert.AreEqual(14, ret[2]);
		}

		[Test]
		public void HomogArray_NullType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><i4>13</i4></value>
      <value><i4>14</i4></value>
    </data>
  </array>
</value>";
			var obj = Utils.Parse(xml, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is int[], "result is array of int");
			var ret = obj as int[];
			Assert.AreEqual(12, ret[0]);
			Assert.AreEqual(13, ret[1]);
			Assert.AreEqual(14, ret[2]);
		}

		[Test]
		public void HomogArray_ObjectArrayType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><i4>13</i4></value>
      <value><i4>14</i4></value>
    </data>
  </array>
</value>";
			var obj = Utils.Parse(xml, typeof(object[]), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is object[], "result is array of object");
			var ret = obj as object[];
			Assert.AreEqual(12, ret[0]);
			Assert.AreEqual(13, ret[1]);
			Assert.AreEqual(14, ret[2]);
		}

		[Test]
		public void HomogArray_ObjectType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><i4>13</i4></value>
      <value><i4>14</i4></value>
    </data>
  </array>
</value>";
			var obj = Utils.Parse(xml, typeof(object), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is int[], "result is array of int");
			var ret = obj as int[];
			Assert.AreEqual(12, ret[0]);
			Assert.AreEqual(13, ret[1]);
			Assert.AreEqual(14, ret[2]);
		}

		[Test]
		public void IgnoreNonSerialized()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <struct>
    <member>
      <name>y</name>
      <value><i4>18</i4></value>
    </member>
  </struct>
</value>";
			var obj = Utils.Parse(xml, typeof(Struct4), MappingAction.Error,
								  out parsedType, out parsedArrayType);
		}

		[Test]
		public void Int_IntType()
		{
			Type parsedType, parsedArrayType;
			var  xml = @"<?xml version=""1.0"" ?><value><int>12345</int></value>";
			var obj = Utils.Parse(xml, typeof(int), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(12345, (int) obj);
		}

		//---------------------- int -------------------------------------------// 
		[Test]
		public void Int_NullType()
		{
			Type parsedType, parsedArrayType;
			var  xml = @"<?xml version=""1.0"" ?><value><int>12345</int></value>";
			var obj = Utils.Parse(xml, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(12345, (int) obj);
		}

		[Test]
		public void Int_ObjectType()
		{
			Type parsedType, parsedArrayType;
			var  xml = @"<?xml version=""1.0"" ?><value><int>12345</int></value>";
			var obj = Utils.Parse(xml, typeof(object), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(12345, (int) obj);
		}

		//---------------------- Int64 -------------------------------------------// 
		[Test]
		public void Int_TooLarge()
		{
			Type parsedType, parsedArrayType;
			var  xml = @"<?xml version=""1.0"" ?><value><int>123456789012</int></value>";
			Assert.That(() => Utils.Parse(xml, typeof(int), MappingAction.Error,
										  out parsedType, out parsedArrayType),
						Throws.TypeOf<XmlRpcInvalidXmlRpcException>());
		}

		[Test]
		public void Int64_IntType()
		{
			Type parsedType, parsedArrayType;
			var  xml = @"<?xml version=""1.0"" ?><value><i8>123456789012</i8></value>";
			var obj = Utils.Parse(xml, typeof(long), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(123456789012, (long) obj);
		}

		[Test]
		public void Int64_NullType()
		{
			Type parsedType, parsedArrayType;
			var  xml = @"<?xml version=""1.0"" ?><value><i8>123456789012</i8></value>";
			var obj = Utils.Parse(xml, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(123456789012, (long) obj);
		}

		[Test]
		public void Int64_ObjectType()
		{
			Type parsedType, parsedArrayType;
			var  xml = @"<?xml version=""1.0"" ?><value><i8>123456789012</i8></value>";
			var obj = Utils.Parse(xml, typeof(object), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(123456789012, (long) obj);
		}

		[Test]
		public void Issue72()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20090209T22:20:01+01:00</dateTime.iso8601></value>";
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var obj = Utils.Parse(xml, typeof(DateTime), MappingAction.Error,
								  serializer, out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2009, 2, 9, 21, 20, 01),
							(DateTime) obj);
		}

		//---------------------- array -----------------------------------------// 
		[Test]
		public void MixedArray_NullType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><string>Egypt</string></value>
      <value><boolean>0</boolean></value>
    </data>
  </array>
</value>";
			var obj = Utils.Parse(xml, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is object[], "result is array of object");
			var ret = obj as object[];
			Assert.AreEqual(12, ret[0]);
			Assert.AreEqual("Egypt", ret[1]);
			Assert.AreEqual(false, ret[2]);
		}

		[Test]
		public void MixedArray_ObjectArrayType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><string>Egypt</string></value>
      <value><boolean>0</boolean></value>
    </data>
  </array>
</value>";
			var obj = Utils.Parse(xml, typeof(object[]), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is object[], "result is array of object");
			var ret = obj as object[];
			Assert.AreEqual(12, ret[0]);
			Assert.AreEqual("Egypt", ret[1]);
			Assert.AreEqual(false, ret[2]);
		}

		[Test]
		public void MixedArray_ObjectType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value><i4>12</i4></value>
      <value><string>Egypt</string></value>
      <value><boolean>0</boolean></value>
    </data>
  </array>
</value>";
			var obj = Utils.Parse(xml, typeof(object), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is object[], "result is array of object");
			var ret = obj as object[];
			Assert.AreEqual(12, ret[0]);
			Assert.AreEqual("Egypt", ret[1]);
			Assert.AreEqual(false, ret[2]);
		}

		//---------------------- struct ------------------------------------------// 
		[Test]
		public void NameEmptyString()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <struct>
    <member>
      <name/>
      <value><i4>18</i4></value>
    </member>
  </struct>
</value>";
			Assert.That(() => Utils.Parse(xml, null, MappingAction.Error,
										  out parsedType, out parsedArrayType),
						Throws.TypeOf<XmlRpcInvalidXmlRpcException>());
		}

		[Test]
		public void NonSerializedInStruct()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <struct>
    <member>
      <name>x</name>
      <value><i4>12</i4></value>
    </member>
    <member>
      <name>y</name>
      <value><i4>18</i4></value>
    </member>
  </struct>
</value>";
			var obj = Utils.Parse(xml, typeof(Struct4), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			var ret = (Struct4) obj;
			Assert.That(ret.x, Throws.TypeOf<XmlRpcNonSerializedMember>());
			//Assert.AreEqual(0, ret.x);
			//Assert.AreEqual(18, ret.y);
		}

		[Test]
		public void NonSerializedNonXmlRpcType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <struct>
    <member>
      <name>y</name>
      <value><i4>18</i4></value>
    </member>
  </struct>
</value>";
			var obj = Utils.Parse(xml, typeof(Struct4), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			var ret = (Struct4) obj;
			Assert.AreEqual(0, ret.x);
			Assert.AreEqual(18, ret.y);
		}

		[Test]
		public void PropertyXmlRpcName()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <struct>
    <member>
      <name>IntField</name>
      <value><i4>18</i4></value>
    </member>
    <member>
      <name>IntProperty</name>
      <value><i4>18</i4></value>
    </member>
  </struct>
</value>";
			var obj = Utils.Parse(xml, typeof(Struct3), MappingAction.Error,
								  out parsedType, out parsedArrayType);
		}

		[Test]
		public void RecursiveArrayMemberTest()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <struct>
    <member>
      <name>Level</name>
      <value>1</value>
    </member>
    <member>
      <name>childExamples</name>
      <value>
       <array>
          <data>
            <value>
              <struct>
                <member>
                  <name>Level</name>
                  <value>1-1</value>
                </member>
                <member>
                  <name>childExamples</name>
                  <value>
                   <array>
                      <data>
                        <value>
                          <struct>
                            <member>
                              <name>Level</name>
                              <value>1-1-1</value>
                            </member>
                            <member>
                              <name>childExamples</name>
                              <value>
                               <array>
                                  <data>
                                  </data>
                                </array>
                              </value>
                            </member>
                          </struct>
                        </value>
                        <value>
                          <struct>
                            <member>
                              <name>Level</name>
                              <value>1-1-2</value>
                            </member>
                            <member>
                              <name>childExamples</name>
                              <value>
                               <array>
                                  <data>
                                  </data>
                                </array>
                              </value>
                            </member>
                          </struct>
                        </value>
                      </data>
                    </array>
                  </value>
                </member>
              </struct>
            </value>
            <value>
              <struct>
                <member>
                  <name>Level</name>
                  <value>1-2</value>
                </member>
                <member>
                  <name>childExamples</name>
                  <value>
                   <array>
                      <data>
                        <value>
                          <struct>
                            <member>
                              <name>Level</name>
                              <value>1-2-1</value>
                            </member>
                            <member>
                              <name>childExamples</name>
                              <value>
                               <array>
                                  <data>
                                  </data>
                                </array>
                              </value>
                            </member>
                          </struct>
                        </value>
                        <value>
                          <struct>
                            <member>
                              <name>Level</name>
                              <value>1-2-2</value>
                            </member>
                            <member>
                              <name>childExamples</name>
                              <value>
                               <array>
                                  <data>
                                  </data>
                                </array>
                              </value>
                            </member>
                          </struct>
                        </value>
                      </data>
                    </array>
                  </value>
                </member>
              </struct>
            </value>
          </data>
        </array>
      </value>
    </member>
  </struct>
</value>";
			var obj = Utils.Parse(xml, typeof(RecursiveArrayMember), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			var ret = (RecursiveArrayMember) obj;
		}

		[Test]
		public void RecursiveMemberTest()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <struct>
    <member>
      <name>Level</name>
      <value>1</value>
    </member>
    <member>
      <name>childExample</name>
      <value>
        <struct>
          <member>
            <name>Level</name>
            <value>2</value>
          </member>
          <member>
            <name>childExample</name>
            <value>
              <struct>
                <member>
                  <name>Level</name>
                  <value>3</value>
                </member>
              </struct>
            </value>
          </member>
        </struct>
      </value>
    </member>
  </struct>
</value>";
			var obj = Utils.Parse(xml, typeof(RecursiveMember), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			var ret = (RecursiveMember) obj;
		}

		//---------------------- string ----------------------------------------// 
		[Test]
		public void String_NullType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><string>astring</string></value>";
			var obj = Utils.Parse(xml, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual("astring", (string) obj);
		}

		[Test]
		public void String_ObjectType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><string>astring</string></value>";
			var obj = Utils.Parse(xml, typeof(object), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual("astring", (string) obj);
		}

		[Test]
		public void String_StringType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><string>astring</string></value>";
			var obj = Utils.Parse(xml, typeof(string), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual("astring", (string) obj);
		}

		[Test]
		public void UnexpectedMember()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <struct>
    <member>
      <name>x</name>
      <value><i4>12</i4></value>
    </member>
    <member>
      <name>y</name>
      <value><i4>18</i4></value>
    </member>
  </struct>
</value>";
			var obj = Utils.Parse(xml, typeof(Struct5), MappingAction.Error,
								  out parsedType, out parsedArrayType);
		}

		//---------------------- XmlRpcBoolean ---------------------------------// 
		[Test]
		public void XmlRpcBoolean_XmlRpcBooleanType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><boolean>1</boolean></value>";
			var obj = Utils.Parse(xml, typeof(XmlRpcBoolean), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(new XmlRpcBoolean(true), (XmlRpcBoolean) obj);
		}

		//---------------------- XmlRpcDateTime --------------------------------// 
		[Test]
		public void XmlRpcDateTime_XmlRpcDateTimeType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37</dateTime.iso8601></value>";
			var obj = Utils.Parse(xml, typeof(XmlRpcDateTime), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(
				new XmlRpcDateTime(new DateTime(2002, 7, 6, 11, 25, 37)),
				(XmlRpcDateTime) obj);
		}

		//---------------------- XmlRpcDouble ----------------------------------// 
		[Test]
		public void XmlRpcDouble_XmlRpcDoubleType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><double>543.21</double></value>";
			var obj = Utils.Parse(xml, typeof(XmlRpcDouble), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(new XmlRpcDouble(543.21), (XmlRpcDouble) obj);
		}

		//---------------------- XmlRpcInt -------------------------------------// 
		[Test]
		public void XmlRpcInt_XmlRpcIntType()
		{
			Type parsedType, parsedArrayType;
			var  xml = @"<?xml version=""1.0"" ?><value><int>12345</int></value>";
			var obj = Utils.Parse(xml, typeof(XmlRpcInt), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsInstanceOf<XmlRpcInt>(obj);
			Assert.AreEqual(12345, (XmlRpcInt) obj);
		}

		//---------------------- XmlRpcStruct array ----------------------------// 
		[Test]
		public void XmlRpcStructArray()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <array>
    <data>
      <value>
        <struct>
          <member>
            <name>mi</name>
            <value><i4>18</i4></value>
          </member>
        </struct>
      </value>
      <value>
        <struct>
          <member>
            <name>mi</name>
            <value><i4>28</i4></value>
          </member>
        </struct>
      </value>
    </data>
  </array>
</value>";

			var obj = Utils.Parse(xml, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);

			Assert.AreEqual(obj.GetType(), typeof(object[]));
			var objarray = (object[]) obj;
			Assert.AreEqual(objarray[0].GetType(), typeof(XmlRpcStruct));
			Assert.AreEqual(objarray[1].GetType(), typeof(XmlRpcStruct));
			var xstruct1 = objarray[0] as XmlRpcStruct;
			var xstruct2 = objarray[1] as XmlRpcStruct;

			Assert.AreEqual(xstruct1["mi"], 18);
			Assert.AreEqual(xstruct2["mi"], 28);
		}

		[Test]
		public void XmlRpcStructOrder()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
<value>
  <struct>
    <member>
      <name>a</name>
      <value><i4>1</i4></value>
    </member>
    <member>
      <name>c</name>
      <value><i4>3</i4></value>
    </member>
    <member>
      <name>b</name>
      <value><i4>2</i4></value>
    </member>
  </struct>
</value>";
			var obj = Utils.Parse(xml, typeof(XmlRpcStruct), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsInstanceOf<XmlRpcStruct>(obj);
			var strct       = obj as XmlRpcStruct;
			var denumerator = strct.GetEnumerator();
			denumerator.MoveNext();
			Assert.AreEqual("a", denumerator.Key);
			Assert.AreEqual(1, denumerator.Value);
			denumerator.MoveNext();
			Assert.AreEqual("c", denumerator.Key);
			Assert.AreEqual(3, denumerator.Value);
			denumerator.MoveNext();
			Assert.AreEqual("b", denumerator.Key);
			Assert.AreEqual(2, denumerator.Value);
		}

#if !FX1_0
		//---------------------- int? -------------------------------------// 
		[Test]
		public void nullableIntType()
		{
			Type parsedType, parsedArrayType;
			var  xml = @"<?xml version=""1.0"" ?><value><int>12345</int></value>";
			var obj = Utils.Parse(xml, typeof(int?), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsInstanceOf<int?>(obj);
			Assert.AreEqual(12345, obj);
		}

		//---------------------- bool? ---------------------------------// 
		[Test]
		public void nullableBoolType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><boolean>1</boolean></value>";
			var obj = Utils.Parse(xml, typeof(bool?), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(true, obj);
		}

		//---------------------- double? ----------------------------------// 
		[Test]
		public void nullableDoubleType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><double>543.21</double></value>";
			var obj = Utils.Parse(xml, typeof(double?), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(543.21, obj);
		}

		//---------------------- DateTime? --------------------------------// 
		[Test]
		public void nullableDateTimeType()
		{
			Type parsedType, parsedArrayType;
			var xml = @"<?xml version=""1.0"" ?>
        <value><dateTime.iso8601>20020706T11:25:37</dateTime.iso8601></value>";
			var obj = Utils.Parse(xml, typeof(DateTime?), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37), obj);
		}
#endif
	}
}