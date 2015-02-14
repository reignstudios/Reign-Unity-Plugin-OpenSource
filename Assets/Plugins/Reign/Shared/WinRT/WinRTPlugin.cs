#if REIGN_POSTBUILD
using System;
using System.Threading;

namespace Reign
{
	public static class WinRTPlugin
	{
		public static void Init()
		{
			initMethodPointers();
		}
		
		private static void initMethodPointers()
		{
			// classic types
			System.Text.Reign.Encoding.GetEncoding = Encoding_GetEncoding_get;
			System.Text.Reign.Encoding.Singleton.GetString = WinRTLegacy.Text.Encoding.Default.GetString;
			System.Text.Reign.Encoding.Singleton.GetBytes = WinRTLegacy.Text.Encoding.Default.GetBytes;
			
			System.Text.Reign.Encoding.ASCII.GetString = WinRTLegacy.Text.Encoding.ASCII.GetString;
			System.Text.Reign.Encoding.ASCII.GetBytes = WinRTLegacy.Text.Encoding.ASCII.GetBytes;
			
			System.Text.Reign.Encoding.UTF8.GetString = WinRTLegacy.Text.Encoding.UTF8.GetString;
			System.Text.Reign.Encoding.UTF8.GetBytes = WinRTLegacy.Text.Encoding.UTF8.GetBytes;
			
			System.Text.Reign.CultureInfo.ANSICodePage = CultureInfo_ANSICodePage_Get;
		}
		
		private static System.Text.Reign.Encoding Encoding_GetEncoding_get(int codepage)
		{
			return System.Text.Reign.Encoding.Singleton;
		}
		
		private static int CultureInfo_ANSICodePage_Get()
		{
			return WinRTLegacy.Text.Encoding.Default.CodePage;
		}
	}
	
	static class Thread
	{
		public static void Sleep(int milli)
		{
			using (var _event = new ManualResetEvent(false))
			{
				_event.WaitOne(milli);
			}
		}
	}
}
#elif UNITY_WINRT
namespace System.Text.Reign
{
	public class Encoding
	{
		public static Encoding Singleton;
		
		public delegate Encoding GetEncodingCallbackMethod(int codepage);
		public static GetEncodingCallbackMethod GetEncoding;
		
		public delegate string GetStringCallbackMathod(byte[] bytes, int index, int count);
		public GetStringCallbackMathod GetString;
		
		public delegate byte[] GetBytesCallbackMethod(string s);
		public GetBytesCallbackMethod GetBytes;
		
		public static EncodingASCII ASCII
		{
			get
			{
				if (EncodingASCII.Singleton == null) EncodingASCII.Singleton = new EncodingASCII();
				return EncodingASCII.Singleton;
			}
		}
		
		public static EncodingUTF8 UTF8
		{
			get
			{
				if (EncodingUTF8.Singleton == null) EncodingUTF8.Singleton = new EncodingUTF8();
				return EncodingUTF8.Singleton;
			}
		}
		
		static Encoding()
		{
			Singleton = new Encoding();
		}
	}
	
	public class EncodingASCII : Encoding
	{
		public new static EncodingASCII Singleton;
		
		public new delegate string GetStringCallbackMathod(byte[] bytes, int index, int count);
		public new GetStringCallbackMathod GetString;
		
		public new delegate byte[] GetBytesCallbackMethod(string s);
		public new GetBytesCallbackMethod GetBytes;
		
		public EncodingASCII()
		{
			EncodingASCII.Singleton = this;
		}
	}
	
	public class EncodingUTF8 : Encoding
	{
		public new static EncodingUTF8 Singleton;
		
		public new delegate string GetStringCallbackMathod(byte[] bytes, int index, int count);
		public new GetStringCallbackMathod GetString;
		
		public new delegate byte[] GetBytesCallbackMethod(string s);
		public new GetBytesCallbackMethod GetBytes;
		
		public EncodingUTF8()
		{
			EncodingUTF8.Singleton = this;
		}
	}
	
	public class CultureInfo
	{
		public delegate int ANSICodePageCallbackMethod();
		public static ANSICodePageCallbackMethod ANSICodePage;
	}
}
#endif