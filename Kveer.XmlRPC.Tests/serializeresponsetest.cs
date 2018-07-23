using System;
using System.Data;
using System.IO;
using CookComputing.XmlRpc;
using NUnit.Framework;

namespace Kveer.XmlRPC.Tests
{
	[TestFixture]
	public class SerializeResponseTest
	{
		private class FooClass
		{
			public void Foo()
			{
				Console.WriteLine("Foo called");
			}
		}

		[Test]
		public void PaoloLiveraniProblem()
		{
			try
			{
				var    resp           = new XmlRpcResponse(new DataSet());
				Stream responseStream = new MemoryStream();
				var    serializer     = new XmlRpcSerializer();
				serializer.SerializeResponse(responseStream, resp);
			}
			catch (XmlRpcInvalidReturnType ex)
			{
				var s = ex.Message;
			}
		}

//    [Test]
//    public void VoidReturn()
//    {
//      MethodInfo mi = typeof(FooClass).GetMethod("Foo");
//      FooClass fooInst = new FooClass();
//      object ret = mi.Invoke(fooInst, new object[0]);
//      XmlRpcResponse xmlRpcResp = new XmlRpcResponse(ret);
//      Stream responseStream = new MemoryStream();
//      XmlRpcSerializer serializer = new XmlRpcSerializer();
//      serializer.SerializeResponse(responseStream, xmlRpcResp);
//      responseStream.Seek(0, SeekOrigin.Begin);
//      TextReader trdr = new StreamReader(responseStream);
//      String s = trdr.ReadLine();
//      while (s != null)
//      {
//        Console.WriteLine(s);
//        s = trdr.ReadLine();
//      }
//      responseStream.Seek(0, SeekOrigin.Begin);
//      xmlRpcResp = serializer.DeserializeResponse(responseStream, 
//        typeof(string));
//
//    }
	}
}