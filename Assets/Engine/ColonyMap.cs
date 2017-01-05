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
            { Hazard.Bats, 2 },
            { Hazard.Pit, 2 },
        },
        Rooms = new List<List<int>>
        {
            new List<int> { 15, 16, 19 }, // 20
            new List<int> { 2, 5, 6 },
            new List<int> { 1, 3, 8 },
            new List<int> { 2, 4, 10 },
            new List<int> { 3, 5, 12 },
            new List<int> { 1, 4, 14 },
            new List<int> { 1, 7, 15 },
            new List<int> { 6, 8, 16 },
            new List<int> { 2, 7, 9 },
            new List<int> { 8, 10, 17 },
            new List<int> { 3, 9, 11 },
            new List<int> { 10, 12, 18 },
            new List<int> { 4, 11, 13 },
            new List<int> { 12, 14, 19 },
            new List<int> { 5, 13, 15 },
            new List<int> { 6, 14, 0 },
            new List<int> { 7, 17, 0 },
            new List<int> { 9, 16, 18 },
            new List<int> { 11, 17, 19 },
            new List<int> { 13, 18, 0 },
        },
    };
}
