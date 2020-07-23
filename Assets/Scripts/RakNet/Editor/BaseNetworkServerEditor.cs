#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(BaseNetworkServer), true)]
[CanEditMultipleObjects]
public class BaseNetworkServerEditor : Editor
{
    protected BaseNetworkServer _baseserver;

    protected void Init()
    {
        if (_baseserver == null)
        {
            _baseserver = target as BaseNetworkServer;
        }
    }

    private bool default_inspector = false;

    public override void OnInspectorGUI()
    {
        Init();
        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox(_baseserver.IsStarted ? "Server is started ("+_baseserver.Address+":"+_baseserver.Port+")" : "Server is offline", _baseserver.IsStarted ? MessageType.Info : MessageType.Warning);

            if (!_baseserver.IsStarted)
            {
                if (GUILayout.Button("Start server on port 27015"))
                {
                    _baseserver.StartServer(port: 27015);
                }
            }
            else
            {
                if (GUILayout.Button("Stop server"))
                {
                    _baseserver.StopServer();
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("To use the server, you must run the game", MessageType.Warning);
        }

        if (!default_inspector)
        {
            if (GUILayout.Button("Draw default inspector"))
            {
                default_inspector = true;
            }
        }
        else
        {
            if (GUILayout.Button("Hide default inspector"))
            {
                default_inspector = false;
            }
            DrawDefaultInspector();
        }
    }
}
#endif