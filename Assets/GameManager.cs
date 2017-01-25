﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;
using VRTK;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public const float ExitRadius = 5.0f;

    public System.Random Random
    {
        get
        {
            return _random;
        }
    }

    public List<ColonyRoom> AllRooms
    {
        get
        {
            return _colony.Rooms;
        }
    }

    public Dictionary<ushort, string> NamedRooms
    {
        get
        {
            return _namedRooms;
        }
    }

    public List<HistoryEntry> History
    {
        get
        {
            return _history;
        }
    }

    public GameState CurrentGameState
    {
        get
        {
            return _gameState;
        }
    }

    public ColonyRoom CurrentPlayerRoom
    {
        get
        {
            return _playerRoom;
        }
    }

    public ColonyRoom CurrentWumpusRoom
    {
        get
        {
            return _wumpusRoom;
        }
    }

    public int ShotsTaken
    {
        get
        {
            return _shotsTaken;
        }
    }

    public int RandomSeed;
    public Light RoomLight;
    public Transform PlayerTransform;
    public string RoomFormat = "<color=\"#{0:x2}{1:x2}{2:x2}{3:x2}\">{4}</color>";
    public GameObject TravelIndicator;
    public float ExitDoorTriggerDistance = 1.5f;
    public string TravelIndicatorFormat = "Press Fire1 to travel to {0} sector.";
    public float WumpusMovementMessageDuration = 5.0f;
    public ExitDoor ExitDoorPrefab;
    public Transform ExitDoorSpawn;
    public VRTK_HeadsetFade HeadsetFade;
    public float FadeDuration = 0.15f;
    public float TeleportFadeDuration = 0.15f;
    public float HazardFadeDuration = 5f;
    public ParticleSystem CrowsTalons;
    public ParticleSystem FairyPath;
    public ParticleSystem Wumpus;
    public GameObject MiniHUD;
    public GameObject HistoryView;
    public ScutterTargeting ScutterTargeting;

    private System.Random _random;
    private List<HistoryEntry> _history = new List<HistoryEntry>();
    private GameState _gameState;
    private Colony _colony;
    private ColonyRoom _playerRoom;
    private ColonyRoom _wumpusRoom;
    private Dictionary<ushort, Color> _roomColors = new Dictionary<ushort, Color>();
    private Dictionary<ushort, string> _namedRooms = new Dictionary<ushort, string>();
    private RoomNicknames _nextRoomName = RoomNicknames.Alfa;
    private int _shotsTaken = 0;

    public string GetRoomText(ColonyRoom room, bool createRoom = true)
    {
        if (createRoom || _namedRooms.ContainsKey(room.Color))
        {
            var color = GetOrCreateRoomColor(room);
            return string.Format(RoomFormat,
                Mathf.FloorToInt(color.r * 255),
                Mathf.FloorToInt(color.g * 255),
                Mathf.FloorToInt(color.b * 255),
                Mathf.FloorToInt(color.a * 255),
                GetOrCreateRoomNickname(room).ToUpperInvariant());
        }
        else
        {
            return string.Format("UNKNOWN-{0:D2}", AllRooms.FindIndex(r => r.Color == room.Color));
        }
    }

    public void MoveToRoom(ColonyRoom nextRoom, Color doorColor)
    {
        _gameState = GameState.MovingToNewRoom;
        _playerRoom = nextRoom;
        FadeDuration = TeleportFadeDuration;
        HeadsetFade.Fade(doorColor, FadeDuration);
    }

    public void FireScutter(List<ColonyRoom> path)
    {
        _gameState = GameState.ScutterCutscene;
        StartCoroutine(ScutterCutscene(path));
    }

    public void CancelScutter()
    {
        _gameState = GameState.WaitingForPlayer;
    }

    public Color GetOrCreateRoomColor(ColonyRoom room)
    {
        return GetOrCreateRoomColor(room.Color);
    }

    public string GetOrCreateRoomNickname(ColonyRoom room)
    {
        return GetOrCreateRoomNickname(room.Color);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogAssertion("Two GameManagers present!");
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        if (RandomSeed == 0)
        {
            // This way, 0 is never possible.
            if (UnityEngine.Random.Range(0, 1) == 0)
            {
                RandomSeed = UnityEngine.Random.Range(int.MinValue, 0);
            }
            else
            {
                RandomSeed = UnityEngine.Random.Range(1, int.MaxValue);
            }
        }

        _random = new System.Random(RandomSeed);
        UnityEngine.Random.InitState(RandomSeed);
        Debug.LogFormat("Random seed set: {0}", RandomSeed);

        _colony = new Colony(_random);
        _playerRoom = _colony.PlayerStart;
        _wumpusRoom = _colony.WumpusStart;
        HeadsetFade.HeadsetFadeComplete += MoveToNextRoom;
    }

    private void MoveToNextRoom(object sender, HeadsetFadeEventArgs e)
    {
        CrowsTalons.gameObject.SetActive(false);
        FairyPath.gameObject.SetActive(false);
        Wumpus.gameObject.SetActive(false);
        foreach (var exitDoor in ExitDoorSpawn.GetComponentsInChildren<ExitDoor>())
        {
            exitDoor.gameObject.SetActive(false);
            DestroyObject(exitDoor.gameObject);
        }
        InitializeCurrentRoom();
        HeadsetFade.Unfade(FadeDuration);
    }

    private void Start()
    {
        InitializeCurrentRoom();
        //Debug.LogFormat("Wumpus is in {0}", _colony.Rooms.FindIndex(r => r.Color == _wumpusRoom.Color));
        //var currentIndex = 0;
        //foreach (var room in _colony.Rooms)
        //{
        //    if (room.Hazard == Hazard.FairyPath)
        //    {
        //        Debug.LogFormat("Bats are in {0}", currentIndex);
        //    }
        //    else if (room.Hazard == Hazard.CrowsTalons)
        //    {
        //        Debug.LogFormat("Pits are in {0}", currentIndex);
        //    }
        //    currentIndex++;
        //}
    }

    private void Update()
    {
        bool playerCloseToExit = false;
        foreach (var exitDoor in ExitDoorSpawn.GetComponentsInChildren<ExitDoor>())
        {
            var playerDistance = Mathf.Abs(Vector3.Distance(exitDoor.transform.position, PlayerTransform.position));
            if (playerDistance <= ExitDoorTriggerDistance)
            {
                if (Input.GetButtonUp("Fire1"))
                {
                    Instance.MoveToRoom(exitDoor.Destination, exitDoor.DoorColor);
                }
                else
                {
                    playerCloseToExit = true;
                    var text = TravelIndicator.GetComponentInChildren<Text>();
                    text.text = string.Format(TravelIndicatorFormat, GetRoomText(exitDoor.Destination));
                }
                break;
            }
        }
        TravelIndicator.SetActive(playerCloseToExit);

        if (_gameState == GameState.WaitingForPlayer && Input.GetButtonUp("Fire4"))
        {
            ScutterTargeting.gameObject.SetActive(true);
            _gameState = GameState.ScutterTargeting;
            return;
        }

        if (_gameState == GameState.WaitingForPlayer || _gameState == GameState.ScutterTargeting)
        {
            if (Input.GetButtonUp("Fire3") && !HistoryView.activeInHierarchy)
            {
                HistoryView.SetActive(true);
                MiniHUD.SetActive(false);
                return;
            }

            if (HistoryView.activeInHierarchy &&
                 Input.GetButtonUp("Fire2") ||
                 Input.GetButtonUp("Fire3") ||
                 Input.GetButtonUp("Cancel"))
            {
                HistoryView.SetActive(false);
                MiniHUD.SetActive(true);
                return;
            }

            // TODO: Cancel favors the history window. I think it should be the other way, but don't feel like fixing it
            //       right now.
            // TODO: These Early returns means we won't catch if multiple buttons are pressed
            //       Fix later.
        }
    }

    private void InitializeCurrentRoom()
    {
        _gameState = GameState.InitializingRoom;

        if (!VRSettings.enabled || !VRDevice.isPresent)
        {
            PlayerTransform.position = Vector3.zero;
            PlayerTransform.rotation = Quaternion.identity;
        }

        GetOrCreateRoomNickname(_playerRoom);
        var roomColor = GetOrCreateRoomColor(_playerRoom.Color);
        RoomLight.color = roomColor;
        var historyEntry = new MovementEntry(_playerRoom, false);

        if (_playerRoom.Color == _wumpusRoom.Color)
        {
            HandleWumpus();
            if (_playerRoom.Color == _wumpusRoom.Color)
            {
                _gameState = GameState.WumpusCutscene;
                StartCoroutine(WumpusCutscene());
                return;
            }
            else
            {
                _gameState = GameState.WumpusMovementMessage;
                StartCoroutine(ShowWumpusMovementMessage());
            }
        }

        if (_playerRoom.Hazard == Hazard.FairyPath)
        {
            _history.Add(historyEntry);
            StartCoroutine(FairyPathCutscene());
            return;
        }
        else if (_playerRoom.Hazard == Hazard.CrowsTalons)
        {
            StartCoroutine(CrowsTalonsCutscene());
            return;
        }

        foreach (var exitKeyValuePair in _playerRoom.Exits)
        {
            float exitAngleDeg = exitKeyValuePair.Key;
            float exitAngleRad = exitAngleDeg * Mathf.Deg2Rad;
            Vector3 exitPosition = new Vector3(ExitRadius * Mathf.Cos(exitAngleRad), 0.0f, ExitRadius * Mathf.Sin(exitAngleRad));
            Quaternion exitRotation = Quaternion.Euler(0.0f, 270f - exitAngleDeg, 0.0f);

            var exitRoom = exitKeyValuePair.Value;
            string exitNickname = GetOrCreateRoomNickname(exitRoom.Color);
            var exitColor = GetOrCreateRoomColor(exitRoom.Color);
            var exitDoor = Instantiate(ExitDoorPrefab, exitPosition, exitRotation, ExitDoorSpawn);
            exitDoor.DoorColor = exitColor;
            exitDoor.RoomNickname = exitNickname;
            exitDoor.Destination = exitKeyValuePair.Value;
            exitDoor.gameObject.SetActive(true);

            if (exitRoom.Color == _wumpusRoom.Color)
            {
                historyEntry.WumpusNearby = true;
            }
        }
        _history.Add(historyEntry);

        if (_gameState != GameState.WumpusMovementMessage)
        {
            _gameState = GameState.WaitingForPlayer;
        }
    }

    private IEnumerator FairyPathCutscene()
    {
        while (_gameState == GameState.WumpusMovementMessage)
        {
            yield return null;
        }
        _gameState = GameState.FairyPathCutscene;
        MiniHUD.SetActive(true);
        HistoryView.SetActive(false);
        FairyPath.gameObject.SetActive(true);
        yield return new WaitForSeconds(10f);
        var nextRoom = _random.Next(_colony.Rooms.Count);
        while (_colony.Rooms[nextRoom].Color == _playerRoom.Color || _colony.Rooms[nextRoom].Hazard == Hazard.FairyPath)
        {
            nextRoom++;
            nextRoom %= _colony.Rooms.Count;
        }
        _playerRoom = _colony.Rooms[nextRoom];
        var fadeColor = new Color(0f, 1f, 1f, 1f);
        FadeDuration = HazardFadeDuration;
        HeadsetFade.Fade(fadeColor, FadeDuration);
    }

    private IEnumerator CrowsTalonsCutscene()
    {
        while (_gameState == GameState.WumpusMovementMessage)
        {
            yield return null;
        }
        _gameState = GameState.CrowsTalonsCutscene;
        MiniHUD.SetActive(true);
        HistoryView.SetActive(false);
        CrowsTalons.gameObject.SetActive(true);
        yield return new WaitForSeconds(10f);
        HeadsetFade.HeadsetFadeComplete -= MoveToNextRoom;
        var fadeColor = new Color(0f, 0f, 0f, 1f);
        FadeDuration = HazardFadeDuration;
        HeadsetFade.Fade(fadeColor, FadeDuration);
    }

    private void HandleWumpus()
    {
        var result = UnityEngine.Random.value;
        if (result > .25f)
        {
            var exits = _wumpusRoom.Exits.Values.ToList();
            var chosenExit = Random.Next(exits.Count);
            _wumpusRoom = exits[chosenExit];
        }
    }


    private IEnumerator ShowWumpusMovementMessage()
    {
        MiniHUD.SetActive(true);
        HistoryView.SetActive(false);
        yield return new WaitForSeconds(WumpusMovementMessageDuration);
        _gameState = GameState.WaitingForPlayer;
    }

    private IEnumerator WumpusCutscene()
    {
        MiniHUD.SetActive(true);
        HistoryView.SetActive(false);
        Wumpus.gameObject.SetActive(true);
        yield return new WaitForSeconds(10f);
        HeadsetFade.HeadsetFadeComplete -= MoveToNextRoom;
        var fadeColor = new Color(1f, 0f, 0f, 1f);
        FadeDuration = HazardFadeDuration;
        HeadsetFade.Fade(fadeColor, FadeDuration);
    }

    private IEnumerator ScutterCutscene(List<ColonyRoom> path)
    {
        HistoryView.SetActive(false);
        MiniHUD.SetActive(true);
        yield return new WaitForSeconds(5.0f);
        var onPath = true;
        var currentScutterRoom = _playerRoom;
        var missed = true;
        for (int currentPathStep = 0; currentPathStep < path.Count; currentPathStep++)
        {
            var currentExits = currentScutterRoom.Exits.Values.ToList();
            var nextTarget = path[currentPathStep];
            Debug.LogFormat("Arrow Currently in {0}, heading to {1}", _colony.Rooms.IndexOf(currentScutterRoom), _colony.Rooms.IndexOf(nextTarget));
            if (!onPath || currentExits.All(r => r.Color != nextTarget.Color))
            {
                nextTarget = currentExits[_random.Next(currentExits.Count)];
                Debug.LogFormat("Destination was bad, going to {0}", _colony.Rooms.IndexOf(nextTarget));
            }

            if (nextTarget.Color == _playerRoom.Color)
            {
                missed = false;
                _gameState = GameState.ArrowedCutscene;
                StartCoroutine(ArrowedCutscene());
                break;
            }

            if (nextTarget.Color == _wumpusRoom.Color)
            {
                missed = false;
                _gameState = GameState.WinningCutscene;
                StartCoroutine(WinningCutscene());
                break;
            }

            currentScutterRoom = nextTarget;
        }

        if (missed)
        {
            _gameState = GameState.MissedCutscene;
            yield return new WaitForSeconds(5.0f);

            HandleWumpus();
            if (_wumpusRoom.Color == _playerRoom.Color || ScutterTargeting.ScuttersLeft == 0)
            {
                _gameState = GameState.WumpusCutscene;
                StartCoroutine(WumpusCutscene());
            }
            else
            {
                _history.Add(new ScutterEntry(_playerRoom, _shotsTaken + 1, path));
                _gameState = GameState.WaitingForPlayer;
            }
        }
        _shotsTaken++;
    }
    private IEnumerator ArrowedCutscene()
    {
        MiniHUD.SetActive(true);
        HistoryView.SetActive(false);
        Wumpus.gameObject.SetActive(true); // TODO: Create a different arrow animation
        yield return new WaitForSeconds(10f);
        HeadsetFade.HeadsetFadeComplete -= MoveToNextRoom;
        var fadeColor = new Color(1f, 1f, 0f, 1f);
        FadeDuration = HazardFadeDuration;
        HeadsetFade.Fade(fadeColor, FadeDuration);
    }

    private IEnumerator WinningCutscene()
    {
        MiniHUD.SetActive(true);
        HistoryView.SetActive(false);
        FairyPath.gameObject.SetActive(true); // TODO: Create a different win animation
        yield return new WaitForSeconds(10f);
        HeadsetFade.HeadsetFadeComplete -= MoveToNextRoom;
        var fadeColor = new Color(0f, 1f, 0f, 1f);
        FadeDuration = HazardFadeDuration;
        HeadsetFade.Fade(fadeColor, FadeDuration);
    }


    private Color GetOrCreateRoomColor(ushort roomHue)
    {
        if (!_roomColors.ContainsKey(roomHue))
        {
            float roomHueFloat = roomHue / 360.0f;
            Color roomColor;
            if (roomHue >= 180)
            {
                // Blues are desaturated to make them more visible
                roomColor = UnityEngine.Random.ColorHSV(roomHueFloat, roomHueFloat, 0.625f, 0.625f, 1f, 1f);
            }
            else
            {
                roomColor = UnityEngine.Random.ColorHSV(roomHueFloat, roomHueFloat, 0.75f, 0.75f, 1f, 1f);
            }

            _roomColors[roomHue] = roomColor;
        }
        return _roomColors[roomHue];
    }

    private string GetOrCreateRoomNickname(ushort roomColor)
    {
        if (!_namedRooms.ContainsKey(roomColor))
        {
            _namedRooms[roomColor] = string.Format("{0}-{1:D2}",
                _nextRoomName.ToString(),
                _colony.Rooms.FindIndex(r => r.Color == roomColor));
            _nextRoomName++;
        }

        return _namedRooms[roomColor];
    }
}
