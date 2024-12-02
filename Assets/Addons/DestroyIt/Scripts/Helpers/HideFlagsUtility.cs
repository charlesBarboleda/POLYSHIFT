using UnityEngine;
using UnityEditor;

namespace DestroyIt
{
    public static class HideFlagsUtility
    {

        private static void ShowAll()
        {
            var allGameObjects = Object.FindObjectsOfType<GameObject>();
            foreach (var go in allGameObjects)
            {
                switch (go.hideFlags)
                {
                    case HideFlags.HideAndDontSave:
                        go.hideFlags = HideFlags.DontSave;
                        break;
                    case HideFlags.HideInHierarchy:
                    case HideFlags.HideInInspector:
                        go.hideFlags = HideFlags.None;
                        break;
                }
            }
        }
    }
}