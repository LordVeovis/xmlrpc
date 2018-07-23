using System;
using System.Globalization;
using System.IO;
using System.Threading;
using CookComputing.XmlRpc;
using NUnit.Framework;

namespace ntest
{
	[TestFixture]
	public class DeserializeRequestTest
	{
		[XmlRpcMethod]
		public string MethodNoArgs()
		{
			return "";
		}

		private struct Struct3
		{
			private int _member1;

			public int member1
			{
				get => _member1;
				set => _member1 = value;
			}

			private int _member2;
			public  int member2 => _member2;

			private int _member3;

			[XmlRpcMember("member-3")]
			public int member3
			{
				get => _member3;
				set => _member3 = value;
			}

			private int _member4;

			[XmlRpcMember("member-4")]
			public int member4 => _member4;
		}


		//    // test array handling
		//
		//
		//
		//    // tests of handling of structs
		//    public void testMissingMemberStruct()
		//    {
		//      string xml = @"<?xml version=""1.0"" ?> 
		//<methodCall>
		//  <methodName>TestStruct</methodName> 
		//  <params>
		//    <param>
		//    </param>
		//  </params>
		//</methodCall>";
		//      StringReader sr = new StringReader(xml);
		//      XmlRpcSerializer serializer = new XmlRpcSerializer();
		//      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
		//    }
		//
		//    public void testAdditonalMemberStruct()
		//    {
		//      string xml = @"<?xml version=""1.0"" ?> 
		//<methodCall>
		//  <methodName>TestStruct</methodName> 
		//  <params>
		//    <param>
		//    </param>
		//  </params>
		//</methodCall>";
		//      StringReader sr = new StringReader(xml);
		//      XmlRpcSerializer serializer = new XmlRpcSerializer();
		//      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
		//    }
		//
		//    public void testReversedMembersStruct()
		//    {
		//      string xml = @"<?xml version=""1.0"" ?> 
		//<methodCall>
		//  <methodName>TestStruct</methodName> 
		//  <params>
		//    <param>
		//    </param>
		//  </params>
		//</methodCall>";
		//      StringReader sr = new StringReader(xml);
		//      XmlRpcSerializer serializer = new XmlRpcSerializer();
		//      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
		//    }
		//    
		//    public void testWrongTypeMembersStruct()
		//    {
		//      string xml = @"<?xml version=""1.0"" ?> 
		//<methodCall>
		//  <methodName>TestStruct</methodName> 
		//  <params>
		//    <param>
		//    </param>
		//  </params>
		//</methodCall>";
		//      StringReader sr = new StringReader(xml);
		//      XmlRpcSerializer serializer = new XmlRpcSerializer();
		//      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
		//    }
		//
		//    public void testDuplicateMembersStruct()
		//    {
		//      string xml = @"<?xml version=""1.0"" ?> 
		//<methodCall>
		//  <methodName>TestStruct</methodName> 
		//  <params>
		//    <param>
		//    </param>
		//  </params>
		//</methodCall>";
		//      StringReader sr = new StringReader(xml);
		//      XmlRpcSerializer serializer = new XmlRpcSerializer();
		//      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
		//    }
		//
		//    public void testNonAsciiMemberNameStruct()
		//    {
		//      string xml = @"<?xml version=""1.0"" ?> 
		//<methodCall>
		//  <methodName>TestStruct</methodName> 
		//  <params>
		//    <param>
		//    </param>
		//  </params>
		//</methodCall>";
		//      StringReader sr = new StringReader(xml);
		//      XmlRpcSerializer serializer = new XmlRpcSerializer();
		//      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
		//    }
		//
		//    // test various invalid requests
		//    public void testIncorrectParamType()
		//    {
		//      string xml = @"<?xml version=""1.0"" ?> 
		//<methodCall>
		//  <methodName>TestStruct</methodName> 
		//  <params>
		//    <param>
		//    </param>
		//  </params>
		//</methodCall>";
		//      StringReader sr = new StringReader(xml);
		//      XmlRpcSerializer serializer = new XmlRpcSerializer();
		//      XmlRpcRequest request = serializer.DeserializeRequest(sr, null);
		//    }


