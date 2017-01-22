using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct HistoryEntry
{
    public float Time;
    public ColonyRoom Room;
    public bool WumpusNearby; // Need to keep track if Wumpus was nearby, since she can move.

    public HistoryEntry(ColonyRoom room, bool wumpusNearby = false)
    {
        Time = UnityEngine.Time.time;
        Room = room;
        WumpusNearby = wumpusNearby;
    }
}
