using System.Collections.Generic;
using ClipperLib;
using MeshEditor.ClipperLib;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;

namespace MeshEditor.ComplexGeometry
{
    class SimplifiedGeometryShape
    {
        List<List<Vector2>> polygons;
        PolyTree tree;

        public List<int[]> Indices { get; private set; }

        public List<Vertex[]> Vertices { get; private set; }

        public bool Triangulated { get; private set; }

        public SimplifiedGeometryShape()
        {
            Triangulated = false;
        }

        public SimplifiedGeometryShape(List<List<Vector2>> polygons, PolyTree tree)
        {
            this.polygons = polygons;
            this.tree = tree;

            Triangulated = false;
        }

        private SimplifiedGeometryShape(List<GeometryFigure> figures, StrokeStyle strokeStyle, float strokeWidth)
        {
            if (figures.Count < 1)
            {
                polygons = null;
                return;
            }

            polygons = figures[0].Outline(strokeStyle, strokeWidth, out tree);

            var currentFigures = new List<List<Vector2>>();
            for (int i = 1; i < figures.Count; i++)
            {
                currentFigures.AddRange(figures[i].Outline(strokeStyle, strokeWidth, out tree));
            }

            polygons = ClipperHelper.Combine(polygons,
                    currentFigures,
                    FillMode.Alternate,
                    FillMode.Winding,
                    CombineMode.Union,
                    out tree);

            Triangulated = false;
        }

        public static SimplifiedGeometryShape FromOutlines(List<GeometryFigure> figures, StrokeStyle strokeStyle, float strokeWidth)
        {
            return new SimplifiedGeometryShape(figures, strokeStyle, strokeWidth);
        }

        public void AddFigures(List<GeometryFigure> figures, List<CombineMode> combineModes)
        {
            int start = 0;

            if (polygons == null)
            {
                polygons = ClipperHelper.Simplify(figures[0].Points, figures[0].FillMode, out tree);
                start = 1;
            }

            for (int i = start; i < figures.Count; i++)
            {
                CombineMode combineMode = combineModes[i];
                FillMode fillMode = figures[i].FillMode;
                var currentFigures = new List<List<Vector2>>();
                currentFigures.Add(figures[i].Points);

                while ((i + 1 < figures.Count) && (combineModes[i + 1] == combineMode) && (figures[i + 1].FillMode == fillMode))
                {
                    i++;
                    currentFigures.Add(figures[i].Points);
                }

                polygons = ClipperHelper.Combine(polygons,
                    currentFigures,
                    FillMode.Alternate,
                    fillMode,
                    combineMode,
                    out tree);
            }
        }

        public void AddShape(SimplifiedGeometryShape shape, CombineMode combineMode)
        {
            polygons = ClipperHelper.Combine(polygons, shape.polygons, FillMode.Alternate, FillMode.Alternate, combineMode, out tree);
        }

        static void TransformRecursiveley(PolyNode node, Matrix2x3 matrix)
        {
            foreach (PolyNode childNode in node.Childs)
            {
                childNode.Contour = matrix.ApplyTo(childNode.Contour).ToList();
                TransformRecursiveley(childNode, matrix);
            }
        }

        public void Transform(Matrix2x3 matrix)
        {
            if (Triangulated)
            {
                for (int i = 0; i < Vertices.Count; i++)
                    Vertices[i] = matrix.ApplyTo(Vertices[i]);
            }
            else
            {
                if (polygons != null)
                {
                    for (int i = 0; i < polygons.Count; i++)
                        polygons[i] = matrix.ApplyTo(polygons[i]).ToList();
                }

                if (tree != null)
                {
                    TransformRecursiveley(tree, matrix);
                }
            }
        }

        static bool IsInside(PolyNode node, Vector2 point)
        {
            foreach (PolyNode childNode in node.Childs)
            {
                var polygon = new Polygon(childNode.Contour);
                if (polygon.Contains(point)) return false;
            }
            return new Polygon(node.Contour).Contains(point);
        }

        static void SearchTree(InputGeometry inputGeometry, ref int pointIndex, PolyNode node)
        {
            foreach (PolyNode childNode in node.Childs)
            {
                int startIndex = pointIndex;

                foreach (Vector2 point in childNode.Contour)
                {
                    inputGeometry.AddPoint(point.X, point.Y);
                    if (pointIndex > startIndex) inputGeometry.AddSegment(pointIndex - 1, pointIndex);
                    pointIndex++;
                }
                inputGeometry.AddSegment(pointIndex - 1, startIndex);

                if (childNode.IsHole)
                {
                    for (int i = 0, j = childNode.Contour.Count - 1, k = childNode.Contour.Count - 2; i < childNode.Contour.Count; k = j, j = i, i++)
                    {
                        Vector2 a1 = childNode.Contour[k];
                        Vector2 a2 = childNode.Contour[j];
                        Vector2 a3 = childNode.Contour[i];
                        
                        if (Vector2.VectorProduct(a2 - a1, a3 - a1) < 0)
                        {
                            Vector2 c = (a1 + a3) / 2;
                            Vector2 d = a2 - c;
                            float x = c.Length * 2;
                            Vector2 hole;
                            do
                            {
                                x /= 2;
                                hole = c + (1 - x) * d;
                            } while (!IsInside(childNode, hole));
                            x /= 512;
                            hole = c + (1 - x) * d;
                            inputGeometry.AddHole(hole.X, hole.Y);
                            break;
                        }
                    }
                    
                }

                SearchTree(inputGeometry, ref pointIndex, childNode);
            }
        }

