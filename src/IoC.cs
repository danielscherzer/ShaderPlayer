using System;
using Unity;

namespace ShaderPlayer
{
	static class IoC
	{
		public static readonly IUnityContainer Container = new UnityContainer();
		public static void RegisterInstance<TYPE>(TYPE instance) => Container.RegisterInstance(instance);
		public static TYPE Resolve<TYPE>() where TYPE : class => Container.Resolve<TYPE>() ?? throw new NullReferenceException(nameof(TYPE));
	}
}
