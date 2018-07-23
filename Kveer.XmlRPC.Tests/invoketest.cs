using System;
using System.Reflection;
using System.Threading;
using CookComputing.XmlRpc;
using NUnit.Framework;

public struct TestStruct
{
	public int x;
	public int y;
}

[XmlRpcUrl("http://localhost/test/")]
internal class Foo : XmlRpcClientProtocol
{
	[XmlRpcMethod]
	public int Send_Param(object[] toSend)
	{
		return (int) Invoke("Send_Param", toSend);
	}

	[XmlRpcMethod]
	public int SendTwoParams(int param1, int param2)
	{
		return (int) Invoke("SendTwoParams", param1);
	}

	[XmlRpcMethod]
	public string Send(string str)
	{
		return (string) Invoke("Send", new object[] {str});
	}

	[XmlRpcMethod]
	public string Send(TestStruct strct)
	{
		return (string) Invoke("Send", strct);
	}
}

[XmlRpcUrl("http://localhost:8005/statename.rem")]
internal class StateName : XmlRpcClientProtocol
{
	[XmlRpcMethod("examples.getStateName")]
	public string GetStateNameUsingMethodName(int stateNumber)
	{
		return (string) Invoke("GetStateNameUsingMethodName", stateNumber);
	}

	[XmlRpcMethod("examples.getStateNameFromString")]
	public string GetStateNameUsingMethodName(string stateNumber)
	{
		return (string) Invoke("GetStateNameUsingMethodName",
							   new object[] {stateNumber});
	}

	[XmlRpcMethod("examples.getStateName")]
	public string GetStateNameUsingMethodInfo(int stateNumber)
	{
		return (string) Invoke(MethodBase.GetCurrentMethod(), stateNumber);
	}

	[XmlRpcMethod("examples.getStateNameFromString")]
	public string GetStateNameUsingMethodInfo(string stateNumber)
	{
		return (string) Invoke(MethodBase.GetCurrentMethod(),
							   new object[] {stateNumber});
	}

	[XmlRpcMethod("examples.getStateName")]
	public IAsyncResult BeginGetStateName(int stateNumber, AsyncCallback callback,
										  object asyncState)
	{
		return BeginInvoke(MethodBase.GetCurrentMethod(),
						   new object[] {stateNumber}, callback, asyncState);
	}

	[XmlRpcMethod("examples.getStateName")]
	public IAsyncResult BeginGetStateName(int stateNumber)
	{
		return BeginInvoke(MethodBase.GetCurrentMethod(),
						   new object[] {stateNumber}, null, null);
	}

	public string EndGetStateName(IAsyncResult asr)
	{
		return (string) EndInvoke(asr);
	}
}

namespace Kveer.XmlRPC.Tests
{
	//[TestFixture]
	//public class InvokeTest
	//{
	//	[OneTimeSetUp]
	//	public void Setup()
	//	{
	//		StateNameService.Start(8005);
	//	}

	//	[OneTimeTearDown]
	//	public void TearDown()
	//	{
	//		StateNameService.Stop();
	//	}

	//	private class CBInfo
	//	{
	//		public readonly ManualResetEvent _evt;
	//		public          Exception        _excep;
	//		public          string           _ret;

	//		public CBInfo(ManualResetEvent evt)
	//		{
	//			_evt = evt;
	//		}
	//	}

	//	private void StateNameCallback(IAsyncResult asr)
	//	{
	//		var clientResult = (XmlRpcAsyncResult) asr;
	//		var proxy        = (StateName) clientResult.ClientProtocol;
	//		var info         = (CBInfo) asr.AsyncState;
	//		try
	//		{
	//			info._ret = proxy.EndGetStateName(asr);
	//		}
	//		catch (Exception ex)
	//		{
	//			info._excep = ex;
	//		}

	//		info._evt.Set();
	//	}

	//	[Test]
	//	public void MakeAsynchronousCallCallBack()
	//	{
	//		var proxy = new StateName();
	//		var evt   = new ManualResetEvent(false);
	//		var info  = new CBInfo(evt);
	//		var asr3  = proxy.BeginGetStateName(1, StateNameCallback, info);
	//		evt.WaitOne();
	//		Assert.AreEqual(null, info._excep, "Async call threw exception");
	//		Assert.AreEqual("Alabama", info._ret);
	//	}

	//	[Test]
	//	public void MakeAsynchronousCallIsCompleted()
	//	{
	//		var proxy = new StateName();
	//		var asr1  = proxy.BeginGetStateName(1);
	//		while (asr1.IsCompleted == false)
	//			Thread.Sleep(10);
	//		var ret1 = proxy.EndGetStateName(asr1);
	//		Assert.AreEqual("Alabama", ret1);
	//	}

	//	[Test]
	//	public void MakeAsynchronousCallWait()
	//	{
	//		var proxy = new StateName();
	//		var asr2  = proxy.BeginGetStateName(1);
	//		asr2.AsyncWaitHandle.WaitOne();
	//		var ret2 = proxy.EndGetStateName(asr2);
	//		Assert.AreEqual("Alabama", ret2);
	//	}

	//	[Test]
	//	public void MakeSynchronousCalls()
	//	{
	//		var proxy = new StateName();
	//		var ret1  = proxy.GetStateNameUsingMethodName(1);
	//		Assert.AreEqual("Alabama", ret1);
	//		var ret2 = proxy.GetStateNameUsingMethodInfo(1);
	//		Assert.AreEqual("Alabama", ret2);
	//		var ret3 = proxy.GetStateNameUsingMethodName("1");
	//		Assert.AreEqual("Alabama", ret3);
	//		var ret4 = proxy.GetStateNameUsingMethodInfo("1");
	//		Assert.AreEqual("Alabama", ret4);
	//	}

	//	[Test]
	//	public void MakeSystemListMethodsCall()
	//	{
	//		var proxy = new StateName();
	//		var ret   = proxy.SystemListMethods();
	//		Assert.AreEqual(3, ret.Length);
	//		Assert.AreEqual(ret[0], "examples.getStateName");
	//		Assert.AreEqual(ret[1], "examples.getStateNameFromString");
	//		Assert.AreEqual(ret[2], "examples.getStateStruct");
	//	}

	//	// TODO: add sync fault exception
	//	// TODO: add async fault exception

	//	[Test]
	//	public void Massimo()
	//	{
	//		var parms = new object[12] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
	//		var foo   = new Foo();
	//		Assert.That(() => foo.Send_Param(parms), Throws.TypeOf<XmlRpcInvalidParametersException>());
	//	}

	//	[Test]
	//	public void NullArg()
	//	{
	//		var foo = new Foo();
	//		Assert.That(() => foo.Send(null), Throws.TypeOf<XmlRpcNullParameterException>());
	//	}
	//}
}