		public class TestClass
		{
			public int    _int;
			public string _string;
		}

		[XmlRpcMethod]
		public void TestClassMethod(TestClass testClass) { }


		public struct simple
		{
			public int    number;
			public string detail;
		}

		[XmlRpcMethod("rtx.useArrayOfStruct")]
		public string UseArrayOfStruct(simple[] myarr)
		{
			return "";
		}

		[XmlRpcMethod("rtx.EchoString")]
		public string EchoString(string str)
		{
			return str;
		}

		[XmlRpcMethod("blogger.getUsersBlogs")]
		public void GetUsersBlogs(string username, string password) { }

		[Test]
		public void Base64()
		{
			var xml = @"<?xml version=""1.0""?>
<methodCall>
  <methodName>TestHex</methodName>
  <params>
    <param>
      <value>
        <base64>AQIDBAUGBwg=</base64>
      </value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.AreEqual(request.args[0].GetType(), typeof(byte[]),
							"argument is byte[]");
			var ret = (byte[]) request.args[0];
			Assert.AreEqual(8, ret.Length, "argument is byte[8]");
			for (var i = 0; i < ret.Length; i++)
				Assert.AreEqual(i + 1, ret[i], "members are 1 to 8");
		}


		//    // test handling double values
		//
		//
		//    // test handling string values
		//


		//
		//    // test handling base64 values
		[Test]
		public void Base64Empty()
		{
			var xml = @"<?xml version=""1.0""?>
<methodCall>
  <methodName>TestHex</methodName>
  <params>
    <param>
      <value>
        <base64></base64>
      </value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.AreEqual(request.args[0].GetType(), typeof(byte[]),
							"argument is byte[]");
			Assert.AreEqual(request.args[0], new byte[0],
							"argument is zero length byte[]");
		}

		[Test]
		public void Base64MultiLine()
		{
			var xml = @"<?xml version=""1.0""?>
<methodCall>
  <methodName>TestHex</methodName>
  <params>
    <param>
      <value>
        <base64>AQIDBAUGBwgJ
AQIDBAUGBwg=</base64>
      </value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.AreEqual(request.args[0].GetType(), typeof(byte[]),
							"argument is byte[]");
			var ret = (byte[]) request.args[0];
			Assert.AreEqual(17, ret.Length, "argument is byte[17]");
			for (var i = 0; i < 9; i++)
				Assert.AreEqual(i + 1, ret[i], "first 9 members are 1 to 9");
			for (var i = 0; i < 8; i++)
				Assert.AreEqual(i + 1, ret[i + 9], "last 8 members are 1 to 9");
		}

		[Test]
		public void Blakemore()
		{
			var xml = @"<?xml version=""1.0""?>
<methodCall><methodName>rtx.useArrayOfStruct</methodName>
<params>
<param><value><array>
<data><value>
<struct><member><name>detail</name><value><string>elephant</string></value></member><member><name>number</name><value><int>76</int></value></member></struct>
</value></data>
<data><value>
<struct><member><name>detail</name><value><string>rhino</string></value></member><member><name>number</name><value><int>33</int></value></member></struct>
</value></data>
<data><value>
<struct><member><name>detail</name><value><string>porcupine</string></value></member><member><name>number</name><value><int>106</int></value></member></struct>
</value></data>
</array></value></param>
</params></methodCall>";

			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());

			Assert.AreEqual(request.args[0].GetType(), typeof(simple[]),
							"argument is simple[]");
			Assert.IsTrue((request.args[0] as simple[]).Length == 1,
						  "argument is simple[] of length 1");
		}

		[Test]
		public void Class()
		{
			var xml = @"<?xml version=""1.0""?>
<methodCall>
  <methodName>TestClassMethod</methodName>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>_int</name>
            <value>
              <i4>456</i4>
            </value>
          </member>
          <member>
            <name>_string</name>
            <value>
              <string>Test Class</string>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodCall>";

			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());

