﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using VRTK;

public class ExitDoor : MonoBehaviour
{

    public string RoomNickname = "Roger Wilco"; // Test Value
    public Color DoorColor;
    public MeshRenderer DoorMesh;
    public VRTK_ObjectTooltip Tooltip;
    public int TooltipFontSize = 36;
    public Vector2 TooltipContainerSize = new Vector2(500f, 50f);
    public float NonVrMultiplier = 2.0f;

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

    protected void OnEnable()
    {
        DoorMesh.material.color = DoorColor;
        Tooltip.containerColor = DoorColor; // TODO: Will need to make sure this is readable
        Tooltip.fontSize = TooltipFontSize;
        Tooltip.containerSize = TooltipContainerSize;
        if (!VRSettings.enabled || !VRDevice.isPresent)
        {
            Tooltip.fontSize *= (int)NonVrMultiplier;
            Tooltip.containerSize *= NonVrMultiplier;
        }
        Tooltip.UpdateText("Door to Sector " + RoomNickname);
    }
}
