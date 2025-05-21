using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public enum GameSessionState
{
    Lobby,
    Match,
    InGame
}

public class SessionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public class GameRoomInfo
    {
        public PlayerRef HostPlayer { get; set; }
        public PlayerRef ClinetPlayer { get; set; }
    }
    private const int LOBBY_SCENE_INDEX = 1;
    private const int IN_GAME_SCENE_INDEX = 2;
    
    private const int MAX_PLAYER_COUNT = 2;

    public static SessionManager Instance;

    private NetworkRunner runner;
    private NetworkSceneManagerDefault sceneManager;
    
    private bool isInitialized = false;

    //[Header("Prefabs")]
    // 플레이어 프리팹
    
    public GameRoomInfo CurrentGameRoomInfo { get; private set; } 
    
    public GameSessionState CurrentState = GameSessionState.Lobby;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitRunnerAsync();
    }

    private void InitRunnerAsync()
    {
        if (isInitialized) return;
            isInitialized = true;
            sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(); //  Fusion에서 기본으로 제공하는 씬 전환 매니저 클래스로, 네트워크 플레이어들이 씬을 같이 전환하고 동기화되도록 함
            runner = gameObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;
            runner.AddCallbacks(this);
            DontDestroyOnLoad(gameObject);
    }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }
}
