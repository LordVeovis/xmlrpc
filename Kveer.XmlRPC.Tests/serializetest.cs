using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using CookComputing.XmlRpc;
using NUnit.Framework;

namespace ntest

// TODO: test any culture dependencies
{
	[TestFixture]
	public class SerializeTest
	{
		//---------------------- struct ----------------------------------------// 
		private struct Struct1
		{
			public int      mi;
			public string   ms;
			public bool     mb;
			public double   md;
			public DateTime mdt;
			public byte[]   mb64;
			public int[]    ma;

			public bool Equals(Struct1 str)
			{
				if (mi != str.mi || ms != str.ms || md != str.md || mdt != str.mdt)
					return false;
				if (mb64.Length != str.mb64.Length)
					return false;
				for (var i = 0; i < mb64.Length; i++)
					if (mb64[i] != str.mb64[i])
						return false;
				for (var i = 0; i < ma.Length; i++)
					if (ma[i] != str.ma[i])
						return false;
				return true;
			}
		}

		private struct Struct2
		{
			[XmlRpcMember("member_1")] public int member1;

			[XmlRpcMissingMapping(MappingAction.Ignore)]
			public XmlRpcInt member2;

			[XmlRpcMember("member_3")] [XmlRpcMissingMapping(MappingAction.Ignore)]
			public XmlRpcInt member3;
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


		public class TestClass
		{
			public int    _int;
			public string _string;
		}


		private struct Struct4
		{
			[NonSerialized] public int x;
			public                 int y;
		}

		private class Class4
		{
			[NonSerialized] public int x;

			public int y;
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

		//---------------------- struct params -----------------------------------// 
		[XmlRpcMethod(StructParams = true)]
		public int Foo(int x, string y, double z)
		{
			return 1;
		}

		[XmlRpcMethod(StructParams = true)]
		public int FooWithParams(int x, string y, params double[] z)
		{
			return 1;
		}


		[XmlRpcMethod("artist.getInfo", StructParams = true)]
		public string getInfo(string artist, string api_key)
		{
			return "";
		}

		//---------------------- array -----------------------------------------// 
		[Test]
		public void Array()
		{
			var testary = {12, "Egypt", false};
			var xdoc = Utils.Serialize("SerializeTest.testArray",
									   testary,
									   Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is object[], "result is array of object");
			var ret = obj as object[];
			Assert.AreEqual(12, ret[0]);
			Assert.AreEqual("Egypt", ret[1]);
			Assert.AreEqual(false, ret[2]);
		}

		//---------------------- base64 ----------------------------------------// 
		[Test]
		public void Base64()
		{
			var testb =
			{
				121, 111, 117, 32, 99, 97, 110, 39, 116, 32, 114, 101, 97, 100,
				32, 116, 104, 105, 115, 33
			};
			var xdoc = Utils.Serialize("SerializeTest.testBase64",
									   testb,
									   Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is byte[], "result is array of byte");
			var ret = obj as byte[];
			Assert.IsTrue(ret.Length == testb.Length);
			for (var i = 0; i < testb.Length; i++)
				Assert.IsTrue(testb[i] == ret[i]);
		}

		//---------------------- boolean ---------------------------------------// 
		[Test]
		public void Boolean()
		{
			var xdoc = Utils.Serialize("SerializeTest.testBoolean",
									   true,
									   Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(true, obj);
		}

		[Test]
		public void Class()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			var    arg = new TestClass();
			arg._int    = 456;
			arg._string = "Test Class";
			req.args    = new object[] {arg};
			req.method  = "Foo";
			var ser = new XmlRpcSerializer();
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();

			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
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
</methodCall>", reqstr);
		}

		//---------------------- dateTime ------------------------------------// 
		[Test]
		public void DateTime()
		{
			var oldci = Thread.CurrentThread.CurrentCulture;
			try
			{
				foreach (var locale in Utils.GetLocales())
				{
					var ci = new CultureInfo(locale);
					Thread.CurrentThread.CurrentCulture = ci;
					var testDate = new DateTime(2002, 7, 6, 11, 25, 37);
					var xdoc = Utils.Serialize("SerializeTest.testDateTime",
											   testDate, Encoding.UTF8, MappingAction.Error);
					Type parsedType, parsedArrayType;
					var obj = Utils.Parse(xdoc, null, MappingAction.Error,
										  out parsedType, out parsedArrayType);
					Assert.AreEqual(testDate, obj);
				}
			}
			catch (Exception ex)
			{
				Assert.Fail("unexpected exception: " + ex.Message);
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = oldci;
			}
		}

		[Test]
		public void DateTimeWarekiCalendar()
		{
			var oldci = Thread.CurrentThread.CurrentCulture;
			try
			{
				var ci = new CultureInfo("ja-JP");
				Thread.CurrentThread.CurrentCulture = ci;
				ci.DateTimeFormat.Calendar          = new JapaneseCalendar();
				var xdoc = Utils.Serialize("SerializeTest.testDateTime",
										   new DateTime(2002, 7, 6, 11, 25, 37),
										   Encoding.UTF8, MappingAction.Ignore);
				Type parsedType, parsedArrayType;
				var obj = Utils.Parse(xdoc, null, MappingAction.Error,
									  out parsedType, out parsedArrayType);
				Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37), obj);
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = oldci;
			}
		}

		//---------------------- formatting ----------------------------------// 
		[Test]
		public void DefaultFormatting()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {1234567};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();

			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
      <value>
        <i4>1234567</i4>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
		}

		//---------------------- double ----------------------------------------// 
		[Test]
		public void Double()
		{
			var xdoc = Utils.Serialize("SerializeTest.testDouble",
									   543.21,
									   Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(543.21, obj);
		}

		//---------------------- HashTable----------------------------------------// 
		[Test]
		[ExpectedException(typeof(XmlRpcUnsupportedTypeException))]
		public void Hashtable()
		{
			var hashtable = new Hashtable();
			hashtable["mi"] = 34567;
			hashtable["ms"] = "another test string";

			var xdoc = Utils.Serialize("SerializeTest.testXmlRpcStruct",
									   hashtable, Encoding.UTF8, MappingAction.Ignore);
		}

		[Test]
		public void IncreasedIndentation()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {1234567};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.Indentation = 4;
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();

			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
    <methodName>Foo</methodName>
    <params>
        <param>
            <value>
                <i4>1234567</i4>
            </value>
        </param>
    </params>
</methodCall>", reqstr);
		}

		//---------------------- int -------------------------------------------// 
		[Test]
		public void Int()
		{
			var xdoc = Utils.Serialize("SerializeTest.testInt",
									   12345,
									   Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(12345, obj);
		}

		//---------------------- i64 -------------------------------------------// 
		[Test]
		public void Int64()
		{
			var xdoc = Utils.Serialize("SerializeTest.testInt64",
									   123456789012,
									   Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(123456789012, obj);
		}

		//---------------------- array -----------------------------------------// 
		[Test]
		public void MultiDimArray()
		{
			var myArray = {{1, 2}, {3, 4}};
			var xdoc = Utils.Serialize("SerializeTest.testMultiDimArray",
									   myArray,
									   Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, typeof(int[,]), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is int[,], "result is 2 dim array of int");
			var ret = obj as int[,];
			Assert.AreEqual(1, ret[0, 0]);
			Assert.AreEqual(2, ret[0, 1]);
			Assert.AreEqual(3, ret[1, 0]);
			Assert.AreEqual(4, ret[1, 1]);
		}

		[Test]
		public void NoIndentation()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {1234567};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.UseIndentation = false;
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();

			Assert.AreEqual(
				"<?xml version=\"1.0\"?><methodCall><methodName>Foo</methodName>" +
				"<params><param><value><i4>1234567</i4></value></param></params>" +
				"</methodCall>", reqstr);
		}


		[Test]
		public void NonSerialized()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {new Struct4()};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();
			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>y</name>
            <value>
              <i4>0</i4>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
		}

		[Test]
		public void NonSerializedClass()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {new Class4()};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();
			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>y</name>
            <value>
              <i4>0</i4>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
		}

		//---------------------- null parameter ----------------------------------// 
		[Test]
		[ExpectedException(typeof(XmlRpcNullParameterException))]
		public void NullParameter()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {null};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.SerializeRequest(stm, req);
		}

		[Test]
		public void RecursiveArrayMemberTest()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			var example = new RecursiveArrayMember
						  {
							  Level = "1",
							  childExamples = new[]
											  {
												  new RecursiveArrayMember
												  {
													  Level = "1-1",
													  childExamples = new[]
																	  {
																		  new RecursiveArrayMember
																		  {
																			  Level = "1-1-1",
																			  childExamples = new RecursiveArrayMember[]
																							  { }
																		  },
																		  new RecursiveArrayMember
																		  {
																			  Level = "1-1-2",
																			  childExamples = new RecursiveArrayMember[]
																							  { }
																		  }
																	  }
												  },
												  new RecursiveArrayMember
												  {
													  Level = "1-2",
													  childExamples = new[]
																	  {
																		  new RecursiveArrayMember
																		  {
																			  Level = "1-2-1",
																			  childExamples = new RecursiveArrayMember[]
																							  { }
																		  },
																		  new RecursiveArrayMember
																		  {
																			  Level = "1-2-2",
																			  childExamples = new RecursiveArrayMember[]
																							  { }
																		  }
																	  }
												  }
											  }
						  };
			req.args   = new object[] {example};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.UseStringTag = false;
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();
			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
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
                                        <data />
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
                                        <data />
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
                                        <data />
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
                                        <data />
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
      </value>
    </param>
  </params>
