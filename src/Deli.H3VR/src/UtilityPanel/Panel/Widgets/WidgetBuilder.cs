using ModUtilityPanel;
using UnityEngine;

namespace UnityTemplateProjects.ModUtilityPanel
{
    public abstract class WidgetBuilder<T> : IBuildableWidget where T : UtilityWidget
    {
        public ModUtilityPanel Panel { get; set; }
        
        public string Name { get; set; }

        protected virtual T Build(T widget)
        {
            return widget;
        }
        

        protected GameObject Instantiate(GameObject source, Transform parent)
        {
            return Panel.InstantiateInternal(source, parent);
        }

        public virtual GameObject Build(Transform parent)
        {
            // Create the game object for this menu
            var go = new GameObject(Name);
            go.SetActive(false);
            go.transform.parent = parent;
            
            // Make the widget component
            var widget = go.AddComponent<T>();
            
            // Let the derived class build the rest and return the built object
            Build(widget);
            return go;
        }
    }
}