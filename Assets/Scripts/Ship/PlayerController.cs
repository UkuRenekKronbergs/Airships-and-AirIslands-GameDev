using UnityEngine;

namespace Airships.Ship
{
    public class PlayerController : MonoBehaviour
    {
        public GameObject location;
        Transform transform;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            transform = gameObject.GetComponent<Transform>();
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void moveLocation(GameObject destination)
        {
            if (destination == null) return;

            location = destination;

            Vector3 destinationPos = destination.GetComponent<Transform>().position;
            transform.position = destinationPos + new Vector3(0, 1, 0);
        }
    }
}
