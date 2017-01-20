using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct HistoryEntry
{
    public float Time;
    public Color RoomColor;
    public string RoomNickname;
    public List<string> ExitNicknames;
    public bool FairyPathRoom;
    public bool FairyPathNearby;
    public bool CrowsTalonsNearby;
    public bool WumpusNearby;

    public HistoryEntry(Color roomColor, string roomNickname, bool fairyPathRoom = false, bool fairyPathNearby = false, bool crowsTalonsNearby = false, bool wumpusNearby = false)
    {
        Time = UnityEngine.Time.time;
        RoomColor = roomColor;
        RoomNickname = roomNickname;
        ExitNicknames = new List<string>();
        FairyPathRoom = fairyPathRoom;
        FairyPathNearby = fairyPathNearby;
        CrowsTalonsNearby = crowsTalonsNearby;
        WumpusNearby = wumpusNearby;
    }
}
