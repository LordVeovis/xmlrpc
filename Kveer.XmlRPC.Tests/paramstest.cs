using System;
using System.IO;
using CookComputing.XmlRpc;
using NUnit.Framework;

namespace ntest

{
	[TestFixture]
	public class ParamsTest
	{
		public interface IFoo
		{
			[XmlRpcMethod]
			int Foo(params object[] parms);

			[XmlRpcMethod]
			int FooNotParams(object[] parms);

			[XmlRpcMethod]
			int FooZeroParameters();

			[XmlRpcMethod]
			int Bar(params int[] parms);

			[XmlRpcMethod]
			int BarNotParams(int[] parms);
		}

		[XmlRpcMethod]
		public int Foo(params object[] args)
		{
			return args.Length;
		}

		[XmlRpcMethod]
		public int FooZeroParameters()
		{
			return 1;
		}

		[XmlRpcMethod]
		public int Foo1(int arg1, params object[] args)
		{
			return args.Length;
		}

		[XmlRpcMethod]
		public int Bar(params string[] args)
		{
			return args.Length;
		}

		[XmlRpcMethod]
		public int Send_Param(string task, params object[] args)
		{
			return args.Length;
		}


		[XmlRpcMethod]
		public object[] Linisgre(params object[] args)
		{
			return args;
		}


		private readonly string massimoRequest =
			@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Send_Param</methodName>
  <params>
    <param>
      <value>
        <string>IFTASK</string>
      </value>
    </param>
    <param>
      <value>
        <array>
          <data>
            <value>
              <string>test/Gain1</string>
            </value>
            <value>
              <string>Gain</string>
            </value>
            <value>
              <i4>1</i4>
            </value>
            <value>
              <i4>1</i4>
            </value>
            <value>
              <array>
                <data>
                  <value>
                    <double>0.5</double>
                  </value>
                </data>
              </array>
            </value>
          </data>
        </array>
      </value>
    </param>
    <param>
      <value>
        <array>
          <data>
            <value>
              <string>test/METER</string>
            </value>
            <value>
              <string>P1</string>
            </value>
            <value>
              <i4>1</i4>
            </value>
            <value>
              <i4>1</i4>
            </value>
            <value>
              <array>
                <data>
                  <value>
                    <double>-1</double>
                  </value>
                </data>
              </array>
            </value>
          </data>
        </array>
      </value>
    </param>
  </params>
</methodCall>";

		[Test]
		public void BuildProxy()
		{
			var newType = XmlRpcProxyGen.Create(typeof(IFoo)).GetType();
			var mi      = newType.GetMethod("Foo");
			var pis     = mi.GetParameters();
			Assert.IsTrue(Attribute.IsDefined(pis[pis.Length - 1],
											  typeof(ParamArrayAttribute)), "method has params argument");
		}


