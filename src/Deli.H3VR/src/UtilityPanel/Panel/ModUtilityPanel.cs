using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityTemplateProjects.ModUtilityPanel.Widget;

namespace UnityTemplateProjects.ModUtilityPanel
{
    public class ModUtilityPanel : MonoBehaviour
    {
        [Header("References")] public MenuWidget MainMenu;
        public GameObject Menus;
        public Button BackButton;

        [Header("Prefabs")] public GameObject ButtonPrefab;

        private string _activeMenu = null;
        private Dictionary<string, GameObject> _menus = new Dictionary<string, GameObject>();
        private Stack<string> _menuStack = new Stack<string>();

        public GameObject ActiveMenu => _menus[_activeMenu];

        private void Awake()
        {
            _menus.Add(MainMenu.Tag, MainMenu.gameObject);
            _activeMenu = MainMenu.Tag;
            BackButton.interactable = false;

            BackButton.onClick.AddListener(() => { Navigate(null); });
        }

        public void CreateTopLevelMenu<T>(Action<T> configuration) where T : MenuWidgetBuilder, new()
        {
            // Let the configuration action run
            var menuConfiguration = new T() {Panel = this};
            configuration(menuConfiguration);

            // Create the button and add it to the main menu
            var button = Instantiate(ButtonPrefab, MainMenu.transform).GetComponent<ButtonWidget>();
            button.TextComponent.text = menuConfiguration.Name;
            button.ButtonComponent.onClick.AddListener(() => { Navigate(menuConfiguration.Tag); });

            // Create the new game object and pass it into the build method of the configuration
            var built = menuConfiguration.Build(Menus.transform);
            _menus.Add(menuConfiguration.Tag, built);
        }

        public void Navigate(string menuTag)
        {
            // If it is not null, try switching to the new tag
            if (menuTag != null)
            {
                // Make sure the tag exists
                if (!_menus.ContainsKey(menuTag))
                    throw new KeyNotFoundException($"The requested utility panel menu '{menuTag}' does not exist.");

                // Update the stack and enable the back button
                _menuStack.Push(_activeMenu);
                BackButton.interactable = true;

                // Switch the currently enabled menu
                ActiveMenu.SetActive(false);
                _activeMenu = menuTag;
                ActiveMenu.SetActive(true);
            }
            // Else if we want to go back and there's something in the stack
            else if (_menuStack.Count > 0)
            {
                // Switch the currently enabled menu
                ActiveMenu.SetActive(false);
                _activeMenu = _menuStack.Pop();
                ActiveMenu.SetActive(true);

                // If there's anything left in the stack, keep the button enabled
                BackButton.interactable = _menuStack.Count > 0;
            }
        }

        internal GameObject InstantiateInternal(GameObject source, Transform parent)
        {
            return Instantiate(source, parent);
        }
    }
}
