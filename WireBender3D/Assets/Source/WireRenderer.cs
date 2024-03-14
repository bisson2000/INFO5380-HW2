using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
  


[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class WireRenderer : MonoBehaviour
{
    public bool showDebugPoints = true;
    // Use list of transforms instead
    public List<Vector3> positions = new List<Vector3>() 
        { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, 2) };
    public List<Quaternion> orientations = new List<Quaternion>() 
        { Quaternion.identity, Quaternion.identity, Quaternion.identity };
    
    
    private MeshFilter _meshFilter;
    private Mesh _mesh;
    public Mesh CurrentMesh => _mesh;
    
    // Start is called before the first frame update
    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        if (_mesh == null) _mesh = new Mesh();
        _mesh.name = "WireRenderer";
        _meshFilter.mesh = _mesh;
        
        Debug.Log("Awoke");
    }

    // Update is called once per frame
    void Update()
    {
        List<Vector3> newVertices = new List<Vector3>();
        newVertices.Add(new Vector3(0, 0, 0));
        newVertices.Add(new Vector3(0, 1, 0));
        newVertices.Add(new Vector3(1, 1, 0));
        
        List<int> newTris = new List<int>();
        newTris.Add(0);
        newTris.Add(1);
        newTris.Add(2);
        
        List<Vector2> newUVs = new List<Vector2>();
        newUVs.Add(new Vector2(0, 0));
        newUVs.Add(new Vector2(0, 1));
        newUVs.Add(new Vector2(1, 1));


        _mesh.SetVertices(newVertices);
        _mesh.SetTriangles(newTris, 0);
        _mesh.SetUVs(0, newUVs);
    }

    private Mesh BuildMesh()
    {
        return _meshFilter.mesh;
    }

    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_mesh == null)
        {
            return;
        }

        Color previousGizmosColor = Gizmos.color;
        Gizmos.color = Color.blue;
        
        // Draw
        
        Gizmos.color = previousGizmosColor;
    }
    
    public void OnGUI()
    {
        showDebugPoints = GUILayout.Toggle(showDebugPoints, "Show debug points");
    }
#endif
    
}

#if UNITY_EDITOR

[CustomEditor(typeof(WireRenderer))]
public class WireRendererEditor : Editor
{
    // Custom in-scene UI
    public void OnSceneGUI()
    {
        WireRenderer wireRenderer = target as WireRenderer;
        
        if (wireRenderer == null || wireRenderer.CurrentMesh == null || !wireRenderer.showDebugPoints)
        {
            return;
        }

        List<Vector3> positions = wireRenderer.positions;
        List<Quaternion> orientations = wireRenderer.orientations;
        for (int i = 1; i < positions.Count; i++)
        {
            positions[i] = Handles.PositionHandle(positions[i], orientations[i]);
        }

        // Vector3 start = positions[0];
        // Quaternion startOrient = Quaternion.Euler(35,35,10);
        // Handles.PositionHandle(start, startOrient);
        // 
        // Vector3 endOriginal = start + new Vector3(0.5f,0.0f,0.0f);
        // Vector3 endX = startOrient * (endOriginal - start) + start;
        // Handles.DrawLine(start, endX, 5.0f);
        // 
        // Vector3 endZ = startOrient * Quaternion.Euler(0, -90, 0) * (endOriginal - start) + start;
        // Handles.DrawLine(start, endZ, 5.0f);
        // 
        // Vector3 endY = startOrient * Quaternion.Euler(0, 0, 90) * (endOriginal - start) + start;
        // Handles.DrawLine(start, endY, 5.0f);
        
        
        // display object "value" in scene
        // GUI.color = Color.blue;
        // Handles.Label(pos, t.value.ToString("F1"));
    }
}

#endif
