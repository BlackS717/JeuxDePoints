using System;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace JeuxDePoints {
    public partial class MenuPanel : UserControl {
        private Controller controller;

        public MenuPanel(Controller controller) {
            InitializeComponent();

            this.controller = controller;

            ShowGameRules();
        }

        private void ShowGameRules() {
            flowLayoutPanel1.Controls.Clear(); // clear existing controls
            flowLayoutPanel1.WrapContents = false;

            // --- Game Configuration ---
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = "=== Game Configuration ===", Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Total points to form line: {GameRule.TOTAL_POINTS_IN_LINE}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Total points gained per line: {GameRule.TOTAL_POINTS_PER_LINE}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Number of players: {GameRule.NUMBER_OF_PLAYERS}" });

            // --- Gameplay Behavior ---
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = "=== Gameplay Behavior ===", Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Turn-based: {(GameRule.IS_TURN_BASE ? "Yes" : "No")}" });

            // --- Line Behavior ---
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = "=== Line Behavior ===", Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Can use points already in lines: {(GameRule.CAN_USE_POINTS_IN_LINES ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Can cut through opponent lines: {(GameRule.CAN_CUT_THROUGH_OPPONENT_LINE ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Can cut through own lines: {(GameRule.CAN_CUT_THROUGH_OWN_LINE ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Connect to longest chain: {(GameRule.CONNECT_TO_LONGEST_CHAIN ? "Yes" : "No")}" });

            // --- Cannon Behavior ---
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = "=== Cannon Behavior ===", Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Line points immune to opponent cannon: {(GameRule.LINE_POINTS_ARE_IMMUNE_TO_OPPONENT_CANNON ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Line points immune to own cannon: {(GameRule.LINE_POINTS_ARE_IMMUNE_TO_OWN_CANNON ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Successful shot uses turn: {(GameRule.SUCCESSFUL_SHOT_CONSUME_TURN ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Failed shot uses turn {(GameRule.MISSED_SHOT_CONSUME_TURN ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Can shoot own points: {(GameRule.CAN_SHOOT_OWN_POINTS ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Can shoot opponent points: {(GameRule.CAN_SHOOT_OPPONENT_POINTS ? "Yes" : "No")}" });

            // --- Cannon Power ---
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = "=== Cannon Power ===", Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Minimum cannon power: {GameRule.MIN_CANNON_POWER}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Maximum cannon power: {GameRule.MAX_CANNON_POWER}" });

            // --- Ammo Rules ---
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = "=== Ammo Rule ===", Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Ifinite ammo: {(GameRule.INFINITE_AMMO ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Can reload: {(GameRule.CAN_RELOAD_AMMO ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Reloading consume turn: {(GameRule.RELOADING_USE_TURN ? "Yes" : "No")}" });

            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Max ammo: {GameRule.MAX_AMMO}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Ammo reload amount: {GameRule.AMMO_RELOAD_AMOUNT}" });
        }

        private void newGameBtn_Click(object sender, EventArgs e) {
            controller.StartNewGame();
        }

    }
}