</methodCall>", reqstr);
		}

		[Test]
		public void RecursiveMemberTest()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			var example = new RecursiveMember
						  {
							  Level = "1",
							  childExample = new RecursiveMember
											 {
												 Level = "2",
												 childExample = new RecursiveMember
																{
																	Level = "3"
																}
											 }
						  };
			req.args   = new object[] {example};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.UseStringTag = false;
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();
			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
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
      </value>
    </param>
  </params>
</methodCall>", reqstr);
		}

		//---------------------- string ----------------------------------------// 
		[Test]
		public void String()
		{
			var xdoc = Utils.Serialize("SerializeTest.testString",
									   "this is a string",
									   Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual("this is a string", obj);
		}

		[Test]
		public void StringDefaultTag()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {"string default tag"};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.Indentation = 4;
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();

			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
    <methodName>Foo</methodName>
    <params>
        <param>
            <value>
                <string>string default tag</string>
            </value>
        </param>
    </params>
</methodCall>", reqstr);
		}

		[Test]
		public void StringNoStringTag()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {"string no string tag"};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.UseStringTag = false;
			ser.Indentation  = 4;
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();

			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
    <methodName>Foo</methodName>
    <params>
        <param>
            <value>string no string tag</value>
        </param>
    </params>
</methodCall>", reqstr);
		}

		[Test]
		public void StringStringTag()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {"string string tag"};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.UseStringTag = true;
			ser.Indentation  = 4;
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();

			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
    <methodName>Foo</methodName>
    <params>
        <param>
            <value>
                <string>string string tag</string>
            </value>
        </param>
    </params>
