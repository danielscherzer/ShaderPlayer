using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ShaderPlayer
{
	public static class NotificationExtensions
	{
		public static void SetNotify<TYPE>(this PropertyChangedEventHandler propertyChanged, ref TYPE valueBackend, TYPE value, Action<TYPE> action = null, [CallerMemberName] string memberName = "")
		{
			valueBackend = value;
			action?.Invoke(value);
			propertyChanged?.Invoke(null, new PropertyChangedEventArgs(memberName));
		}
	}
}
