using System.IO;
using System.Net;
using CookComputing.XmlRpc;
using NUnit.Framework;

#if !FX1_0

namespace ntest
{
	[TestFixture]
	public class ListenerTest
	{
		private readonly Listener _listener = new Listener(new StateNameListnerService(),
														   "http://127.0.0.1:11000/");

		private readonly Listener _listenerDerived = new Listener(new StateNameListnerDerivedService(),
																  "http://127.0.0.1:11001/");

		[OneTimeSetUp]
		public void Setup()
		{
			_listener.Start();
			_listenerDerived.Start();
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			_listener.Stop();
			_listenerDerived.Stop();
		}

		[Test]
		public void GetAutoDocumentation()
		{
			var req = WebRequest.Create("http://127.0.0.1:11000/");
			var stm = req.GetResponse().GetResponseStream();
			var doc = new StreamReader(stm).ReadToEnd();
			Assert.IsNotNull(doc);
			Assert.IsTrue(doc.StartsWith("<html>"));
		}

		[Test]
		public void GetAutoDocumentationDerived()
		{
			var req = WebRequest.Create("http://127.0.0.1:11001/");
			var stm = req.GetResponse().GetResponseStream();
			var doc = new StreamReader(stm).ReadToEnd();
			Assert.IsNotNull(doc);
			Assert.IsTrue(doc.StartsWith("<html>"));
		}

		[Test]
		public void GetCookie()
		{
			var proxy = XmlRpcProxyGen.Create<IStateName>();
			proxy.Url = "http://127.0.0.1:11000/";
			var name    = proxy.GetStateName(1);
			var cookies = proxy.ResponseCookies;
			var value   = cookies["FooCookie"].Value;
			Assert.AreEqual("FooValue", value);
		}

		[Test]
		public void GetHeader()
		{
			var proxy = XmlRpcProxyGen.Create<IStateName>();
			proxy.Url = "http://127.0.0.1:11000/";
			var name    = proxy.GetStateName(1);
			var headers = proxy.ResponseHeaders;
			var value   = headers["BarHeader"];
			Assert.AreEqual("BarValue", value);
		}


		[Test]
		public void MakeCall()
		{
			var proxy = XmlRpcProxyGen.Create<IStateName>();
			proxy.Url               = "http://127.0.0.1:11000/";
			proxy.AllowAutoRedirect = false;
			var name = proxy.GetStateName(1);
		}

		[Test]
		public void MakeSystemListMethodsCall()
		{
			var proxy = XmlRpcProxyGen.Create<IStateName>();
			proxy.Url = "http://127.0.0.1:11000/";
			var ret = proxy.SystemListMethods();
			Assert.AreEqual(1, ret.Length);
			Assert.AreEqual(ret[0], "examples.getStateName");
		}
	}
}

#endif