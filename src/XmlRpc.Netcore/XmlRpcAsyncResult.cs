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
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace CookComputing.XmlRpc
{
	public class XmlRpcAsyncResult : IAsyncResult
	{
		private          bool                 _completedSynchronously;
		private          bool                 _isCompleted;
		private          ManualResetEvent     _manualResetEvent;
		private readonly AsyncCallback        _userCallback;

		//internal members
		internal XmlRpcAsyncResult(
			XmlRpcClientProtocol clientProtocol,
			XmlRpcRequest xmlRpcReq,
			Encoding xmlEncoding,
			bool useEmptyParamsTag,
			bool useIndentation,
			int indentation,
			bool useIntTag,
			bool useStringTag,
			WebRequest request,
			AsyncCallback userCallback,
			object userAsyncState)
		{
			XmlRpcRequest          = xmlRpcReq;
			ClientProtocol         = clientProtocol;
			Request                = request;
			AsyncState         = userAsyncState;
			_userCallback           = userCallback;
			_completedSynchronously = true;
			XmlEncoding            = xmlEncoding;
			UseEmptyParamsTag     = useEmptyParamsTag;
			UseIndentation        = useIndentation;
			Indentation           = indentation;
			UseIntTag             = useIntTag;
			UseStringTag          = useStringTag;
		}

#if (!COMPACT_FRAMEWORK)
		public CookieCollection ResponseCookies => _responseCookies;
#endif

#if (!COMPACT_FRAMEWORK)
		public WebHeaderCollection ResponseHeaders => _responseHeaders;
#endif


		public bool UseEmptyParamsTag { get; }

		public bool UseIndentation { get; }

		public int Indentation { get; }

		public bool UseIntTag { get; }

		public bool UseStringTag { get; }

		public Exception Exception { get; private set; }

		public XmlRpcClientProtocol ClientProtocol { get; }

		internal bool EndSendCalled { get; set; }

		internal byte[] Buffer { get; set; }

		internal WebRequest Request { get; }

		internal WebResponse Response { get; set; }

		internal Stream ResponseStream { get; set; }

		internal XmlRpcRequest XmlRpcRequest { get; set; }

		internal Stream ResponseBufferedStream { get; set; }

		internal Encoding XmlEncoding { get; }

		// IAsyncResult members
		public object AsyncState { get; }

		public WaitHandle AsyncWaitHandle
		{
			get {
				var completed = _isCompleted;
				if (_manualResetEvent == null)
					lock (this)
					{
						if (_manualResetEvent == null)
							_manualResetEvent = new ManualResetEvent(completed);
					}

				if (!completed && _isCompleted)
					_manualResetEvent.Set();
				return _manualResetEvent;
			}
		}

		public bool CompletedSynchronously
		{
			get => _completedSynchronously;
			set {
				if (_completedSynchronously)
					_completedSynchronously = value;
			}
		}

		public bool IsCompleted => _isCompleted;

		// public members
		public void Abort()
		{
			Request?.Abort();
		}

		internal void Complete(
			Exception ex)
		{
			Exception = ex;
			Complete();
		}

		internal void Complete()
		{
			try
			{
				if (ResponseStream != null)
				{
					ResponseStream.Close();
					ResponseStream = null;
				}

				if (ResponseBufferedStream != null)
					ResponseBufferedStream.Position = 0;
			}
			catch (Exception ex)
			{
				if (Exception == null)
					Exception = ex;
			}

			_isCompleted = true;
			try
			{
				_manualResetEvent?.Set();
			}
			catch (Exception ex)
			{
				if (Exception == null)
					Exception = ex;
			}

			_userCallback?.Invoke(this);
		}

		internal WebResponse WaitForResponse()
		{
			if (!_isCompleted)
				AsyncWaitHandle.WaitOne();
			if (Exception != null)
				throw Exception;
			return Response;
		}
#if (!COMPACT_FRAMEWORK)
		internal CookieCollection    _responseCookies;
		internal WebHeaderCollection _responseHeaders;
#endif
	}
}