			Assert.AreEqual(request.args[0].GetType(), typeof(TestClass),
							"argument is TestClass");
//      XmlRpcStruct xrs = (XmlRpcStruct)request.args[0];
//      Assert.IsTrue(xrs.Count == 4, "XmlRpcStruct has 4 members");
//      Assert.IsTrue(xrs.ContainsKey("member1") && (int)xrs["member1"] == 1, 
//        "member1");
//      Assert.IsTrue(xrs.ContainsKey("member2") && (int)xrs["member2"] == 2, 
//        "member2");
//      Assert.IsTrue(xrs.ContainsKey("member-3") && (int)xrs["member-3"] == 3,
//        "member-3");
//      Assert.IsTrue(xrs.ContainsKey("member-4") && (int)xrs["member-4"] == 4,
//        "member-4");
		}

		// test handling dateTime values


		[Test]
		public void DateTimeFormats()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName>TestDateTime</methodName> 
<params>
  <param>
    <value><dateTime.iso8601>20020707T11:25:37Z</dateTime.iso8601></value>
  </param>
  <param>
    <value><dateTime.iso8601>20020707T11:25:37</dateTime.iso8601></value>
  </param>
  <param>
    <value><dateTime.iso8601>2002-07-07T11:25:37Z</dateTime.iso8601></value>
  </param>
  <param>
    <value><dateTime.iso8601>2002-07-07T11:25:37</dateTime.iso8601></value>
  </param>
