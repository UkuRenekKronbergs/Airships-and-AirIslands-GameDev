using UnityEngine;
using AirshipsAndAirIslands.Events;

namespace Airships.Ship
{
    public class PlayerController : MonoBehaviour
    {
        public GameObject location;

        void Start()
        {
            location = GameObject.Find(GameState.Instance.GetPlayerLocation());
            transform.position = location.transform.position + new Vector3(0f, 1f, 0f);
        }

        public void moveLocation(GameObject destination)
        {
            Debug.Log(destination);
            if (destination == null) return;
            
            location = destination;
            GameState.Instance.MovePlayerLocation(destination.name);

            var destinationPos = destination.transform.position;
            transform.position = destinationPos + new Vector3(0f, 1f, 0f);
        }
    }
}
