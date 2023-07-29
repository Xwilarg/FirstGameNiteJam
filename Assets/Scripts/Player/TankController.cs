using System.Collections;
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

        private Rigidbody _rb;
        private bool Down, Up, Right, Left;

        private bool _canShoot = true;
        private const float _reloadTime = 1f;

        private bool _isAttacker;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _isAttacker = GameManager.Instance.IsAttacker;
        }

        private void FixedUpdate()
        {
            Vector2Int v = new();
            if (Up) v.y = 1;
            if (Down) v.y = -1;
            if (Right) v.x += 1;
            if (Left) v.x -= 1;

            _rb.velocity = transform.forward * v.y * Time.fixedDeltaTime * _info.LinearSpeed;
            _rb.rotation = Quaternion.Euler(0f, _rb.rotation.eulerAngles.y + v.x * Time.fixedDeltaTime * _info.AngularSpeed, 0f);
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
            if (_isAttacker)
            {
                if (_canShoot)
                {
                    var bullet = Instantiate(_bulletPrefab, transform.position + transform.forward, Quaternion.identity);
                    bullet.GetComponent<Rigidbody>().AddForce((transform.forward + Vector3.up).normalized * _info.BulletForce, ForceMode.Impulse);
                    Destroy(bullet, 5f);
                    StartCoroutine(Reload());
                }
            }
            else
            {
                // TODO: Decoy
            }
        }

        private IEnumerator Reload()
        {
            _canShoot = false;
            yield return new WaitForSeconds(_reloadTime);
            _canShoot = true;
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
