using System;
using UnityEngine;

namespace UnityTemplateProjects.ModUtilityPanel.Widget
{
    public class ButtonWidgetBuilder : WidgetBuilder<ButtonWidget>
    {
        public string Text { get; set; }

        public Action<ModUtilityPanel> OnClick { get; set; }

        public override GameObject Build(Transform parent)
        {
            // Instantiate the button prefab
            var button = Instantiate(Panel.ButtonPrefab, parent).GetComponent<ButtonWidget>();

            // Set it's settings
            button.TextComponent.text = Text;
            button.ButtonComponent.onClick.AddListener(() => OnClick(Panel));

            // Return the button widget
            return button.gameObject;
        }
    }
}