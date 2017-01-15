using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VRTK;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public const float ExitRadius = 3.0f;

    public System.Random Random
    {
        get
        {
            return _random;
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
    private Colony _colony;
    private ColonyRoom _playerRoom;
    private ColonyRoom _wumpusRoom;
    private Dictionary<ushort, Color> _roomColors = new Dictionary<ushort, Color>();
    private Dictionary<ushort, RoomNicknames> _namedRooms = new Dictionary<ushort, RoomNicknames>();
    private RoomNicknames _nextRoomName = RoomNicknames.Alfa;

    public void MoveToRoom(ColonyRoom nextRoom, Color doorColor)
    {
        Debug.LogFormat("Moving to room {0}", nextRoom.Color);
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
            _random = new System.Random();
        }
        else
        {
            _random = new System.Random(RandomSeed);
            UnityEngine.Random.InitState(RandomSeed);
        }

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

    private void InitializeCurrentRoom()
    {
        var roomColor = GetRoomColor(_playerRoom.Color);
        RoomLight.color = roomColor;
        RoomTooltip.containerColor = roomColor;
        RoomTooltip.displayText = "Sector " + GetRoomNickname(_playerRoom.Color);
        RoomTooltip.Reset();

        if (_playerRoom.Hazard == Hazard.Bats)
        {
            StartCoroutine(PerformTeleport());
            return;
        }
        else if (_playerRoom.Hazard == Hazard.Pit)
        {
            StartCoroutine(Pitfall());
            return;
        }
        else if (_playerRoom.Color == _wumpusRoom.Color)
        {
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
        }
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
            _namedRooms[roomColor] = _nextRoomName;
            _nextRoomName++;
        }

        return _namedRooms[roomColor].ToString();
    }
}