		[Test]
		public void DeserializeLinisgre()
		{
			var xml =
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Linisgre</methodName>
  <params>
    <param>
      <value>
        <i4>1</i4>
      </value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());
			Assert.AreEqual(request.method, "Linisgre", "method is Linisgre");
			Assert.AreEqual(request.args[0].GetType(), typeof(object[]),
							"argument is object[]");
			Assert.AreEqual((object[]) request.args[0], new object[] {1},
							"argument is params array 1");
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidXmlRpcException))]
		public void DeserializeLinisgreEmptyParam()
		{
			var xml =
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Linisgre</methodName>
  <params>
    <param/>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());
		}

		[Test]
		public void DeserializeLinisgreNoArgs()
		{
			var xml =
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Linisgre</methodName>
  <params>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());
			Assert.AreEqual(request.method, "Linisgre", "method is Linisgre");
			Assert.AreEqual(request.args[0].GetType(), typeof(object[]),
							"argument is object[]");
			Assert.AreEqual((object[]) request.args[0], new object[0],
							"argument is empty params array");
		}

		[Test]
		public void DeserializeMassimo()
		{
			var sr         = new StringReader(massimoRequest);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());
			Assert.AreEqual(request.method, "Send_Param", "method is Send_Param");
			Assert.AreEqual(typeof(string), request.args[0].GetType(),
							"argument is string");
			Assert.AreEqual(typeof(object[]), request.args[1].GetType(),
							"argument is object[]");
		}

		[Test]
		public void DeserializeObjectInvalidParams()
		{
			var xml =
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Bar</methodName>
  <params>
    <param>
      <value>
        <string>string one</string>
      </value>
    </param>
    <param>
      <value>
        <i4>2</i4>
      </value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			try
			{
				var request = serializer.DeserializeRequest(sr,
															GetType());
				Assert.Fail("Should detect invalid type of parameter #2");
			}
			catch (XmlRpcTypeMismatchException) { }
		}

		[Test]
		public void DeserializeObjectParams()
		{
			var xml =
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params>
    <param>
      <value>
        <i4>1</i4>
      </value>
    </param>
    <param>
      <value>
        <string>one</string>
      </value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());
			Assert.AreEqual(request.method, "Foo", "method is Foo");
			Assert.AreEqual(request.args[0].GetType(), typeof(object[]),
							"argument is object[]");
			Assert.AreEqual((object[]) request.args[0], new object[] {1, "one"},
							"argument is params array 1, \"one\"");
		}

		[Test]
		public void DeserializeObjectParams1()
		{
			var xml =
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo1</methodName>
  <params>
    <param>
      <value>
        <i4>5678</i4>
      </value>
    </param>
    <param>
      <value>
        <i4>1</i4>
      </value>
    </param>
    <param>
      <value>
        <string>one</string>
      </value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());
			Assert.AreEqual(request.method, "Foo1", "method is Foo");
			Assert.AreEqual((int) request.args[0], 5678, "first argument is int");
			Assert.AreEqual(request.args[1].GetType(), typeof(object[]),
							"argument is object[]");
			Assert.AreEqual((object[]) request.args[1], new object[] {1, "one"},
							"second argument is params array 1, \"one\"");
		}

		[Test]
		[ExpectedException(typeof(XmlRpcInvalidParametersException))]
		public void DeserializeObjectParamsInsufficientParams()
		{
			var xml =
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo1</methodName>
  <params>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());
		}

		[Test]
		public void DeserializeObjectParamsStrings()
		{
			var xml =
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Bar</methodName>
  <params>
    <param>
      <value>
        <string>one</string>
      </value>
    </param>
    <param>
      <value>
        <string>two</string>
      </value>
    </param>
  </params>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());
			Assert.AreEqual(request.method, "Bar", "method is Foo");
			Assert.AreEqual(request.args[0].GetType(), typeof(string[]),
							"argument is string[]");
			Assert.AreEqual((string[]) request.args[0], new[] {"one", "two"},
							"argument is params array \"one\", \"two\"");
		}

		[Test]
		public void DeserializeParamsEmpty()
		{
			var xml =
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Foo</methodName>
  <params/>
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request    = serializer.DeserializeRequest(sr, GetType());

			Assert.AreEqual(request.method, "Foo", "method is Foo");
			Assert.AreEqual(request.args[0].GetType(), typeof(object[]),
							"argument is obj[]");
			Assert.AreEqual((request.args[0] as object[]).Length, 0,
							"argument is empty array of object");
		}

		[Test]
		public void DeserializeZeroParameters()
		{
			var xml =
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>FooZeroParameters</methodName>
  <params />
</methodCall>";
			var sr         = new StringReader(xml);
			var serializer = new XmlRpcSerializer();
			var request = serializer.DeserializeRequest(sr,
														GetType());
			Assert.AreEqual(request.method, "FooZeroParameters",
							"method is FooZeroParameters");
			Assert.AreEqual(0, request.args.Length, "no arguments");
		}

		[Test]
		public void SerializeIntNoParams()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {new[] {1, 2, 3}};
			req.method = "BarNotParams";
			req.mi     = typeof(IFoo).GetMethod("BarNotParams");
			var ser = new XmlRpcSerializer();
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();
			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>BarNotParams</methodName>
  <params>
    <param>
      <value>
        <array>
          <data>
            <value>
              <i4>1</i4>
            </value>
            <value>
              <i4>2</i4>
            </value>
            <value>
              <i4>3</i4>
            </value>
          </data>
        </array>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
		}

		[Test]
		public void SerializeIntParams()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {new[] {1, 2, 3}};
			req.method = "Bar";
			req.mi     = typeof(IFoo).GetMethod("Bar");
			var ser = new XmlRpcSerializer();
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();
			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>Bar</methodName>
  <params>
    <param>
      <value>
        <i4>1</i4>
      </value>
    </param>
    <param>
      <value>
        <i4>2</i4>
      </value>
    </param>
    <param>
      <value>
        <i4>3</i4>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
		}

		[Test]
		public void SerializeMassimo()
		{
			var param1 =
			{
				"test/Gain1", "Gain", 1, 1,
				new[] {0.5}
			};
			var param2 =
			{
				"test/METER", "P1", 1, 1,
				new[] {-1.0}
			};
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args = new object[]
					   {
						   "IFTASK",
						   new object[] {param1, param2}
					   };
			req.method = "Send_Param";
			req.mi     = GetType().GetMethod("Send_Param");
			var ser = new XmlRpcSerializer();
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();
			Assert.AreEqual(massimoRequest, reqstr);
		}

		[Test]
		public void SerializeObjectNoParams()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {new object[] {1, "one"}};
			req.method = "FooNotParams";
			req.mi     = typeof(IFoo).GetMethod("FooNotParams");
			var ser = new XmlRpcSerializer();
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();
			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>FooNotParams</methodName>
  <params>
    <param>
      <value>
        <array>
          <data>
            <value>
              <i4>1</i4>
            </value>
            <value>
              <string>one</string>
            </value>
          </data>
        </array>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
		}

		[Test]
		public void SerializeObjectParams()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[] {new object[] {1, "one"}};
			req.method = "Foo";
			req.mi     = typeof(IFoo).GetMethod("Foo");
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
        <i4>1</i4>
      </value>
    </param>
    <param>
      <value>
        <string>one</string>
      </value>
    </param>
  </params>
</methodCall>", reqstr);
		}

		[Test]
		public void SerializeZeroParameters()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[0];
			req.method = "FooZeroParameters";
			req.mi     = typeof(IFoo).GetMethod("FooZeroParameters");
			var ser = new XmlRpcSerializer();
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();
			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>FooZeroParameters</methodName>
  <params />
</methodCall>", reqstr);
		}

		[Test]
		public void SerializeZeroParametersNoParams()
		{
			Stream stm = new MemoryStream();
			var    req = new XmlRpcRequest();
			req.args   = new object[0];
			req.method = "FooZeroParameters";
			req.mi     = typeof(IFoo).GetMethod("FooZeroParameters");
			var ser = new XmlRpcSerializer();
			ser.UseEmptyParamsTag = false;
			ser.SerializeRequest(stm, req);
			stm.Position = 0;
			TextReader tr     = new StreamReader(stm);
			var        reqstr = tr.ReadToEnd();
			Assert.AreEqual(
				@"<?xml version=""1.0""?>
<methodCall>
  <methodName>FooZeroParameters</methodName>
</methodCall>", reqstr);
		}
	}
}