</params>
</methodCall>";

			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			serializer.NonStandard = XmlRpcNonStandard.AllowNonStandardDateTime;
			var request = serializer.DeserializeRequest(sr, null);

			Assert.IsTrue(request.args[0] is DateTime, "argument is DateTime");
			var dt0 = (DateTime) request.args[0];
			var dt1 = (DateTime) request.args[1];
			var dt2 = (DateTime) request.args[2];
			var dt3 = (DateTime) request.args[3];

			var dt = new DateTime(2002, 7, 7, 11, 25, 37);
			Assert.AreEqual(dt0, dt, "DateTime WordPress");
			Assert.AreEqual(dt0, dt, "DateTime XML-RPC spec");
			Assert.AreEqual(dt0, dt, "DateTime TypePad");
			Assert.AreEqual(dt0, dt, "DateTime other");
		}

		[Test]
		public void DateTimeLocales()
		{
			var oldci = Thread.CurrentThread.CurrentCulture;
			try
			{
				foreach (var locale in Utils.GetLocales())
					try
					{
						var ci = new CultureInfo(locale);
						Thread.CurrentThread.CurrentCulture = ci;
						if (ci.LCID == 0x401     // ar-SA  (Arabic - Saudi Arabia)
							|| ci.LCID == 0x465  // div-MV (Dhivehi - Maldives)
							|| ci.LCID == 0x41e) // th-TH  (Thai - Thailand)
							break;

						var dt = new DateTime(1900, 01, 02, 03, 04, 05);
						while (dt < DateTime.Now)
						{
							Stream stm = new MemoryStream();
							var    req = new XmlRpcRequest();
							req.args   = new object[] {dt};
							req.method = "Foo";
							var ser = new XmlRpcSerializer();
							ser.SerializeRequest(stm, req);
							stm.Position = 0;

							var serializer = new XmlRpcSerializer();
							var request    = serializer.DeserializeRequest(stm, null);

							Assert.IsTrue(request.args[0] is DateTime,
										  "argument is DateTime");
							var dt0 = (DateTime) request.args[0];
							Assert.AreEqual(dt0, dt, "DateTime argument 0");
							dt += new TimeSpan(100, 1, 1, 1);
						}
					}
					catch (Exception ex)
					{
						Assert.Fail("unexpected exception {0}: {1}", locale, ex.Message);
					}
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = oldci;
			}
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void EmptyI8()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName>TestInt</methodName> 
<params>
  <param>
    <value><i8></i8></value>
  </param>
</params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void EmptyInteger()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName>TestInt</methodName> 
<params>
  <param>
    <value><i4></i4></value>
  </param>
</params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		[Test]
		public void EmptyLines()
		{
			var xml = @"<?xml version=""1.0""?>
<methodCall><methodName>rtx.EchoString</methodName>
<params>
<param><value>
</value></param>
</params></methodCall>";

			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());

			Assert.AreEqual(request.args[0].GetType(), typeof(string),
							"argument is string");
			Assert.AreEqual("\r\n", request.args[0]);
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void EmptyMethodName()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName/> 
<params>
  <param>
    <value>test string</value>
  </param>
</params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void EmptyParam1()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName>TestString</methodName> 
<params>
  <param/>
</params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void EmptyParam2()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName>TestString</methodName> 
<params>
  <param>
  </param>
</params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		[Test]
		[ExpectedException(typeof(XmlRpcIllFormedXmlException))]
		public void EmptyRequestStream()
		{
			var sr         = new StringReader("");
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		[Test]
		public void FlatXml()
		{
			var xml =
				@"<?xml version=""1.0"" ?><methodCall><methodName>TestString</methodName><params><param><value>test string</value></param></params></methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.AreEqual(request.method, "TestString", "method is TestString");
			Assert.AreEqual(request.args[0].GetType(), typeof(string),
							"argument is string");
			Assert.AreEqual((string) request.args[0], "test string",
							"argument is 'test string'");
		}

		[Test]
		public void I4Integer()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i4>666</i4></value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.IsTrue(request.method == "TestInt", "method is TestInt");
			Assert.AreEqual(request.args[0].GetType(), typeof(int),
							"argument is int");
			Assert.AreEqual((int) request.args[0], 666, "argument is 666");
		}

		// test handling i8 values
		[Test]
		public void I8()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value>
        <i8>123456789012</i8>
      </value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
			Assert.AreEqual(request.args[0].GetType(), typeof(long),
							"argument is long");
			Assert.AreEqual((long) request.args[0], 123456789012, "argument is 123456789012");
		}

		[Test]
		public void I8WithPlus()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i8>+123456789012</i8></value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
			Assert.AreEqual(request.args[0].GetType(), typeof(long),
							"argument is long");
			Assert.AreEqual((long) request.args[0], 123456789012, "argument is 123456789012");
		}

		// test handling integer values
		[Test]
		public void Integer()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value>
        <int>666</int>
      </value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.IsTrue(request.method == "TestInt", "method is TestInt");
			Assert.AreEqual(request.args[0].GetType(), typeof(int),
							"argument is int");
			Assert.AreEqual((int) request.args[0], 666, "argument is 666");
		}

		[Test]
		public void IntegerWithPlus()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i4>+666</i4></value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.IsTrue(request.method == "TestInt", "method is TestInt");
			Assert.AreEqual(request.args[0].GetType(), typeof(int),
							"argument is int");
			Assert.AreEqual((int) request.args[0], 666, "argument is 666");
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void InvalidI8()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName>TestInt</methodName> 
<params>
  <param>
    <value><i8>12kiol</i8></value>
  </param>
</params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void InvalidInteger()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName>TestInt</methodName> 
<params>
  <param>
    <value><i4>12kiol</i4></value>
  </param>
