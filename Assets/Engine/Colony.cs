using System;
using System.Collections.Generic;

/// <summary>
/// Represents the colony with all of its sub-rooms
/// </summary>
public class Colony
{
    public ColonyRoom PlayerStart
    {
        get
        {
            return _rooms[_playerStart];
        }
    }
    public ColonyRoom WumpusStart
    {
        get
        {
            return _rooms[_wumpusStart];
        }
    }

    public List<ColonyRoom> _rooms;
    private int _playerStart;
    private int _wumpusStart;

    public Colony(Random random = null, ColonyMap map = null)
    {
        if (random == null)
        {
            random = new Random();
        }

        if (map == null)
        {
            map = ColonyMap.Dodecahedron;
        }

        _rooms = new List<ColonyRoom>();
        int roomCount = map.Rooms.Count;
        int colorDelta = 360 / roomCount;
        int currentColor = random.Next(0, 360);
        foreach (List<int> room in map.Rooms)
        {
            _rooms.Add(new ColonyRoom((ushort)currentColor));
            currentColor = (currentColor + colorDelta) % 360;
        }

        for (int currentRoom = 0; currentRoom < roomCount; currentRoom++)
        {
            var mapRoom = map.Rooms[currentRoom];
            var colonyRoom = _rooms[currentRoom];
            int exitLocation = random.Next(0, 360);
            int exitDelta = 360 / mapRoom.Count;
            foreach (int exitRoomIndex in mapRoom)
            {
                colonyRoom.Exits.Add((ushort)exitLocation, _rooms[exitRoomIndex]);
                exitLocation = (exitLocation + exitDelta) % 360;
            }
        }

        _wumpusStart = random.Next(roomCount);
        _playerStart = random.Next(roomCount);

        if (_playerStart == _wumpusStart)
        {
            _playerStart = (_playerStart + 1) % roomCount;
        }

        foreach (var hazardCount in map.HazardCount)
        {
            var hazardType = hazardCount.Key;
            var hazardsToAdd = hazardCount.Value;
            for (int currentHazard = 0; currentHazard < hazardsToAdd; currentHazard++)
            {
                int hazardLocation = random.Next(roomCount);
                while (hazardLocation == _wumpusStart || hazardLocation == _playerStart || _rooms[hazardLocation].Hazard != Hazard.None)
                {
                    hazardLocation = (hazardLocation + 1) % roomCount;
                }
                _rooms[hazardLocation].Hazard = hazardType;
            }
        }
    }
}
