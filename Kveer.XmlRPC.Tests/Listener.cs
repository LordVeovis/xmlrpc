using System.Net;
using System.Threading;

#if !FX1_0

namespace ntest
{
	internal class Listener
	{
		private readonly HttpListener          _lstner = new HttpListener();
		private          bool                  _running;
		private readonly XmlRpcListenerService _svc;

		public Listener(XmlRpcListenerService svc, string uri)
		{
			_svc = svc;
			_lstner.Prefixes.Add(uri);
		}

		public void Start()
		{
			var thrd = new Thread(Run);
			_running = true;
			_lstner.Start();
			thrd.Start();
		}

		public void Stop()
		{
			_running = false;
			_lstner.Stop();
		}

		public void Run()
		{
			try
			{
				while (_running)
				{
					var context = _lstner.GetContext();
					context.Response.Headers.Add("BarHeader", "BarValue");
					context.Response.Cookies.Add(new Cookie("FooCookie", "FooValue"));
					_svc.ProcessRequest(context);
				}
			}
			catch (HttpListenerException) { }
		}
	}
}

#endif