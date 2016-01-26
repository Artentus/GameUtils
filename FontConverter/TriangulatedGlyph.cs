using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClipperLib;
using FontConverter.ClipperLib;
using TriangleNet;
using TriangleNet.Geometry;

namespace FontConverter
{
    sealed class TriangulatedGlyph
    {
        readonly List<int> indices;
        readonly List<Vertex> vertices;
        readonly int advanceWidth;
        readonly int leftSideBearing;

        List<LinkedList<ControlPoint>> CreateContours(Glyph glyphData, out List<Triangle> triangles)
        {
            triangles = new List<Triangle>();
            var contours = new List<LinkedList<ControlPoint>>();

            foreach (List<ControlPoint> contour in glyphData.Contours)
            {
                if (contour.Count > 1)
                {
                    var innerPoints = new LinkedList<ControlPoint>();
                    contours.Add(innerPoints);

                    Triangle firstTriangle = null;
                    for (int i = 0, a = contour.Count - 1, b = 1; i < contour.Count; a = i, b = (b + 1) % contour.Count, i++)
                    {
                        ControlPoint point = contour[i];

                        if (point.IsOnCurve)
                        {
                            innerPoints.AddLast(point);
                        }
                        else
                        {
                            var pointBefore = contour[a];
                            if (!pointBefore.IsOnCurve)
                            {
                                pointBefore = (pointBefore + point) / 2;
                                innerPoints.AddLast(pointBefore);
                            }

                            var pointAfter = contour[b];
                            if (!pointAfter.IsOnCurve) pointAfter = (pointAfter + point) / 2;

                            var triangle = new Triangle(point, pointBefore, pointAfter);
                            if (i > 0)
                                triangle.BNode = innerPoints.Last;
                            else
                                firstTriangle = triangle;
                            triangles.Add(triangle);
                        }
                    }
                    if (firstTriangle != null && firstTriangle.BNode == null)
                        firstTriangle.BNode = contour.Last().IsOnCurve ? innerPoints.Last : innerPoints.First;
                }
            }

            return contours;
        }

        bool SplitTriangles(ref List<Triangle> triangles)
        {
            bool result = false;

            var toRemove = new List<int>();
            var toAdd = new List<Triangle>();
            for (int i = triangles.Count - 1; i >= 0; i--)
            {
                Triangle triangleA = triangles[i];

                for (int j = 0; j < triangles.Count; j++)
                {
                    if ((i == j) || (i - 1 == j) || (i + 1 == j) || (i == triangles.Count - 1 && j == 0) || (i == 0 && j == triangles.Count - 1)) continue;

                    Triangle triangleB = triangles[j];

                    if (triangleA.IntersectsWith(triangleB) && triangleA.Area() > triangleB.Area())
                    {
                        result = true;

                        ControlPoint newOnPoint;
                        Triangle[] newTriangles = triangleA.Split(out newOnPoint);

                        var bNode2 = triangleA.Contour.AddAfter(triangleA.BNode, newOnPoint);
                        newTriangles[0].BNode = triangleA.BNode;
                        newTriangles[1].BNode = bNode2;

                        toRemove.Add(i);
                        toAdd.AddRange(newTriangles);
                    }
                }
            }

            foreach (int index in toRemove)
                triangles.RemoveAt(index);
            triangles.AddRange(toAdd);

            return result;
        }

        int GetWindingNumber(ControlPoint p, Glyph glyphData)
        {
            ControlPoint p2 = new ControlPoint(p.X + 1000, p.Y);
            int result = 0;

            foreach (var contour in glyphData.Contours)
            {
                for (int a = contour.Count - 1, b = 0; b < contour.Count; a = b, b++)
                {
                    ControlPoint pa = contour[a];
                    ControlPoint pb = contour[b];
                    
                    if (pa.Y > p.Y && pb.Y < p.Y)
                    {
                        if (pa.X > p.X && pb.X > p.X)
                        {
                            result++;
                        }
                        else if ((pa.X > p.X && pb.X < p.X) || (pa.X < p.X && pb.X > p.X))
                        {
                            ControlPoint r = p2 - p;
                            ControlPoint s = pb - pa;

                            double t = ControlPoint.VectorProduct((pa - p), s) / ControlPoint.VectorProduct(r, s);
                            double u = ControlPoint.VectorProduct((p - pa), r) / ControlPoint.VectorProduct(s, r);

                            if (t >= 0 && u >= 0 && u <= 1) result++;
                        }
                    }
                    else if (pa.Y < p.Y && pb.Y > p.Y)
                    {
                        if (pa.X > p.X && pb.X > p.X)
                        {
                            result--;
                        }
                        else if ((pa.X > p.X && pb.X < p.X) || (pa.X < p.X && pb.X > p.X))
                        {
                            ControlPoint r = p2 - p;
                            ControlPoint s = pb - pa;

                            double t = ControlPoint.VectorProduct((pa - p), s) / ControlPoint.VectorProduct(r, s);
                            double u = ControlPoint.VectorProduct((p - pa), r) / ControlPoint.VectorProduct(s, r);

                            if (t >= 0 && u >= 0 && u <= 1) result--;
                        }
                    }
                }
            }

            return result;
        }

