using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ScutterTargeting : MonoBehaviour
{
    public int ScuttersLeft = 5;
    public float InputDelay = 0.250f;

    private bool _acceptInput = true;
    private int _currentNewRoom;
    private Text _scutterTargetingText;
    private List<ColonyRoom> _roomChoices;
    private List<ColonyRoom> _currentPath = new List<ColonyRoom>();

    private void Awake()
    {
        _scutterTargetingText = GetComponentInChildren<Text>();
    }

    private void OnEnable()
    {
        _currentPath.Clear();
        _currentNewRoom = 0;
        UpdateRoomChoices();
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentGameState != GameState.ScutterTargeting)
        {
            return;
        }

        if (Input.GetButtonUp("Fire1") || Input.GetButtonUp("Fire4"))
        {
            ScuttersLeft--;
            _currentPath.Add(_roomChoices[_currentNewRoom]);
            GameManager.Instance.FireScutter(new List<ColonyRoom>(_currentPath)); // send a copy of the list
            gameObject.SetActive(false);
            return;
        }

        if (Input.GetButtonUp("Fire2") || Input.GetButtonUp("Cancel"))
        {
            GameManager.Instance.CancelScutter();
            gameObject.SetActive(false);
            return;
        }

        var stringBuilder = new StringBuilder();

        if (ScuttersLeft > 1)
        {
            stringBuilder.AppendFormat("{0} Scutters left.", ScuttersLeft);
        }
        else
        {
            stringBuilder.AppendFormat("ONLY 1 SCUTTER LEFT. We almost have them, Cyllo...");
        }
        stringBuilder.AppendLine();

        if (_acceptInput)
        {
            bool inputMade = false;
            if (Input.GetAxis("Horizontal") > 0 && _currentPath.Count < 4)
            {
                _currentPath.Add(_roomChoices[_currentNewRoom]);
                UpdateRoomChoices();
                _currentNewRoom = 0;
                inputMade = true;
            }

            if (Input.GetAxis("Horizontal") < 0 && _currentPath.Count > 0)
            {
                var lastRoom = _currentPath[_currentPath.Count - 1];
                _currentPath.RemoveAt(_currentPath.Count - 1);
                UpdateRoomChoices();
                _currentNewRoom = _roomChoices.IndexOf(lastRoom);
                inputMade = true;
            }

            if (Input.GetAxis("Vertical") > 0)
            {
                _currentNewRoom = (_currentNewRoom + 1) % _roomChoices.Count;
                inputMade = true;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                _currentNewRoom = _currentNewRoom == 0 ?
                    _roomChoices.Count - 1 :
                    _currentNewRoom - 1;
                inputMade = true;
            }

            if (inputMade)
            {
                StartCoroutine(BlockInput());
            }
        }

        if (_currentPath.Count > 0)
        {
            stringBuilder.Append(string.Join(" > ", _currentPath.Select(r => GameManager.Instance.GetRoomText(r, false)).ToArray()));
            stringBuilder.Append(" > ");
        }
        stringBuilder.Append(GameManager.Instance.GetRoomText(_roomChoices[_currentNewRoom], false));
        stringBuilder.AppendLine();

        stringBuilder.Append("↑/↓ change target, ←/→ move between targets, Fire1/Fire4 confirm, Fire2 cancel.");

        _scutterTargetingText.text = stringBuilder.ToString();
    }

    private void UpdateRoomChoices()
    {
        if (_currentPath.Count == 0)
        {
            _roomChoices = GameManager.Instance.CurrentPlayerRoom.Exits.Values
                .OrderBy(r => GameManager.Instance.GetOrCreateRoomNickname(r).ToLowerInvariant())
                .ToList();
        }
        else
        {
            // Note that we do NOT prevent circling back. The original one did this, but the
            // second one did not. since we want to eventually add more maps, I'm sticking with
            // the second implementation
            var namedRooms = GameManager.Instance.NamedRooms;
            var allRooms = GameManager.Instance.AllRooms;
            _roomChoices = allRooms
                .OrderBy(r => namedRooms.ContainsKey(r.Color) ? namedRooms[r.Color] : "~", StringComparer.OrdinalIgnoreCase) // Forces unknown to end
                .ToList();
        }
    }

    private IEnumerator BlockInput()
    {
        _acceptInput = false;
        yield return new WaitForSeconds(InputDelay);
        _acceptInput = true;
    }
}
