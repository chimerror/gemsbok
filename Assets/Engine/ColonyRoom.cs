using System;
using System.Collections.Generic;

public class ColonyRoom
{
    public ushort Color { get; set; }
    public Hazard Hazard { get; set; }
    public Dictionary<ushort, ColonyRoom> Exits { get; set; }

    public ColonyRoom(ushort color, Hazard hazard = Hazard.None)
    {
        if (color >= 360)
        {
            throw new ArgumentException("Color must be less than 360");
        }

        Color = color;
        Hazard = hazard;
        Exits = new Dictionary<ushort, ColonyRoom>();
    }
}
