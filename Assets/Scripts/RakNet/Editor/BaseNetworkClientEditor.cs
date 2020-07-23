#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

using static RakNet_Enums;
[CustomEditor(typeof(BaseNetworkClient), true)]
[CanEditMultipleObjects]
public class BaseNetworkClientEditor : Editor
{
    protected BaseNetworkClient _baseclient;

    protected void Init()
    {
        if (_baseclient == null)
        {
            _baseclient = target as BaseNetworkClient;
        }
    }

    private bool default_inspector = false;


    public override void OnInspectorGUI()
    {
        Init();
        if (EditorApplication.isPlaying)
        {
            if (_baseclient.IsConnected)
            {
                EditorGUILayout.HelpBox("Connected to " + _baseclient.Address + ":" + _baseclient.Port, MessageType.Info);
            }
            else if(!_baseclient.IsConnecting)
            {
                EditorGUILayout.HelpBox("Client disconnected", MessageType.Error);
            }
            if (_baseclient.IsConnecting)
            {
                EditorGUILayout.HelpBox("Connecting to " + _baseclient.Address + ":" + _baseclient.Port, MessageType.Warning);

            }
            if (!_baseclient.IsConnected && !_baseclient.IsConnecting)
            {
                if (GUILayout.Button("Connect to 127.0.0.1"))
                {
                    _baseclient.Connect(port:27015);
                }
            }
            else
            {
                if (GUILayout.Button(_baseclient.IsConnecting ? "Cancel" : "Disconnect"))
                {
                    _baseclient.Disconnect(DisconnectionType.ByUser);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("To use the client, you must run the game", MessageType.Warning);
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