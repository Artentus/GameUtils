using System.Collections.Generic;
using ClipperLib;

namespace FontConverter.ClipperLib
{
    static class ClipperHelper
    {
        static readonly Clipper Clipper;

        static ClipperHelper()
        {
            Clipper = new Clipper();
        }

        public static PolyTree Combine(List<List<ControlPoint>> polygons)
        {
            Clipper.Clear();
            Clipper.AddPaths(polygons, PolyType.ptSubject, true);
            Clipper.AddPaths(polygons, PolyType.ptClip, true);

            var tree = new PolyTree();
            Clipper.Execute(ClipType.ctUnion, tree, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
            return tree;
        }
    }
}
