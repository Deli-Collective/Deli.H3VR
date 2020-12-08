namespace Deli.H3VR.Legacy.LSIIC
{
	internal class Behaviour : DeliBehaviour
	{
		public Behaviour()
		{
			Deli.AddAssetLoader("lsiic.virtualobject", new VirtualObjectAssetLoader(Logger));
		}
	}
}
