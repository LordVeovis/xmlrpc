using System;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using CookComputing.XmlRpc;
using NUnit.Framework;

namespace ntest
{
	[XmlRpcUrl("http://localhost/test/")]
	public interface ITest
	{
		[XmlRpcMethod]
		string Method1(int x);
	}

	public interface ITest2 : IXmlRpcProxy
	{
		[XmlRpcMethod]
		string Method1(int x);
	}


	[TestFixture]
	public class ProxyGenTest
	{
		[OneTimeSetUp]
		public void Setup()
		{
			StateNameService.Start(5678);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			StateNameService.Stop();
		}

		public interface IParent : IXmlRpcProxy
		{
			[XmlRpcMethod]
			string Foo(int x);
		}

		public interface IChild : IParent
		{
			[XmlRpcMethod]
			string Bar(int x);
		}

		public interface IOverrides
		{
			[XmlRpcMethod("account.info")]
			string acct_info(int SITECODE, string username);

			[XmlRpcMethod("account.info")]
			string acct_info(int SITECODE, int account);
		}

		public interface IOverridesChild : IOverrides
		{
			[XmlRpcMethod("account.info")]
			new string acct_info(int SITECODE, int account);
		}

		private class CBInfo
		{
			public readonly ManualResetEvent _evt;
			public          Exception        _excep;
			public          string           _ret;

			public CBInfo(ManualResetEvent evt)
			{
				_evt = evt;
			}
		}

		private void StateNameCallback(IAsyncResult asr)
		{
			var clientResult = (XmlRpcAsyncResult) asr;
			var proxy        = (IStateName) clientResult.ClientProtocol;
			var info         = (CBInfo) asr.AsyncState;
			try
			{
				info._ret = proxy.EndGetStateName(asr);
			}
			catch (Exception ex)
			{
				info._excep = ex;
			}

			info._evt.Set();
		}

		private void StateNameCallbackNoState(IAsyncResult asr)
		{
			var clientResult = (XmlRpcAsyncResult) asr;
			var proxy        = (IStateName) clientResult.ClientProtocol;
			try
			{
				_ret = proxy.EndGetStateName(asr);
			}
			catch (Exception ex)
			{
				_excep = ex;
			}

			_evt.Set();
		}

		private ManualResetEvent _evt;
		private Exception        _excep;
		private string           _ret;

		[XmlRpcUrl("http://localhost:5678/statename.rem")]
		public interface IStateName2 : IXmlRpcProxy
		{
			[XmlRpcMethod("examples.getStateStruct", StructParams = true)]
			string GetStateNames(int state1, int state2, int state3);
		}

		[Test]
		public void AsynchronousFaultException()
		{
			var proxy = (IStateName) XmlRpcProxyGen.Create(typeof(IStateName));
			var asr   = proxy.BeginGetStateName(100);
			asr.AsyncWaitHandle.WaitOne();
			try
			{
				var ret = proxy.EndGetStateName(asr);
				Assert.Fail("exception not thrown on async call");
			}
			catch (XmlRpcFaultException fex)
			{
				Assert.AreEqual(1, fex.FaultCode);
				Assert.AreEqual("Invalid state number", fex.FaultString);
			}
		}

		[Test]
		public void CheckProperties()
		{
			var proxy     = (ITest2) XmlRpcProxyGen.Create(typeof(ITest2));
			var certs     = proxy.ClientCertificates;
			var groupName = proxy.ConnectionGroupName;
#if (!FX1_0)
			var expect100 = proxy.Expect100Continue;
#endif
			var header      = proxy.Headers;
			var indentation = proxy.Indentation;
			var keepAlive   = proxy.KeepAlive;
			var nonStandard = proxy.NonStandard;
			var preauth     = proxy.PreAuthenticate;
			var version     = proxy.ProtocolVersion;
			var webProxy    = proxy.Proxy;
			var container   = proxy.CookieContainer;
			var timeout     = proxy.Timeout;
			var url         = proxy.Url;
			var useIndent   = proxy.UseIndentation;
			var encoding    = proxy.XmlEncoding;
			var method      = proxy.XmlRpcMethod;
			var useIntTag   = proxy.UseIntTag;

			// introspection methods
			try
			{
				proxy.SystemListMethods();
			}
			catch (XmlRpcMissingUrl) { }

			try
			{
				proxy.SystemMethodSignature("Foo");
			}
			catch (XmlRpcMissingUrl) { }

			try
			{
				proxy.SystemMethodHelp("Foo");
			}
			catch (XmlRpcMissingUrl) { }
		}

