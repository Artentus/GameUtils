using System;
using System.Windows.Forms;
using GameUtils;
using GameUtils.Graphics;
using GameUtils.Logging;
using GameUtils.Math;

namespace TestApp
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            var logFile = new LogFile(@".\log.txt", false, LogMessagePriority.Engine);
            GameEngine.RegisterComponent(logFile);

            var loop = new GameLoop();
            GameEngine.RegisterComponent(loop);

            var window = new GameWindow();
            GameEngine.RegisterComponent(window);

            var renderer = new Renderer(window, AntiAliasingMode._16xQCSAA);
            //renderer.Transform = Matrix2x3.Translation(50, 50);
            renderer.SyncInterval = 1;
            GameEngine.RegisterComponent(renderer);

            //var intro = new EngineIntro();
            //var introHandle = loop.Register(intro);
            //intro.AnimationComplete += (sender, e) =>
            //{
            //    introHandle.Dispose();
            loop.Register(new Tester());
            //};
            //var keyboard = new Keyboard();
            //GameEngine.RegisterComponent(keyboard);
            //keyboard.Initialize(window);
            //keyboard.RegisterListener(window);

            //keyboard.BeginCapture();
            loop.Start();

            window.Closing += (sender, e) =>
            {
                //keyboard.EndCapture();
                loop.Stop();
                logFile.Close();

                renderer.Dispose();
            };
            Application.Run(window);
        }
    }
}
