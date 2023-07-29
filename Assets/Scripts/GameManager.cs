using System.Collections.Generic;
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

        public static GameManager Instance { private set; get; }

        public readonly Dictionary<string, TankController> _controllers = new();

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
        }

        public void HandleMessage(string id, string msg)
        {
            if (!_controllers.ContainsKey(id))
            {
                var go = Instantiate(_tank, Vector3.zero, Quaternion.identity);
                go.GetComponent<PlayerInput>().enabled = false;
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