        static List<Vertex> IndexVertices(Triangle triangle, Dictionary<int, int> indexDict, ref int highestIndex, out int[] newIndices)
        {
            var newVertices = new List<Vertex>();
            newIndices = new int[3];

            if (indexDict.ContainsKey(triangle.P0))
            {
                newIndices[0] = indexDict[triangle.P0];
            }
            else
            {
                TriangleNet.Data.Vertex vertex0 = triangle.GetVertex(0);
                newVertices.Add(new Vertex((float)vertex0.X, (float)vertex0.Y));
                indexDict.Add(vertex0.ID, highestIndex);
                newIndices[0] = highestIndex;
                highestIndex++;
            }

            if (indexDict.ContainsKey(triangle.P1))
            {
                newIndices[1] = indexDict[triangle.P1];
            }
            else
            {
                TriangleNet.Data.Vertex vertex1 = triangle.GetVertex(1);
                newVertices.Add(new Vertex((float)vertex1.X, (float)vertex1.Y));
                indexDict.Add(vertex1.ID, highestIndex);
                newIndices[1] = highestIndex;
                highestIndex++;
            }

            if (indexDict.ContainsKey(triangle.P2))
            {
                newIndices[2] = indexDict[triangle.P2];
            }
            else
            {
                TriangleNet.Data.Vertex vertex2 = triangle.GetVertex(2);
                newVertices.Add(new Vertex((float)vertex2.X, (float)vertex2.Y));
                indexDict.Add(vertex2.ID, highestIndex);
                newIndices[2] = highestIndex;
                highestIndex++;
            }

            return newVertices;
        }

        void Triangulate(out List<int[]> indices, out List<Vertex[]> vertices)
        {
            indices = new List<int[]>();
            vertices = new List<Vertex[]>();
            if (polygons == null) return;

            int pointIndex = 0;
            var inputGeometry = new InputGeometry();
            SearchTree(inputGeometry, ref pointIndex, tree);

            var mesh = new Mesh();
            mesh.Triangulate(inputGeometry);

            int highestIndex = 0;
            var currentIndices = new List<int>();
            var currentVertices = new List<Vertex>();
            var indexDict = new Dictionary<int, int>();
            foreach (Triangle triangle in mesh.Triangles)
            {
                bool indexOverflow = currentIndices.Count + 3 > Renderer.IndexBufferSize;
                if (indexOverflow)
                {
                    indices.Add(currentIndices.ToArray());
                    vertices.Add(currentVertices.ToArray());

                    currentIndices.Clear();
                    currentVertices.Clear();
                    indexDict.Clear();
                    highestIndex = 0;
                }

                int[] newIndices;
                List<Vertex> newVertices = IndexVertices(triangle, indexDict, ref highestIndex, out newIndices);
                if (currentVertices.Count + newVertices.Count > Renderer.VertexBufferSize)
                {
                    indices.Add(currentIndices.ToArray());
                    vertices.Add(currentVertices.ToArray());

                    currentIndices.Clear();
                    currentVertices.Clear();
                    indexDict.Clear();

                    highestIndex = 3;
                    newIndices[0] = 0;
                    newIndices[1] = 1;
                    newIndices[2] = 2;

                    indexDict.Add(triangle.P0, 0);
                    indexDict.Add(triangle.P1, 1);
                    indexDict.Add(triangle.P2, 2);

                    newVertices.Clear();
                    TriangleNet.Data.Vertex vertex0 = triangle.GetVertex(0);
                    TriangleNet.Data.Vertex vertex1 = triangle.GetVertex(1);
                    TriangleNet.Data.Vertex vertex2 = triangle.GetVertex(2);
                    newVertices.Add(new Vertex((float)vertex0.X, (float)vertex0.Y));
                    newVertices.Add(new Vertex((float)vertex1.X, (float)vertex1.Y));
                    newVertices.Add(new Vertex((float)vertex2.X, (float)vertex2.Y));
                }

                currentIndices.AddRange(newIndices);
                currentVertices.AddRange(newVertices);
            }
            indices.Add(currentIndices.ToArray());
            vertices.Add(currentVertices.ToArray());
        }

        public void Triangulate()
        {
            List<int[]> indices;
            List<Vertex[]> vertices;
            Triangulate(out indices, out vertices);
            Indices = indices;
            Vertices = vertices;

            Triangulated = true;
        }

        public static SimplifiedGeometryShape Combine(SimplifiedGeometryShape shape1, SimplifiedGeometryShape shape2, CombineMode combineMode)
        {
            PolyTree tree;
            List<List<Vector2>> polygons = ClipperHelper.Combine(shape1.polygons,
                shape2.polygons,
                FillMode.Alternate,
                FillMode.Alternate,
                combineMode,
                out tree);

            return new SimplifiedGeometryShape(polygons, tree);
        }

        public SimplifiedGeometryShape Clone()
        {
            List<List<Vector2>> list = null;
            if (polygons != null)
            {
                list = new List<List<Vector2>>(polygons.Count);
                for (int i = 0; i < polygons.Count; i++)
                    list.Add(new List<Vector2>(polygons[i]));
            }

            return new SimplifiedGeometryShape(list, tree.Clone());
        }
    }
}
