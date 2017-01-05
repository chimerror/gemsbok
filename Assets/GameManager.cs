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
    public int RandomSeed;
    public Light RoomLight;
    public VRTK_ObjectTooltip RoomTooltip;
    public ExitDoor ExitDoorPrefab;
    public Transform ExitDoorSpawn;
    public VRTK_HeadsetFade HeadsetFade;
    public float FadeDuration = 0.15f;

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
        HeadsetFade.Fade(doorColor, FadeDuration);
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
    }

    private void InitializeCurrentRoom()
    {
        var roomColor = GetRoomColor(_playerRoom.Color);
        RoomLight.color = roomColor;
        RoomTooltip.containerColor = roomColor;
        RoomTooltip.displayText = "Sector " + GetRoomNickname(_playerRoom.Color);
        RoomTooltip.Reset();

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
            exitDoor.DoorColor = exitColor;
            exitDoor.RoomNickname = exitNickname;
            exitDoor.Destination = exitKeyValuePair.Value;
            exitDoor.gameObject.SetActive(true);
        }
    }

    private Color GetRoomColor(ushort roomHue)
    {
        if (!_roomColors.ContainsKey(roomHue))
        {
            float roomHueFloat = _playerRoom.Color / 360.0f;
            Color roomColor = UnityEngine.Random.ColorHSV(roomHueFloat, roomHueFloat, 0.25f, 1f, 0.25f, 1f);
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
