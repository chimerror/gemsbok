using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MiniHUD : MonoBehaviour
{
    public string TextFormat = "Currently in {0} sector\nExits to: {1}\nHazards:";
    public Text MiniHUDText;
    public string FairyPathText = "INTRUDER DETECTED.\nPASSIVE ICE \"Fairy Path\" ENGAGED.\nINTRUDER TO BE REMOVED FROM SECTOR.\nSAFETY IS NOT GUARANTEED.";
    public Image FairyPathWarning;
    public string CrowsTalonsText = "INTRUDER DETECTED.\nACTIVE ICE \"Crows' Talons\" DEPLOYED.\nINTRUDER WILL BE TERMINATED.";
    public Image CrowsTalonsWarning;
    public string WumpusText = "INTRUDER DETECTED.\nWUMPUS-CLASS ACTIVE ICE \"Cyllo\" ALERTED.\nINTRUDER WILL BE TERMINATED.\nGood girl, Cyllo.";
    public Image WumpusWarning;
    public string WumpusMovementText = "WARNING: WUMPUS-CLASS ACTIVE ICE \"Cyllo\" RE-DEPLOYED.\nNot yet, girl...";

    private void Update()
    {
        switch (GameManager.Instance.CurrentGameState)
        {
            case GameState.FairyPathCutscene:
                MiniHUDText.text = FairyPathText;
                FairyPathWarning.gameObject.SetActive(true);
                CrowsTalonsWarning.gameObject.SetActive(false);
                WumpusWarning.gameObject.SetActive(false);
                break;

            case GameState.CrowsTalonsCutscene:
                MiniHUDText.text = CrowsTalonsText;
                FairyPathWarning.gameObject.SetActive(false);
                CrowsTalonsWarning.gameObject.SetActive(true);
                WumpusWarning.gameObject.SetActive(false);
                break;

            case GameState.WumpusCutscene:
                MiniHUDText.text = WumpusText;
                FairyPathWarning.gameObject.SetActive(false);
                CrowsTalonsWarning.gameObject.SetActive(false);
                WumpusWarning.gameObject.SetActive(true);
                break;

            case GameState.WumpusMovementMessage:
                MiniHUDText.text = WumpusMovementText;
                FairyPathWarning.gameObject.SetActive(false);
                CrowsTalonsWarning.gameObject.SetActive(false);
                WumpusWarning.gameObject.SetActive(true);
                break;

            case GameState.WaitingForPlayer:
                var currentRoom = GameManager.Instance.CurrentPlayerRoom;
                var currentRoomText = GameManager.Instance.GetRoomText(currentRoom);
                var exits = currentRoom.Exits.Values.ToArray();
                var exitsText = string.Join(", ", exits.Select(e => GameManager.Instance.GetRoomText(e)).ToArray());
                MiniHUDText.text = string.Format(TextFormat, currentRoomText, exitsText);
                FairyPathWarning.gameObject.SetActive(exits.Any(e => e.Hazard == Hazard.FairyPath));
                CrowsTalonsWarning.gameObject.SetActive(exits.Any(e => e.Hazard == Hazard.CrowsTalons));
                var wumpusRoom = GameManager.Instance.CurrentWumpusRoom;
                WumpusWarning.gameObject.SetActive(exits.Any(e => e.Color == wumpusRoom.Color));
                break;

            default:
                MiniHUDText.text = "Please wait...";
                FairyPathWarning.gameObject.SetActive(false);
                CrowsTalonsWarning.gameObject.SetActive(false);
                WumpusWarning.gameObject.SetActive(false);
                break;
        }
    }
}
