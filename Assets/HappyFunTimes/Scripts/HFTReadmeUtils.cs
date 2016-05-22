using UnityEngine;
using System;
using System.Collections;

namespace HappyFunTimes {

    public class HFTReadmeUtils {

        const string kLastScene = "happyFunTimesReadmeSceneHash";

        #if UNITY_EDITOR
        static public void CloseReadme(Component component)
        {
            System.Type t = GetWindowType();
            if (t != null)
            {
                UnityEditor.EditorWindow w = t.GetMethod("GetInstanceIfExists").Invoke(null, null) as UnityEditor.EditorWindow;
                if (w != null)
                {
                    t.GetMethod("CloseIfOurs").Invoke(w, new object[]{ component });
                }
            }
        }

        static System.Type GetWindowType() {
            string[] names = new string[]
            {
                "HappyFunTimesEditor.HFTReadmeWindow",
                "HappyFunTimesEditor.HFTReadmeWindow, Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                "HappyFunTimesEditor.HFTReadmeWindow, Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                "HFTReadmeWindow",
            };
            foreach (string name in names)
            {
                System.Type t = System.Type.GetType(name);
                if (t != null)
                {
                    return t;
                }
            }
            return null;
        }

        static string GetSceneHash()
        {
            // NOTE: I tried getting the list of loaded scenes but it's 0
            // when I get here. But active scene seems to be valid
            string sceneHash = "";
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (!String.IsNullOrEmpty(scene.path))
            {
                sceneHash = scene.path;
            }
            else if (!String.IsNullOrEmpty(scene.name))
            {
                sceneHash = scene.name;
            }
            return sceneHash;
        }

        // This is an attempt to not make the README too annoying.
        // Without this, every time the scene loads (which means every time
        // you press stop after runnign the game) the readme would re-appear
        // It's in the way so you close it only to have it show up again
        //
        // With this it will only open if the user loads a new scene. Not sure
        // that's the best but I want the readme to appear.
        static bool IsSameSceneAsLastTime()
        {
            string sceneHash = GetSceneHash();
            string oldSceneHash = PlayerPrefs.GetString(kLastScene, "");
            if (sceneHash.Equals(oldSceneHash))
            {
                return true;
            }

            return false;
        }

        static void RememberScene()
        {
            PlayerPrefs.SetString(kLastScene, GetSceneHash());
        }

        static bool ShouldShowReadme()
        {
            if (Application.isPlaying)
            {
                return false;
            }

            bool isSameScene = IsSameSceneAsLastTime();
            if (isSameScene)
            {
                return false;
            }
            RememberScene();

            return true;
        }

        static public void ShowReadme(string name, string text, bool richText, Component component)
        {
            if (!ShouldShowReadme())
            {
                return;
            }

            System.Type t = GetWindowType();
            if (t != null)
            {
                UnityEditor.EditorWindow w = t.GetMethod("GetInstance").Invoke(null, null) as UnityEditor.EditorWindow;
                if (w != null)
                {
                    t.GetMethod("SetContent").Invoke(w, new object[]{ name, text, richText, component });
                    w.ShowUtility();
                }
            }
        }
        #endif
    }
}


