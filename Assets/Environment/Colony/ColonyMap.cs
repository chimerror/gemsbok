using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ColonyMap
{
    public Dictionary<Hazard, int> HazardCount { get; private set; }
    public List<List<int>> Rooms { get; private set; }

    public static ColonyMap Dodecahedron = new ColonyMap
    {
        HazardCount = new Dictionary<Hazard, int>
        {
            { Hazard.FairyPath, 2 },
            { Hazard.CrowsTalons, 2 },
        },
        Rooms = new List<List<int>>
        {
            new List<int> { 13, 16, 19 }, // 20
            new List<int> { 2, 5, 8 },
            new List<int> { 1, 3, 10 },
            new List<int> { 2, 4, 12 },
            new List<int> { 3, 5, 14 },
            new List<int> { 1, 4, 6 },
            new List<int> { 5, 7, 15 },
            new List<int> { 6, 8, 17 },
            new List<int> { 1, 7, 9 },
            new List<int> { 8, 10, 18 },
            new List<int> { 2, 9, 11 },
            new List<int> { 10, 12, 19 },
            new List<int> { 3, 11, 13 },
            new List<int> { 12, 14, 0 },
            new List<int> { 4, 13, 15 },
            new List<int> { 6, 14, 16 },
            new List<int> { 15, 17, 0 },
            new List<int> { 7, 16, 18 },
            new List<int> { 9, 17, 19 },
            new List<int> { 11, 18, 0 },
        },
    };
}
