using CookComputing.XmlRpc;
using NUnit.Framework;

namespace ntest
{
	[TestFixture]
	public class RemotingServerTest
	{
		[Test]
		public void Method1()
		{
			var proxy = (ITest) XmlRpcProxyGen.Create(typeof(ITest));
			var cp    = (XmlRpcClientProtocol) proxy;
		}
	}
}