// AlwaysTooLate.PlayMode (c) 2018-2020 Always Too Late.

using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AlwaysTooLate.PlayMode
{
    [InitializeOnLoadAttribute]
    internal static class PlayMode
    {
        private const string PlayModeMenu = "Always Too Late/PlayMode/Enter PlayMode %q";
        private const string OverwritePlayMode = "Always Too Late/PlayMode/Overwrite default playmode";
        
        private const string OverwritePlayModeSetting = "ATLPlayMode.EnablePlayModeOverwrite";

        private static bool _playmodeEnter = false;
        
        static PlayMode()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
            EditorSceneManager.sceneOpened += OnSceneOpened;

            if (EditorPrefs.GetBool(OverwritePlayModeSetting))
            {
                Menu.SetChecked(OverwritePlayMode, true);
                SetupDefaultScene();
            }
            else
            {
                Menu.SetChecked(OverwritePlayMode, false);
            }
        }
        
        [MenuItem(PlayModeMenu, false, 10)]
        public static void EnterPlayMode()
        {
            _playmodeEnter = true;
            
            // Setup the game scene
            SetupGameScene();
            
            // Force-enter the playmode.
            EditorApplication.EnterPlaymode();
        }

        [MenuItem(OverwritePlayMode, false, 11)]
        public static void ToggleOverwriteDefaultPlayMode()
        {
            var enabled = EditorPrefs.GetBool(OverwritePlayModeSetting);

            // Toggle the state
            enabled = !enabled;
            
            // Set pref and menu item check
            EditorPrefs.SetBool(OverwritePlayModeSetting, enabled);
            Menu.SetChecked(OverwritePlayMode, enabled);

            if (enabled)
            {
                // Overwrite the default scene
                SetupDefaultScene();
            }
            else
            {
                // Restore the currently opened scene
                RestoreOpenScene();
            }
        }

        private static void OnPlayModeStateChange(PlayModeStateChange state)
        {
            // If playmode has been overwritten, do not restore the last scene
            if (EditorPrefs.GetBool(OverwritePlayModeSetting)) return;
            
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                // Restore the default scene (if this playmode has not been triggered using shortcut)
                
                if (_playmodeEnter)
                {
                    _playmodeEnter = false;
                    return;
                }
                
                // Yep, restore.
                RestoreOpenScene();
            }
        }

        private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            if (EditorPrefs.GetBool(OverwritePlayModeSetting)) return;
            
            // Make sure that we always set the default playmode scene on scene open while playmode overwrite is enabled.
            var sceneAsset = GetSceneByIndex(scene.buildIndex);
            SetCurrentScene(sceneAsset);
        }

        private static void SetupDefaultScene()
        {
            if (EditorBuildSettings.scenes.Length == 0) return;
            
            var sceneAsset = GetSceneByIndex(0);
            SetCurrentScene(sceneAsset);
        }

        private static void SetupGameScene()
        {
            Assert.IsTrue(EditorBuildSettings.scenes.Length > 0,
                "You have no specified scenes in BuildSettings! Cannot enter playmode.");
            
            var sceneAsset = GetSceneByIndex(0);
            SetCurrentScene(sceneAsset);
        }

        private static void RestoreOpenScene()
        {
            // Restore playmode to the currently open scene
            var sceneAsset = GetSceneByIndex(SceneManager.GetActiveScene().buildIndex);
            SetCurrentScene(sceneAsset);
        }

        private static void SetCurrentScene(SceneAsset sceneAsset)
        {
            EditorSceneManager.playModeStartScene = sceneAsset;
        }

        private static SceneAsset GetSceneByIndex(int index)
        {
            var pathOfFirstScene = EditorBuildSettings.scenes[index].path;
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathOfFirstScene);

            return sceneAsset;
        }
    }
}