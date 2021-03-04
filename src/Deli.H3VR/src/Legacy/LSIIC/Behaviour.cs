using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Deli.Runtime;
using Deli.Runtime.Yielding;
using Deli.Setup;
using Deli.VFS;
using FistVR;
using UnityEngine;

namespace Deli.H3VR.Legacy.LSIIC
{
	internal class Behaviour : DeliBehaviour
	{
		private const string LoaderName = Constants.LegacyLoaderPrefix + "virtual_object";
		private const string DirectoryLoaderName = LoaderName + ".directory";

		private readonly ModRegistrar _registrar;
		private BundleLoader _bundleLoader;

		public Behaviour()
		{
			_registrar = AnvilRedirector.Instance[Source];

			var defered = new BundleLoaders.Deferred(Logger);
			On.FistVR.IM.GenerateItemDBs += InjectObjects(defered);
			_bundleLoader = defered.Load;

			Stages.Runtime += OnRuntime;
		}

		private void OnRuntime(RuntimeStage stage)
		{
			stage.RuntimeAssetLoaders[Source, LoaderName] = LoadAssetFile;
			stage.RuntimeAssetLoaders[Source, DirectoryLoaderName] = LoadAssetDirectory;
		}

		private IEnumerator LoadAssetFile(RuntimeStage stage, Mod mod, IHandle handle)
		{
			if (handle is not IFileHandle file)
			{
				throw new ArgumentException("Handle was not a file. If you intended to pass a directory of VirtualObjects, use the '" + DirectoryLoaderName + "' loader instead.", nameof(handle));
			}

			return LoadAssetFile(stage, file);
		}

		private IEnumerator LoadAssetFile(RuntimeStage stage, IFileHandle file)
		{
			var reader = stage.GetReader<AssetBundle>();
			var typed = new DelayedTypedFileHandle<AssetBundle>(file, reader);

			var bundle = _registrar.Add(typed, out var key);
			yield return bundle;

			var objects = new AsyncOperationYieldInstruction<AssetBundleRequest, IEnumerable<FVRObject>>(bundle.Result.LoadAllAssetsAsync<FVRObject>(),
				operation => operation.allAssets.Cast<FVRObject>());
			var ids = new AsyncOperationYieldInstruction<AssetBundleRequest, IEnumerable<ItemSpawnerID>>(bundle.Result.LoadAllAssetsAsync<ItemSpawnerID>(),
				operation => operation.allAssets.Cast<ItemSpawnerID>());

			var objectsCoroutine = StartCoroutine(objects);
			var idsCoroutine = StartCoroutine(ids);

			yield return objectsCoroutine;
			yield return idsCoroutine;

			_bundleLoader(new VirtualObjectAssetBundle(key, objects.Result, ids.Result));
		}

		private IEnumerator LoadAssetDirectory(RuntimeStage stage, Mod mod, IHandle handle)
		{
			if (handle is not IDirectoryHandle directory)
			{
				throw new ArgumentException("Handle must be a directory.", nameof(handle));
			}

			// Parallelize the process
			var buffer = new Queue<Coroutine>();

			foreach (var file in directory.GetFilesRecursive().Where(x => x.GetExtension() is null))
			{
				buffer.Enqueue(StartCoroutine(LoadAssetFile(stage, file)));
			}

			foreach (var item in buffer)
			{
				yield return item;
			}
		}

		private On.FistVR.IM.hook_GenerateItemDBs InjectObjects(BundleLoaders.Deferred loader)
		{
			void Closure(On.FistVR.IM.orig_GenerateItemDBs orig, IM self)
			{
				orig(self);

				try
				{
					loader.AppendAll(self);
				}
				finally
				{
					_bundleLoader = new BundleLoaders.Immediate(Logger, self).Load;
					On.FistVR.IM.GenerateItemDBs -= Closure;
				}
			}

			return Closure;
		}
	}

	internal static class ExtensionMethods
	{
		public static TValue GetOrInsertWith<TKey, TValue>(this Dictionary<TKey, TValue> @this, TKey key, Func<TValue> value)
		{
			if (!@this.TryGetValue(key, out var existing))
			{
				existing = value();
				@this.Add(key, existing);
			}

			return existing;
		}
	}
}
