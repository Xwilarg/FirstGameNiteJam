using UnityEngine;

namespace FirstGameNiteJam.Player
{
    public class Bullet : MonoBehaviour
    {
        public void OnCollisionEnter(Collision collision)
        {
            Destroy(gameObject);
        }

        private void Update()
        {
            // TODO: DEBUG
            if (transform.position.y < 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
