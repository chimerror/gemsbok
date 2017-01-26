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
    public string[] ScutterNames = { "Tsisdu", "Di'la", "Tsisqua", "U'tlauh", "Clyde" };
    public string ScutterFormat = "SCUTTER \"{0}\" DEPLOYED. WAITING FOR HIT CONFIRMATION...";
    public string MissedFormat = "SCUTTER \"{0}\" MISSED!\n{1}";
    public string[] MissedMessages = { "Oh, our guest has toys...", "Don't worry, girl...", "They don't seem to have very good aim...", "I suggest you start using your brain...", "That's the last one, isn't it... Sic 'em, girl." };
    public string ArrowedFormat = "SCUTTER \"{0}\" IS HEADING RIGHT BACK TO {1} SECTOR!\nHoist by your own petard, eh?";
    public string WinningFormat = "SCUTTER \"{0}\" HIT IN {1} SECTOR!\nWUMPUS-CLASS ACTIVE ICE \"Cyllo\" TERMINATED!\n\nI guess it had to happen some day, girl.";

    private void Update()
    {
        var gameState = GameManager.Instance.CurrentGameState;
        if (gameState == GameState.Titles || gameState == GameState.MenuScreen || gameState == GameState.InitializingRoom || gameState == GameState.GameOver)
        {
            return;
        }

        var currentRoom = GameManager.Instance.CurrentPlayerRoom;
        var currentRoomText = GameManager.Instance.GetRoomText(currentRoom, false);
        var currentWumpusRoom = GameManager.Instance.CurrentWumpusRoom;
        var currentWumpusRoomText = GameManager.Instance.GetRoomText(currentWumpusRoom, false);
        var currentScutter = ScutterNames[GameManager.Instance.ShotsTaken % ScutterNames.Length];
        var currentMissedMesage = MissedMessages[GameManager.Instance.ShotsTaken % MissedMessages.Length];
        switch (gameState)
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
            case GameState.ScutterTargeting:
                var exits = currentRoom.Exits.Values.ToArray();
                var exitsText = string.Join(", ", exits.Select(e => GameManager.Instance.GetRoomText(e)).ToArray());
                MiniHUDText.text = string.Format(TextFormat, currentRoomText, exitsText);
                FairyPathWarning.gameObject.SetActive(exits.Any(e => e.Hazard == Hazard.FairyPath));
                CrowsTalonsWarning.gameObject.SetActive(exits.Any(e => e.Hazard == Hazard.CrowsTalons));
                var wumpusRoom = GameManager.Instance.CurrentWumpusRoom;
                WumpusWarning.gameObject.SetActive(exits.Any(e => e.Color == wumpusRoom.Color));
                break;

            case GameState.ScutterCutscene:
                MiniHUDText.text = string.Format(ScutterFormat, currentScutter);
                FairyPathWarning.gameObject.SetActive(false);
                CrowsTalonsWarning.gameObject.SetActive(false);
                WumpusWarning.gameObject.SetActive(false);
                break;

            case GameState.MissedCutscene:
                MiniHUDText.text = string.Format(MissedFormat, currentScutter, currentMissedMesage);
                FairyPathWarning.gameObject.SetActive(false);
                CrowsTalonsWarning.gameObject.SetActive(false);
                WumpusWarning.gameObject.SetActive(false);
                break;

            case GameState.ArrowedCutscene:
                MiniHUDText.text = string.Format(ArrowedFormat, currentScutter, currentRoomText);
                FairyPathWarning.gameObject.SetActive(false);
                CrowsTalonsWarning.gameObject.SetActive(false);
                WumpusWarning.gameObject.SetActive(false);
                break;

            case GameState.WinningCutscene:
                MiniHUDText.text = string.Format(WinningFormat, currentScutter, currentWumpusRoomText);
                FairyPathWarning.gameObject.SetActive(false);
                CrowsTalonsWarning.gameObject.SetActive(false);
                WumpusWarning.gameObject.SetActive(false);
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
