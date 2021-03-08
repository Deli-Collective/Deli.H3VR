using System;
using System.Collections.Generic;
using ModUtilityPanel;
using UnityEngine;
using UnityTemplateProjects.ModUtilityPanel.Widget;

namespace UnityTemplateProjects.ModUtilityPanel
{
    public abstract class MenuWidgetBuilder : WidgetBuilder<MenuWidget>
    {
        public string Tag { get; set; }

        protected readonly List<IBuildableWidget> widgets = new List<IBuildableWidget>();

        public void CreateButton(Action<ButtonWidgetBuilder> configuration)
        {
            var builder = new ButtonWidgetBuilder{ Panel = Panel};
            configuration(builder);
            
            widgets.Add(builder);
        }

        protected override MenuWidget Build(MenuWidget widget)
        {
            // Set the properties
            widget.Name = Name;
            widget.Tag = Tag;
            
            // Set the anchor on the transform
            var t = widget.GetComponent<RectTransform>();
            t.anchorMin = Vector2.zero;
            t.anchorMax = Vector2.one;
            t.offsetMin = Vector2.zero;
            t.offsetMax = Vector2.zero;

            // Return the menu widget
            return widget;
        }
    }
}