﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class HistoryView : MonoBehaviour
{
    public Scrollbar Scrollbar;
    public RectTransform ViewportTransform;
    private RectTransform _contentTransform;
    private Text _historyList;
    private int _lastHistoryCount = 0;
    private string _viewEntryFormat = @"{0,-16}{1,-16}{2,-16}{3,-32}";

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
                var entryExits = entry.Room.Exits.Values;
                var entryExitsString = string.Join(", ",
                    entryExits.Select(e => GameManager.Instance.GetRoomNickname(e)[0].ToString()).ToArray());
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
                builder.AppendFormat(_viewEntryFormat,
                    string.Format("H+{0:00}:{1:00}.{2:000}", entryTime.Minutes, entryTime.Seconds, entryTime.Milliseconds),
                    GameManager.Instance.GetRoomNickname(entry.Room).ToUpperInvariant(),
                    entryExitsString,
                    hazardString);
                builder.AppendLine();
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
}
