using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Deli.Runtime;
using Deli.Setup;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using Object = System.Object;

namespace Deli.H3VR.AnvilAssetBundleInjector
{
	public class AnvilRedirector : DeliBehaviour
	{
		private delegate bool TryGet<in TKey, TValue>(TKey key, [MaybeNullWhen(false)] out TValue value);

		private static AnvilRedirector? _instance;
		// ReSharper disable once Unity.NoNullCoalescing
		public static AnvilRedirector Instance => _instance ?? throw new InvalidOperationException("Attempted to access the registrar before it was initialized.");

		private readonly GlobalRegistrar _global = new();
		private readonly Dictionary<Mod, ModRegistrar> _registrars = new();

		private AnvilRedirector()
		{
			_instance = this;

			IL.AnvilManager.GetAssetBundleAsyncInternal += RedirectBundles;
		}

		public ModRegistrar this[Mod mod]
		{
			get
			{
				if (!_registrars.TryGetValue(mod, out var registrar))
				{
					registrar = new ModRegistrar(mod, _global);
					_registrars.Add(mod, registrar);
				}

				return registrar;
			}
		}

		private void RedirectBundles(ILContext il)
		{
			var c = new ILCursor(il);

			// arguments:
			// 0: bundle path (string)

			// Get the label after result is determined
			c.GotoNext(
				MoveType.Before,
				i => i.MatchLdloc(1),
				i => i.MatchLdnull(),
				i => i.MatchNewobj<AnvilCallback<AssetBundle>>()
			);

			var gotoIfTrue = c.MarkLabel();

			// Move after the anvil cache
			c.GotoPrev(
				MoveType.AfterLabel,
				i => i.MatchCall(typeof(Application), nameof(Application.streamingAssetsPath)),
				i => i.MatchLdloc(0),
				i => i.MatchCall(typeof(Path), nameof(Path.Combine))
			);

			// Insert our own cache
			// if (!_global.TryGet(bundle, out result) { ... }
			c.Emit(OpCodes.Ldarg_0)
				.Emit(OpCodes.Ldloca, 1)
				.EmitDelegate<TryGet<string, AsyncOperation>>(_global.TryGet);
			c.Emit(OpCodes.Brtrue, gotoIfTrue);
		}
	}
}
