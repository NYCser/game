#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HollowManor.EditorTools
{
    public static class SceneTools
    {
        [MenuItem("Death Forest/Create Empty Play Scene")]
        public static void CreateEmptyPlayScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            const string scenePath = "Assets/Scenes/DeathForest_Play.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Death Forest",
                "Da tao scene rong cho Death Forest. Mo scene nay roi bam Play de runtime bootstrap tu sinh map.",
                "OK");
        }
    }
}
#endif
