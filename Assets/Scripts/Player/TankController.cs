using UnityEngine;

namespace FirstGameNiteJam
{
    public class TankController : MonoBehaviour
    {
        private Rigidbody _rb;
        private bool Down, Up, Right, Left;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            Vector2Int v = new();
            if (Up) v.y = 1;
            if (Down) v.y = -1;
            if (Right) v.x += 1;
            if (Left) v.x -= 1;

            _rb.velocity = transform.forward * v.y * Time.fixedDeltaTime * 100f;
            _rb.rotation = Quaternion.Euler(0f, _rb.rotation.eulerAngles.y + v.x * Time.fixedDeltaTime * 100f, 0f);
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
    }
}
