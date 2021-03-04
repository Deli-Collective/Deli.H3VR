using System.Collections.Generic;
using Deli.VFS;
using FistVR;

namespace Deli.H3VR.Legacy.LSIIC
{
	public readonly struct VirtualObjectAssetBundle
	{
		public string Key { get; }
		public IEnumerable<FVRObject> Objects { get; }
		public IEnumerable<ItemSpawnerID> IDs { get; }

		public VirtualObjectAssetBundle(string key, IEnumerable<FVRObject> objects, IEnumerable<ItemSpawnerID> ids)
		{
			Key = key;
			Objects = objects;
			IDs = ids;
		}

		public override string ToString()
		{
			return Key;
		}
	}
}
