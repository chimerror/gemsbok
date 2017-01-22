using System;
public enum Hazard
{
    /// <summary>
    /// No hazard in this room.
    /// </summary>
    None,
    /// <summary>
    /// The equivalent of the bats from the original game. Transports player to a random room.
    /// </summary>
    FairyPath,
    /// <summary>
    /// The equivalent of the pits from the original game. Kills player.
    /// </summary>
    CrowsTalons,
}
