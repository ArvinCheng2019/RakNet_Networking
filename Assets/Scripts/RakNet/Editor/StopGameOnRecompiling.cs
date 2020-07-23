using UnityEditor;
using UnityEngine;
//Subscribe!   My Youtube channel https://www.youtube.com/channel/UCPQ04Xpbbw2uGc1gsZtO3HQ

//ATTENTION! WRITTEN EVERYTHING FOR MYSELF! IF YOU DO NOT LIKE THE IMPLEMENTATION YOU CAN WRITE YOUR OWN

[InitializeOnLoad]
public class StopGameOnRecompile
{
    static StopGameOnRecompile()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        if (EditorApplication.isCompiling)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[Editor] Stopping game! Compiling code...");
                EditorApplication.isPlaying = false;
            }
        }
    }
}
