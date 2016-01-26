using System.ComponentModel;

namespace MeshEditor.ComplexGeometry
{
    sealed class GeometryFigure : INotifyPropertyChanged
    {
        public readonly BindingList<FigureSection> Sections;

        public GeometryFigure()
        {
            Sections = new BindingList<FigureSection>();
        }
    }
}
