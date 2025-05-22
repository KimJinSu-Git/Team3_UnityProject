using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameSessionState
{
    Lobby,
    Match,
    InGame
}

public class SessionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static SessionManager Instance;

    public class GameRoomInfo
    {
        public PlayerRef HostPlayer { get; set; }
        public PlayerRef ClientPlayer { get; set; }
    }
    private const int LOBBY_SCENE_INDEX = 1;
    private const int IN_GAME_SCENE_INDEX = 2;
    
    private const int MAX_PLAYER_COUNT = 2;

    private NetworkRunner runner;
    private NetworkSceneManagerDefault sceneManager;

    private SceneRef lobbySceneRef;
    private SceneRef inGameSceneRef;
    
    private bool isInitialized = false;

    //[Header("Prefabs")]
    // 플레이어 프리팹
    
    public GameRoomInfo CurrentGameRoomInfo { get; private set; } 
    
    public GameSessionState CurrentState = GameSessionState.Lobby;
    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else 
        {
            Destroy(gameObject);
        }
        lobbySceneRef = SceneRef.FromIndex(LOBBY_SCENE_INDEX);
        inGameSceneRef = SceneRef.FromIndex(IN_GAME_SCENE_INDEX);
    }

    private void Start()
    {
        InitRunnerAsync();
    }

    private void InitRunnerAsync()
    {
        if (isInitialized) return; 
        isInitialized = true;
        if(runner != null) Destroy(runner);
        sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(); //  Fusion에서 기본으로 제공하는 씬 전환 매니저 클래스로, 네트워크 플레이어들이 씬을 같이 전환하고 동기화되도록 함
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;
        runner.AddCallbacks(this);
        DontDestroyOnLoad(gameObject);
    }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"활성화된 방 = {sessionList.Count}");
        SessionInfo joinAble = sessionList.FirstOrDefault(sessionInfo =>
        {
            return sessionInfo.PlayerCount <
                   sessionInfo.MaxPlayers && // 여기의 PlayerCount는 접속해있는 Player수
                   sessionInfo.IsOpen && sessionInfo.IsVisible; // 이 결과가 true인 첫 번째 sessionInfo가 joinAble에 할당
        });
        if (joinAble != null)
        {
            JoinRoom(joinAble.Name);
        }
        else
        {
            CreateRoom("room_" + Guid.NewGuid());
        }
    }

    private async void CreateRoom(string roomName)
    {
        StartGameArgs args = new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = roomName,
            PlayerCount = MAX_PLAYER_COUNT,
            SceneManager = sceneManager
        };
        await runner.StartGame(args);
        CurrentGameRoomInfo = new GameRoomInfo();
        CurrentState = GameSessionState.Match;
    }

    private async void JoinRoom(string roomName)
    {
        StartGameArgs args = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = roomName,
            SceneManager = sceneManager
        };
        await runner.StartGame(args);
        CurrentGameRoomInfo = new GameRoomInfo();
        CurrentState = GameSessionState.Match;
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player != this.runner.LocalPlayer)
        {
            CurrentGameRoomInfo.HostPlayer = runner.IsServer ? runner.LocalPlayer : player;
            CurrentGameRoomInfo.ClientPlayer = runner.IsServer ? player : runner.LocalPlayer; ;
        }

        if (this.runner.IsServer && runner.ActivePlayers.Count() == MAX_PLAYER_COUNT && 
            CurrentState == GameSessionState.Match)
        {
            StartInGame(); 
        }
    }
    private async Task StartInGame()
    {
        await runner.LoadScene(inGameSceneRef);
    }
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (runner.IsServer)
        {
            // 프리팹생성해주기
        }
    }
    private void ReturnToLobby()
    {
        runner.Shutdown(); // 기존 세션 종료
        Destroy(runner);
        CurrentGameRoomInfo = null;
        CurrentState = GameSessionState.Lobby;
        SceneManager.LoadScene(LOBBY_SCENE_INDEX);
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }
    #region MyRegion
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }
    
    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }
    #endregion
}
