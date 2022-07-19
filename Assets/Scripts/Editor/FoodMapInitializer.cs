using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class FoodMapInitializer : EditorWindow
    {
        private RenderTexture _texture;

        [MenuItem("Window/Food Map Initializer")]
        private static void ShowWindow()
        {
            var window = GetWindow<FoodMapInitializer>();
            window.titleContent = new GUIContent("Food Map");
            window.Show();
        }

        private void OnGUI()
        {
        }
    }
}












