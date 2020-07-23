#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

using static RakNet_Enums;

public class BaseNetworkClientDebugEditorWindow : EditorWindow
{
    [MenuItem("RakNet/Client Debug")]
    static void Init()
    {
        BaseNetworkClientDebugEditorWindow window = (BaseNetworkClientDebugEditorWindow)GetWindow(typeof(BaseNetworkClientDebugEditorWindow), true);
        window.titleContent = new GUIContent("Client Debugger");
        window.Show();
    }

    BaseNetworkClient _baseclient;

    private void Update()
    {
        Repaint();
    }


    void OnGUI()
    {
        if (EditorApplication.isPlaying)
        {
            if (Selection.activeGameObject && Selection.activeGameObject.TryGetComponent(out BaseNetworkClient t))
            {
                _baseclient = t;
            }

            if (_baseclient)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical("box");
                GUILayout.Label("Client information", EditorStyles.boldLabel);
                GUILayout.Label("Address " + _baseclient.Address, EditorStyles.boldLabel);
                GUILayout.Label("Port " + _baseclient.Port, EditorStyles.boldLabel);
                GUILayout.Space(15);
                GUILayout.Label("Ping (last) " + _baseclient.GetPingLast(), EditorStyles.boldLabel);
                GUILayout.Label("Ping (average) " + _baseclient.GetPingAverage(), EditorStyles.boldLabel);
                GUILayout.Label("Ping (lowest) " + _baseclient.GetPingLowest(), EditorStyles.boldLabel);
                GUILayout.Label("\nMinimal network stats");

                float b_sent = _baseclient.GetNetStat(RNSPerSecondMetrics.ACTUAL_BYTES_SENT);
                float b_rcv = _baseclient.GetNetStat(RNSPerSecondMetrics.ACTUAL_BYTES_RECEIVED);

                if (b_sent < 1024)
                {
                    GUILayout.Label("Sended " + b_sent.ToString("f0") + " bytes", EditorStyles.boldLabel);
                }

                if (b_sent > 1024 && b_sent < 1048576)
                {
                    GUILayout.Label("Sended " + (b_sent / 1024).ToString("f1") + " kb", EditorStyles.boldLabel);
                }
                else if(b_sent >= 1048576)
                {
                    GUILayout.Label("Sended " + ((b_sent / 1024) / 1024).ToString("f2") + " mb", EditorStyles.boldLabel);
                }

                if (b_rcv < 1024)
                {
                    GUILayout.Label("Received " + b_rcv.ToString("f0") + " bytes", EditorStyles.boldLabel);
                }

                if (b_rcv > 1024 && b_rcv < 1048576)
                {
                    GUILayout.Label("Received " + (b_rcv / 1024).ToString("f1") + " kb", EditorStyles.boldLabel);
                }
                else if (b_rcv >= 1048576)
                {
                    GUILayout.Label("Received " + ((b_rcv / 1024) / 1024).ToString("f2") + " mb", EditorStyles.boldLabel);
                }
                GUILayout.Label("Loss: " + _baseclient.GetLoss() + "%", EditorStyles.boldLabel);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                GUILayout.Label("Full Network Stats\n" + _baseclient.GetNetworkStats());
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Pick NetworkClient component for get debug information...", MessageType.Error);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("To get client debug information, you must run the game", MessageType.Error);
        }
    }
}
#endif
