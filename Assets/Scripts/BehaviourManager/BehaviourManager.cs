using System.Collections.Generic;
using UnityEngine;

public interface ITickUpdate
{
    void TickUpdate(float dt);
}

public interface IFrameUpdate
{
    void FrameUpdate(float dt);
}

public interface IAfterFrameUpdate
{
    void AfterFrameUpdate(float dt);
}

[DefaultExecutionOrder(int.MinValue)]
[DisallowMultipleComponent]
//Behavior Manager is a handy tool I wrote to add a little optimization to update calls.
//See https://blog.theknightsofunity.com/monobehavior-calls-optimization/ for details
//Subscribe!   My Youtube channel https://www.youtube.com/channel/UCPQ04Xpbbw2uGc1gsZtO3HQ

//ATTENTION! WRITTEN EVERYTHING FOR MYSELF! IF YOU DO NOT LIKE THE IMPLEMENTATION YOU CAN WRITE YOUR OWN

public class BehaviourManager : MonoBehaviour
{
    public static bool Initialized { get; private set; } = false;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        if (Initialized)
            return;

        GameObject go = new GameObject("BehaviourManager");
        Instance = go.AddComponent<BehaviourManager>();
        go.hideFlags = HideFlags.NotEditable | HideFlags.DontSave;
        DontDestroyOnLoad(go);
        Initialized = true;
    }

    private void Awake()
    {
        /*     This fragment can be cut
        CommandHandler.RegisterCommand("_interfaces_info", delegate {
            Debug.Log(
                "\nInterfaces performance time\n"+
                "IFrameUpdate = " + frameupdate_behaviours.Count + " ("+IFrameUpdateExecTime+" ms) Calls = "+FrameCalls+"\n" +
                "IAfterFrameUpdate = " + afterframeupdate_behaviours.Count + " (" + IAfterFrameUpdateExecTime + " ms) Calls = " + AfterFrameCalls + "\n" +
                "ITickUpdate = " + tickupdate_behaviours.Count + " (" + ITickUpdateExecTime + " ms) Calls = " + TickCalls+"\nTotal Calls = "+(FrameCalls+AfterFrameCalls+TickCalls)
            ); ; }, "-");

        CommandHandler.RegisterCommand("_behaviour_types", delegate {
            Debug.Log("\nRegistered Behaviour Types");
            for(int i = 0; i < behaviours_names.Count; i++)
            {
                Debug.Log(behaviours_names[i]);
            }
        }, "-");
        */
    }

    public static BehaviourManager Instance;
    public List<string> behaviours_names { get; private set; } = new List<string>();
    public List<IFrameUpdate> frameupdate_behaviours { get; private set; } = new List<IFrameUpdate>();
    public List<IAfterFrameUpdate> afterframeupdate_behaviours { get; private set; } = new List<IAfterFrameUpdate>();
    public List<ITickUpdate> tickupdate_behaviours { get; private set; } = new List<ITickUpdate>();

    public static bool Access()
    {
        if (!Instance)
        {
            Debug.LogError("BehaviourManager is not initialized!");
        }
        return Instance;
    }

    public static void RegisterBehaviour(IFrameUpdate behaviour)
    {
        if (!Access())
        {
            return;
        }
        string type_name = (behaviour as MonoBehaviour).GetType().Name;
        //Add type name
        if (!Instance.behaviours_names.Contains(type_name))
        {
            Instance.behaviours_names.Add(type_name);
        }

        Instance.frameupdate_behaviours.Add(behaviour);
    }
    public static void RegisterBehaviour(ITickUpdate behaviour)
    {
        if (!Access())
        {
            return;
        }

        string type_name = (behaviour as MonoBehaviour).GetType().Name;
        //Add type name
        if (!Instance.behaviours_names.Contains(type_name))
        {
            Instance.behaviours_names.Add(type_name);
        }

        Instance.tickupdate_behaviours.Add(behaviour);
    }
    public static void RegisterBehaviour(IAfterFrameUpdate behaviour)
    {
        if (!Access())
        {
            return;
        }

        string type_name = (behaviour as MonoBehaviour).GetType().Name;
        //Add type name
        if (!Instance.behaviours_names.Contains(type_name))
        {
            Instance.behaviours_names.Add(type_name);
        }

        Instance.afterframeupdate_behaviours.Add(behaviour);
    }
    public static void UnregisterBehaviour(IFrameUpdate behaviour)
    {
        if (!Access())
        {
            return;
        }

        string type_name = (behaviour as MonoBehaviour).GetType().Name;
        //Add type name
        if (Instance.behaviours_names.Contains(type_name))
        {
            Instance.behaviours_names.Remove(type_name);
        }

        Instance.frameupdate_behaviours.Remove(behaviour);
    }
    public static void UnregisterBehaviour(ITickUpdate behaviour)
    {
        if (!Access())
        {
            return;
        }

        string type_name = (behaviour as MonoBehaviour).GetType().Name;
        //Add type name
        if (Instance.behaviours_names.Contains(type_name))
        {
            Instance.behaviours_names.Remove(type_name);
        }

        Instance.tickupdate_behaviours.Remove(behaviour);
    }
    public static void UnregisterBehaviour(IAfterFrameUpdate behaviour)
    {
        if (!Access())
        {
            return;
        }

        string type_name = (behaviour as MonoBehaviour).GetType().Name;
        //Add type name
        if (Instance.behaviours_names.Contains(type_name))
        {
            Instance.behaviours_names.Remove(type_name);
        }

        Instance.afterframeupdate_behaviours.Remove(behaviour);
    }

    public static ulong FrameCalls = 0;
    System.Diagnostics.Stopwatch IFrameWatch = new System.Diagnostics.Stopwatch();
    public static long IFrameUpdateExecTime { get; private set; }
    public static float frame_delta { get; private set; } = 0;
    private void Update()
    {
        frame_delta = Time.deltaTime;
        IFrameWatch.Restart();
        UnityEngine.Profiling.Profiler.BeginSample("BehaviourManager FrameUpdate");
        for (int i = 0; i < frameupdate_behaviours.Count; i++)
        {
            frameupdate_behaviours[i].FrameUpdate(frame_delta);
            FrameCalls++;
        }
        UnityEngine.Profiling.Profiler.EndSample();
        IFrameWatch.Stop();
        IFrameUpdateExecTime = IFrameWatch.ElapsedMilliseconds;
    }

    public static ulong AfterFrameCalls = 0;
    System.Diagnostics.Stopwatch IAfterFrameWatch = new System.Diagnostics.Stopwatch();
    public static long IAfterFrameUpdateExecTime { get; private set; }
    private void LateUpdate()
    {
        IAfterFrameWatch.Restart();
        UnityEngine.Profiling.Profiler.BeginSample("BehaviourManager AfterFrameUpdate");
        for (int i = 0; i < afterframeupdate_behaviours.Count; i++)
        {
            afterframeupdate_behaviours[i].AfterFrameUpdate(frame_delta);
            AfterFrameCalls++;
        }
        UnityEngine.Profiling.Profiler.EndSample();
        IAfterFrameWatch.Stop();
        IAfterFrameUpdateExecTime = IAfterFrameWatch.ElapsedMilliseconds;
    }

    public static ulong TickCalls = 0;
    System.Diagnostics.Stopwatch ITickUpdateWatch = new System.Diagnostics.Stopwatch();
    public static long ITickUpdateExecTime { get; private set; }
    private void FixedUpdate()
    {
        ITickUpdateWatch.Restart();
        UnityEngine.Profiling.Profiler.BeginSample("BehaviourManager TickUpdate");
        for (int i = 0; i < tickupdate_behaviours.Count; i++)
        {
            tickupdate_behaviours[i].TickUpdate(Time.fixedDeltaTime);
            TickCalls++;
        }
        UnityEngine.Profiling.Profiler.EndSample();
        ITickUpdateWatch.Stop();
        ITickUpdateExecTime = ITickUpdateWatch.ElapsedMilliseconds;
    }
}