		[Test]
		public void FileIOPermission()
		{
			var f = new FileIOPermission(PermissionState.Unrestricted);
			f.Deny();
			try
			{
				var proxy = (IStateName2) XmlRpcProxyGen.Create(typeof(IStateName2));
			}
			finally
			{
				CodeAccessPermission.RevertDeny();
			}
		}

		[Test]
		public void InheritedInterface()
		{
			// Test problem reported by Sean Rohead. This will throw an exception 
			// if method Foo in the base class Parent is not implemented
			var proxy = (IChild) XmlRpcProxyGen.Create(typeof(IChild));
		}

		[Test]
		public void ListMethods()
		{
			var proxy = (IChild) XmlRpcProxyGen.Create(typeof(IChild));
		}

		[Test]
		public void MakeAsynchronousCallCallback()
		{
			var proxy = (IStateName) XmlRpcProxyGen.Create(typeof(IStateName));
			var evt   = new ManualResetEvent(false);
			var info  = new CBInfo(evt);
			var asr3  = proxy.BeginGetStateName(1, StateNameCallback, info);
			evt.WaitOne();
			Assert.AreEqual(null, info._excep, "Async call threw exception");
			Assert.AreEqual("Alabama", info._ret);
		}

		[Test]
		public void MakeAsynchronousCallCallbackNoState()
		{
			var proxy = (IStateName) XmlRpcProxyGen.Create(typeof(IStateName));
			_evt = new ManualResetEvent(false);
			var asr3 = proxy.BeginGetStateName(1, StateNameCallbackNoState);
			_evt.WaitOne();
			Assert.AreEqual(null, _excep, "Async call threw exception");
			Assert.AreEqual("Alabama", _ret);
		}

		[Test]
		public void MakeAsynchronousCallIsCompleted()
		{
			var proxy = (IStateName) XmlRpcProxyGen.Create(typeof(IStateName));
			var asr1  = proxy.BeginGetStateName(1);
			while (asr1.IsCompleted == false)
				Thread.Sleep(10);
			var ret1 = proxy.EndGetStateName(asr1);
			Assert.AreEqual("Alabama", ret1);
		}

		[Test]
		public void MakeAsynchronousCallWait()
		{
			var proxy = (IStateName) XmlRpcProxyGen.Create(typeof(IStateName));
			var asr2  = proxy.BeginGetStateName(1);
			asr2.AsyncWaitHandle.WaitOne();
			var ret2 = proxy.EndGetStateName(asr2);
			Assert.AreEqual("Alabama", ret2);
		}

		[Test]
		public void MakeStructParamsCall()
		{
			var proxy = (IStateName2) XmlRpcProxyGen.Create(typeof(IStateName2));
			var ret   = proxy.GetStateNames(1, 2, 3);
			Assert.AreEqual("Alabama Alaska Arizona", ret);
		}

		[Test]
		public void MakeSynchronousCalls()
		{
			var proxy = (IStateName) XmlRpcProxyGen.Create(typeof(IStateName));
			var ret1  = proxy.GetStateName(1);
			Assert.AreEqual("Alabama", ret1);
			var ret2 = proxy.GetStateName("1");
			Assert.AreEqual("Alabama", ret2);
		}

		[Test]
		public void Method1()
		{
			var proxy = (ITest) XmlRpcProxyGen.Create(typeof(ITest));
			var cp    = (XmlRpcClientProtocol) proxy;
			Assert.IsTrue(cp is ITest);
			Assert.IsTrue(cp is XmlRpcClientProtocol);
		}

#if !FX1_0
		[Test]
		public void Method1Generic()
		{
			var proxy = XmlRpcProxyGen.Create<ITest2>();
			var cp    = (XmlRpcClientProtocol) proxy;
			Assert.IsTrue(cp is ITest2);
			Assert.IsTrue(cp is IXmlRpcProxy);
			Assert.IsTrue(cp is XmlRpcClientProtocol);
		}
#endif

		[Test]
		public void Overrides()
		{
			var proxy = (IOverrides) XmlRpcProxyGen.Create(typeof(IOverrides));
		}

		[Test]
		public void OverridesChild()
		{
			var proxy = (IOverridesChild) XmlRpcProxyGen.Create(typeof(IOverridesChild));
		}

		[Test]
		public void SynchronousFaultException()
		{
			var proxy = (IStateName) XmlRpcProxyGen.Create(typeof(IStateName));
			try
			{
				var ret1 = proxy.GetStateName(100);
				Assert.Fail("exception not thrown on sync call");
			}
			catch (XmlRpcFaultException fex)
			{
				Assert.AreEqual(1, fex.FaultCode);
				Assert.AreEqual("Invalid state number", fex.FaultString);
			}
		}
	}
}