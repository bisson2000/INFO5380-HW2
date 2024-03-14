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

    public List<GameObject> debug = new List<GameObject>();

    private MeshFilter _meshFilter;
    private Mesh _mesh;
    public Mesh CurrentMesh => _mesh;
    
    // Start is called before the first frame update
    void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter.mesh == null)
        {
            _mesh = new Mesh();
            _mesh.name = "WireRenderer";
            _meshFilter.mesh = _mesh;
        }
        else
        {
            _mesh = _meshFilter.mesh;
        }
    }

    // Update is called once per frame
    void Update()
    {
        List<Vector3> newVertices = new List<Vector3>();
        //newVertices.Add(new Vector3(0, 0, 0));
        //newVertices.Add(new Vector3(0, 1, 0));
        //newVertices.Add(new Vector3(1, 1, 0));
        
        List<int> newTris = new List<int>();
        //newTris.Add(0);
        //newTris.Add(1);
        //newTris.Add(2);
        
        List<Vector2> newUVs = new List<Vector2>();
        //newUVs.Add(new Vector2(0, 0));
        //newUVs.Add(new Vector2(0, 1));
        //newUVs.Add(new Vector2(1, 1));

        foreach (var debugo in debug)
        {
            GameObject.DestroyImmediate(debugo);
        }
        debug.Clear();

        int nSegments = 6;
        float width = 0.5f;
        Vector3 center = positions[0];
        Quaternion rotation = orientations[0];
        for (int j = 1; j <= nSegments; j++)
        {
            Vector3 newVertex;
            float newX = width * Mathf.Cos(j * 2 * Mathf.PI / nSegments);
            float newY = width * Mathf.Sin(j * 2 * Mathf.PI / nSegments);
            newVertex = center + rotation * (new Vector3(newX, newY, 0));

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = newVertex;
            go.transform.rotation = rotation;
            go.transform.localScale = Vector3.one * 0.25f;
            debug.Add(go);
        }


        _mesh.SetVertices(newVertices);
        _mesh.SetTriangles(newTris, 0);
        _mesh.SetUVs(0, newUVs);
    }

    private Mesh BuildMesh()
    {
        return _meshFilter.mesh;
    }

    private Vector3 GetRight(Vector3 point, Quaternion rotation)
    {
        Vector3 end = rotation * Vector3.right + point;
        return (end - point).normalized;
    }
    
    private Vector3 GetForward(Vector3 point, Quaternion rotation)
    {
        Vector3 end = rotation * Vector3.forward + point;
        return (end - point).normalized;
    }
    
    private Vector3 GetUp(Vector3 point, Quaternion rotation)
    {
        Vector3 end = rotation * Vector3.up + point;
        return (end - point).normalized;
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
        
        Vector3 start = positions[0];
        Quaternion startOrient = orientations[0];
        float scale = 1.0f;
        Handles.TransformHandle(ref start, ref startOrient, ref scale);
        
        positions[0] = start;
        orientations[0] = startOrient;
        
        Vector3 endOriginal = start + new Vector3(1.0f, 0.0f, 0.0f);
        Vector3 endX = startOrient * Vector3.right + start;
        Handles.DrawLine(start, endX, 5.0f);
        
        Vector3 endZ = (startOrient * Vector3.forward + start);
        Handles.DrawLine(start, endZ, 5.0f);
        
        Vector3 endY = (startOrient * Vector3.up + start);
        Handles.DrawLine(start, endY, 5.0f);
        
        // display object "value" in scene
        // GUI.color = Color.blue;
        // Handles.Label(pos, t.value.ToString("F1"));
    }
}

#endif
