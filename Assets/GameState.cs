using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum GameState
{
    InitializingRoom,
    WaitingForPlayer,
    ScutterTargeting,
    MovingToNewRoom,
    FairyPathCutscene,
    CrowsTalonsCutscene,
    WumpusCutscene,
    WumpusMovementMessage,
    ScutterCutscene,
    MissedCutscene,
    ArrowedCutscene,
    WinningCutscene,
}
