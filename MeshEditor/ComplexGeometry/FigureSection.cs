using System.Collections.Generic;

namespace MeshEditor.ComplexGeometry
{
    abstract class FigureSection
    {
        public abstract IEnumerable<Vector2> GenerateVertices(float precisition);
    }
}
