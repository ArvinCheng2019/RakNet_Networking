#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class BaseNetworkServerDebugEditorWindow : EditorWindow
{
    [MenuItem("RakNet/Server Debug")]
    static void Init()
    {
        BaseNetworkServerDebugEditorWindow window = (BaseNetworkServerDebugEditorWindow)GetWindow(typeof(BaseNetworkServerDebugEditorWindow),true);
        window.titleContent = new GUIContent("Server Debugger");
        window.Show();
    }

    BaseNetworkServer _baseserver;
    Vector2 connections_scroll = Vector2.zero;

    private void Update()
    {
        Repaint();
    }

    void OnGUI()
    {
        if (EditorApplication.isPlaying)
        {
            if(Selection.activeGameObject && Selection.activeGameObject.TryGetComponent(out BaseNetworkServer t))
            {
                _baseserver = t;
            }

            if (_baseserver)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical("box");
                GUILayout.Label("Server information", EditorStyles.boldLabel);
                GUILayout.Label("Bind address " + _baseserver.Address, EditorStyles.boldLabel);
                GUILayout.Label("Port " + _baseserver.Port, EditorStyles.boldLabel);
                GUILayout.Label("Connections " + _baseserver.connections.Count, EditorStyles.boldLabel);
                GUILayout.EndVertical();

                GUILayout.Space(30);

                GUILayout.BeginVertical();

                GUILayout.Label("Connections", EditorStyles.boldLabel);
                connections_scroll = GUILayout.BeginScrollView(connections_scroll);
                for(int i = 0; i < _baseserver.connections.Count; i++)
                {
                    GUILayout.BeginVertical("box", GUILayout.Width(256));
                    GUILayout.Box("Guid: "+_baseserver.connections[i]);
                    GUILayout.Box("IP: "+_baseserver.server_peer.GetAddress(_baseserver.connections[i].guid));
                    GUILayout.Box("Ping: (last="+_baseserver.server_peer.GetPingLast(_baseserver.connections[i].guid) +") (avg="+ _baseserver.server_peer.GetPingAverage(_baseserver.connections[i].guid) + ") (low="+ _baseserver.server_peer.GetPingLowest(_baseserver.connections[i].guid) + ")");
                    GUILayout.BeginHorizontal();
                    if(GUILayout.Button("Kick"))
                    {
                        _baseserver.Kick(_baseserver.connections[i].guid, "Kicked by editor!");
                    }
                    if (GUILayout.Button("Get stats"))
                    {
                        EditorUtility.DisplayDialog("Network statistics for " + _baseserver.server_peer.GetAddress(_baseserver.connections[i].guid),
                            _baseserver.GetNetworkStats(_baseserver.connections[i].guid),
                            "Close");
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Pick NetworkServer component for get debug information...", MessageType.Error);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("To get server debug information, you must run the game", MessageType.Error);
        }
    }
}
#endif