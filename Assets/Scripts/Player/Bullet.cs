using UnityEngine;

namespace FirstGameNiteJam.Player
{
    public class Bullet : MonoBehaviour
    {
        public void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                collision.collider.GetComponent<TankController>().TakeDamage();
            }
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