</methodCall>", reqstr);
		}

		[Test]
		public void StructOrderTest()
		{
			var testb =
			{
				121, 111, 117, 32, 99, 97, 110, 39, 116, 32, 114, 101, 97, 100,
				32, 116, 104, 105, 115, 33
			};

			var str1 = new Struct1();
			str1.mi   = 34567;
			str1.ms   = "another test string";
			str1.mb   = true;
			str1.md   = 8765.123;
			str1.mdt  = new DateTime(2002, 7, 6, 11, 25, 37);
			str1.mb64 = testb;
			str1.ma   = new[] {1, 2, 3, 4, 5};

			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {str1};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();
			Assert.Less(reqstr.IndexOf(">mi</"), reqstr.IndexOf(">ms</"));
			Assert.Less(reqstr.IndexOf(">ms</"), reqstr.IndexOf(">mb</"));
			Assert.Less(reqstr.IndexOf(">mb</"), reqstr.IndexOf(">md</"));
			Assert.Less(reqstr.IndexOf(">md</"), reqstr.IndexOf(">mdt</"));
			Assert.Less(reqstr.IndexOf(">mdt</"), reqstr.IndexOf(">mb64</"));
			Assert.Less(reqstr.IndexOf(">mb64</"), reqstr.IndexOf(">ma</"));
		}

		[Test]
		public void StructParams()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {1234, "test", 10.1};
			req.method = "Foo";
			req.mi     = GetType().GetMethod("Foo");
			var ser = new XmlRpcSerializer();
			ser.Indentation = 2;
			ser.UseIntTag   = true;
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();

			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>x</name>
            <value>
              <int>1234</int>
            </value>
          </member>
          <member>
            <name>y</name>
            <value>
              <string>test</string>
            </value>
          </member>
          <member>
            <name>z</name>
            <value>
              <double>10.1</double>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
		}

		[Test]
		public void StructParamsGetInfo()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {"Bob Dylan", "abcd1234"};
			req.method = "artist.getInfo";
			req.mi     = GetType().GetMethod("getInfo");
			var ser = new XmlRpcSerializer();
			ser.Indentation = 2;
			ser.UseIntTag   = true;
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();

			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>artist.getInfo</methodName>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>artist</name>
            <value>
              <string>Bob Dylan</string>
            </value>
          </member>
          <member>
            <name>api_key</name>
            <value>
              <string>abcd1234</string>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidParametersException))]
		public void StructParamsTooManyParams()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {1234, "test", 10.1, "lopol"};
			req.method = "Foo";
			req.mi     = GetType().GetMethod("Foo");
			var ser = new XmlRpcSerializer();
			ser.Indentation = 2;
			ser.UseIntTag   = true;
			ser.SerializeRequest(stm, req);
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidParametersException))]
		public void StructParamsWithParams()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {1234, "test", new[] {10.1}};
			req.method = "FooWithParams";
			req.mi     = GetType().GetMethod("FooWithParams");
			var ser = new XmlRpcSerializer();
			ser.Indentation = 2;
			ser.UseIntTag   = true;
			ser.SerializeRequest(stm, req);
		}

		[Test]
		public void StructProperties()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {new Struct3()};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();

			Assert.AreEqual(@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>member1</name>
            <value>
              <i4>0</i4>
            </value>
          </member>
          <member>
            <name>member2</name>
            <value>
              <i4>0</i4>
            </value>
          </member>
          <member>
            <name>member-3</name>
            <value>
              <i4>0</i4>
            </value>
          </member>
          <member>
            <name>member-4</name>
            <value>
              <i4>0</i4>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
		}

		[Test]
		public void StructTest()
		{
			var testb =
			{
				121, 111, 117, 32, 99, 97, 110, 39, 116, 32, 114, 101, 97, 100,
				32, 116, 104, 105, 115, 33
			};

			var str1 = new Struct1();
			str1.mi   = 34567;
			str1.ms   = "another test string";
			str1.mb   = true;
			str1.md   = 8765.123;
			str1.mdt  = new DateTime(2002, 7, 6, 11, 25, 37);
			str1.mb64 = testb;
			str1.ma   = new[] {1, 2, 3, 4, 5};
			var xdoc = Utils.Serialize("SerializeTest.testStruct",
									   str1,
									   Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, typeof(Struct1), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is Struct1, "result is Struct1");
			var str2 = (Struct1) obj;
			Assert.IsTrue(str2.Equals(str1));
		}

		[Test]
		public void UseInt()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {1234};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.Indentation = 4;
			ser.UseIntTag   = true;
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();

			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
    <methodName>Foo</methodName>
    <params>
        <param>
            <value>
                <int>1234</int>
            </value>
        </param>
    </params>
