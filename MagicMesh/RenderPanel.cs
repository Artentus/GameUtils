using System.Windows.Forms;

namespace MagicMesh
{
    class RenderPanel : Control
    {
        public RenderPanel()
        {
            this.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            this.UpdateStyles();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        { }
    }
}
