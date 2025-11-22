using UnityEngine;

namespace Airships.Ship
{
    public class PlayerController : MonoBehaviour
    {
        public GameObject location;

        // Update is called once per frame
        void Start()
        {
            location = GameObject.Find(DataManager.Instance.playerLocationName);

            var destinationPos = location.transform.position;
            transform.position = destinationPos + new Vector3(0f, 1f, 0f);
        }

        public void moveLocation(GameObject destination)
        {
            if (destination == null) return;

            location = destination;
            DataManager.Instance.playerLocationName = destination.name;

            var destinationPos = destination.transform.position;
            transform.position = destinationPos + new Vector3(0f, 1f, 0f);
        }
    }
}
