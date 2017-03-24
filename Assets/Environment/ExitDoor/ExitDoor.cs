using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;
using VRTK;

public class ExitDoor : MonoBehaviour
{
    private static ExitDoor CurrentHighlightedExitDoor = null;

    public string RoomNickname = "Roger Wilco"; // Test Value
    public Color DoorColor;
    public MeshRenderer DoorMesh;
    public VRTK_ObjectTooltip Tooltip;
    public int TooltipFontSize = 36;
    public Vector2 TooltipContainerSize = new Vector2(500f, 50f);

    public ColonyRoom Destination
    {
        get
        {
            return _destination;
        }

        set
        {
            if (value == null)
            {
                // Create a random room for testing purposes.
                _destination = new ColonyRoom((ushort)GameManager.Instance.Random.Next(360));
            }
            else
            {
                _destination = value;
            }
        }
    }

    public ExitDoor()
    {
        _destination = new ColonyRoom(177);
    }

    private ColonyRoom _destination;

    public void OnMarkerEnter(object sender, DestinationMarkerEventArgs e)
    {
        if (GameManager.Instance.CurrentGameState != GameState.WaitingForPlayer)
        {
            return;
        }

        var exitDoorHit = e.raycastHit.transform.GetComponentInParent<ExitDoor>();
        var travelIndicator = GameManager.Instance.TravelIndicator;
        if (exitDoorHit != null && CurrentHighlightedExitDoor == null && !travelIndicator.activeInHierarchy)
        {
            var text = travelIndicator.GetComponentInChildren<Text>();
            text.text = string.Format("Release touchpad to travel to {0} sector.", GameManager.Instance.GetRoomText(exitDoorHit._destination));
            travelIndicator.SetActive(true);
            CurrentHighlightedExitDoor = exitDoorHit;
        }
    }

    public void OnMarkerExit(object sender, DestinationMarkerEventArgs e)
    {
        if (GameManager.Instance.CurrentGameState != GameState.WaitingForPlayer)
        {
            return;
        }

        var exitDoorHit = e.raycastHit.transform.GetComponentInParent<ExitDoor>();
        var travelIndicator = GameManager.Instance.TravelIndicator;
        if (exitDoorHit != null && exitDoorHit == CurrentHighlightedExitDoor && travelIndicator.activeInHierarchy)
        {
            travelIndicator.SetActive(false);
            CurrentHighlightedExitDoor = null;
        }
    }

    public void OnMarkerSet(object sender, DestinationMarkerEventArgs e)
    {
        if (GameManager.Instance.CurrentGameState != GameState.WaitingForPlayer)
        {
            return;
        }

        var exitDoorHit = e.raycastHit.transform.GetComponentInParent<ExitDoor>();
        var travelIndicator = GameManager.Instance.TravelIndicator;
        if (exitDoorHit != null && exitDoorHit == CurrentHighlightedExitDoor)
        {
            travelIndicator.SetActive(false);
            CurrentHighlightedExitDoor = null;
            GameManager.Instance.MoveToRoom(exitDoorHit._destination, exitDoorHit.DoorColor);
        }
    }

    protected void OnEnable()
    {
        DoorMesh.material.color = DoorColor;
        Tooltip.containerColor = DoorColor; // TODO: Will need to make sure this is readable
        Tooltip.fontSize = TooltipFontSize;
        Tooltip.containerSize = TooltipContainerSize;
        Tooltip.UpdateText("Door to Sector " + RoomNickname);
    }

    protected void Update()
    {
        if (GameManager.Instance.CurrentGameState != GameState.WaitingForPlayer)
        {
            CurrentHighlightedExitDoor = null;
            GameManager.Instance.TravelIndicator.SetActive(false);
        }
    }
}
