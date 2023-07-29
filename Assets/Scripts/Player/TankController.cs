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
            Vector2Int dir = new();
            if (Up) dir.y += 1;
            if (Down) dir.y -= 1;
            if (Left) dir.x += 1;
            if (Right) dir.x -= 1;

            _rb.velocity = (Vector2)dir * Time.fixedDeltaTime * 100f;
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
