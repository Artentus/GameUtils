using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using GameUtils;
using GameUtils.Graphics;
//using GameUtils.Input;
//using GameUtils.Input.DefaultDevices;

namespace TestApp
{
    [DesignerCategory("code")]
    public class GameWindow : GameWindowBase//, IInputListener<KeyboardState>
    {
        readonly GameLoop gameLoop;

        public GameWindow()
            : base(false, false)
        {
            ClientSize = new System.Drawing.Size(1280, 720);
            gameLoop = GameEngine.QueryComponent<GameLoop>();
            var timer = new Timer { Interval = 50 };
            timer.Tick += (sender, e) => Text = gameLoop.Fps.ToString("0.0FPS");
            timer.Start();
        }

        //void IInputListener<KeyboardState>.OnInputReceived(KeyboardState state)
        //{
        //    if (state.IsPressed(Key.F))
        //        Fullscreen = !Fullscreen;

        //    if (state.IsPressed(Key.Escape))
        //        BeginInvoke(new Action(Close));
        //}
    }
}
