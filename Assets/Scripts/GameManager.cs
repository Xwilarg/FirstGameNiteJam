using System.Collections.Generic;
using UnityEngine;

namespace FirstGameNiteJam
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _tank;

        public static GameManager Instance { private set; get; }

        public readonly Dictionary<string, TankController> _controllers = new();

        private void Awake()
        {
            Instance = this;
        }

        public void HandleMessage(string id, string msg)
        {
            if (!_controllers.ContainsKey(id))
            {
                var go = Instantiate(_tank, Vector3.zero, Quaternion.identity);
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
                    default: Debug.LogError($"Unknown message {id}"); break;
                }
            }
        }
    }
}