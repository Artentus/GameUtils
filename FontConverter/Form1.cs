using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FontConverter
{
    public partial class Form1 : Form
    {
        Glyph glyph;
        TriangulatedGlyph triGlyph;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var fontFile = new TrueTypeFile(ofd.FileName);
                    var font = new GameUtilsFont(fontFile);
                    font.Save(font.FontName + ".guf");
                    //glyph = fontFile.Glyphs[fontFile.GlyphDictionary[' ']];
                    //triGlyph = font.glyphs[font.glyphDictionary[' ']];
                    //Invalidate();
                }
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //if (triGlyph != null)
            //{
            //    e.Graphics.ScaleTransform(0.25f, -0.25f);
            //    e.Graphics.TranslateTransform(300, -1500);

            //    foreach (var contour in glyph.Contours)
            //    {
            //        e.Graphics.DrawPolygon(Pens.Black, contour.Select(point => new PointF((float)point.X, (float)point.Y)).ToArray());
            //        foreach (var point in contour)
            //        {
            //            if (point.IsOnCurve)
            //                e.Graphics.FillRectangle(Brushes.Green, (float)point.X - 8, (float)point.Y - 8, 16, 16);
            //            else
            //                e.Graphics.FillRectangle(Brushes.Blue, (float)point.X - 8, (float)point.Y - 8, 16, 16);
            //        }
            //    }

            //    e.Graphics.TranslateTransform(1500, 0);

            //    for (int i = 0; i < triGlyph.indices.Count; i += 3)
            //    {
            //        Vertex a = triGlyph.vertices[triGlyph.indices[i]];
            //        Vertex b = triGlyph.vertices[triGlyph.indices[i + 1]];
            //        Vertex c = triGlyph.vertices[triGlyph.indices[i + 2]];
            //        e.Graphics.DrawPolygon(Pens.Black, new PointF[] { new PointF(a.X, a.Y), new PointF(b.X, b.Y), new PointF(c.X, c.Y) });
            //    }

            //    foreach (var triangle in triGlyph.triangles)
            //    {
            //        e.Graphics.DrawPolygon(Pens.Red,
            //            new PointF[]
            //            {
            //                new PointF((float)triangle.A.X, (float)triangle.A.Y),
            //                new PointF((float)triangle.B.X, (float)triangle.B.Y),
            //                new PointF((float)triangle.C.X, (float)triangle.C.Y)
            //            });
            //    }
            //}
        }
    }
}
