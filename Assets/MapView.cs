using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapView : MonoBehaviour
{
    public Scrollbar Scrollbar;
    public RectTransform ViewportTransform;
    public RectTransform ContentTransform;
    public float ScrollbarDelta = 24.0f;
    public Text[] MapNodes = new Text[20];

    private void Update()
    {
        Scrollbar.value += Input.GetAxis("ScrollHistory") / ScrollbarDelta;

        var rooms = GameManager.Instance.AllRooms;
        for (int currentNode = 0; currentNode < MapNodes.Length; currentNode++)
        {
            MapNodes[currentNode].text = GameManager.Instance.GetRoomText(rooms[currentNode], false);
        }
    }
}
