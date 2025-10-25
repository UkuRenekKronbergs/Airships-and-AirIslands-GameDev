using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct NodePair
{
    public GameObject a;
    public GameObject b;

    public int distance;
}

public class PathController : MonoBehaviour
{
    public List<NodePair> pairs = new List<NodePair>(); // assign in inspector
}