</params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
			Assert.Fail("Invalid integer should cause exception");
		}

		[Test]
		[ExpectedException(typeof(XmlRpcIllFormedXmlException))]
		public void InvalidXml()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall> </duffMmethodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}


		[Test]
		public void ISO_8859_1()
		{
			using (Stream stm = new FileStream("../iso-8859-1_request.xml",
											   FileMode.Open, FileAccess.Read))
			{
				var serializer = new XmlRpcSerializer();
				var request    = serializer.DeserializeRequest(stm, null);
				Assert.AreEqual(request.args[0].GetType(), typeof(string),
								"argument is string");
				Assert.AreEqual((string) request.args[0], "hæ hvað segirðu þá",
								"argument is 'hæ hvað segirðu þá'");
			}
		}

		[Test]
		public void LeadingSpace()
		{
			var xml = @"<?xml version=""1.0""?>
<methodCall><methodName>rtx.EchoString</methodName>
<params>
<param><value> ddd</value></param>
</params></methodCall>";

			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());

			Assert.AreEqual(request.args[0].GetType(), typeof(string),
							"argument is string");
			Assert.AreEqual(" ddd", request.args[0]);
		}

		// test handling of methodCall element
		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void MissingMethodCall()
		{
			var xml        = @"<?xml version=""1.0"" ?> <elem/>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		// test handling of methodName element
		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void MissingMethodName()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<params>
  <param>
    <value>test string</value>
  </param>
</params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		// test handling of params element
		[Test]
		public void MissingParams()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName>TestString</methodName> 
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		[Test]
		public void NegativeI8()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i8>-123456789012</i8></value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
			Assert.AreEqual(request.args[0].GetType(), typeof(long),
							"argument is long");
			Assert.AreEqual((long) request.args[0], -123456789012, "argument is -123456789012");
		}

		[Test]
		public void NegativeInteger()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i4>-666</i4></value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.IsTrue(request.method == "TestInt", "method is TestInt");
			Assert.AreEqual(request.args[0].GetType(), typeof(int),
							"argument is int");
			Assert.AreEqual((int) request.args[0], -666, "argument is -666");
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void NegativeOverflowI8()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName>TestInt</methodName> 
<params>
  <param>
    <value><i8>-9999999999999999999999999999999999999999999</i8></value>
  </param>
</params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void NegativeOverflowInteger()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName>TestInt</methodName> 
<params>
  <param>
    <value><i4>-99999999999999999999</i4></value>
  </param>
</params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		// test handling of params element
		[Test]
		public void NoParam1()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName>MethodNoArgs</methodName> 
