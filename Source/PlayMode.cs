// AlwaysTooLate.PlayMode (c) 2018-2020 Always Too Late.

using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AlwaysTooLate.PlayMode
{
    [InitializeOnLoadAttribute]
    internal static class PlayMode
    {
        private const string PlayModeMenu = "Tools/Always Too Late/PlayMode/Enter PlayMode %q";
        private const string OverwritePlayMode = "Tools/Always Too Late/PlayMode/Overwrite default playmode";
        
        private const string OverwritePlayModeSetting = "ATLPlayMode.EnablePlayModeOverwrite";

        private static bool _isEnteringPlaymode = false;
        
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
        
        /// <summary>
        ///     Enter playmode menu item.
        /// </summary>
        [MenuItem(PlayModeMenu, false, 10)]
        public static void EnterPlayMode()
        {
            _isEnteringPlaymode = true;
            
            // Setup the default scene
            SetupDefaultScene();
            
            // Force-enter the playmode.
            EditorApplication.EnterPlaymode();
        }


        /// <summary>
        ///     Owerwrite playmode menu item with toggle.
        /// </summary>
        [MenuItem(OverwritePlayMode, false, 11)]
        public static void ToggleOverwriteDefaultPlayMode()
        {
            var isDefaultPlaymodeEnabled = EditorPrefs.GetBool(OverwritePlayModeSetting);

            // Toggle the state
            isDefaultPlaymodeEnabled = !isDefaultPlaymodeEnabled;
            
            // Set pref and menu item check
            EditorPrefs.SetBool(OverwritePlayModeSetting, isDefaultPlaymodeEnabled);
            Menu.SetChecked(OverwritePlayMode, isDefaultPlaymodeEnabled);

            if (isDefaultPlaymodeEnabled)
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
                
                if (_isEnteringPlaymode)
                {
                    _isEnteringPlaymode = false;
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
            SetPlaymodeScene(sceneAsset);
        }

        private static void SetupDefaultScene()
        {
            if (EditorBuildSettings.scenes.Length == 0)
            {
                Debug.LogWarning("Missing scenes in BuildSettings! Cannot enter main-scene playmode.");
                return;
            }
            
            var sceneAsset = GetSceneByIndex(0);
            SetPlaymodeScene(sceneAsset);
        }

        private static void RestoreOpenScene()
        {
            // Restore playmode to the currently open scene
            var sceneAsset = GetSceneByIndex(SceneManager.GetActiveScene().buildIndex);
            SetPlaymodeScene(sceneAsset);
        }

        private static void SetPlaymodeScene(SceneAsset sceneAsset)
        {
            EditorSceneManager.playModeStartScene = sceneAsset;
        }

        private static SceneAsset GetSceneByIndex(int index)
        {
            var scenePath = EditorBuildSettings.scenes[index].path;
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        }
    }
}
