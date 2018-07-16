/* 
XML-RPC.NET library
Copyright (c) 2001-2006, Charles Cook <charlescook@cookcomputing.com>

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, 
and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.
*/

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CookComputing.XmlRpc
{
	public class XmlRpcClientProtocol : Component, IXmlRpcProxy
	{


#if (!COMPACT_FRAMEWORK)
		public XmlRpcClientProtocol(IContainer container)
		{
			container.Add(this);
			InitializeComponent();
		}
#endif
		public XmlRpcClientProtocol()
		{
			InitializeComponent();
		}

		public object Invoke(
		  MethodBase mb,
		  params object[] parameters)
		{
			return Invoke(this, mb as MethodInfo, parameters);
		}

		public object Invoke(
		  MethodInfo mi,
		  params object[] parameters)
		{
			return Invoke(this, mi, parameters);
		}

		public object Invoke(
		  string methodName,
		  params object[] parameters)
		{
			return Invoke(this, methodName, parameters);
		}

		public object Invoke(
		  object clientObj,
		  string methodName,
		  params object[] parameters)
		{
			var mi = GetMethodInfoFromName(clientObj, methodName, parameters);
			return Invoke(this, mi, parameters);
		}

		public object Invoke(
		  object clientObj,
		  MethodInfo mi,
		  params object[] parameters)
		{
#if (!COMPACT_FRAMEWORK)
			ResponseHeaders = null;
			ResponseCookies = null;
#endif
			WebRequest webReq = null;
			object reto;
			try
			{
				var useUrl = GetEffectiveUrl(clientObj);
				webReq = GetWebRequest(new Uri(useUrl));
				var req = MakeXmlRpcRequest(webReq, mi, parameters,
				  XmlRpcMethod, Id);
				SetProperties(webReq);
				SetRequestHeaders(Headers, webReq);
#if (!COMPACT_FRAMEWORK)
				SetClientCertificates(ClientCertificates, webReq);
#endif
				Stream serStream;
				Stream reqStream = null;
				var logging = (RequestEvent != null);
				if (!logging)
					serStream = reqStream = webReq.GetRequestStream();
				else
					serStream = new MemoryStream(2000);
				try
				{
					var serializer = new XmlRpcSerializer();
					if (XmlEncoding != null)
						serializer.XmlEncoding = XmlEncoding;
					serializer.UseIndentation = UseIndentation;
					serializer.Indentation = Indentation;
					serializer.NonStandard = NonStandard;
					serializer.UseStringTag = UseStringTag;
					serializer.UseIntTag = UseIntTag;
					serializer.UseEmptyParamsTag = UseEmptyParamsTag;
					serializer.SerializeRequest(serStream, req);
					if (logging)
					{
						reqStream = webReq.GetRequestStream();
						serStream.Position = 0;
						Util.CopyStream(serStream, reqStream);
						reqStream.Flush();
						serStream.Position = 0;
						OnRequest(new XmlRpcRequestEventArgs(req.proxyId, req.number,
						  serStream));
					}
				}
				finally
				{
					reqStream?.Close();
				}
				var webResp = GetWebResponse(webReq) as HttpWebResponse;
#if (!COMPACT_FRAMEWORK)
				ResponseCookies = webResp.Cookies;
				ResponseHeaders = webResp.Headers;
#endif
				Stream respStm = null;
				logging = (ResponseEvent != null);
				try
				{
					respStm = webResp.GetResponseStream();
					Stream deserStream;
					if (!logging)
					{
						deserStream = respStm;
					}
					else
					{
						deserStream = new MemoryStream(2000);
						Util.CopyStream(respStm, deserStream);
						deserStream.Flush();
						deserStream.Position = 0;
					}
#if (!COMPACT_FRAMEWORK && !FX1_0)
					deserStream = MaybeDecompressStream(webResp,
					  deserStream);
#endif
					try
					{
						var resp = ReadResponse(req, webResp, deserStream, null);
						reto = resp.retVal;
					}
					finally
					{
						if (logging)
						{
							deserStream.Position = 0;
							OnResponse(new XmlRpcResponseEventArgs(req.proxyId, req.number,
							  deserStream));
						}
					}
				}
				finally
				{
					respStm?.Close();
				}
			}
			finally
			{
				if (webReq != null)
					webReq = null;
			}
			return reto;
		}

		#region Properties

		public bool AllowAutoRedirect { get; set; } = true;

#if (!COMPACT_FRAMEWORK)
		[Browsable(false)]
		public X509CertificateCollection ClientCertificates { get; } = new X509CertificateCollection();
#endif

#if (!COMPACT_FRAMEWORK)
		public string ConnectionGroupName { get; set; } = null;
#endif

		[Browsable(false)]
		public ICredentials Credentials { get; set; } = null;

#if (!COMPACT_FRAMEWORK && !FX1_0)
		public bool EnableCompression { get; set; } = false;
#endif

		[Browsable(false)]
		public WebHeaderCollection Headers { get; } = new WebHeaderCollection();

#if (!COMPACT_FRAMEWORK && !FX1_0)
		public bool Expect100Continue { get; set; } = false;
#endif

#if (!COMPACT_FRAMEWORK)
		public CookieContainer CookieContainer { get; } = new CookieContainer();
#endif

		public Guid Id { get; } = Util.NewGuid();

		public int Indentation { get; set; } = 2;

		public bool KeepAlive { get; set; } = true;

		public XmlRpcNonStandard NonStandard { get; set; } = XmlRpcNonStandard.None;

		public bool PreAuthenticate { get; set; } = false;

		[Browsable(false)]
		public Version ProtocolVersion { get; set; } = HttpVersion.Version11;

		[Browsable(false)]
		public IWebProxy Proxy { get; set; } = null;

#if (!COMPACT_FRAMEWORK)
		public CookieCollection ResponseCookies { get; private set; }
#endif

#if (!COMPACT_FRAMEWORK)
		public WebHeaderCollection ResponseHeaders { get; private set; }
#endif

		public int Timeout { get; set; } = 100000;

		public string Url { get; set; } = null;

		public bool UseEmptyParamsTag { get; set; } = true;

		public bool UseIndentation { get; set; } = true;

		public bool UseIntTag { get; set; } = false;

		public string UserAgent { get; set; } = "XML-RPC.NET";

		public bool UseStringTag { get; set; } = true;

		[Browsable(false)]
		public Encoding XmlEncoding { get; set; } = null;

		public string XmlRpcMethod { get; set; } = null;

		#endregion

		public void SetProperties(WebRequest webReq)
		{
			if (Proxy != null)
				webReq.Proxy = Proxy;
			var httpReq = (HttpWebRequest)webReq;
			httpReq.UserAgent = UserAgent;
			httpReq.ProtocolVersion = ProtocolVersion;
			httpReq.KeepAlive = KeepAlive;
#if (!COMPACT_FRAMEWORK)
			httpReq.CookieContainer = CookieContainer;
#endif
#if (!COMPACT_FRAMEWORK && !FX1_0)
			httpReq.ServicePoint.Expect100Continue = Expect100Continue;
#endif
			httpReq.AllowAutoRedirect = AllowAutoRedirect;
			webReq.Timeout = Timeout;
#if (!COMPACT_FRAMEWORK)
			webReq.ConnectionGroupName = ConnectionGroupName;
#endif
			webReq.Credentials = Credentials;
			webReq.PreAuthenticate = PreAuthenticate;
			// Compact Framework sets this to false by default
			((HttpWebRequest) webReq).AllowWriteStreamBuffering = true;
#if (!COMPACT_FRAMEWORK && !FX1_0)
			if (EnableCompression)
				webReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
#endif
		}

		private void SetRequestHeaders(
		  WebHeaderCollection headers,
		  WebRequest webReq)
		{
			foreach (string key in headers)
			{
				webReq.Headers.Add(key, headers[key]);
			}
		}
#if (!COMPACT_FRAMEWORK)
		private void SetClientCertificates(
		  X509CertificateCollection certificates,
		  WebRequest webReq)
		{
			foreach (X509Certificate certificate in certificates)
			{
				HttpWebRequest httpReq = (HttpWebRequest)webReq;
				httpReq.ClientCertificates.Add(certificate);
			}
		}
#endif
		private static XmlRpcRequest MakeXmlRpcRequest(WebRequest webReq, MethodInfo mi,
		  object[] parameters, string xmlRpcMethod,
		  Guid proxyId)
		{
			webReq.Method = "POST";
			webReq.ContentType = "text/xml";
			string rpcMethodName = GetRpcMethodName(mi);
			XmlRpcRequest req = new XmlRpcRequest(rpcMethodName, parameters, mi,
			  xmlRpcMethod, proxyId);
			return req;
		}

		XmlRpcResponse ReadResponse(
		  XmlRpcRequest req,
		  WebResponse webResp,
		  Stream respStm,
		  Type returnType)
		{
			HttpWebResponse httpResp = (HttpWebResponse)webResp;
			if (httpResp.StatusCode != HttpStatusCode.OK)
			{
				// status 400 is used for errors caused by the client
				// status 500 is used for server errors (not server application
				// errors which are returned as fault responses)
				if (httpResp.StatusCode == HttpStatusCode.BadRequest)
					throw new XmlRpcException(httpResp.StatusDescription);
				else
					throw new XmlRpcServerException(httpResp.StatusDescription);
			}

			XmlRpcSerializer serializer = new XmlRpcSerializer {NonStandard = NonStandard};
			Type retType = returnType;
			if (retType == null)
				retType = req.mi.ReturnType;
			XmlRpcResponse xmlRpcResp
			  = serializer.DeserializeResponse(respStm, retType);
			return xmlRpcResp;
		}

		MethodInfo GetMethodInfoFromName(object clientObj, string methodName,
		  object[] parameters)
		{
			Type[] paramTypes = new Type[0];
			if (parameters != null)
			{
				paramTypes = new Type[parameters.Length];
				for (var i = 0; i < paramTypes.Length; i++)
				{
					if (parameters[i] == null)
						throw new XmlRpcNullParameterException("Null parameters are invalid");
					paramTypes[i] = parameters[i].GetType();
				}
			}
			Type type = clientObj.GetType();
			var mi = type.GetMethod(methodName, paramTypes);
			if (mi == null)
			{
				try
				{
					mi = type.GetMethod(methodName);
				}
				catch (AmbiguousMatchException)
				{
					throw new XmlRpcInvalidParametersException("Method parameters match "
					  + "the signature of more than one method");
				}
				if (mi == null)
					throw new Exception(
					  "Invoke on non-existent or non-public proxy method");

				throw new XmlRpcInvalidParametersException("Method parameters do "
				  + "not match signature of any method called " + methodName);
			}
			return mi;
		}

		private static string GetRpcMethodName(MethodInfo mi)
		{
			string rpcMethod;
			var methodName = mi.Name;
			var attr = Attribute.GetCustomAttribute(mi,
			  typeof(XmlRpcBeginAttribute));
			if (attr != null)
			{
				rpcMethod = ((XmlRpcBeginAttribute)attr).Method;
				if (rpcMethod == "")
				{
					if (!methodName.StartsWith("Begin") || methodName.Length <= 5)
						throw new Exception(string.Format(
						  "method {0} has invalid signature for begin method",
						  methodName));
					rpcMethod = methodName.Substring(5);
				}
				return rpcMethod;
			}
			// if no XmlRpcBegin attribute, must have XmlRpcMethod attribute   
			attr = Attribute.GetCustomAttribute(mi, typeof(XmlRpcMethodAttribute));
			if (attr == null)
			{
				throw new Exception("missing method attribute");
			}
			var xrmAttr = attr as XmlRpcMethodAttribute;
			rpcMethod = xrmAttr.Method;
			if (rpcMethod == "")
			{
				rpcMethod = mi.Name;
			}
			return rpcMethod;
		}

		public IAsyncResult BeginInvoke(
		  MethodBase mb,
		  object[] parameters,
		  AsyncCallback callback,
		  object outerAsyncState)
		{
			return BeginInvoke(mb as MethodInfo, parameters, this, callback,
			  outerAsyncState);
		}

		public IAsyncResult BeginInvoke(
		  MethodInfo mi,
		  object[] parameters,
		  AsyncCallback callback,
		  object outerAsyncState)
		{
			return BeginInvoke(mi, parameters, this, callback,
			  outerAsyncState);
		}

		public IAsyncResult BeginInvoke(
		  string methodName,
		  object[] parameters,
		  object clientObj,
		  AsyncCallback callback,
		  object outerAsyncState)
		{
			MethodInfo mi = GetMethodInfoFromName(clientObj, methodName, parameters);
			return BeginInvoke(mi, parameters, this, callback,
			  outerAsyncState);
		}

		public IAsyncResult BeginInvoke(
		  MethodInfo mi,
		  object[] parameters,
		  object clientObj,
		  AsyncCallback callback,
		  object outerAsyncState)
		{
			string useUrl = GetEffectiveUrl(clientObj);
			WebRequest webReq = GetWebRequest(new Uri(useUrl));
			XmlRpcRequest xmlRpcReq = MakeXmlRpcRequest(webReq, mi,
			  parameters, XmlRpcMethod, Id);
			SetProperties(webReq);
			SetRequestHeaders(Headers, webReq);
#if (!COMPACT_FRAMEWORK)
			SetClientCertificates(ClientCertificates, webReq);
#endif
			Encoding useEncoding = null;
			if (XmlEncoding != null)
				useEncoding = XmlEncoding;
			XmlRpcAsyncResult asr = new XmlRpcAsyncResult(this, xmlRpcReq,
			  useEncoding, UseEmptyParamsTag, UseIndentation, Indentation,
			  UseIntTag, UseStringTag, webReq, callback, outerAsyncState);
			webReq.BeginGetRequestStream(GetRequestStreamCallback,
			  asr);
			if (!asr.IsCompleted)
				asr.CompletedSynchronously = false;
			return asr;
		}

		private static void GetRequestStreamCallback(IAsyncResult asyncResult)
		{
			var clientResult
			  = (XmlRpcAsyncResult)asyncResult.AsyncState;
			clientResult.CompletedSynchronously = asyncResult.CompletedSynchronously;
			try
			{
				Stream serStream;
				Stream reqStream = null;
				var logging = (clientResult.ClientProtocol.RequestEvent != null);
				if (!logging)
				{
					serStream = reqStream
					  = clientResult.Request.EndGetRequestStream(asyncResult);
				}
				else
					serStream = new MemoryStream(2000);
				try
				{
					var req = clientResult.XmlRpcRequest;
					var serializer = new XmlRpcSerializer();
					if (clientResult.XmlEncoding != null)
						serializer.XmlEncoding = clientResult.XmlEncoding;
					serializer.UseEmptyParamsTag = clientResult.UseEmptyParamsTag;
					serializer.UseIndentation = clientResult.UseIndentation;
					serializer.Indentation = clientResult.Indentation;
					serializer.UseIntTag = clientResult.UseIntTag;
					serializer.UseStringTag = clientResult.UseStringTag;
					serializer.SerializeRequest(serStream, req);
					if (logging)
					{
						reqStream = clientResult.Request.EndGetRequestStream(asyncResult);
						serStream.Position = 0;
						Util.CopyStream(serStream, reqStream);
						reqStream.Flush();
						serStream.Position = 0;
						clientResult.ClientProtocol.OnRequest(
						  new XmlRpcRequestEventArgs(req.proxyId, req.number, serStream));
					}
				}
				finally
				{
					reqStream?.Close();
				}
				clientResult.Request.BeginGetResponse(
				  GetResponseCallback, clientResult);
			}
			catch (Exception ex)
			{
				ProcessAsyncException(clientResult, ex);
			}
		}

		static void GetResponseCallback(IAsyncResult asyncResult)
		{
			var result = (XmlRpcAsyncResult)asyncResult.AsyncState;
			result.CompletedSynchronously = asyncResult.CompletedSynchronously;
			try
			{
				result.Response = result.ClientProtocol.GetWebResponse(result.Request,
				  asyncResult);
			}
			catch (Exception ex)
			{
				ProcessAsyncException(result, ex);
				if (result.Response == null)
					return;
			}
			ReadAsyncResponse(result);
		}

		static void ReadAsyncResponse(XmlRpcAsyncResult result)
		{
			if (result.Response.ContentLength == 0)
			{
				result.Complete();
				return;
			}
			try
			{
				result.ResponseStream = result.Response.GetResponseStream();
				ReadAsyncResponseStream(result);
			}
			catch (Exception ex)
			{
				ProcessAsyncException(result, ex);
			}
		}

		static void ReadAsyncResponseStream(XmlRpcAsyncResult result)
		{
			IAsyncResult asyncResult;
			do
			{
				var buff = result.Buffer;
				var contLen = result.Response.ContentLength;
				if (buff == null)
				{
					result.Buffer = contLen == -1 ? new byte[1024] : new byte[contLen];
				}
				else
				{
					if (contLen != -1 && contLen > result.Buffer.Length)
						result.Buffer = new byte[contLen];
				}
				buff = result.Buffer;
				asyncResult = result.ResponseStream.BeginRead(buff, 0, buff.Length,
				  ReadResponseCallback, result);
				if (!asyncResult.CompletedSynchronously)
					return;
			}
			while (!(ProcessAsyncResponseStreamResult(result, asyncResult)));
		}

		static bool ProcessAsyncResponseStreamResult(XmlRpcAsyncResult result,
		  IAsyncResult asyncResult)
		{
			int endReadLen = result.ResponseStream.EndRead(asyncResult);
			long contLen = result.Response.ContentLength;
			bool completed;
			if (endReadLen == 0)
				completed = true;
			else if (contLen > 0 && endReadLen == contLen)
			{
				result.ResponseBufferedStream = new MemoryStream(result.Buffer);
				completed = true;
			}
			else
			{
				if (result.ResponseBufferedStream == null)
				{
					result.ResponseBufferedStream = new MemoryStream(result.Buffer.Length);
				}
				result.ResponseBufferedStream.Write(result.Buffer, 0, endReadLen);
				completed = false;
			}
			if (completed)
				result.Complete();
			return completed;
		}


		static void ReadResponseCallback(IAsyncResult asyncResult)
		{
			XmlRpcAsyncResult result = (XmlRpcAsyncResult)asyncResult.AsyncState;
			result.CompletedSynchronously = asyncResult.CompletedSynchronously;
			if (asyncResult.CompletedSynchronously)
				return;
			try
			{
				bool completed = ProcessAsyncResponseStreamResult(result, asyncResult);
				if (!completed)
					ReadAsyncResponseStream(result);
			}
			catch (Exception ex)
			{
				ProcessAsyncException(result, ex);
			}
		}

		static void ProcessAsyncException(XmlRpcAsyncResult clientResult,
		  Exception ex)
		{
			WebException webex = ex as WebException;
			if (webex?.Response != null)
			{
				clientResult.Response = webex.Response;
				return;
			}
			if (clientResult.IsCompleted)
				throw new Exception("error during async processing");
			clientResult.Complete(ex);
		}

		public object EndInvoke(
		  IAsyncResult asr)
		{
			return EndInvoke(asr, null);
		}

		public object EndInvoke(
		  IAsyncResult asr,
		  Type returnType)
		{
			object reto;
			Stream responseStream = null;
			try
			{
				var clientResult = (XmlRpcAsyncResult)asr;
				if (clientResult.Exception != null)
					throw clientResult.Exception;
				if (clientResult.EndSendCalled)
					throw new Exception("dup call to EndSend");
				clientResult.EndSendCalled = true;
				var webResp = (HttpWebResponse)clientResult.WaitForResponse();
#if (!COMPACT_FRAMEWORK)
				clientResult._responseCookies = webResp.Cookies;
				clientResult._responseHeaders = webResp.Headers;
#endif
				responseStream = clientResult.ResponseBufferedStream;
				if (ResponseEvent != null)
				{
					OnResponse(new XmlRpcResponseEventArgs(
					  clientResult.XmlRpcRequest.proxyId,
					  clientResult.XmlRpcRequest.number,
					  responseStream));
					responseStream.Position = 0;
				}
#if (!COMPACT_FRAMEWORK && !FX1_0)
				responseStream = MaybeDecompressStream((HttpWebResponse)webResp,
				  responseStream);
#endif
				var resp = ReadResponse(clientResult.XmlRpcRequest,
				  webResp, responseStream, returnType);
				reto = resp.retVal;
			}
			finally
			{
				responseStream?.Close();
			}
			return reto;
		}

		private string GetEffectiveUrl(object clientObj)
		{
			var type = clientObj.GetType();
			// client can either have define URI in attribute or have set it
			// via proxy's ServiceURI property - but must exist by now
			var useUrl = "";
			if (string.IsNullOrEmpty(Url))
			{
				var urlAttr = Attribute.GetCustomAttribute(type,
				  typeof(XmlRpcUrlAttribute));
				if (urlAttr != null)
				{
					var xrsAttr = urlAttr as XmlRpcUrlAttribute;
					useUrl = xrsAttr.Uri;
				}
			}
			else
			{
				useUrl = Url;
			}
			if (useUrl == "")
			{
				throw new XmlRpcMissingUrl("Proxy XmlRpcUrl attribute or Url "
				  + "property not set.");
			}
			return useUrl;
		}

		#region Introspection Methods
		[XmlRpcMethod("system.listMethods")]
		public string[] SystemListMethods()
		{
			return (string[])Invoke("SystemListMethods", new object[0]);
		}

		[XmlRpcMethod("system.listMethods")]
		public IAsyncResult BeginSystemListMethods(
		  AsyncCallback callback,
		  object state)
		{
			return BeginInvoke("SystemListMethods", new object[0], this, callback,
			  state);
		}

		public string[] EndSystemListMethods(IAsyncResult asyncResult)
		{
			return (string[])EndInvoke(asyncResult);
		}

		[XmlRpcMethod("system.methodSignature")]
		public object[] SystemMethodSignature(string methodName)
		{
			return (object[])Invoke("SystemMethodSignature",
			  new object[] { methodName });
		}

		[XmlRpcMethod("system.methodSignature")]
		public IAsyncResult BeginSystemMethodSignature(
		  string methodName,
		  AsyncCallback callback,
		  object state)
		{
			return BeginInvoke("SystemMethodSignature",
			  new Object[] { methodName }, this, callback, state);
		}

		public Array EndSystemMethodSignature(IAsyncResult asyncResult)
		{
			return (Array)EndInvoke(asyncResult);
		}

		[XmlRpcMethod("system.methodHelp")]
		public string SystemMethodHelp(string methodName)
		{
			return (string)Invoke("SystemMethodHelp",
			  new Object[] { methodName });
		}

		[XmlRpcMethod("system.methodHelp")]
		public IAsyncResult BeginSystemMethodHelp(
		  string methodName,
		  AsyncCallback callback,
		  object state)
		{
			return BeginInvoke("SystemMethodHelp",
			  new object[] { methodName }, this, callback, state);
		}

		public string EndSystemMethodHelp(IAsyncResult asyncResult)
		{
			return (string)EndInvoke(asyncResult);
		}
		#endregion

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}
		#endregion

		protected virtual WebRequest GetWebRequest(Uri uri)
		{
			var req = WebRequest.Create(uri);
			return req;
		}

		protected virtual WebResponse GetWebResponse(WebRequest request)
		{
			WebResponse ret;
			try
			{
				ret = request.GetResponse();
			}
			catch (WebException ex)
			{
				if (ex.Response == null)
					throw;
				ret = ex.Response;
			}
			return ret;
		}

#if (!COMPACT_FRAMEWORK && !FX1_0)
		// support for gzip and deflate
		protected Stream MaybeDecompressStream(HttpWebResponse httpWebResp,
		  Stream respStream)
		{
			Stream decodedStream;
			var contentEncoding = (httpWebResp.ContentEncoding ?? "").ToLower();
			if (contentEncoding.Contains("gzip"))
			{
				decodedStream = new System.IO.Compression.GZipStream(respStream,
				  System.IO.Compression.CompressionMode.Decompress);
			}
			else if (contentEncoding.Contains("deflate"))
			{
				decodedStream = new System.IO.Compression.DeflateStream(respStream,
				  System.IO.Compression.CompressionMode.Decompress);
			}
			else
				decodedStream = respStream;
			return decodedStream;
		}
#endif

		protected virtual WebResponse GetWebResponse(WebRequest request,
		  IAsyncResult result)
		{
			return request.EndGetResponse(result);
		}

		public event XmlRpcRequestEventHandler RequestEvent;
		public event XmlRpcResponseEventHandler ResponseEvent;


		protected virtual void OnRequest(XmlRpcRequestEventArgs e)
		{
			RequestEvent?.Invoke(this, e);
		}

		internal bool LogResponse => ResponseEvent != null;

		protected virtual void OnResponse(XmlRpcResponseEventArgs e)
		{
			ResponseEvent?.Invoke(this, e);
		}

		internal void InternalOnResponse(XmlRpcResponseEventArgs e)
		{
			OnResponse(e);
		}
	}

#if (COMPACT_FRAMEWORK)
  // dummy attribute because System.ComponentModel.Browsable is not
  // support in the compact framework
  [AttributeUsage(AttributeTargets.Property)]
  public class BrowsableAttribute : Attribute
  {
    public BrowsableAttribute(bool dummy)
    {
    }
  }
#endif

	public delegate void XmlRpcRequestEventHandler(object sender,
	  XmlRpcRequestEventArgs args);

	public delegate void XmlRpcResponseEventHandler(object sender,
	  XmlRpcResponseEventArgs args);
}


