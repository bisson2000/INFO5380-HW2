using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
  
// Notes: renderer is finished
// Create another class called WireCreator, which will create the positions

// TODO: Only update when there is a change
// TODO: Cleanup
// TODO: Documentation
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class WireRenderer : MonoBehaviour
{
    [Tooltip("How many edges each circle will contain")]
    [Min(2)]
    [SerializeField]
    private int nSegments = 8;
    
    [Tooltip("The radius of the wire")]
    [Min(0.0f)]
    [SerializeField]
    private float radius = 0.5f;
    
    private MeshFilter _meshFilter;
    private Mesh _mesh;
    public Mesh CurrentMesh => _mesh;
    
    [Header("--------Debug Information--------")]
    public bool showDebugPoints = true;
    public List<Vector3> positions = new List<Vector3>() 
        { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 0, 2) };
    public List<Quaternion> orientations = new List<Quaternion>() 
        { Quaternion.identity, Quaternion.identity, Quaternion.identity };
    
    // Start is called before the first frame update
    public void Start()
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
    public void Update()
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTris = new List<int>();
        List<Vector2> newUVs = new List<Vector2>();
        List<Vector3> newNormals = new List<Vector3>();

        // Contour
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 center = positions[i];
            Quaternion rotation = orientations[i];
            
            for (int j = 1; j <= nSegments; j++)
            {
                int absoluteIndex = i * nSegments + (j - 1);
                
                // Vertex
                float newX = radius * Mathf.Cos(j * 2 * Mathf.PI / nSegments);
                float newY = radius * Mathf.Sin(j * 2 * Mathf.PI / nSegments);
                Vector3 newVertex = center + rotation * (new Vector3(newX, newY, 0));
                newVertices.Add(newVertex);

                // Normal
                Vector3 newVertexNormal = (newVertex - center).normalized;
                newNormals.Add(newVertexNormal);
                
                // UV
                Vector2 newVertexUV = new Vector2((float)j / nSegments, (float)i / (positions.Count - 1));//currentPositionIndex / positions.Count);
                newUVs.Add(newVertexUV);

                // Triangles
                // Every new vertex adds 2 triangles
                if (i < positions.Count - 1)
                {
                    // Triangle with 2 vertices on current segment
                    newTris.Add(absoluteIndex + 1); // next vertex on same segment
                    newTris.Add(absoluteIndex + nSegments); // vertex in front
                    newTris.Add(absoluteIndex); // current vertex
                
                    // Triangle with 2 vertices on opposite segment
                    newTris.Add(absoluteIndex + nSegments); // vertex in front
                    newTris.Add(absoluteIndex + nSegments - 1); // next vertex in front
                    newTris.Add(absoluteIndex); // current vertex
                }
            }
        }
        
        // sides
        // Start
        newVertices.Add(positions[0]);
        newNormals.Add(-1.0f * GetForward(positions[0], orientations[0]));
        newUVs.Add(new Vector2(0.5f, 0f));
        for (int i = 0; i < nSegments; i++)
        {
            newTris.Add((i + 1) % nSegments);
            newTris.Add(i);
            newTris.Add(newVertices.Count - 1); // Current Vertex
        }
        
        // End
        newVertices.Add(positions[positions.Count - 1]);
        newNormals.Add(GetForward(positions[positions.Count - 1], orientations[positions.Count - 1]));
        newUVs.Add(new Vector2(0.5f, 1.0f));
        int absoluteStartIndex = newVertices.Count - nSegments - 2; // -2 because we just added the start
        for (int i = 0; i < nSegments; i++)
        {
            newTris.Add(absoluteStartIndex + i);
            newTris.Add(absoluteStartIndex + (i + 1) % nSegments);
            newTris.Add(newVertices.Count - 1); // Current Vertex
        }

        _mesh.Clear();
        _mesh.SetVertices(newVertices);
        _mesh.SetTriangles(newTris, 0);
        _mesh.SetUVs(0, newUVs);
        _mesh.SetNormals(newNormals);
        _mesh.RecalculateBounds();
    }

    private Mesh BuildMesh()
    {
        return _meshFilter.mesh;
    }

    public (Vector3, Quaternion) GetLastPositionRotation()
    {
        return (positions[^1], orientations[^1]);
    }

    public int GetPositionsCount()
    {
        return positions.Count;
    }

    public void EraseRange(int start, int count)
    {
        positions.RemoveRange(start, count);
        orientations.RemoveRange(start, count);
    }
    
    public void AddPositionRotation(Vector3 position, Quaternion rotation)
    {
        positions.Add(position);
        orientations.Add(rotation);
    }

    public void MarkDirty()
    {
        
    }

    public static Vector3 GetRight(Vector3 point, Quaternion rotation)
    {
        Vector3 end = rotation * Vector3.right + point;
        return (end - point).normalized;
    }
    
    public static Vector3 GetForward(Vector3 point, Quaternion rotation)
    {
        Vector3 end = rotation * Vector3.forward + point;
        return (end - point).normalized;
    }
    
    public static Vector3 GetUp(Vector3 point, Quaternion rotation)
    {
        Vector3 end = rotation * Vector3.up + point;
        return (end - point).normalized;
    } 
    
#if UNITY_EDITOR
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
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        WireRenderer wireRenderer = target as WireRenderer;
        if (wireRenderer == null || wireRenderer.CurrentMesh == null)
        {
            return;
        }

        if (GUILayout.Button("Quick add point"))
        {
            Vector3 newPos = wireRenderer.positions[wireRenderer.positions.Count - 1];
            newPos.x += 0.5f;
            Quaternion newRot = wireRenderer.orientations[wireRenderer.orientations.Count - 1];
            wireRenderer.AddPositionRotation(newPos, newRot);
        }
        
        if (GUILayout.Button("Quick delete last point"))
        {
            wireRenderer.positions.RemoveAt(wireRenderer.positions.Count - 1);
            wireRenderer.orientations.RemoveAt(wireRenderer.orientations.Count - 1);
        }
    }
    
    
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
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 position = positions[i];
            Quaternion rotation = orientations[i];
            Vector3 scale = Vector3.one * 0.5f;
            Handles.TransformHandle(ref position, ref rotation, ref scale);
            positions[i] = position;
            orientations[i] = rotation;
        }
        
        // Vector3 start = positions[0];
        // Quaternion startOrient = orientations[0];
        // float scale = 1.0f;
        // Handles.TransformHandle(ref start, ref startOrient, ref scale);
        // 
        // positions[0] = start;
        // orientations[0] = startOrient;
        // 
        // Vector3 endOriginal = start + new Vector3(1.0f, 0.0f, 0.0f);
        // Vector3 endX = startOrient * Vector3.right + start;
        // Handles.DrawLine(start, endX, 5.0f);
        // 
        // Vector3 endZ = (startOrient * Vector3.forward + start);
        // Handles.DrawLine(start, endZ, 5.0f);
        // 
        // Vector3 endY = (startOrient * Vector3.up + start);
        // Handles.DrawLine(start, endY, 5.0f);
        
        // display object "value" in scene
        // GUI.color = Color.blue;
        // Handles.Label(pos, t.value.ToString("F1"));
    }
}

#endif
