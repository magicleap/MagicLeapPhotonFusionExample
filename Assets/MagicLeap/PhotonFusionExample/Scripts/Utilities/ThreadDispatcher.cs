using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace MagicLeap
{
/// <summary>
/// A class that handles dispatching calls to main and worker a thread
/// </summary>
public class ThreadDispatcher : MonoBehaviour
{

    private static int processorCount = 0;

    public static int maxThreads
    {
        get
        {
            if (processorCount == 0) processorCount = SystemInfo.processorCount;
            return processorCount;
        }
    }

    public static int numThreads = 0;
    
    /// <summary>
    /// Check to see if the object is being destroyed.
    /// </summary>
    private static bool _shuttingDown = false;

    /// <summary>
    /// Used to lock the main thread when calling <see cref="Instance"/>.
    /// </summary>
    private static object _lock = new object();
    
    /// <summary>
    /// instance used to check if the class is present in a scene. 
    /// </summary>
    /// <remarks>
    /// Class checks this on <see cref="Awake"/> to determine if it is unique to the scene or needs to be destroyed 
    /// </remarks>
    private static ThreadDispatcher _instance;

    /// <summary>
    ///  Gets singleton instance of the class. Creates the instance if needed.
    /// </summary>
    public static ThreadDispatcher Instance
    {
        get
        {
            if (_shuttingDown)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(ThreadDispatcher) + "' already destroyed. Returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // Search for existing instance.
                    _instance = FindObjectOfType<ThreadDispatcher>();

                    // Create new instance if one doesn't already exist.
                    if (_instance == null)
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<ThreadDispatcher>();
                        singletonObject.name = typeof(ThreadDispatcher).ToString() + " (Singleton)";

                        // Make instance persistent.
                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// A queue of actions to be executed on the main thread
    /// </summary>
    private ConcurrentQueue<Action> _mainThreadQueue;

    /// <summary>
    /// A queue of actions to be executed on a worker thread
    /// </summary>
    private ConcurrentQueue<Action> _workerThreadQueue;
    
    /// <summary>
    /// Cancellation source for shutdown
    /// </summary>
    private static CancellationTokenSource _shutDownTokenSource = new();
    /// <summary>
    /// A flag to indicate if the worker thread is running
    /// </summary>
    private bool _workerThreadRunning;

    /// <summary>
    /// // A reference to the worker thread
    /// </summary>
    private Thread _workerThread;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    /// <remarks>
    ///  Used to initialize the singleton instance and the queues.
    /// </remarks>
    private void Awake()
    {
        if (_instance == null)
        {

            _instance = this;
            _mainThreadQueue = new ConcurrentQueue<Action>();
            _workerThreadQueue = new ConcurrentQueue<Action>();
            _workerThreadRunning = true;
            _workerThread = new Thread(WorkerThreadLoop);
            _workerThread.Start();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Occurs when a Scene or game ends.
    /// </summary>
    /// <remarks>
    /// Used to make sure the object is not spawned when calling <see cref="Instance"/>.
    /// </remarks>
    private void OnDestroy()
    {
        _shuttingDown = true;
    }
    /// <summary>
    /// Called by Unity before the application quits.
    /// </summary>
    /// <remarks>
    /// Used to stop the worker thread and clear the queues when the application quits.
    /// </remarks>
    private void OnApplicationQuit()
    {
        _shuttingDown = true;
        _workerThreadRunning = false;
        _shutDownTokenSource?.Cancel();
        _workerThread?.Join();
        _mainThreadQueue?.Clear();
        _workerThreadQueue?.Clear();
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    /// <remarks>
    /// Called to dispatch tasks on the main thread.
    /// </remarks>
    private void Update()
    {
        MainThreadLoop();
    }

    /// <summary>
    /// Queues an action to be executed on the main thread.
    /// </summary>
    /// <remarks>
    /// Accessed publicly by <see cref="RunOnMainThread"/>
    /// </remarks>
    private void runOnMainThread(Action action)
    {
        if (action == null|| _shutDownTokenSource.IsCancellationRequested)
        {
            return;
        }

        _mainThreadQueue.Enqueue(action);
     
    }

    /// <summary>
    /// Queues an action to be executed on the worker thread.
    /// </summary>
    /// <remarks>
    /// Accessed publicly by <see cref="RunOnWorkerThread"/>
    /// </remarks>
    private void runOnWorkerThread(Action action)
    {
        if (_shutDownTokenSource.IsCancellationRequested || action == null)
        {
            return;
        }
        _workerThreadQueue.Enqueue(action);
    }

    /// <summary>
    /// Queues an action to be executed on the main thread.
    /// </summary>
    public static void RunOnMainThread(Action action)
    {
        if (Instance == null)
        {
            try {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return;
        }
        Instance.runOnMainThread(action);
    }
    
    /// <summary>
    /// Queues an action on a worker thread.
    /// </summary>
    public static void RunOnWorkerThread(Action action)
    {
        if (Instance == null)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return;
        }
        Instance.runOnWorkerThread(action);
    }



    private Thread runAsync(Action action)
    {
       
        while (numThreads >= maxThreads)
        {
            Thread.Sleep(1);
        }

        Interlocked.Increment(ref numThreads);
        try
        {
            ThreadPool.QueueUserWorkItem(RunAction, action);
        }
        catch (Exception e)
        {
           Debug.LogError(e);
        }

        return null;
    }

    public static Thread RunAsync(Action action)
    {
        if (Instance == null)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return null;
        }
        return Instance.runAsync(action);
    }
    private  void RunAction(object action)
    {
        try
        {
            ((Action)action)?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            Interlocked.Decrement(ref numThreads);
        }

    }

    /// <summary>
    /// Processes the actions in the main thread queue.
    /// </summary>
    private void MainThreadLoop()
    {
        while (_mainThreadQueue.TryDequeue(out Action action))
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
    

    /// <summary>
    /// Processes the actions in the worker thread queue.
    /// </summary>
    private void WorkerThreadLoop()
    {
        Thread.CurrentThread.IsBackground = true;
        //Allows native android functions to be called on the worker thread
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJNI.AttachCurrentThread();
        }

        while (_workerThreadRunning)
        {

            if (_workerThreadQueue.TryDequeue(out Action action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else if (_shutDownTokenSource.IsCancellationRequested)
            {
                break;
            }
            else
            {
                // To avoid busy waiting
                Thread.Sleep(5);
            }
        }

        //After calling AndroidJNI.AttachCurrentThread(), we have to detach it at the end
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJNI.DetachCurrentThread();
        }

    }
}
}
