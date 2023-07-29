using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TouhouPrideGameJam5.SO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FirstGameNiteJam
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _tank;

        [SerializeField]
        private GameObject _preparationUI;

        [SerializeField]
        private Transform[] _spawnPoints;

        [SerializeField]
        private TMP_Text _timer;

        [SerializeField]
        private GameInfo _info;

        private float? _timerVal;

        public static GameManager Instance { private set; get; }

        private controlpads_glue _net;

        private readonly Dictionary<string, TankController> _controllers = new();
        private List<TankController> _registeredTanks = new();

        private bool _didWin;

        private int _playerJoined;
        private bool _isAttacker = true;
        public bool IsAttacker
        {
            get
            {
                _playerJoined++;
                if (_isAttacker)
                {
                    _isAttacker = false;
                    return true;
                }
                if (_playerJoined == 2)
                {
                    _preparationUI.SetActive(false);
                    _timerVal = _info.RoundDuration;
                }
                return false;
            }
        }

        private void Awake()
        {
            Instance = this;
            _net = GetComponent<controlpads_glue>();
        }

        private void Update()
        {
            if (_timerVal != null && !_didWin)
            {
                _timerVal -= Time.deltaTime;
                if (_timerVal <= 0)
                {
                    _timerVal = 0;
                    EndGame(false);
                }
                _timer.text = $"{(int)_timerVal.Value}";
            }
        }

        public void RemoveTank(TankController tc)
        {
            tc.GetComponent<MeshRenderer>().enabled = false;
            tc.enabled = false;
            if (_registeredTanks.Count(x => x.enabled) == 1)
            {
                EndGame(true);
            }
        }

        public void EndGame(bool didAttackerWin)
        {
            if (!_didWin)
            {
                StartCoroutine(ResetGame());
            }
        }

        private IEnumerator ResetGame()
        {
            _didWin = true;
            yield return new WaitForSeconds(2f);

            foreach (var tc in _registeredTanks)
            {
                tc.GetComponent<MeshRenderer>().enabled = true;
                tc.enabled = true;
                SetPosition(tc);
                tc.ResetTank();
            }
            _registeredTanks[Random.Range(0, _registeredTanks.Count)].IsAttacker = true;
            _timerVal = _info.RoundDuration;

            _didWin = false;
        }

        public void SendMessageToClient(string client, string msg)
        {
            _net.SendMessageToClient(client, msg);
        }

        private void SetPosition(TankController tc)
        {
            var furthestPos = _registeredTanks.Any()
                ? _spawnPoints.OrderByDescending(x => _registeredTanks.Min(t => Vector3.Distance(x.position, t.transform.position))).ElementAt(0)
                : _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            tc.transform.position = furthestPos.position;
        }

        public void Register(TankController tc)
        {
            SetPosition(tc);
            _registeredTanks.Add(tc);
        }

        public void HandleMessage(string id, string msg)
        {
            if (!_controllers.ContainsKey(id))
            {
                var go = Instantiate(_tank, Vector3.zero, Quaternion.identity);
                go.GetComponent<PlayerInput>().enabled = false;
                go.GetComponent<TankController>().ClientId = id;
                _controllers.Add(id, go.GetComponent<TankController>());
            }

            var target = _controllers[id];
            var data = msg.Split(";");
            if (data.Length == 2)
            {
                bool isPressed = data[1] == "1";
                switch (data[0])
                {
                    case "{up}": target.GoForward(isPressed); break;
                    case "{down}": target.GoBackward(isPressed); break;
                    case "{left}": target.GoLeft(isPressed); break;
                    case "{right}": target.GoRight(isPressed); break;
                    case "{action}": if (isPressed) target.DoAction(); break;
                    default: Debug.LogError($"Unknown message {id}"); break;
                }
            }
        }
    }
}