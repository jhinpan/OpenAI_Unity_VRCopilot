namespace OpenAI
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.IO;

    public class ObjLoader : MonoBehaviour
    {
        public static Mesh Load(string filePath)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            // Add code for normals and UVs if needed

            string[] lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (line.StartsWith("v "))
                {
                    Vector3 vertex = ParseVertex(line);
                    vertices.Add(vertex);
                }
                if (line.StartsWith("f "))
                {
                    int[] triangle = ParseFace(line);
                    triangles.AddRange(triangle);
                }
            }

            // Validate triangle count before creating Mesh
            if (triangles.Count % 3 != 0)
            {
                Debug.LogError("The number of supplied triangle indices must be a multiple of 3.");
                return null;
            }

            Mesh mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
                // Add code for normals and UVs if needed
            };

            return mesh;
        }

        private static Vector3 ParseVertex(string line)
        {
            string[] parts = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            float x = float.Parse(parts[1]);
            float y = float.Parse(parts[2]);
            float z = float.Parse(parts[3]);
            return new Vector3(x, y, z);
        }

        private static int[] ParseFace(string line)
        {
            string[] parts = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

            // Assuming faces are triangular
            if (parts.Length == 4)  // A face with 3 vertices (triangular)
            {
                int[] indices = new int[3];
                for (int i = 1; i <= 3; i++) // Only read three indices
                {
                    indices[i - 1] = int.Parse(parts[i].Split('/')[0]) - 1;  // OBJ indices start at 1
                }
                return indices;
            }
            else
            {
                Debug.LogError("Only triangular faces are supported.");
                return new int[0];
            }
        }
    }
}
