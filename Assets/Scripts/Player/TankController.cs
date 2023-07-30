using System.Collections;
using System.Collections.Generic;
using TouhouPrideGameJam5.SO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FirstGameNiteJam
{
    public class TankController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _bulletPrefab;

        [SerializeField]
        private PlayerInfo _info;

        [SerializeField]
        private Transform _gunOut;

        [SerializeField]
        private GameObject _tankChaser, _tankRunner;

        [SerializeField]
        private GameObject _vCanvas;

        [SerializeField]
        private Transform _trophyHContainer;

        [SerializeField]
        private GameObject _trophy;

        [SerializeField]
        private Sprite _trophyImg, _trophyImgSpe;

        public string ClientId { set; get; }

        private Rigidbody _rb;
        private bool Down, Up, Right, Left;

        private bool _canDoAction = true;

        [SerializeField] private GameObject _decoy;
        private Transform _currentDecoy;
        private Rigidbody _rbDecoy;

        public int Victories { private set; get; }

        public bool IsControllerPhone { set; get; }
        public bool Unarmed { set; get; }

        private Color _color;

        // I renamed this one because I didn't want to confuse it with _decoy 
        private readonly List<GameObject> _decoysList = new();

        private bool? _isAttacker;
        public bool? IsAttacker
        {
            set
            {
                _isAttacker = value;
                SetModel();
                if (ClientId != null)
                {
                    var cStr = $"{(int)(_color.r * 255f)};{(int)(_color.g * 255f)};{(int)(_color.b * 255f)}";
                    GameManager.Instance.SendMessageToClient(ClientId, $"ATT;{(IsAttacker.Value ? 1 : 0)};{cStr}");
                }
            }
            get => _isAttacker;
        }
        private int _health;

        public void Disable()
        {
            _tankChaser.SetActive(false);
            _tankRunner.SetActive(false);
        }

        public void SetModel()
        {
            Disable();
            if (IsAttacker.Value)
            {
                _tankChaser.SetActive(true);
            }
            else
            {
                _tankRunner.SetActive(true);
            }
            GetComponentInChildren<MeshRenderer>().material.SetColor("_Color", _color);
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _health = _info.BaseHealth;
            _color = new(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            GetComponentInChildren<Canvas>().worldCamera = Camera.main;
        }

        public void Win()
        {
            _vCanvas.SetActive(true);
            Sprite targetSprite;
            if (_trophyHContainer.childCount == 0)
            {
                targetSprite = _trophyImg;
            }
            else
            {
                targetSprite = Random.Range(0, 100) > 90 ? _trophyImgSpe : _trophyImg;
            }
            var t = Instantiate(_trophy, _trophyHContainer);
            t.GetComponent<Image>().sprite = targetSprite;
            Victories++;
        }

        public void ResetTank(bool removeWin)
        {
            _health = _info.BaseHealth;
            IsAttacker = false;

            // Remove all decoys
            foreach (var d in _decoysList)
            {
                Destroy(d);
            }
            _decoysList.Clear();
            if (removeWin)
            {
                _vCanvas.SetActive(false);
            }

            Up = false;
            Down = false;
            Left = false;
            Right = false;
            _rb.velocity = Vector3.zero;
            _canDoAction = true;

            if (ClientId != null)
            {
                GameManager.Instance.SendMessageToClient(ClientId, "RESET");
            }
        }

        private void Start()
        {
            GameManager.Instance.Register(this);
            IsAttacker = GameManager.Instance.IsAttacker;
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

            if(_currentDecoy != null)
            {
                _rbDecoy.velocity = _currentDecoy.forward * v.y * Time.fixedDeltaTime * _info.LinearSpeed;
                _currentDecoy.rotation = Quaternion.Euler(0f, _rbDecoy.rotation.eulerAngles.y - v.x * Time.fixedDeltaTime * _info.AngularSpeed, 0f);
            }
        }

        public void TakeDamage()
        {
            if (IsAttacker.HasValue && IsAttacker.Value) return;
            if (GameManager.Instance.DidWin && !GameManager.Instance.GameEnded) return;
            _health--;
            if (_health == 0)
            {
                GameManager.Instance.RemoveTank(this);
            }
        }

        public void GoForward(bool isPressed)
        {
            if (IsControllerPhone)
            {
                if (isPressed)
                {
                    Up = !Up;
                    if (Up) Down = false;
                }
            }
            else
            {
                Up = isPressed;
            }
        }

        public void GoBackward(bool isPressed)
        {
            if (IsControllerPhone)
            {
                if (isPressed)
                {
                    Down = !Down;
                    if (Down) Up = false;
                }
            }
            else
            {
                Down = isPressed;
            }
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
            if (Unarmed) return;
            if (_canDoAction && IsAttacker.HasValue)
            {
                if (IsAttacker.Value)
                {
                    var bullet = Instantiate(_bulletPrefab, _gunOut.position, Quaternion.identity);
                    bullet.GetComponent<Rigidbody>().AddForce(transform.forward * _info.BulletForce, ForceMode.Impulse);
                    Destroy(bullet, 5f);
                    StartCoroutine(Reload(_info.ShootReloadTime));
                }
                else
                {
                    _currentDecoy = Instantiate(_decoy, transform.position, transform.rotation, null).transform;
                    _currentDecoy.GetComponentInChildren<MeshRenderer>().material.SetColor("_Color", _color);
                    Destroy(_currentDecoy.gameObject, _info.DecoyLifetime);
                    _decoysList.Add(_currentDecoy.gameObject);
                    _rbDecoy = _currentDecoy.GetComponent<Rigidbody>();
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
