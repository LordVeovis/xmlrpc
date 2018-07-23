using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading;
using CookComputing.XmlRpc;
using NUnit.Framework;

#if !FX1_0

namespace Kveer.XmlRPC.Tests
{
	[TestFixture]
	public class AcceptEncodingClient
	{
		private bool         _running;
		private HttpListener _lstner;
		private string       encoding;

		[OneTimeSetUp]
		public void Setup()
		{
			_lstner = new HttpListener();
			_lstner.Prefixes.Add("http://127.0.0.1:11002/");
			var thrd = new Thread(Run);
			_running = true;
			_lstner.Start();
			thrd.Start();
		}

		public void Run()
		{
			try
			{
				while (_running)
				{
					var xml = @"<?xml version=""1.0"" ?> 
<methodResponse>
  <params>
    <param>
      <value>Alabama</value>
    </param>
  </params>
</methodResponse>";
					var context = _lstner.GetContext();
					switch (encoding)
					{
						case "gzip":
							context.Response.Headers.Add("Content-Encoding", "gzip");
							break;
						case "deflate":
							context.Response.Headers.Add("Content-Encoding", "deflate");
							break;
						default:
							break;
					}

					context.Response.ContentEncoding = Encoding.UTF32;
					var    respStm = context.Response.OutputStream;
					Stream compStm;
					switch (encoding)
					{
						case "gzip":
							compStm = new GZipStream(respStm,
													 CompressionMode.Compress);
							break;
						case "deflate":
							compStm = new DeflateStream(respStm,
														CompressionMode.Compress);
							break;
						default:
							compStm = null;
							break;
					}

					var wrtr = new StreamWriter(compStm);
					wrtr.Write(xml);
					wrtr.Close();
				}
			}
			catch (HttpListenerException) { }
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			_running = false;
			_lstner.Stop();
		}

		//[Test]
		//public void DeflateCall()
		//{
		//	encoding = "deflate";
		//	var proxy = XmlRpcProxyGen.Create<IStateName>();
		//	proxy.Url               = "http://127.0.0.1:11002/";
		//	proxy.EnableCompression = true;
		//	var name = proxy.GetStateName(1);
		//}

		//[Test]
		//public void GZipCall()
		//{
		//	encoding = "gzip";
		//	var proxy = XmlRpcProxyGen.Create<IStateName>();
		//	proxy.Url               = "http://127.0.0.1:11002/";
		//	proxy.EnableCompression = true;
		//	var name = proxy.GetStateName(1);
		//}
	}
}

#endif