using System.Collections.Generic;
using GameUtils.Graphics;
using ClipperLib;

namespace MeshEditor.ClipperLib
{
    static class ClipperHelper
    {
        static readonly Clipper Clipper;
        static readonly ClipperOffset Offsetter;

        static ClipperHelper()
        {
            Clipper = new Clipper(Clipper.ioStrictlySimple);
            Offsetter = new ClipperOffset();
        }

        static PolyFillType ToPolyFillType(this FillMode fillMode)
        {
            switch (fillMode)
            {
                case FillMode.Winding:
                    return PolyFillType.pftNonZero;
                default:
                    return PolyFillType.pftEvenOdd;
            }
        }

        public static List<List<Vector2>> Simplify(List<Vector2> polygon, FillMode fillMode, out PolyTree tree)
        {
            Clipper.Clear();
            Clipper.AddPath(polygon, PolyType.ptSubject, true);
            Clipper.AddPath(polygon, PolyType.ptClip, true);

            tree = new PolyTree();
            PolyFillType fillType = fillMode.ToPolyFillType();
            Clipper.Execute(ClipType.ctUnion, tree, fillType, fillType);
            return Clipper.ClosedPathsFromPolyTree(tree);
        }

        static ClipType ToClipType(this CombineMode mode)
        {
            switch (mode)
            {
                case CombineMode.Or:
                    return ClipType.ctUnion;
                case CombineMode.Not:
                    return ClipType.ctDifference;
                case CombineMode.XOr:
                    return ClipType.ctXor;
                default:
                    return ClipType.ctIntersection;
            }
        }

        public static List<List<Vector2>> Combine(List<List<Vector2>> subjectPolygons, List<List<Vector2>> clippingPolygons,
                                                  FillMode subjectFillMode, FillMode clipFillMode, CombineMode combineMode, out PolyTree tree)
        {
            Clipper.Clear();
            Clipper.AddPaths(subjectPolygons, PolyType.ptSubject, true);
            Clipper.AddPaths(clippingPolygons, PolyType.ptClip, true);

            tree = new PolyTree();
            Clipper.Execute(combineMode.ToClipType(), tree, subjectFillMode.ToPolyFillType(), clipFillMode.ToPolyFillType());
            return Clipper.ClosedPathsFromPolyTree(tree);
        }

        static EndType ToEndType(this CapStyle capStyle)
        {
            switch (capStyle)
            {
                case CapStyle.Square:
                    return EndType.etOpenSquare;
                case CapStyle.Round:
                    return EndType.etOpenRound;
                default:
                    return EndType.etOpenButt;
            }
        }

        public static List<List<Vector2>> Outline(List<Vector2> polygon, FillMode fillMode, bool closed, StrokeStyle strokeStyle, float strokeWidth, out PolyTree tree)
        {
            List<List<Vector2>> simplified = Clipper.SimplifyPolygon(polygon, fillMode.ToPolyFillType());

            Offsetter.Clear();
            Offsetter.MiterLimit = strokeStyle.MiterLimit;
            Offsetter.AddPaths(simplified, (JoinType)strokeStyle.LineJoin, closed ? EndType.etClosedLine : strokeStyle.CapStyle.ToEndType());

            tree = new PolyTree();
            Offsetter.Execute(ref tree, strokeWidth / 2);
            return Clipper.ClosedPathsFromPolyTree(tree);
        }
    }
}
