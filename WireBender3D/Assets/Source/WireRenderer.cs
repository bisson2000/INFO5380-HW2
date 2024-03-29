using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif
  

// TODO: Cleanup
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class WireRenderer : MonoBehaviour
{
    
    [Tooltip("How many edges each circle will contain")]
    [Min(2)]
    [SerializeField]
    private int nEdgesInSegments = 8;
    public int NEdgesInSegments
    {
        get => nEdgesInSegments;
        set
        {
            nEdgesInSegments = value;
            MarkDirty(true);
        }
    }
    
    [Tooltip("The radius of the wire")]
    [Min(0.0f)]
    [SerializeField]
    private float radius = 0.5f;
    public float Radius
    {
        get => radius;
        set
        {
            radius = value;
            MarkDirty(true);
        }
    }
    
    // The modified mesh
    private MeshFilter _meshFilter;
    private Mesh _mesh;
    public Mesh CurrentMesh => _mesh;
    
    // Submesh information
    private int _submeshCount = 3;
    private List<SubmeshInfo> _submeshInfos = new List<SubmeshInfo>();
    
    // Mesh collider
    private MeshCollider _meshCollider;
    private bool _isMeshColliderSet = false;
    
    [Header("--------Debug Information--------")]
    public bool showDebugPoints = true;

    public IReadOnlyList<Vector3> Positions => positions.AsReadOnly();
    public IReadOnlyList<Quaternion> Rotations => orientations.AsReadOnly();
    
    [Tooltip("The center position of each segment in the wire")]
    [SerializeField]
    private List<Vector3> positions = new List<Vector3>();
    
    [Tooltip("The orientation of each segment in the wire. Forward is in the Z axis")]
    [SerializeField]
    private List<Quaternion> orientations = new List<Quaternion>();

    // callBack to mesh generated
    public event Action OnMeshGenerated;
    
    // List to hold the last created tris
    private List<int> _lastCreatedTris = new List<int>();

    // Flag to indicate to regenerate the mesh
    private bool _dirty = true;
    private bool _rebuildMesh = true;
    
    /// <summary>
    /// Start is called before the first frame update.
    /// At start, initialize the mesh.
    /// </summary>
    public void Start()
    {
        // set mesh;
        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter.sharedMesh == null)
        {
            _mesh = new Mesh();
            _mesh.name = "WireRenderer";
            _meshFilter.sharedMesh = _mesh;
        }
        else
        {
            _mesh = _meshFilter.sharedMesh;
        }

        // set submesh
        for (int i = 0; i < _submeshCount; i++)
        {
            _submeshInfos.Add(new SubmeshInfo());
        }
        
        // set Mesh Collider
        _meshCollider = GetComponent<MeshCollider>();
        _isMeshColliderSet = false;
        if (_meshCollider != null)
        {
            _meshCollider.sharedMesh = _mesh;
            _isMeshColliderSet = true;
        }
        
        // first build
        BuildMesh();
        MarkDirty(true);
    }

    
    /// <summary>
    /// Update is called once per frame.
    /// Update the mesh when it has changed
    /// </summary>
    public void Update()
    {
#if UNITY_EDITOR
        // Force update in unity editor, since variables can be changed without
        // any check.
        if (!Application.isPlaying)
        {
            _dirty = true;
        }
#endif
        // Only build if the mesh has changed
        if (_dirty)
        {
            BuildMesh();
            _dirty = false;
            if (_rebuildMesh)
            {
                _rebuildMesh = false;
                OnMeshGenerated?.Invoke();
            }
        }
    }

    /// <summary>
    /// Builds the mesh by assigning:
    /// vertices, tris, UVs, Normals.
    /// The mesh is built with the positions and orientations
    /// </summary>
    private void BuildMesh()
    {
        // Edge case
        if (positions.Count == 0)
        {
            _mesh.Clear();
            _mesh.subMeshCount = _submeshInfos.Count;
            _mesh.SetVertices(new List<Vector3>());
            _mesh.SetTriangles(new List<int>(), 0);
            _mesh.SetUVs(0, new List<Vector2>());
            _mesh.SetNormals(new List<Vector3>());
            _mesh.RecalculateBounds();
            return;
        }

        // Special flag for speed up
        if (!_rebuildMesh)
        {
            SetMeshTriangles(_lastCreatedTris);
            _mesh.RecalculateBounds();
            return;
        }
        else
        {
            // clear collisions
            // TODO: Test if we need to add _submeshInfos[1].affectedPoints.Clear(); as well
            _submeshInfos[2].affectedPoints.Clear();
        }
        
        GenerateMesh();
    }

    /// <summary>
    /// Generates the mesh
    /// </summary>
    private void GenerateMesh()
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
            
            // Note: The total number of vertices added is
            // equal to NEdgesInSegments + 1, for better looking UVs
            // The last vertex added is at the same position as the start vertex, but is not connected to it.
            for (int j = 0; j <= NEdgesInSegments; j++)
            {
                int absoluteIndex = i * (NEdgesInSegments + 1) + j;
                
                // Vertex
                float newX = Radius * Mathf.Cos(j * 2 * Mathf.PI / NEdgesInSegments);
                float newY = Radius * Mathf.Sin(j * 2 * Mathf.PI / NEdgesInSegments);
                Vector3 newVertex = center + rotation * (new Vector3(newX, newY, 0));
                newVertices.Add(newVertex);

                // Normal
                Vector3 newVertexNormal = (newVertex - center).normalized;
                newNormals.Add(newVertexNormal);
                
                // UV
                Vector2 newVertexUV = new Vector2((float)j / NEdgesInSegments, (float)i / (positions.Count - 1));
                newUVs.Add(newVertexUV);

                // Triangles
                // Every new vertex adds 2 triangles
                if (i < positions.Count - 1)
                {
                    if (j < NEdgesInSegments)
                    {
                        // Triangle with 2 vertices on current segment
                        newTris.Add(absoluteIndex + 1); // next vertex on same segment
                        newTris.Add(absoluteIndex + NEdgesInSegments + 1); // vertex in front
                        newTris.Add(absoluteIndex); // current vertex
                    }

                    if (j > 0)
                    {
                        // Triangle with 2 vertices on opposite segment
                        newTris.Add(absoluteIndex + NEdgesInSegments + 1); // vertex in front
                        newTris.Add(absoluteIndex + NEdgesInSegments - 1 + 1); // next vertex in front
                        newTris.Add(absoluteIndex); // current vertex
                    }
                }
            }
        }
        
        // sides
        // Start
        newVertices.Add(positions[0]);
        newNormals.Add(-1.0f * GetForward(positions[0], orientations[0]));
        newUVs.Add(new Vector2(0.5f, 0f));
        for (int i = 0; i < NEdgesInSegments; i++)
        {
            newTris.Add(i + 1);
            newTris.Add(i);
            newTris.Add(newVertices.Count - 1); // Current Vertex
        }
        
        // End
        newVertices.Add(positions[^1]);
        newNormals.Add(GetForward(positions[^1], orientations[^1]));
        newUVs.Add(new Vector2(0.5f, 1.0f));
        // We start at the first vertex of the last segement
        // it is located at newVertices.Count - NEdgesInSegments - 2
        // But we add a -1 because we just added the start
        int absoluteStartIndex = newVertices.Count - NEdgesInSegments - 3;
        for (int i = 0; i < NEdgesInSegments; i++)
        {
            newTris.Add(absoluteStartIndex + i);
            newTris.Add(absoluteStartIndex + i + 1);
            newTris.Add(newVertices.Count - 1); // Current Vertex
        }
        

        // Set the new mesh
        _mesh.Clear();
        _mesh.SetVertices(newVertices);
        SetMeshTriangles(newTris);
        _mesh.SetUVs(0, newUVs);
        _mesh.SetNormals(newNormals);
        _mesh.RecalculateBounds();
        
        // Set the mesh collider
        if (_isMeshColliderSet)
        {
            _meshCollider.sharedMesh = null;
            _meshCollider.sharedMesh = _mesh;
        }
    }

    /// <summary>
    /// Set the triangles for each mesh
    /// </summary>
    /// <param name="newTris"></param>
    private void SetMeshTriangles(List<int> newTris)
    {
        _lastCreatedTris = new List<int>(newTris);
        _mesh.subMeshCount = _submeshInfos.Count;

        List<List<int>> submeshesLists = new List<List<int>>();
        for (int i = 0; i < _submeshInfos.Count; ++i)
        {
            submeshesLists.Add(new List<int>());
        }
        
        int triCount = 6 * nEdgesInSegments;
        for (int i = 0; i < Positions.Count - 1; i++)
        {
            int triIndexStart = i * triCount;
            
            // skip 0
            bool taken = false;
            for (int j = _submeshInfos.Count - 1; j > 0; j--)
            {
                if (_submeshInfos[j].affectedPoints.Contains(i))
                {
                    submeshesLists[j].AddRange(newTris.GetRange(triIndexStart, triCount));
                    taken = true;
                    break;
                }
            }
            
            // 0 accumulates everything not taken
            if (!taken)
            {
                submeshesLists[0].AddRange(newTris.GetRange(triIndexStart, triCount));
            }
        }
        
        // edges
        int triCountEdge = 3 * nEdgesInSegments;
        submeshesLists[0].AddRange(newTris.GetRange(newTris.Count - 2 * triCountEdge, 2 * triCountEdge));
        
        for (int i = 0; i < submeshesLists.Count; ++i)
        {
            _mesh.SetTriangles(submeshesLists[i], i);
        }
    }

    /// <summary>
    /// Sets the submesh at a specific index
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="count"></param>
    /// <param name="submeshIndex"></param>
    public void SetSubmesh(int startPoint, int count, int submeshIndex)
    {
        // int triIndexStart = startPoint * 6 * nEdgesInSegments;
        // int triCount = count * 6 * nEdgesInSegments;
        
        if (startPoint < 0)
        {
            _submeshInfos[submeshIndex].affectedPoints.Clear();
        }
        else
        {
            _submeshInfos[submeshIndex].affectedPoints = Enumerable.Range(startPoint, count).ToHashSet();
        }
        
        MarkDirty(false);
    }
    
    public void SetSubmesh(HashSet<int> affectedPoints, int submeshIndex)
    {
        _submeshInfos[submeshIndex].affectedPoints = new HashSet<int>(affectedPoints);
        MarkDirty(false);
    }

    /// <summary>
    /// Gets the position and rotation of the last point in the wire
    /// </summary>
    /// <returns>The last point's position and rotation</returns>
    public (Vector3, Quaternion) GetLastPositionRotation()
    {
        return (positions[^1], orientations[^1]);
    }
    
    /// <summary>
    /// Gets the position and rotation of a point at a specific index
    /// </summary>
    /// <param name="index">The index to get the information from</param>
    /// <returns>The point's position and rotation</returns>
    public (Vector3, Quaternion) GetPositionRotation(int index)
    {
        return (positions[index], orientations[index]);
    }

    /// <summary>
    /// Get the number of points in the wire
    /// </summary>
    /// <returns>The number of points in the wire</returns>
    public int GetPositionsCount()
    {
        return positions.Count;
    }

    /// <summary>
    /// Erase an arbitrary range of points
    /// </summary>
    /// <param name="start">The start index to erase (included)</param>
    /// <param name="count">The number of points to erase from the start index</param>
    public void EraseRange(int start, int count)
    {
        positions.RemoveRange(start, count);
        orientations.RemoveRange(start, count);
        MarkDirty(true);
    }

    /// <summary>
    /// Set the position and the rotation of a point at a specific index
    /// </summary>
    /// <param name="position">The new position</param>
    /// <param name="rotation">The new rotation</param>
    /// <param name="index">The index of the point</param>
    public void SetPositionRotation(Vector3 position, Quaternion rotation, int index)
    {
        positions[index] = position;
        orientations[index] = rotation.normalized;
        MarkDirty(true);
    }
    
    /// <summary>
    /// Adds a new point to the wire
    /// </summary>
    /// <param name="position">The position of the point</param>
    /// <param name="rotation">The rotation of the point</param>
    public void AddPositionRotation(Vector3 position, Quaternion rotation)
    {
        positions.Add(position);
        orientations.Add(rotation.normalized);
        MarkDirty(true);
    }
    
    /// <summary>
    /// Inserts a new point to the wire
    /// </summary>
    /// <param name="position">The position of the point</param>
    /// <param name="rotation">The rotation of the point</param>
    /// <param name="index">The index to insert to</param>
    public void InsertPositionRotation(Vector3 position, Quaternion rotation, int index)
    {
        positions.Insert(index, position);
        orientations.Insert(index, rotation.normalized);
        MarkDirty(true);
    }

    /// <summary>
    /// Mark the wire dirty.
    /// This will make the wire mesh rebuild itself
    /// Call this when the wire has seen changes
    /// </summary>
    /// <param name="rebuildMesh"></param>
    private void MarkDirty(bool rebuildMesh)
    {
        _dirty = true;
        _rebuildMesh = _rebuildMesh || rebuildMesh;
    }

    /// <summary>
    /// Get the right vector of a point based on its rotation
    /// </summary>
    /// <param name="point">The current point</param>
    /// <param name="rotation">The current point's rotation</param>
    /// <returns>Normalized right vector</returns>
    public static Vector3 GetRight(Vector3 point, Quaternion rotation)
    {
        Vector3 end = rotation * Vector3.right + point;
        return (end - point).normalized;
    }
    
    /// <summary>
    /// Get the forward vector of a point based on its rotation
    /// </summary>
    /// <param name="point">The current point</param>
    /// <param name="rotation">The current point's rotation</param>
    /// <returns>Normalized forward vector</returns>
    public static Vector3 GetForward(Vector3 point, Quaternion rotation)
    {
        Vector3 end = rotation * Vector3.forward + point;
        return (end - point).normalized;
    }
    
    /// <summary>
    /// Get the up vector of a point based on its rotation
    /// </summary>
    /// <param name="point">The current point</param>
    /// <param name="rotation">The current point's rotaztion</param>
    /// <returns>Normalized up vector</returns>
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
    
    [Serializable]
    private class SubmeshInfo
    {
        public HashSet<int> affectedPoints;

        public SubmeshInfo()
        {
            affectedPoints = new HashSet<int>();
        }
    }
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
            Vector3 newPos = Vector3.zero;
            Quaternion newRot = Quaternion.identity;
            if (wireRenderer.GetPositionsCount() > 0)
            {
                (newPos, newRot) = wireRenderer.GetLastPositionRotation();
                newPos.z += 1.0f;
            }
            wireRenderer.AddPositionRotation(newPos, newRot);
        }
        
        if (GUILayout.Button("Quick delete last point"))
        {
            int count = wireRenderer.GetPositionsCount();
            if (count == 0)
            {
                return;
            }
            wireRenderer.EraseRange(count - 1, 1);
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

        const int SPARSE_POINTS = 1;

        IReadOnlyList<Vector3> positions = wireRenderer.Positions;
        IReadOnlyList<Quaternion> orientations = wireRenderer.Rotations;
        
        
        Vector3 scale = Vector3.one * 0.5f;
        for (int i = 0; i < positions.Count; i += SPARSE_POINTS)
        {
            
            Vector3 position = positions[i];
            Quaternion rotation = orientations[i];
            
            Handles.TransformHandle(ref position, ref rotation, ref scale);
            
            wireRenderer.SetPositionRotation(position, rotation, i);
        }
    }
}

#endif
