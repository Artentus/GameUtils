using System;
using GameUtils;
using GameUtils.Graphics;
using GameUtils.Math;
using LinearGradientBrush = GameUtils.Graphics.LinearGradientBrush;

namespace TestApp
{
    public class Tester : RegistrationContext<Tester.TesterUpdateable>
    {
        readonly TesterRenderable renderable;

        public Tester()
        {
            renderable = new TesterRenderable();
        }

        public class TesterRenderable : IStateRenderer<TesterUpdateable>
        {
            public void Render(TesterUpdateable state, TimeSpan elapsed, Renderer renderer)
            {
                if (state.Brush1.IsReady && state.Brush2.IsReady)
                {
                    //renderer.SetClip(new Rectangle(100, 100, 1720, 880));
                    renderer.FillRectangle(renderer.SurfaceBounds, state.Brush1);
                    //renderer.FillRectangle(renderer.SurfaceBounds, state.Brush2);
                    //renderer.DrawText("Lorem ipsum dolor sit amet, consetetur sadipscing elitr,\nsed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat,\nsed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum.\nStet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.\nLorem ipsum dolor sit amet, consetetur sadipscing elitr,\nsed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat,\nsed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum.\nStet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.",
                    //    new Vector2(10, 10), state.Font, 24, state.Brush2);
                    //renderer.DrawText("!", new Vector2(100, 100), state.Font, 14, state.Brush2);
                    //renderer.FillRectangle(new Rectangle(100, 100, 1000, 1), state.Brush2);

                    //if (state.Geometry.IsReady)
                    //{
                    //    renderer.FillGeometry(state.Geometry, state.Brush1);
                    //    renderer.FillGeometry(state.Geometry, state.Brush2);
                    //}
                    //if (state.Outline.IsReady)
                    //{
                    //    renderer.FillOutline(state.Outline, state.Brush1);
                    //    renderer.FillOutline(state.Outline, state.Brush2);
                    //}
                }
            }

            public event EventHandler DepthPositionChanged;

            float IStateRenderer<TesterUpdateable>.DepthPosition
            {
                get { return 0; }
            }
        }

        public class TesterUpdateable : IBufferedState<TesterUpdateable>
        {
            public Brush Brush1 { get; private set; }
            public Brush Brush2 { get; private set; }
            public Font Font { get; private set; }
            //public Geometry Geometry { get; private set; }
            //public GeometryOutline Outline { get; private set; }

            public void Update(TesterUpdateable oldState, TimeSpan elapsed)
            {
                if (Brush1 == null)
                {
                    if (oldState.Brush1 != null)
                        Brush1 = oldState.Brush1;
                    else
                    {
                        Brush1 = new TextureBrush(@"..\..\Penrose_triangle_outline.png", true);
                        ((TextureBrush)Brush1).WrapMode = WrapMode.Tile;
                        //Brush.Transform = Matrix2x3.Rotation(0.25f); //Matrix2x3.Scaling(2);

                        //Brush1 = new SolidColorBrush(Color4.White);
                    }
                }

                if (Brush2 == null)
                {
                    if (oldState.Brush2 != null)
                        Brush2 = oldState.Brush2;
                    else
                    {
                        var stops = new GradientStop[]
                        {
                            new GradientStop() { Color = new Color4(1, 0, 0, 1), Position = 0f },
                            new GradientStop() { Color = new Color4(1, 1, 0, 0), Position = 0.333f },
                            new GradientStop() { Color = new Color4(1, 1, 1, 0), Position = 0.667f },
                            new GradientStop() { Color = new Color4(1, 0, 1, 0), Position = 1f }
                        };

                        Brush2 = new RadialGradientBrush(stops, new Vector2(960, 270), 960, 540, 0f, new Vector2(240, 270));
                        //Brush2 = new LinearGradientBrush(stops, new Vector2(0, 0), new Vector2(1920, 1080));
                        //Brush = new LinearGradientBrush()
                        //{
                        //    GradientStops = stops,
                        //    StartPoint = new Vector2(0, 0),
                        //    EndPoint = new Vector2(1920, 1080),
                        //};
                        //Brush = new RadialGradientBrush(stops, new Vector2(960, 540), 10, 10);
                    }
                }
                else
                {
                    //((RadialGradientBrush)Brush2).Angle += 0.05f;
                }

                //if (Font == null)
                //{
                //    if (oldState.Font != null)
                //        Font = oldState.Font;
                //    else
                //        Font = new Font(@"..\..\Consolas.guf");
                //}

                //if (Geometry == null)
                //{
                //    if (oldState.Geometry != null)
                //    {
                //        Geometry = oldState.Geometry;
                //        Outline = oldState.Outline;
                //    }
                //    else
                //    {
                //        Geometry = new Geometry();
                //        Geometry.OpenFigure(new Vector2(300, 0));
                //        Geometry.AddBezier(new Vector2(500, 100), new Vector2(700, 100), new Vector2(600, 300));
                //        Geometry.AddLine(new Vector2(450, 600));
                //        Geometry.AddLine(new Vector2(150, 600));
                //        Geometry.AddLine(new Vector2(0, 300));
                //        Geometry.AddBezier(new Vector2(-100, 100), new Vector2(100, 100), new Vector2(300, 0));
                //        Geometry.CloseFigure(FigureEnd.Closed);
                //        Geometry.OpenFigure(new Vector2(300, 50));
                //        Geometry.AddLine(new Vector2(550, 300));
                //        Geometry.AddLine(new Vector2(450, 550));
                //        Geometry.AddLine(new Vector2(150, 550));
                //        Geometry.AddLine(new Vector2(50, 300));
                //        Geometry.CloseFigure(FigureEnd.Closed);
                //        Geometry.OpenFigure(new Vector2(300, 100));
                //        Geometry.AddLine(new Vector2(500, 300));
                //        Geometry.AddLine(new Vector2(450, 500));
                //        Geometry.AddLine(new Vector2(150, 500));
                //        Geometry.AddLine(new Vector2(100, 300));
                //        Geometry.CloseFigure(FigureEnd.Closed);
                //        Geometry.OpenFigure(new Vector2(300, 150));
                //        Geometry.AddLine(new Vector2(450, 300));
                //        Geometry.AddLine(new Vector2(450, 400));
                //        Geometry.AddArc(new Vector2(400, 450), new Vector2(400, 400), SweepDirection.Clockwise);
                //        Geometry.AddLine(new Vector2(200, 450));
                //        Geometry.AddArc(new Vector2(150, 400), new Vector2(200, 400), SweepDirection.Clockwise);
                //        Geometry.AddLine(new Vector2(150, 300));
                //        Geometry.CloseFigure(FigureEnd.Closed);
                //        Geometry.Close();

                //        var strokeStyle = new StrokeStyle();
                //        strokeStyle.LineJoin = LineJoin.Miter;
                //        Outline = Geometry.Outline(strokeStyle, 20);
                //    }
                //}

                //if (Brush1.IsReady && Brush2.IsReady && Geometry.IsReady && Outline.IsReady)
                //    Outline.Transform(Matrix2x3.Translation(300, 300) * Matrix2x3.Rotation(0.1f * (float)elapsed.TotalSeconds) * Matrix2x3.Translation(-300, -300));
            }
        }

        protected override TesterUpdateable CreateBuffer()
        {
            return new TesterUpdateable();
        }

        protected override IStateRenderer<TesterUpdateable> Renderer
        {
            get { return renderable; }
        }
    }
}
