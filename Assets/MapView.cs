using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class MapView : MonoBehaviour
{
    public Scrollbar Scrollbar;
    public RectTransform ViewportTransform;
    public RectTransform ContentTransform;
    public float ScrollbarDelta = 24.0f;
    public Text[] MapNodes = new Text[20];
    private Vector2? _lastTouchpadAxis;
    private uint _activeController;

    public void OnTouchpadTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        var mapView = GameManager.Instance.MapView.GetComponent<MapView>();
        if (mapView == null || !mapView.gameObject.activeInHierarchy)
        {
            return;
        }

        if (!mapView._lastTouchpadAxis.HasValue)
        {
            mapView._lastTouchpadAxis = e.touchpadAxis;
            mapView._activeController = e.controllerIndex;
        }
    }

    public void OnTouchPadTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        var mapView = GameManager.Instance.MapView.GetComponent<MapView>();
        if (mapView == null || !mapView.gameObject.activeInHierarchy)
        {
            return;
        }
        else if (mapView._activeController == e.controllerIndex)
        {
            var touchpadVerticalDelta = mapView._lastTouchpadAxis.Value.y - e.touchpadAxis.y;
            mapView.Scrollbar.value = Mathf.Clamp(mapView.Scrollbar.value +
                touchpadVerticalDelta / ScrollbarDelta, 0.0f, 1.0f);
        }

        mapView._lastTouchpadAxis = null;
    }


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
