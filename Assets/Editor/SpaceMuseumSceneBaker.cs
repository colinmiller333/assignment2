using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

static class SpaceMuseumSceneBaker
{
    const string ScenePath = "Assets/Scenes/Museum.unity";

    [MenuItem("Tools/Space Museum/Rebuild Exhibit Displays")]
    static void RebuildActiveMuseum()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.path != ScenePath) return;

        SpaceMuseumVisuals builder = FindBuilder(currentScene);
        if (builder == null)
        {
            Debug.LogError("Museum scene is missing SpaceMuseumVisuals on its Room object.");
            return;
        }

        builder.BuildDisplays();
        if (EditorSceneManager.SaveScene(currentScene))
            Debug.Log("Generated and saved the six space exhibit displays in Museum.unity.");
    }

    static SpaceMuseumVisuals FindBuilder(Scene scene)
    {
        foreach (GameObject sceneRoot in scene.GetRootGameObjects())
        {
            SpaceMuseumVisuals builder = sceneRoot.GetComponentInChildren<SpaceMuseumVisuals>(true);
            if (builder != null) return builder;
        }
        return null;
    }
}