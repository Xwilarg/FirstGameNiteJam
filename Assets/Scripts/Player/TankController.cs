using System.Collections;
using System.Collections.Generic;
using TouhouPrideGameJam5.SO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FirstGameNiteJam
{
    public class TankController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _bulletPrefab;

        [SerializeField]
        private PlayerInfo _info;

        public string ClientId { set; get; }

        private Rigidbody _rb;
        private bool Down, Up, Right, Left;

        private bool _canDoAction = true;

        [SerializeField] private GameObject decoy;
        private Transform currentDecoy;
        private Rigidbody rbDecoy;

        private readonly List<GameObject> _decoys = new();

        public bool? IsAttacker { set; private get; } = null;
        private int _health;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _health = _info.BaseHealth;
        }

        public void ResetTank()
        {
            _health = _info.BaseHealth;
            IsAttacker = false;

            // Remove all decoys
            foreach (var d in _decoys)
            {
                Destroy(d);
            }
            _decoys.Clear();
        }

        private void Start()
        {
            GameManager.Instance.Register(this);
            IsAttacker = GameManager.Instance.IsAttacker;
            //GameManager.Instance.SendMessageToClient(ClientId, _isAttacker.Value ? "ATT1" : "ATT0");
        }

        private void FixedUpdate()
        {
            Vector2Int v = new();
            if (Up) v.y = 1;
            if (Down) v.y = -1;
            if (Right) v.x += 1;
            if (Left) v.x -= 1;

            _rb.velocity = transform.forward * v.y * Time.fixedDeltaTime * _info.LinearSpeed;
            transform.rotation = Quaternion.Euler(0f, _rb.rotation.eulerAngles.y + v.x * Time.fixedDeltaTime * _info.AngularSpeed, 0f);

            if(currentDecoy != null)
            {
                rbDecoy.velocity = currentDecoy.forward * v.y * Time.fixedDeltaTime * _info.LinearSpeed;
                currentDecoy.rotation = Quaternion.Euler(0f, rbDecoy.rotation.eulerAngles.y - v.x * Time.fixedDeltaTime * _info.AngularSpeed, 0f);
            }
        }

        public void TakeDamage()
        {
            if (GameManager.Instance.DidWin) return;
            _health--;
            if (_health == 0)
            {
                GameManager.Instance.RemoveTank(this);
            }
        }

        public void GoForward(bool isPressed)
        {
            Up = isPressed;
        }

        public void GoBackward(bool isPressed)
        {
            Down = isPressed;
        }

        public void GoLeft(bool isPressed)
        {
            Left = isPressed;
        }

        public void GoRight(bool isPressed)
        {
            Right = isPressed;
        }

        public void DoAction()
        {
            if (_canDoAction && IsAttacker.HasValue)
            {
                if (IsAttacker.Value)
                {
                    var bullet = Instantiate(_bulletPrefab, transform.position + transform.forward, Quaternion.identity);
                    bullet.GetComponent<Rigidbody>().AddForce(transform.forward * _info.BulletForce, ForceMode.Impulse);
                    Destroy(bullet, 5f);
                    StartCoroutine(Reload(_info.ShootReloadTime));
                }
                else
                {
                    currentDecoy = Instantiate(decoy, transform.position, transform.rotation, null).transform;
                    Destroy(currentDecoy.gameObject, _info.DecoyLifetime);
                    _decoys.Add(currentDecoy.gameObject);
                    rbDecoy = currentDecoy.GetComponent<Rigidbody>();
                    StartCoroutine(Reload(_info.SkillReloadTime));
                }
            }
        }

        private IEnumerator Reload(float reloadTime)
        {
            _canDoAction = false;
            yield return new WaitForSeconds(reloadTime);
            _canDoAction = true;
        }

        public void OnMove(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                var mov = value.ReadValue<Vector2>();
                GoForward(mov.y > .5f);
                GoBackward(mov.y < -.5f);
                GoLeft(mov.x < -.5f);
                GoRight(mov.x > .5f);
            }
            else if (value.phase == InputActionPhase.Canceled)
            {
                GoForward(false);
                GoBackward(false);
                GoLeft(false);
                GoRight(false);
            }
        }

        public void OnAction(InputAction.CallbackContext value)
        {
            if (value.performed)
            {
                DoAction();
            }
        }
    }
}
