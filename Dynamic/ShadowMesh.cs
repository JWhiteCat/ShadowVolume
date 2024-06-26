using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShadowVolume
{
    public static class VectorExtensions
    {
        public static bool MatchesExact(this Vector3 a, Vector3 b)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static Vector3 NormalizeExact(this Vector3 v)
        {
            var length = Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            if (length == 0.0f)
            {
                return Vector3.zero;
            }
            return v * (1.0f / length);
        }
    }

    public class ShadowMesh : ScriptableObject
    {
        protected struct Edge
        {
            public Vector3 a;
            public Vector3 b;
            public float cellSize;

            public Edge(Vector3 a, Vector3 b, float cellSize)
            {
                this.a = a;
                this.b = b;
                this.cellSize = cellSize;
            }

            public bool SameRobust(Edge other)
            {
                return a == other.a && b == other.b ||
                    a == other.b && b == other.a;
            }

            public bool Same(Edge other)
            {
                return a.MatchesExact(other.a) && b.MatchesExact(other.b) ||
                    a.MatchesExact(other.b) && b.MatchesExact(other.a);
            }

            public int CalculateHashCode()
            {
                var hashA = (int)(a.x / cellSize) * 73856093 ^
                            (int)(a.y / cellSize) * 19349663 ^
                            (int)(a.z / cellSize) * 83492791;
                var hashB = (int)(b.x / cellSize) * 73856093 ^
                            (int)(b.y / cellSize) * 19349663 ^
                            (int)(b.z / cellSize) * 83492791;
                int min, max;
                if (hashA < hashB)
                {
                    min = hashA;
                    max = hashB;
                }
                else
                {
                    min = hashB;
                    max = hashA;
                }
                return min ^ max;
            }
        }

        protected struct EdgeEqualityComparerRobust : IEqualityComparer<Edge>
        {
            public bool Equals(Edge x, Edge y)
            {
                return x.SameRobust(y);
            }

            public int GetHashCode(Edge obj)
            {
                return obj.CalculateHashCode();
            }
        }

        protected struct EdgeEqualityComparer : IEqualityComparer<Edge>
        {
            public bool Equals(Edge x, Edge y)
            {
                return x.Same(y);
            }

            public int GetHashCode(Edge obj)
            {
                return obj.CalculateHashCode();
            }
        }

        protected static void AddEdge(IDictionary<Edge, List<int>> edges, Edge edge, int triangleIndex)
        {
            if (!edges.ContainsKey(edge))
            {
                var triangles = new List<int>();
                triangles.Add(triangleIndex);
                edges.Add(edge, triangles);
            }
            else
            {
                var triangles = edges[edge];
                triangles.Add(triangleIndex);
            }
        }

        protected static int[] a = new int[3];
        protected static int[] b = new int[3];

        protected static bool NeighborSameWindingOrder(Vector3[] vertices, int[] indices, int triangleA, int triangleB)
        {
            a[0] = indices[triangleA * 3 + 0];
            a[1] = indices[triangleA * 3 + 1];
            a[2] = indices[triangleA * 3 + 2];

            b[0] = indices[triangleB * 3 + 0];
            b[1] = indices[triangleB * 3 + 1];
            b[2] = indices[triangleB * 3 + 2];

            for (var m = 0; m < 3; m++)
            {
                int a0 = a[m];
                int a1 = a[(m + 1) % 3];

                for (var n = 0; n < 3; n++)
                {
                    var b0 = b[n];
                    var b1 = b[(n + 1) % 3];

                    // Does edge m on triangle A match edge n on triangle B?
                    if (vertices[a0].MatchesExact(vertices[b1]) && vertices[a1].MatchesExact(vertices[b0]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected static void CreateDegenerateQuad(Vector3[] vertices, int[] indices, Vector3 vertexA, Vector3 vertexB, int triangleA, int triangleB, ICollection<int> outIndices)
        {
            a[0] = indices[triangleA * 3 + 0];
            a[1] = indices[triangleA * 3 + 1];
            a[2] = indices[triangleA * 3 + 2];

            b[0] = indices[triangleB * 3 + 0];
            b[1] = indices[triangleB * 3 + 1];
            b[2] = indices[triangleB * 3 + 2];

            for (var m = 0; m < 3; m++)
            {
                var a0 = a[m];
                var a1 = a[(m + 1) % 3];

                for (var n = 0; n < 3; n++)
                {
                    var b0 = b[n];
                    var b1 = b[(n + 1) % 3];

                    // Does edge m on triangle A match edge n on triangle B?
                    if (vertices[a0].MatchesExact(vertices[b1]) && vertices[a1].MatchesExact(vertices[b0]))
                    {
                        // Was this the sought after edge?
                        if (vertices[a0].MatchesExact(vertexA) && vertices[a1].MatchesExact(vertexB) ||
                            vertices[a0].MatchesExact(vertexB) && vertices[a1].MatchesExact(vertexA))
                        {
                            // Create a quad between the two edges
                            outIndices.Add(a0);
                            outIndices.Add(b1);
                            outIndices.Add(a1);

                            outIndices.Add(a1);
                            outIndices.Add(b1);
                            outIndices.Add(b0);

                            return;
                        }
                    }
                }
            }

            Debug.LogError("Could not create degenerate quad!");
        }

        public static bool Create(
            Mesh reference,
            float boundsPadFactor,
            ref Mesh result,
            out bool isAnimated,
            out bool isTwoManifold,
            out bool usesThirtyTwoBitIndices,
            out int vertexCount,
            out int triangleCount,
            out float outBoundsPadFactor)
        {
            if (!reference.isReadable)
            {
                Debug.LogError("Could not create shadow mesh for '" + reference.name + "' because it is not readable, please enable Read/Write in import settings");
                isAnimated = false;
                isTwoManifold = false;
                usesThirtyTwoBitIndices = false;
                vertexCount = 0;
                triangleCount = 0;
                outBoundsPadFactor = 0.0f;
                return false;
            }

            // Reference mesh
            var refBoundsSize = reference.bounds.size;
            var refVertices = reference.vertices;
            var refBoneWeights = reference.boneWeights;
            var refIndices = reference.triangles;

            var refTriangleCount = refIndices.Length / 3;

            // Shadow mesh
            var vertices = new Vector3[refIndices.Length];
            var normals = new Vector3[refIndices.Length];
            var boneWeights = refBoneWeights.Length > 0 ? new BoneWeight[refIndices.Length] : null;
            var indices = new int[refIndices.Length];

            // Create vertices and initial indices
            // Note that indices are useless at this stage
            for (var i = 0; i < refIndices.Length; i++)
            {
                vertices[i] = refVertices[refIndices[i]];
                indices[i] = i;
            }

            // Create normals
            for (var i = 0; i < refTriangleCount; i++)
            {
                var index0 = i * 3 + 0;
                var index1 = i * 3 + 1;
                var index2 = i * 3 + 2;

                var normal = Vector3.Cross(vertices[index1] - vertices[index0], vertices[index2] - vertices[index0]);

                normal.NormalizeExact();

                normals[index0] = normal;
                normals[index1] = normal;
                normals[index2] = normal;
            }

            // Create bone weights
            if (boneWeights != null)
            {
                for (var i = 0; i < refIndices.Length; i++)
                {
                    boneWeights[i] = refBoneWeights[refIndices[i]];
                }
            }

            // Build edge map
            var cellSize = Mathf.Max(refBoundsSize.x, refBoundsSize.y, refBoundsSize.z) * 0.001f; // 1 cm per meter for a 10 m object seems good
            var edges = new Dictionary<Edge, List<int>>(new EdgeEqualityComparer());

            for (var i = 0; i < refTriangleCount; i++)
            {
                var t0 = vertices[i * 3 + 0];
                var t1 = vertices[i * 3 + 1];
                var t2 = vertices[i * 3 + 2];

                AddEdge(edges, new Edge(t0, t1, cellSize), i);
                AddEdge(edges, new Edge(t1, t2, cellSize), i);
                AddEdge(edges, new Edge(t2, t0, cellSize), i);
            }

            // Validate edge map
            var validTwoManifold = true;

            foreach (var edge in edges.Keys)
            {
                var triangles = edges[edge];

                if (triangles.Count != 2 || !NeighborSameWindingOrder(vertices, indices, triangles[0], triangles[1]))
                {
                    validTwoManifold = false;
                    break;
                }
            }

            if (!validTwoManifold)
            {
                // The non-manifold mesh can be visualized as an outer shell. The following code duplicates this outer shell and flips normals
                // to create a new inner shell. The shells are then connected together to form a manifold mesh.
                var vertexOffset = vertices.Length;
                var triangleOffset = refTriangleCount;

                // Duplicate shell and flip normals

                // Duplicate vertices
                var newVertices = new Vector3[vertices.Length * 2];
                vertices.CopyTo(newVertices, 0);
                vertices.CopyTo(newVertices, vertexOffset);

                // Duplicate and flip normals
                var newNormals = new Vector3[vertices.Length * 2];
                normals.CopyTo(newNormals, 0);

                for (var i = 0; i < normals.Length; i++)
                {
                    newNormals[vertexOffset + i] = -normals[i];
                }

                // Duplicate bone weights
                var newBoneWeights = boneWeights != null ? new BoneWeight[vertices.Length * 2] : null;
                if (boneWeights != null)
                {
                    boneWeights.CopyTo(newBoneWeights, 0);
                    boneWeights.CopyTo(newBoneWeights, vertexOffset);
                }

                // Duplicate indices and reverse winding order (From this point and onward, indices matter)
                var newIndices = new int[vertices.Length * 2];
                indices.CopyTo(newIndices, 0);

                for (var i = 0; i < refTriangleCount; i++)
                {
                    var index0 = i * 3 + 0;
                    var index1 = i * 3 + 1;
                    var index2 = i * 3 + 2;

                    newIndices[vertexOffset + index0] = vertexOffset + index0;
                    newIndices[vertexOffset + index1] = vertexOffset + index2;
                    newIndices[vertexOffset + index2] = vertexOffset + index1;
                }

                // Create degenerate quads
                var finalIndices = new List<int>(newIndices);

                foreach (var edge in edges.Keys)
                {
                    var triangles = edges[edge];

                    // Connect triangles on the same shell whenever possible in order to keep stencil buffer overdraw to a minimum
                    if (triangles.Count == 2 && NeighborSameWindingOrder(newVertices, newIndices, triangles[0], triangles[1]))
                    {
                        // Use a quad to connect the two triangles sharing the edge on the outer and inner shell respectively
                        CreateDegenerateQuad(newVertices, newIndices, edge.a, edge.b, triangles[0], triangles[1], finalIndices);
                        CreateDegenerateQuad(newVertices, newIndices, edge.a, edge.b, triangleOffset + triangles[0], triangleOffset + triangles[1], finalIndices);
                    }
                    else
                    {
                        for (int i = 0; i < triangles.Count; i++)
                        {
                            // Use a quad to connect the triangle on the outer shell with the triangle on the inner shell
                            CreateDegenerateQuad(newVertices, newIndices, edge.a, edge.b, triangles[i], triangleOffset + triangles[i], finalIndices);
                        }
                    }
                }

                vertices = newVertices;
                normals = newNormals;
                boneWeights = newBoneWeights;
                indices = finalIndices.ToArray();
            }
            else
            {
                // Create degenerate quads
                var finalIndices = new List<int>(indices);

                foreach (var edge in edges.Keys)
                {
                    var triangles = edges[edge];
                    if (triangles.Count == 2)
                    {
                        // Use a quad to connect the two triangles sharing the edge
                        CreateDegenerateQuad(vertices, indices, edge.a, edge.b, triangles[0], triangles[1], finalIndices);
                    }
                }

                indices = finalIndices.ToArray();
            }

            // Create output mesh
            if (!result)
            {
                // There seems to be some Unity bugs related to skinned meshes with more than one submesh, copying a
                // mesh by setting properties manually wont work as the internal bone mappings are not updated. To work around
                // this, copy the mesh using Instantiate.
                //
                // https://forum.unity.com/threads/number-of-bind-poses-doesnt-match-number-of-bones-in.29871/
                // https://forum.unity.com/threads/optmize-hierarchy-of-a-generated-skinned-mesh.340876/#post-2727944
                result = Instantiate(reference);
                // result = new Mesh();
            }
            
            result.name = reference.name + "_shadow_mesh";
            
            // Assign geometry
            result.Clear();
            
            // Remove components not relevant for shadows to save space
            result.colors32 = null;
            result.tangents = null;
            result.uv = null;
            result.uv2 = null;
            result.uv3 = null;
            result.uv4 = null;
            result.uv5 = null;
            result.uv6 = null;
            result.uv7 = null;
            result.uv8 = null;
            
            var thirtyTwoBit = vertices.Length >= 65536;
            
            result.indexFormat = thirtyTwoBit ? IndexFormat.UInt32 : IndexFormat.UInt16;
            result.vertices = vertices;
            result.normals = normals;

            if (boneWeights != null)
            {
                result.boneWeights = boneWeights;
            }

            result.triangles = indices; // Automatically recalculates bounds

            if (boundsPadFactor != 0.0f)
            {
                var bounds = result.bounds;
                bounds.Expand(bounds.size.magnitude * boundsPadFactor);
                result.bounds = bounds;
            }

            isAnimated = boneWeights != null;
            isTwoManifold = validTwoManifold;
            usesThirtyTwoBitIndices = thirtyTwoBit;
            vertexCount = vertices.Length;
            triangleCount = indices.Length / 3;
            outBoundsPadFactor = boundsPadFactor;
            return true;
        }
    }
}
