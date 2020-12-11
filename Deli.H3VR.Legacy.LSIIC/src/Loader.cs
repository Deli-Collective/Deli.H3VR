using System;
using System.Collections.Generic;
using System.IO;
using ADepIn;
using Anvil;
using BepInEx.Logging;
using FistVR;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Deli.H3VR.Legacy.LSIIC
{
	internal class VirtualObjectAssetLoader : IAssetLoader
	{
		private readonly ManualLogSource _logger;
		private readonly Dictionary<string, AssetBundle> _bundles;

		public VirtualObjectAssetLoader(ManualLogSource logger)
		{
			_logger = logger;
			_bundles = new Dictionary<string, AssetBundle>();

			On.FistVR.IM.GenerateItemDBs += InjectObjects;
			IL.AnvilManager.GetAssetBundleAsyncInternal += RedirectBundles;
		}

		public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
		{
			if (Path.GetExtension(path) == "manifest")
			{
				// We don't care for these files, but the glob will match these files anyway.
				return;
			}

			var bundle = mod.Resources.Get<AssetBundle>(path).Expect("Asset bundle not found at " + path);
			var id = mod.Info.Guid + ":" + path;

			_bundles.Add(id, bundle);
		}

		private void InjectObjects(On.FistVR.IM.orig_GenerateItemDBs orig, IM self)
		{
			orig(self);

			var guidToId = self.SpawnerIDDic;

			foreach (var bundlePair in _bundles)
			{
				var name = bundlePair.Key;
				var bundle = bundlePair.Value;

				// Inject objects
				foreach (var obj in bundle.LoadAllAssets<FVRObject>())
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
				foreach (var id in bundle.LoadAllAssets<ItemSpawnerID>())
				{
					var guid = id.ItemID;

					if (guidToId.ContainsKey(guid))
					{
						_logger.LogError($"ItemID {guid} from {name} is already in use. It must be unique.");
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

		private void RedirectBundles(ILContext il)
		{
			var c = new ILCursor(il);

			// Move right before the dummy operation
			c.GotoNext(i => i.MatchCall(typeof(File), nameof(File.Exists)), i => i.MatchBrtrue(out _))
				.GotoNext(i => i.MatchLdnull(), i => i.MatchNewobj<AnvilDummyOperation>());

			c.Remove(); // Remove ldnull
			c.Emit(OpCodes.Ldarg_0); // bundle
			c.EmitDelegate<Func<string, Object>>(bundle => _bundles.TryGetValue(bundle, out var cached) ? cached : null);
		}
	}
}
