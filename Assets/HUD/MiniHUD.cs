using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MiniHUD : MonoBehaviour
{
    public string TextFormat = "Currently in {0} sector\nExits to: {1}\nHazards:";
    public string RoomFormat = "<color=\"#{0:x2}{1:x2}{2:x2}{3:x2}\">{4}</color>";
    public Text MiniHUDText;
    public Image FairyPathWarning;
    public Image CrowsTalonsWarning;
    public Image WumpusWarning;

    private void Update()
    {
        var currentRoom = GameManager.Instance.CurrentPlayerRoom;
        var currentRoomText = GetRoomText(currentRoom);
        var exits = currentRoom.Exits.Values.ToArray();
        var exitsText = string.Join(", ", exits.Select(e => GetRoomText(e)).ToArray());
        MiniHUDText.text = string.Format(TextFormat, currentRoomText, exitsText);
        FairyPathWarning.gameObject.SetActive(exits.Any(e => e.Hazard == Hazard.Bats));
        CrowsTalonsWarning.gameObject.SetActive(exits.Any(e => e.Hazard == Hazard.Pit));
        var wumpusRoom = GameManager.Instance.CurrentWumpusRoom;
        WumpusWarning.gameObject.SetActive(exits.Any(e => e.Color == wumpusRoom.Color));
    }

    private string GetRoomText(ColonyRoom room)
    {
        var color = GameManager.Instance.GetRoomColor(room);
        return string.Format(RoomFormat,
            Mathf.FloorToInt(color.r * 255),
            Mathf.FloorToInt(color.g * 255),
            Mathf.FloorToInt(color.b * 255),
            Mathf.FloorToInt(color.a * 255),
            GameManager.Instance.GetRoomNickname(room).ToUpperInvariant());
    }
}
