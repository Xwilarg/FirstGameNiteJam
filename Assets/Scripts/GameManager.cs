using FirstGameNiteJam.Translation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TouhouPrideGameJam5.SO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace FirstGameNiteJam
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _tank;

        [SerializeField]
        private GameObject _preparationUI;

        [SerializeField]
        private TMP_Text _startingText;

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

        public bool DidWin { private set; get; } = true;
        public bool GameEnded { private set; get; }

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
                    _startingText.gameObject.SetActive(true);
                    StartCoroutine(WaitAndStartRound());
                }
                return false;
            }
        }

        private IEnumerator WaitAndStartRound()
        {
            for (int i = 0; i < _info.TimeBeforeRound; i++)
            {
                _startingText.text = $"{Translate.Instance.Tr("starting")} {_info.TimeBeforeRound - i}...";
                yield return new WaitForSeconds(1f);
            }
            _startingText.gameObject.SetActive(false);
            _timerVal = _info.RoundDuration;
            DidWin = false;

            // Randomize tank position and role
            var prevAttacker = _registeredTanks.FirstOrDefault(x => x.IsAttacker.HasValue && x.IsAttacker.Value).gameObject.GetInstanceID();
            foreach (var tc in _registeredTanks.Where(x => x.gameObject.GetInstanceID() != prevAttacker))
            {
                SetPosition(tc);
                tc.ResetTank(true);
                tc.enabled = true;
                tc.SetModel();
            }
            _registeredTanks[Random.Range(0, _registeredTanks.Count)].IsAttacker = true;
        }

        private IEnumerator PrepareBackToMenu()
        {
            GameEnded = true;
            _startingText.gameObject.SetActive(true);
            foreach (var tc in _registeredTanks)
            {
                SetPosition(tc);
                if (tc.Victories == _info.PointsBeforeVictory)
                {
                    tc.IsAttacker = true;
                    tc.ResetTank(false);
                }
                else
                {
                    tc.IsAttacker = false;
                    tc.Unarmed = true;
                    tc.ResetTank(true);
                }
            }

            for (int i = 0; i < _info.TimeBeforeRound; i++)
            {
                _startingText.text = $"{Translate.Instance.Tr("backToMenu")} {_info.TimeBeforeRound - i}...";
                yield return new WaitForSeconds(1f);
            }

            SceneManager.LoadScene("MainMenu");
        }

        private void Awake()
        {
            Instance = this;
            _net = GetComponent<controlpads_glue>();
        }

        private void Update()
        {
            if (_timerVal != null && !DidWin)
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
            tc.Disable();
            tc.enabled = false;
            if (_registeredTanks.Count(x => x.enabled) == 1)
            {
                EndGame(true);
            }
        }

        public void EndGame(bool didAttackerWin)
        {
            if (!DidWin)
            {
                if (didAttackerWin)
                {
                    _registeredTanks.FirstOrDefault(x => x.IsAttacker.HasValue && x.IsAttacker.Value).Win();
                }
                else
                {
                    foreach (var tc in _registeredTanks)
                    {
                        if (tc.IsAttacker.HasValue && !tc.IsAttacker.Value)
                        {
                            tc.Win();
                        }
                    }
                }
                if (_registeredTanks.Any(x => x.Victories == _info.PointsBeforeVictory))
                {
                    StartCoroutine(PrepareBackToMenu());
                }
                else
                {
                    StartCoroutine(ResetGame());
                }
            }
        }

        private IEnumerator ResetGame()
        {
            DidWin = true;
            _startingText.gameObject.SetActive(true);
            yield return WaitAndStartRound();

            // Put all tanks back to the game
            foreach (var tc in _registeredTanks)
            {
                tc.enabled = true;
                tc.SetModel();
            }

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
                var tc = go.GetComponent<TankController>();
                tc.ClientId = id;
                tc.IsControllerPhone = true;
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
                    case "{conn}": break;
                    default: Debug.LogError($"Unknown message {id}"); break;
                }
            }
        }
    }
}