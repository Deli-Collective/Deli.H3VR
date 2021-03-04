using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Deli.H3VR.AnvilAssetBundleInjector;
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
		private const string LoaderName = "virtual_object";
		private const string DirectoryLoaderName = LoaderName + ".directory";

		private readonly ModRegistrar _registrar;
		private readonly HashSet<IFileHandle> _loading = new();
		private readonly Queue<VirtualObjectAssetBundle> _bundles = new();

		private readonly struct VirtualObjectAssetBundle
		{
			public readonly Mod Mod;
			public readonly IFileHandle File;
			public readonly IEnumerable<FVRObject> Objects;
			public readonly IEnumerable<ItemSpawnerID> IDs;

			public VirtualObjectAssetBundle(Mod mod, IFileHandle file, IEnumerable<FVRObject> objects, IEnumerable<ItemSpawnerID> ids)
			{
				Mod = mod;
				File = file;
				Objects = objects;
				IDs = ids;
			}
		}

		public Behaviour()
		{
			_registrar = AnvilRedirector.Instance[Source];

			Stages.Setup += OnSetup;
			Stages.Runtime += OnRuntime;
		}

		private void OnSetup(SetupStage stage)
		{
			On.FistVR.IM.GenerateItemDBs += InjectObjects;
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

			return LoadAssetFile(stage, mod, file);
		}

		private IEnumerator LoadAssetFile(RuntimeStage stage, Mod mod, IFileHandle file)
		{
			var reader = stage.GetReader<AssetBundle>();
			var typed = new DelayedTypedFileHandle<AssetBundle>(file, reader);

			_loading.Add(file);
			{
				var bundle = _registrar.Add(typed);
				yield return bundle;

				var objects = new AsyncOperationYieldInstruction<AssetBundleRequest, IEnumerable<FVRObject>>(bundle.Result.LoadAllAssetsAsync<FVRObject>(),
					operation => operation.allAssets.Cast<FVRObject>());
				var ids = new AsyncOperationYieldInstruction<AssetBundleRequest, IEnumerable<ItemSpawnerID>>(bundle.Result.LoadAllAssetsAsync<ItemSpawnerID>(),
					operation => operation.allAssets.Cast<ItemSpawnerID>());

				var objectsCoroutine = StartCoroutine(objects);
				var idsCoroutine = StartCoroutine(ids);

				yield return objectsCoroutine;
				yield return idsCoroutine;

				_bundles.Enqueue(new VirtualObjectAssetBundle(mod, file, objects.Result, ids.Result));
			}
			_loading.Remove(file);
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
				buffer.Enqueue(StartCoroutine(LoadAssetFile(stage, mod, file)));
			}

			foreach (var item in buffer)
			{
				yield return item;
			}
		}

		private void InjectObjects(On.FistVR.IM.orig_GenerateItemDBs orig, IM self)
		{
			orig(self);

			{
				if (_loading.Count > 0)
				{
					Logger.LogWarning("Objects have begun to inject before all asset bundles were loaded. Objects from the following asset bundles will not load:");

					foreach (var file in _loading)
					{
						Logger.LogWarning(file);
					}
				}
			}

			var guidToId = self.SpawnerIDDic;

			foreach (var bundle in _bundles)
			{
				Logger.LogDebug($"Loading objects from {bundle.Mod}: {bundle.File}");

				// Inject objects
				foreach (var obj in bundle.Objects)
				{
					// Redirect the bundle of the object
					obj.m_anvilPrefab.Bundle = name;

					// Add it to the object dictionary (OD)
					IM.OD.Add(obj.ItemID, obj);

					void AppendKeyAll<TKey>(Dictionary<TKey, List<FVRObject>> dict, Dictionary<TKey, HashSet<FVRObject>> set, TKey key)
					{
						dict.GetOrInsertWith(key, () => new List<FVRObject>()).Add(obj);
						set.GetOrInsertWith(key, () => new HashSet<FVRObject>()).Add(obj);
					}

					void AppendKeysAll<TKey>(Dictionary<TKey, List<FVRObject>> dict, Dictionary<TKey, HashSet<FVRObject>> set, IEnumerable<TKey> keys)
					{
						foreach (var key in keys)
						{
							AppendKeyAll(dict, set, key);
						}
					}

					// A lot of appending to dictionaries
					AppendKeyAll(self.odicTagAttachmentFeature, self.ohasgTagAttachmentFeature, obj.TagAttachmentFeature);
					AppendKeyAll(self.odicTagAttachmentMount, self.ohasgTagAttachmentMount, obj.TagAttachmentMount);
					AppendKeyAll(self.odicTagCategory, self.ohashTagCategory, obj.Category);
					AppendKeyAll(self.odicTagFirearmAction, self.ohashTagFirearmAction, obj.TagFirearmAction);
					AppendKeyAll(self.odicTagFirearmEra, self.ohashTagFirearmEra, obj.TagEra);
					AppendKeysAll(self.odicTagFirearmFeedOption, self.ohashTagFirearmFeedOption, obj.TagFirearmFeedOption);
					AppendKeysAll(self.odicTagFirearmFiringMode, self.ohashTagFirearmFiringMode, obj.TagFirearmFiringModes);
					AppendKeysAll(self.odicTagFirearmMount, self.ohashTagFirearmMount, obj.TagFirearmMounts);
					AppendKeyAll(self.odicTagFirearmSize, self.ohashTagFirearmSize, obj.TagFirearmSize);
				}

				// Inject spawners
				foreach (var id in bundle.IDs)
				{
					var guid = id.ItemID;

					if (guidToId.ContainsKey(guid))
					{
						Logger.LogError($"ItemID {guid} from {name} is already in use. It must be unique.");
						continue;
					}

					// Category dictionary (CD)
					IM.CD.GetOrInsertWith(id.Category, () => new List<ItemSpawnerID>()).Add(id);
					// Subcategory dictionary (SCD)
					IM.SCD.GetOrInsertWith(id.SubCategory, () => new List<ItemSpawnerID>()).Add(id);

					guidToId.Add(guid, id);
				}
			}
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
