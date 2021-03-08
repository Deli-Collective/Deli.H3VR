using UnityEngine;

namespace ModUtilityPanel
{
    public interface IBuildableWidget
    {
        GameObject Build(Transform parent);
    }
}