        public TriangulatedGlyph(Glyph glyphData, TrueTypeFile.HorizontalMetric horizontalMetric)
        {
            indices = new List<int>();
            vertices = new List<Vertex>();
            advanceWidth = horizontalMetric.AdvanceWidth;
            leftSideBearing = horizontalMetric.LeftSideBearing;

            if (glyphData.IsCompound)
            {
                
            }
            else
            {
                if (glyphData.Contours.Length == 0)
                    return;

                List<Triangle> triangles;
                List<LinkedList<ControlPoint>> contours = CreateContours(glyphData, out triangles);

                int count = 0;
                bool split;
                do
                {
                    split = SplitTriangles(ref triangles);
                    count++;
                } while (split && count < 4);

                foreach (var triangle in triangles)
                {
                    ControlPoint p = (triangle.B + triangle.C) / 2;
                    int windingNumber = GetWindingNumber(p, glyphData);
                    triangle.IsInside = windingNumber == 0;

                    if (triangle.IsInside) triangle.Contour.AddAfter(triangle.BNode, triangle.A);
                }

                PolyTree tree = ClipperHelper.Combine(contours.Select(contour => contour.ToList()).ToList());

                int pointIndex = 0;
                var inputGeometry = new InputGeometry();
                SearchTree(inputGeometry, ref pointIndex, tree);

                if (inputGeometry.Count > 0)
                {
                    var mesh = new Mesh();
                    mesh.Triangulate(inputGeometry);

                    int highestIndex = 0;
                    var indexDict = new Dictionary<int, int>();
                    foreach (TriangleNet.Data.Triangle triangle in mesh.Triangles)
                    {
                        int[] newIndices;
                        List<Vertex> newVertices = IndexVertices(triangle, indexDict, ref highestIndex, out newIndices);

                        indices.AddRange(newIndices);
                        vertices.AddRange(newVertices);
                    }

                    foreach (var triangle in triangles)
                    {
                        indices.Add(vertices.Count);
                        vertices.Add(new Vertex((float)triangle.B.X, (float)triangle.B.Y, 0, 0, triangle.IsInside));
                        indices.Add(vertices.Count);
                        vertices.Add(new Vertex((float)triangle.A.X, (float)triangle.A.Y, 0.5f, 0, triangle.IsInside));
                        indices.Add(vertices.Count);
                        vertices.Add(new Vertex((float)triangle.C.X, (float)triangle.C.Y, 1, 1, triangle.IsInside));
                    }
                }
            }
        }

        bool Contains(List<ControlPoint> polygon, ControlPoint point)
        {
            int alpha = 0;
            ControlPoint v1 = polygon[polygon.Count - 1];

            int q1;
            if (v1.Y <= point.Y)
            {
                if (v1.X <= point.X)
                    q1 = 0;
                else
                    q1 = 1;
            }
            else if (v1.X <= point.X)
            {
                q1 = 3;
            }
            else
            {
                q1 = 2;
            }

            for (int i = 0; i < polygon.Count; i++)
            {
                ControlPoint v2 = polygon[i];

                int q2;
                if (v2.Y <= point.Y)
                {
                    if (v2.X <= point.X)
                        q2 = 0;
                    else
                        q2 = 1;
                }
                else if (v2.X <= point.X)
                {
                    q2 = 3;
                }
                else
                {
                    q2 = 2;
                }

                switch ((q2 - q1) & 3)
                {
                    case 0:
                        break;
                    case 1:
                        alpha += 1;
                        break;
                    case 3:
                        alpha -= 1;
                        break;
                    default:
                        double zx = ((v2.X - v1.X) * (point.Y - v1.Y) / (v2.Y - v1.Y)) + v1.X;
                        if (point.X - zx == 0)
                            return true;
                        if ((point.X > zx) == (v2.Y > v1.Y))
                            alpha -= 2;
                        else
                            alpha += 2;
                        break;
                }

                v1 = v2;
                q1 = q2;
            }

            return Math.Abs(alpha) == 4;
        }

        bool IsInside(PolyNode node, ControlPoint point)
        {
            foreach (PolyNode childNode in node.Childs)
                if (Contains(childNode.Contour, point)) return false;

            return Contains(node.Contour, point);
        }

        void SearchTree(InputGeometry inputGeometry, ref int pointIndex, PolyNode node)
        {
            foreach (PolyNode childNode in node.Childs)
            {
                int startIndex = pointIndex;

                foreach (ControlPoint point in childNode.Contour)
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
                        ControlPoint a1 = childNode.Contour[k];
                        ControlPoint a2 = childNode.Contour[j];
                        ControlPoint a3 = childNode.Contour[i];

                        if (ControlPoint.VectorProduct(a2 - a1, a3 - a1) < 0)
                        {
                            ControlPoint c = ((a1 + a3) / 2) - a2;
                            double x = 2;
                            ControlPoint hole;
                            do
                            {
                                x /= 2;
                                hole = a2 + (c * x);
                            } while (!IsInside(childNode, hole));

                            inputGeometry.AddHole(hole.X, hole.Y);
                            break;
                        }
                    }
                }

                SearchTree(inputGeometry, ref pointIndex, childNode);
            }
        }

        List<Vertex> IndexVertices(TriangleNet.Data.Triangle triangle, Dictionary<int, int> indexDict, ref int highestIndex, out int[] newIndices)
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

        public void Save(BinaryWriter writer)
        {
            writer.Write(advanceWidth);
            writer.Write(leftSideBearing);

            writer.Write(indices.Count);
            foreach (int index in indices) writer.Write(index);

            writer.Write(vertices.Count);
            foreach (Vertex vertex in vertices)
            {
                writer.Write(vertex.X);
                writer.Write(vertex.Y);
                writer.Write((byte)(Convert.ToInt32(vertex.IsInside) + Convert.ToInt32(vertex.IsCurve)));
                if (vertex.IsCurve)
                {
                    writer.Write(vertex.U);
                    writer.Write(vertex.V);
                }
            }
        }
    }
}
