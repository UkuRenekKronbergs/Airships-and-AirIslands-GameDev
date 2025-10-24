using UnityEngine;
using System.Collections.Generic;

public class AllPathsDottedLines : MonoBehaviour
{
    public PathController pathController;
    public float dashLength = 0.2f;
    public float lineWidth = 0.05f;
    public Material lineMaterial;

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();

    void Start()
    {
        DrawAllPaths();
    }

    void DrawAllPaths()
    {
        foreach (NodePair pair in pathController.pairs)
        {
            GameObject lineObj = new GameObject($"Line_{pair.a.name}_{pair.b.name}");
            lineObj.transform.parent = this.transform;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.useWorldSpace = true;
            lr.textureMode = LineTextureMode.Tile;

            List<Vector3> dashPoints = new List<Vector3>();
            Vector3 start = pair.a.transform.position;
            Vector3 end = pair.b.transform.position;
            Vector3 direction = (end - start).normalized;
            float distance = Vector3.Distance(start, end);
            bool draw = true;

            for (float k = 0; k < distance; k += dashLength)
            {
                if (draw)
                {
                    Vector3 segStart = start + direction * k;
                    Vector3 segEnd = start + direction * Mathf.Min(k + dashLength / 2f, distance);
                    dashPoints.Add(segStart);
                    dashPoints.Add(segEnd);
                }
                draw = !draw;
            }

            lr.positionCount = dashPoints.Count;
            lr.SetPositions(dashPoints.ToArray());
            lineRenderers.Add(lr);
        }
    }
}
