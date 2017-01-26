using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class HistoryEntry
{
    public float Time;
    public ColonyRoom Room;

    public HistoryEntry(ColonyRoom room)
    {
        Time = GameManager.Instance.HistoryTime;
        Room = room;
    }
}

public class MovementEntry : HistoryEntry
{
    public bool WumpusNearby; // Need to keep track if Wumpus was nearby, since she can move.

    public MovementEntry(ColonyRoom room, bool wumpusNearby = false) : base(room)
    {
        WumpusNearby = wumpusNearby;
    }
}

public class ScutterEntry : HistoryEntry
{
    public int ShotNumber;
    public List<ColonyRoom> ShotPath;

    public ScutterEntry(ColonyRoom room, int shotNumber, List<ColonyRoom> shotPath) : base(room)
    {
        ShotNumber = shotNumber;
        ShotPath = shotPath;
    }
}
