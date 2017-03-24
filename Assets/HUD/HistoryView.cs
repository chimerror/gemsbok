using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class HistoryView : MonoBehaviour
{
    public Scrollbar Scrollbar;
    public RectTransform ViewportTransform;
    private RectTransform _contentTransform;
    private Text _historyList;
    private int _lastHistoryCount = 0;
    private string _movementEntryFormat = @"{0,-16}{1,-16}{2,-16}{3,-32}";
    private string _scutterEntryFormat = @"{0,-16}{1}";
    private Vector2? _lastTouchpadAxis;
    private uint _activeController;

    public void OnTouchpadTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        var historyView = GameManager.Instance.HistoryView.GetComponentInChildren<HistoryView>();
        if (historyView == null || !historyView.gameObject.activeInHierarchy)
        {
            return;
        }

        if (!historyView._lastTouchpadAxis.HasValue)
        {
            historyView._lastTouchpadAxis = e.touchpadAxis;
            historyView._activeController = e.controllerIndex;
        }
    }

    public void OnTouchPadTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        var historyView = GameManager.Instance.HistoryView.GetComponentInChildren<HistoryView>();
        if (historyView == null || !historyView.gameObject.activeInHierarchy)
        {
            return;
        }
        else if (historyView._activeController == e.controllerIndex)
        {
            var touchpadVerticalDelta = historyView._lastTouchpadAxis.Value.y - e.touchpadAxis.y;
            var hiddenEntries = (historyView._contentTransform.sizeDelta.y - historyView.ViewportTransform.sizeDelta.y) / historyView._historyList.fontSize;
            if (hiddenEntries > 0)
            {
                historyView.Scrollbar.value = Mathf.Clamp(historyView.Scrollbar.value - touchpadVerticalDelta / hiddenEntries, 0.0f, 1.0f);
            }
        }

        historyView._lastTouchpadAxis = null;
    }

    private void Start()
    {
        _contentTransform = GetComponent<RectTransform>();
        _historyList = GetComponent<Text>();
    }

    private void Update()
    {
        var historyEntries = GameManager.Instance.History.Reverse<HistoryEntry>().ToList();
        if (_lastHistoryCount != historyEntries.Count)
        {
            Scrollbar.value = 1f; // Move Scrollbar back to top
            var builder = new StringBuilder();
            _contentTransform.sizeDelta = new Vector2(_contentTransform.sizeDelta.x, _historyList.fontSize * historyEntries.Count);
            foreach (var entry in historyEntries)
            {
                if (entry is MovementEntry)
                {
                    AddMovementEntry(builder, (MovementEntry)entry);
                }
                else if (entry is ScutterEntry)
                {
                    AddScutterEntry(builder, (ScutterEntry)entry);
                }
                else
                {
                    Debug.LogAssertionFormat("Unknown History Entry type found: {0}", entry.GetType());
                }
            }
            _historyList.text = builder.ToString();
            _lastHistoryCount = historyEntries.Count;
        }

        var hiddenEntries = (_contentTransform.sizeDelta.y - ViewportTransform.sizeDelta.y) / _historyList.fontSize;
        if (hiddenEntries > 0)
        {
            Scrollbar.value = Scrollbar.value + Input.GetAxis("ScrollHistory") / hiddenEntries;
        }
    }

    private void AddMovementEntry(StringBuilder builder, MovementEntry entry)
    {
        var entryExits = entry.Room.Exits.Values;
        var entryExitsString = string.Join(", ",
            entryExits.Select(e => GameManager.Instance.GetOrCreateRoomNickname(e)[0].ToString()).ToArray());
        var hazardString = string.Empty;
        if (entry.Room.Hazard == Hazard.FairyPath)
        {
            hazardString += "---FAIRYPATHFAIRYPATHFAIRYPATH--";
        }
        else
        {
            if (entryExits.Any(e => e.Hazard == Hazard.FairyPath))
            {
                hazardString += "Fairy Path";
            }

            if (entryExits.Any(e => e.Hazard == Hazard.CrowsTalons))
            {
                hazardString += hazardString.Length == 0 ? string.Empty : ", ";
                hazardString += "Crows' Talons";
            }

            if (entry.WumpusNearby)
            {
                hazardString += hazardString.Length == 0 ? string.Empty : ", ";
                hazardString += "Wumpus";
            }
        }
        var entryTime = TimeSpan.FromSeconds(entry.Time);
        builder.AppendFormat(_movementEntryFormat,
            string.Format("H+{0:00}:{1:00}.{2:000}", entryTime.Minutes, entryTime.Seconds, entryTime.Milliseconds),
            GameManager.Instance.GetOrCreateRoomNickname(entry.Room).ToUpperInvariant(),
            entryExitsString,
            hazardString);
        builder.AppendLine();
    }

    private void AddScutterEntry(StringBuilder builder, ScutterEntry entry)
    {
        string pathString = string.Join("→", entry.ShotPath.Select(r => GameManager.Instance.GetRoomText(r, false)).ToArray());
        string hazardString = string.Format("Missed Shot #{0}:\n\t\t{1}→{2}",
            entry.ShotNumber,
            GameManager.Instance.GetRoomText(entry.Room, false),
            pathString);
        var entryTime = TimeSpan.FromSeconds(entry.Time);
        builder.AppendFormat(_scutterEntryFormat,
            string.Format("H+{0:00}:{1:00}.{2:000}", entryTime.Minutes, entryTime.Seconds, entryTime.Milliseconds),
            hazardString);
        builder.AppendLine();
    }
}
