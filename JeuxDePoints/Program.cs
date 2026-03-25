using System;
using System.Windows.Forms;

namespace JeuxDePoints {
    internal static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            int rows = 10;
            int cols = 10;
            GameState game = new GameState(rows, cols);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameForm(game));
        }
    }

}