</methodCall>", reqstr);
		}

		//---------------------- XmlRpcBoolean --------------------------------// 
		[Test]
		public void XmlRpcBoolean()
		{
			var xdoc = Utils.Serialize("SerializeTest.testXmlRpcBoolean",
									   new XmlRpcBoolean(true),
									   Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(true, obj);
		}

		//---------------------- XmlRpcDateTime ------------------------------// 
		[Test]
		public void XmlRpcDateTime()
		{
			var xdoc = Utils.Serialize("SerializeTest.testXmlRpcDateTime",
									   new DateTime(2002, 7, 6, 11, 25, 37),
									   Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(new DateTime(2002, 7, 6, 11, 25, 37), obj);
		}

		//---------------------- XmlRpcDouble ----------------------------------// 
		[Test]
		public void XmlRpcDouble()
		{
			var xdoc = Utils.Serialize("SerializeTest.testXmlRpcDouble",
									   new XmlRpcDouble(543.21),
									   Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(543.21, obj);
		}

		[Test]
		public void XmlRpcDouble_ForeignCulture()
		{
			var         currentCulture = Thread.CurrentThread.CurrentCulture;
			XmlDocument xdoc;
			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-BE");
				var xsd = new XmlRpcDouble(543.21);
				//Console.WriteLine(xsd.ToString());
				xdoc = Utils.Serialize(
					"SerializeTest.testXmlRpcDouble_ForeignCulture",
					new XmlRpcDouble(543.21),
					Encoding.UTF8, MappingAction.Ignore);
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}

			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(543.21, obj);
		}


		//---------------------- XmlRpcInt -------------------------------------// 
		[Test]
		public void XmlRpcInt()
		{
			var xdoc = Utils.Serialize("SerializeTest.testXmlRpcInt",
									   new XmlRpcInt(12345),
									   Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, null, MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.AreEqual(12345, obj);
		}

		[Test]
		public void XmlRpcMember()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {new Struct2()};
			req.method = "Foo";
			var ser = new XmlRpcSerializer();
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();

			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>member_1</name>
            <value>
              <i4>0</i4>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
		}

		//---------------------- XmlRpcStruct ------------------------------------// 
		[Test]
		public void XmlRpcStruct()
		{
			var xmlRpcStruct = new XmlRpcStruct();
			xmlRpcStruct["mi"] = 34567;
			xmlRpcStruct["ms"] = "another test string";

			var xdoc = Utils.Serialize("SerializeTest.testXmlRpcStruct",
									   xmlRpcStruct, Encoding.UTF8, MappingAction.Ignore);
			Type parsedType, parsedArrayType;
			var obj = Utils.Parse(xdoc, typeof(XmlRpcStruct), MappingAction.Error,
								  out parsedType, out parsedArrayType);
			Assert.IsTrue(obj is XmlRpcStruct, "result is XmlRpcStruct");
			xmlRpcStruct = obj as XmlRpcStruct;
			Assert.IsTrue(xmlRpcStruct["mi"] is int);
			Assert.AreEqual((int) xmlRpcStruct["mi"], 34567);
			Assert.IsTrue(xmlRpcStruct["ms"] is string);
			Assert.AreEqual((string) xmlRpcStruct["ms"], "another test string");
		}
	}
}