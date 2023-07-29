using System.Collections.Generic;
using System.Linq;
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

        public static GameManager Instance { private set; get; }

        private controlpads_glue _net;

        private readonly Dictionary<string, TankController> _controllers = new();
        private List<TankController> _registeredTanks = new();

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
                }
                return false;
            }
        }

        private void Awake()
        {
            Instance = this;
            _net = GetComponent<controlpads_glue>();
        }

        public void SendMessageToClient(string client, string msg)
        {
            _net.SendMessage(client, msg);
        }

        public void Register(TankController tc)
        {
            var furthestPos = _registeredTanks.Any()
                ? _spawnPoints.OrderByDescending(x => _registeredTanks.Min(t => Vector3.Distance(x.position, t.transform.position))).ElementAt(0)
                : _spawnPoints[Random.Range(0, _spawnPoints.Length)];
            tc.transform.position = furthestPos.position;
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