<params/>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());
		}

		// test handling of param element
		[Test]
		public void NoParam2()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>MethodNoArgs</methodName> 
  <params>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());
			//Console.WriteLine("");
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NullRequestStream()
		{
			var    serializer = new XmlRpcSerializer();
			Stream stm        = null;
			var    request    = serializer.DeserializeRequest(stm, null);
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void OverflowI8()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName>TestInt</methodName> 
<params>
  <param>
    <value><i8>9999999999999999999999999999999999999999999</i8></value>
  </param>
</params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void OverflowInteger()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName>TestInt</methodName> 
<params>
  <param>
    <value><i4>99999999999999999999</i4></value>
  </param>
</params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}

		[Test]
		public void SingleSpaceString()
		{
			var xml = @"<?xml version=""1.0""?>
<methodCall><methodName>rtx.EchoString</methodName>
<params>
<param><value> </value></param>
</params></methodCall>";

			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());

			Assert.AreEqual(request.args[0].GetType(), typeof(string),
							"argument is string");
			Assert.AreEqual(" ", request.args[0],
							"argument is string containing single space");
		}

		[Test]
		public void StringElement()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestString</methodName> 
  <params>
    <param>
      <value><string>test string</string></value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.AreEqual(request.method, "TestString", "method is TestString");
			Assert.AreEqual(request.args[0].GetType(), typeof(string),
							"argument is string");
			Assert.AreEqual((string) request.args[0], "test string",
							"argument is 'test string'");
		}

		[Test]
		public void StringEmptyValue1()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestString</methodName> 
  <params>
    <param>
      <value></value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.AreEqual(request.method, "TestString", "method is TestString");
			Assert.AreEqual(request.args[0].GetType(), typeof(string),
							"argument is string");
			Assert.AreEqual((string) request.args[0], "", "argument is empty string");
		}

		[Test]
		public void StringEmptyValue2()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestString</methodName> 
  <params>
    <param>
      <value/>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.AreEqual(request.method, "TestString", "method is TestString");
			Assert.AreEqual(request.args[0].GetType(), typeof(string),
							"argument is string");
			Assert.AreEqual((string) request.args[0], "", "argument is empty string");
		}

		[Test]
		public void StringNoStringElement()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestString</methodName> 
  <params>
    <param>
      <value>test string</value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.AreEqual(request.method, "TestString", "method is TestString");
			Assert.AreEqual(request.args[0].GetType(), typeof(string),
							"argument is string");
			Assert.AreEqual((string) request.args[0], "test string",
							"argument is 'test string'");
		}

		[Test]
		public void StructProperties()
		{
			var xml = @"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>member1</name>
            <value>
              <i4>1</i4>
            </value>
          </member>
          <member>
            <name>member2</name>
            <value>
              <i4>2</i4>
            </value>
          </member>
          <member>
            <name>member-3</name>
            <value>
              <i4>3</i4>
            </value>
          </member>
          <member>
            <name>member-4</name>
            <value>
              <i4>4</i4>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodCall>";

			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.AreEqual(request.args[0].GetType(), typeof(XmlRpcStruct),
							"argument is XmlRpcStruct");
			var xrs = (XmlRpcStruct) request.args[0];
			Assert.IsTrue(xrs.Count == 4, "XmlRpcStruct has 4 members");
			Assert.IsTrue(xrs.ContainsKey("member1") && (int) xrs["member1"] == 1,
						  "member1");
			Assert.IsTrue(xrs.ContainsKey("member2") && (int) xrs["member2"] == 2,
						  "member2");
			Assert.IsTrue(xrs.ContainsKey("member-3") && (int) xrs["member-3"] == 3,
						  "member-3");
			Assert.IsTrue(xrs.ContainsKey("member-4") && (int) xrs["member-4"] == 4,
						  "member-4");
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidParametersException))]
		public void TooFewParameters()
		{
			var xml = @"<?xml version=""1.0""?>
<methodCall>
<methodName>blogger.getUsersBlogs</methodName>
<params>
<param>
<value>
<string>myusername</string>
</value>
</param>
</params>
</methodCall>";

			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidParametersException))]
		public void TooManyParameters()
		{
			var xml = @"<?xml version=""1.0""?>
<methodCall>
<methodName>blogger.getUsersBlogs</methodName>
<params>
<param>
<value>
<string>ffffffabffffffce6dffffff93ffffffac29ffffffc9fffffff826ffffffdefffff
fc9ff\
ffffe43c0b763036ffffffa0fffffff3ffffffa963377716</string>
</value>
</param>
<param>
<value>
<string>myusername</string>
</value>
</param>
<param>
<value>
<string>mypassword</string>
</value>
</param>
</params>
</methodCall>";

			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());
		}

		[Test]
		public void TwoLeadingSpace()
		{
			var xml = @"<?xml version=""1.0""?>
<methodCall><methodName>rtx.EchoString</methodName>
<params>
<param><value>  ddd</value></param>
</params></methodCall>";

			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());

			Assert.AreEqual(request.args[0].GetType(), typeof(string),
							"argument is string");
			Assert.AreEqual("  ddd", request.args[0]);
		}

		[Test]
		public void TwoSpaceString()
		{
			var xml = @"<?xml version=""1.0""?>
<methodCall><methodName>rtx.EchoString</methodName>
<params>
<param><value>  </value></param>
</params></methodCall>";

			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());

			Assert.AreEqual(request.args[0].GetType(), typeof(string),
							"argument is string");
			Assert.AreEqual("  ", request.args[0],
							"argument is string containing two spaces");
		}

		[Test]
		public void ZeroI8()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i8>0</i8></value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
			Assert.AreEqual(request.args[0].GetType(), typeof(long),
							"argument is long");
			Assert.AreEqual((long) request.args[0], 0, "argument is 0");
		}

		[Test]
		public void ZeroInteger()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
  <methodName>TestInt</methodName> 
  <params>
    <param>
      <value><i4>0</i4></value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);

			Assert.IsTrue(request.method == "TestInt", "method is TestInt");
			Assert.AreEqual(request.args[0].GetType(), typeof(int),
							"argument is int");
			Assert.AreEqual((int) request.args[0], 0, "argument is 0");
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void ZeroLengthMethodName()
		{
			var xml = @"<?xml version=""1.0"" ?> 
<methodCall>
<methodName></methodName> 
<params>
  <param>
    <value>test string</value>
  </param>
</params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, null);
		}
	}
}