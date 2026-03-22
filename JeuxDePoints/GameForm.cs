using System.Windows.Forms;
using System;
using System.Configuration;
using System.Data;

namespace JeuxDePoints {
    public class GameForm : Form {
        private GamePanel gamePanel;
        private MenuPanel menuPanel;
        private Controller controller;

        public GameForm(GameState state) {
            this.Text = "Jeux de Points";
            this.Width = 1000;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;

            ResizeRedraw = true;
            this.controller = new Controller(state, CreateDatabaseConnectionFactory());

            MenuPanel menu = new MenuPanel(controller);
            GamePanel game = new GamePanel(controller);


            this.menuPanel = menu;
            menuPanel.Dock = DockStyle.Left;
            this.gamePanel = game;

            this.Controls.Add(game);
            this.Controls.Add(menu); // add last so menu is on top of docking
        }

        private static Func<IDbConnection> CreateDatabaseConnectionFactory() {
            string connectionString = ConfigurationManager.AppSettings["PostgresConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString)) {
                return null;
            }

            Type npgsqlConnectionType = Type.GetType("Npgsql.NpgsqlConnection, Npgsql");
            if (npgsqlConnectionType == null) {
                return null;
            }

            return () => {
                object instance = Activator.CreateInstance(npgsqlConnectionType, connectionString);
                return (IDbConnection)instance;
            };
        }
    }
}