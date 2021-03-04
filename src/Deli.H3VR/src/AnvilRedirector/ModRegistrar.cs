using Deli.Runtime;
using Deli.Runtime.Yielding;
using UnityEngine;

namespace Deli.H3VR
{
	public class ModRegistrar
	{
		private readonly Mod _mod;
		private readonly GlobalRegistrar _global;

		internal ModRegistrar(Mod mod, GlobalRegistrar global)
		{
			_mod = mod;
			_global = global;
		}

		public ResultYieldInstruction<AssetBundle> Add(DelayedTypedFileHandle<AssetBundle> file, out string key)
		{
			return _global.Add(_mod, file, out key);
		}
	}
}
