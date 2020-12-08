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

namespace Deli.H3VR
{
	[QuickNamedBind("deli.h3vr.fvrobject")]
	internal class ObjectAssetLoader : IAssetLoader
	{
		private readonly ManualLogSource _logger;
		private readonly Dictionary<string, AssetBundle> _bundles;

		private readonly AccessTools.FieldRef<FVRObject, AssetID> _anvilPrefabOf;
		private readonly AccessTools.FieldRef<IM, Dictionary<string, ItemSpawnerID>> _idsOf;

		public ObjectAssetLoader(ManualLogSource logger)
		{
			_logger = logger;
			_bundles = new Dictionary<string, AssetBundle>();

			_anvilPrefabOf = AccessTools.FieldRefAccess<FVRObject, AssetID>("m_anvilPrefab");
			_idsOf = AccessTools.FieldRefAccess<IM, Dictionary<string, ItemSpawnerID>>("SpawnerIDDic");

			On.FistVR.IM.GenerateItemDBs += InjectObjects;
			IL.AnvilManager.GetAssetBundleAsyncInternal += RedirectBundles;
		}

		private void InjectObjects(On.FistVR.IM.orig_GenerateItemDBs orig, IM self)
		{
			orig(self);

			var guidToId = _idsOf(self);

			foreach (var bundlePair in _bundles)
			{
				var name = bundlePair.Key;
				var bundle = bundlePair.Value;

				foreach (var obj in bundle.LoadAllAssets<FVRObject>())
				{
					var original = _anvilPrefabOf(obj);
					original.Bundle = name;

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

				foreach (var id in bundle.LoadAllAssets<ItemSpawnerID>())
				{
					var guid = id.ItemID;

					if (guidToId.ContainsKey(guid))
					{
						_logger.LogError($"ItemID {guid} from {name} is already in use. It must be unique.");
						continue;
					}

					IM.CD[id.Category].Add(id);
					IM.SCD[id.SubCategory].Add(id);
					guidToId.Add(guid, id);
				}
			}
		}

		private void RedirectBundles(ILContext il)
		{
			var c = new ILCursor(il);

			// Reflective
			var existsMethodReflective = AccessTools.Method(typeof(File), nameof(File.Exists));
			var tryGetReflective = AccessTools.Method(typeof(Dictionary<string, AssetBundle>), nameof(Dictionary<string, AssetBundle>.TryGetValue));

			// Imported reflectives
			var existsMethod = il.Module.ImportReference(existsMethodReflective);
			var tryGetMethod = il.Module.ImportReference(tryGetReflective);

			// Types
			var bundleType = il.Module.ImportReference(typeof(AssetBundle));
			var anvilDummyOperationType = il.Module.ImportReference(typeof(AnvilDummyOperation));

			// Variables
			var outVar = new VariableDefinition(bundleType);
			il.Body.Variables.Add(outVar);

			// Move into if (!File.Exists)
			c.GotoNext(MoveType.After,
				i => i.OpCode == OpCodes.Call && i.Operand == existsMethod,
				il => il.OpCode == OpCodes.Brtrue);

			// Extract the ret-like local and ret-like label
			var fileNotFound = c.MarkLabel();
			Option<int> retLocOpt = default;
			Option<ILLabel> retJmpOpt = default;
			c.GotoNext(i =>
			{
				var result = i.MatchStloc(out int retLoc);
				retLocOpt = Option.Some(retLoc);
				return result;
			}, i =>
			{
				var result = i.MatchBr(out var retJmp);
				retJmpOpt = Option.Some(retJmp);
				return result;
			});
			var retVar = il.Body.Variables[retLocOpt.Unwrap()];
			var retJmp = retJmpOpt.Unwrap();
			c.GotoLabel(fileNotFound);

			//	if (_bundles.TryGetValue(bundle, out var cached))
			//	{ ... }
			//	else...
			c.EmitReference(_bundles);
			c.Emit(OpCodes.Ldarg_0); // bundle
			c.Emit(OpCodes.Ldloca, outVar); // out var cached
			c.Emit(OpCodes.Callvirt, tryGetMethod); // TryGetValue(...)
			c.Emit(OpCodes.Brfalse, fileNotFound); // else...
			{
				//	request = new AnvilDummyOperation(cached.Assets);
				c.Emit(OpCodes.Ldloc, outVar); // cached
				c.Emit(OpCodes.Newobj, anvilDummyOperationType); // new AnvilDummyOperation(...)
				c.Emit(OpCodes.Stloc, retVar); // request = ...
			}
			c.Emit(OpCodes.Br, retJmp);
		}

		public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
		{
			var bundle = mod.Resources.Get<AssetBundle>(path).Expect("Asset bundle not found at " + path);
			var id = mod.Info.Guid + ":" + path;

			_bundles.Add(id, bundle);
		}
	}
}
