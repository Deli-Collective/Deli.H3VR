using System;
using System.Collections.Generic;
using BepInEx.Logging;
using FistVR;

namespace Deli.H3VR.Legacy.LSIIC
{
	public delegate void BundleLoader(VirtualObjectAssetBundle bundle);

	internal static class BundleLoaders
	{
		private static void AppendObjects(ManualLogSource logger, IM im, VirtualObjectAssetBundle bundle)
		{
			logger.LogDebug($"Loading VirtualObjects from {bundle}");

			var guidToId = im.SpawnerIDDic;

			// Inject objects
			foreach (var obj in bundle.Objects)
			{
				// Redirect the bundle of the object
				obj.m_anvilPrefab.Bundle = bundle.Key;

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
				AppendKeyAll(im.odicTagAttachmentFeature, im.ohasgTagAttachmentFeature, obj.TagAttachmentFeature);
				AppendKeyAll(im.odicTagAttachmentMount, im.ohasgTagAttachmentMount, obj.TagAttachmentMount);
				AppendKeyAll(im.odicTagCategory, im.ohashTagCategory, obj.Category);
				AppendKeyAll(im.odicTagFirearmAction, im.ohashTagFirearmAction, obj.TagFirearmAction);
				AppendKeyAll(im.odicTagFirearmEra, im.ohashTagFirearmEra, obj.TagEra);
				AppendKeysAll(im.odicTagFirearmFeedOption, im.ohashTagFirearmFeedOption, obj.TagFirearmFeedOption);
				AppendKeysAll(im.odicTagFirearmFiringMode, im.ohashTagFirearmFiringMode, obj.TagFirearmFiringModes);
				AppendKeysAll(im.odicTagFirearmMount, im.ohashTagFirearmMount, obj.TagFirearmMounts);
				AppendKeyAll(im.odicTagFirearmSize, im.ohashTagFirearmSize, obj.TagFirearmSize);
			}

			// Inject spawners
			foreach (var id in bundle.IDs)
			{
				var guid = id.ItemID;

				if (guidToId.ContainsKey(guid))
				{
					logger.LogError($"ItemID {guid} from {bundle} is already in use. It must be unique.");
					continue;
				}

				// Category dictionary (CD)
				IM.CD.GetOrInsertWith(id.Category, () => new List<ItemSpawnerID>()).Add(id);
				// Subcategory dictionary (SCD)
				IM.SCD.GetOrInsertWith(id.SubCategory, () => new List<ItemSpawnerID>()).Add(id);

				guidToId.Add(guid, id);
			}
		}

		public class Deferred
		{
			private readonly ManualLogSource _logger;
			private readonly Queue<VirtualObjectAssetBundle> _bundles = new();

			public Deferred(ManualLogSource logger)
			{
				_logger = logger;
			}

			public void AppendAll(IM im)
			{
				_logger.LogDebug("Appending delayed VirtualObjects");

				foreach (var bundle in _bundles)
				{
					AppendObjects(_logger, im, bundle);
				}

				_logger.LogDebug("VirtualObjects appending complete");
			}

			public void Load(VirtualObjectAssetBundle bundle)
			{
				_bundles.Enqueue(bundle);
			}
		}

		public class Immediate
		{
			private readonly ManualLogSource _logger;
			private readonly IM _im;

			public Immediate(ManualLogSource logger, IM im)
			{
				_logger = logger;
				_im = im;
			}

			public void Load(VirtualObjectAssetBundle bundle)
			{
				AppendObjects(_logger, _im, bundle);
			}
		}
	}
}
