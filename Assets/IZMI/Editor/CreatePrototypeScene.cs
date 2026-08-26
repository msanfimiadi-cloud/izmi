#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Izmi.Editor
{
    public static class CreatePrototypeScene
    {
        [MenuItem("IZMI/Create Prototype Scene")]
        public static void Create()
        {
            var scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            EditorSceneManager.SaveScene(
                scene,
                "Assets/IZMI/Scenes/GlobePrototype.unity");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(
                    "Assets/IZMI/Scenes/GlobePrototype.unity",
                    true)
            };
        }
    }
}
#endif
