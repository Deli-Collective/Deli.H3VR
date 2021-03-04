using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Deli.Runtime;
using Deli.Runtime.Yielding;
using UnityEngine;

namespace Deli.H3VR.AnvilAssetBundleInjector
{
	internal class GlobalRegistrar : IEnumerable<AssetBundle>
	{
		private readonly Dictionary<string, AssetBundle> _bundles = new();

		public bool TryGet(string path, [MaybeNullWhen(false)] out AsyncOperation operation)
		{
			if (!_bundles.TryGetValue(path, out var bundle))
			{
				operation = null;
				return false;
			}

			operation = new AnvilDummyOperation(bundle);
			return true;
		}

		public ResultYieldInstruction<AssetBundle> Add(Mod mod, DelayedTypedFileHandle<AssetBundle> file)
		{
			var key = mod.Info.Guid + ":" + file.Path;

			return file.GetOrRead().CallbackWith(bundle => _bundles.Add(key, bundle));
		}

		public Dictionary<string, AssetBundle>.ValueCollection.Enumerator GetEnumerator()
		{
			return _bundles.Values.GetEnumerator();
		}

		IEnumerator<AssetBundle> IEnumerable<AssetBundle>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
