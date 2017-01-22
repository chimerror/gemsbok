using System;
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

    public int RandomSeed;
    public Light RoomLight;
    public VRTK_ObjectTooltip RoomTooltip;
    public Transform PlayerTransform;
    public string RoomFormat = "<color=\"#{0:x2}{1:x2}{2:x2}{3:x2}\">{4}</color>";
    public GameObject TravelIndicator;
    public float ExitDoorTriggerDistance = 1.5f;
    public string TravelIndicatorFormat = "Press Fire1 to travel to {0} sector.";
    public ExitDoor ExitDoorPrefab;
    public Transform ExitDoorSpawn;
    public VRTK_HeadsetFade HeadsetFade;
    public float FadeDuration = 0.15f;
    public float TeleportFadeDuration = 0.15f;
    public float HazardFadeDuration = 5f;
    public ParticleSystem CrowsTalons;
    public ParticleSystem FairyPath;
    public ParticleSystem Wumpus;

    private System.Random _random;
    private List<HistoryEntry> _history = new List<HistoryEntry>();
    private GameState _gameState;
    private Colony _colony;
    private ColonyRoom _playerRoom;
    private ColonyRoom _wumpusRoom;
    private Dictionary<ushort, Color> _roomColors = new Dictionary<ushort, Color>();
    private Dictionary<ushort, string> _namedRooms = new Dictionary<ushort, string>();
    private RoomNicknames _nextRoomName = RoomNicknames.Alfa;

    public string GetRoomText(ColonyRoom room)
    {
        var color = GetRoomColor(room);
        return string.Format(RoomFormat,
            Mathf.FloorToInt(color.r * 255),
            Mathf.FloorToInt(color.g * 255),
            Mathf.FloorToInt(color.b * 255),
            Mathf.FloorToInt(color.a * 255),
            GetRoomNickname(room).ToUpperInvariant());
    }

    public void MoveToRoom(ColonyRoom nextRoom, Color doorColor)
    {
        Debug.LogFormat("Moving to room {0}", nextRoom.Color);
        _gameState = GameState.MovingToNewRoom;
        _playerRoom = nextRoom;
        FadeDuration = TeleportFadeDuration;
        HeadsetFade.Fade(doorColor, FadeDuration);
    }

    public Color GetRoomColor(ColonyRoom room)
    {
        return GetRoomColor(room.Color);
    }

    public string GetRoomNickname(ColonyRoom room)
    {
        return GetRoomNickname(room.Color);
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
        Debug.LogFormat("Wumpus is in {0}", _wumpusRoom.Color);
        foreach (var room in _colony.Rooms)
        {
            if (room.Hazard == Hazard.Bats)
            {
                Debug.LogFormat("Bats are in {0}", room.Color);
            }
            else if (room.Hazard == Hazard.Pit)
            {
                Debug.LogFormat("Pits are in {0}", room.Color);
            }
        }
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
    }

    private void InitializeCurrentRoom()
    {
        _gameState = GameState.InitializingRoom;

        if (!VRSettings.enabled || !VRDevice.isPresent)
        {
            PlayerTransform.position = Vector3.zero;
            PlayerTransform.rotation = Quaternion.identity;
        }

        var roomColor = GetRoomColor(_playerRoom.Color);
        RoomLight.color = roomColor;
        RoomTooltip.containerColor = roomColor;
        var roomNickname = GetRoomNickname(_playerRoom.Color);
        RoomTooltip.displayText = "Sector " + roomNickname;
        RoomTooltip.Reset();

        var historyEntry = new HistoryEntry(roomColor, roomNickname);

        if (_playerRoom.Hazard == Hazard.Bats)
        {
            historyEntry.FairyPathRoom = true;
            _history.Add(historyEntry);
            _gameState = GameState.FairyPathCutscene;
            StartCoroutine(PerformTeleport());
            return;
        }
        else if (_playerRoom.Hazard == Hazard.Pit)
        {
            _gameState = GameState.CrowsTalonsCutscene;
            StartCoroutine(Pitfall());
            return;
        }
        else if (_playerRoom.Color == _wumpusRoom.Color)
        {
            _gameState = GameState.WumpusCutscene;
            StartCoroutine(GetEaten());
            return;
        }

        foreach (var exitKeyValuePair in _playerRoom.Exits)
        {
            float exitAngleDeg = exitKeyValuePair.Key;
            float exitAngleRad = exitAngleDeg * Mathf.Deg2Rad;
            Vector3 exitPosition = new Vector3(ExitRadius * Mathf.Cos(exitAngleRad), 0.0f, ExitRadius * Mathf.Sin(exitAngleRad));
            Quaternion exitRotation = Quaternion.Euler(0.0f, 270f - exitAngleDeg, 0.0f);

            var exitRoom = exitKeyValuePair.Value;
            string exitNickname = GetRoomNickname(exitRoom.Color);
            var exitColor = GetRoomColor(exitRoom.Color);
            var exitDoor = Instantiate(ExitDoorPrefab, exitPosition, exitRotation, ExitDoorSpawn);
            Debug.LogFormat("Exit {0} goes to {1}", exitNickname, exitRoom.Color);
            exitDoor.DoorColor = exitColor;
            exitDoor.RoomNickname = exitNickname;
            exitDoor.Destination = exitKeyValuePair.Value;
            exitDoor.gameObject.SetActive(true);

            historyEntry.ExitNicknames.Add(exitNickname);

            if (exitRoom.Hazard == Hazard.Bats)
            {
                historyEntry.FairyPathNearby = true;
            }

            if (exitRoom.Hazard == Hazard.Pit)
            {
                historyEntry.CrowsTalonsNearby = true;
            }

            if (exitRoom.Color == _wumpusRoom.Color)
            {
                historyEntry.WumpusNearby = true;
            }
        }
        _history.Add(historyEntry);
        _gameState = GameState.WaitingForPlayer;
    }

    private IEnumerator PerformTeleport()
    {
        Debug.Log("Can't stop here, it's bat country!");
        FairyPath.gameObject.SetActive(true);
        yield return new WaitForSeconds(10f);
        var nextRoom = _random.Next(_colony.Rooms.Count);
        while (_colony.Rooms[nextRoom].Color == _playerRoom.Color || _colony.Rooms[nextRoom].Hazard == Hazard.Bats)
        {
            nextRoom++;
            nextRoom %= _colony.Rooms.Count;
        }
        _playerRoom = _colony.Rooms[nextRoom];
        var fadeColor = new Color(0f, 1f, 1f, 1f);
        FadeDuration = HazardFadeDuration;
        HeadsetFade.Fade(fadeColor, FadeDuration);
    }

    private IEnumerator Pitfall()
    {
        Debug.Log("Aaaaaaaaaa!");
        CrowsTalons.gameObject.SetActive(true);
        yield return new WaitForSeconds(10f);
        HeadsetFade.HeadsetFadeComplete -= MoveToNextRoom;
        var fadeColor = new Color(0f, 0f, 0f, 1f);
        FadeDuration = HazardFadeDuration;
        HeadsetFade.Fade(fadeColor, FadeDuration);
    }

    private IEnumerator GetEaten()
    {
        Debug.Log("Mmmm, delicious venison! HA HA HA HA!");
        Wumpus.gameObject.SetActive(true);
        yield return new WaitForSeconds(10f);
        HeadsetFade.HeadsetFadeComplete -= MoveToNextRoom;
        var fadeColor = new Color(1f, 0f, 0f, 1f);
        FadeDuration = HazardFadeDuration;
        HeadsetFade.Fade(fadeColor, FadeDuration);
    }

    private Color GetRoomColor(ushort roomHue)
    {
        if (!_roomColors.ContainsKey(roomHue))
        {
            float roomHueFloat = _playerRoom.Color / 360.0f;
            Color roomColor = UnityEngine.Random.ColorHSV(roomHueFloat, roomHueFloat, 0.125f, 1f, 0.50f, 1f);
            _roomColors[roomHue] = roomColor;
        }
        return _roomColors[roomHue];
    }

    private string GetRoomNickname(ushort roomColor)
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
