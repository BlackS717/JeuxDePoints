using System;
using System.Drawing;
using System.Windows.Forms;

namespace JeuxDePoints {
    public partial class MenuPanel : UserControl {
        private Controller controller;

        public MenuPanel(Controller controller) {
            InitializeComponent();

            this.controller = controller;

            controller.ActionPerformedEvent += RefreshMoveHistoryUi;
            controller.StartNewGameEvent += RefreshMoveHistoryUi;

            ShowGameRules();
            RefreshPersistenceModeLabel();
            RefreshMoveHistoryUi();
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
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Missed shot uses turn: {(GameRule.MISSED_SHOT_CONSUME_TURN ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Can shoot own points: {(GameRule.CAN_SHOOT_OWN_POINTS ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Can shoot opponent points: {(GameRule.CAN_SHOOT_OPPONENT_POINTS ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Successful shot refunds ammo: {(GameRule.SUCCESSFUL_SHOT_REFUND_AMMO ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Shot animation enabled: {(GameRule.SHOT_ANIMATION_ENABLED ? "Yes" : "No")}" });

            // --- Cannon Power ---
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = "=== Cannon Power ===", Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Minimum cannon power: {GameRule.MIN_CANNON_POWER}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Maximum cannon power: {GameRule.MAX_CANNON_POWER}" });

            // --- Ammo Rules ---
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = "=== Ammo Rules ===", Font = new Font("Segoe UI", 9, FontStyle.Bold) });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Infinite ammo: {(GameRule.INFINITE_AMMO ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Can reload: {(GameRule.CAN_RELOAD_AMMO ? "Yes" : "No")}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Reloading consumes turn: {(GameRule.RELOADING_USE_TURN ? "Yes" : "No")}" });

            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Max ammo: {GameRule.MAX_AMMO}" });
            flowLayoutPanel1.Controls.Add(new Label { AutoSize = true, Text = $"Ammo reload amount: {GameRule.AMMO_RELOAD_AMOUNT}" });
        }

        private void newGameBtn_Click(object sender, EventArgs e) {
            controller.StartNewGame();
        }

        private void saveBtn_Click(object sender, EventArgs e) {
            string slotName = PromptForSaveName();
            if (string.IsNullOrWhiteSpace(slotName)) {
                return;
            }

            bool saved = controller.SaveCurrentToSlot(slotName);
            if (!saved) {
                MessageBox.Show("Unable to save the current game.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show($"Saved to '{slotName.Trim()}'.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void loadBtn_Click(object sender, EventArgs e) {
            string selectedSlot = PromptForLoadSlot();
            if (string.IsNullOrWhiteSpace(selectedSlot)) {
                return;
            }

            bool loaded = controller.LoadFromSlot(selectedSlot);
            if (!loaded) {
                MessageBox.Show("Unable to load selected save.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RefreshMoveHistoryUi();
        }

        private void undoBtn_Click(object sender, EventArgs e) {
            controller.UndoMove();
            RefreshMoveHistoryUi();
        }

        private void redoBtn_Click(object sender, EventArgs e) {
            controller.RedoMove();
            RefreshMoveHistoryUi();
        }

        private void goToStateBtn_Click(object sender, EventArgs e) {
            int selectedIndex = moveHistoryListBox.SelectedIndex;
            if (selectedIndex < 0) {
                return;
            }

            controller.GoToMoveIndex(selectedIndex);
            RefreshMoveHistoryUi();
        }

        private void moveHistoryListBox_SelectedIndexChanged(object sender, EventArgs e) {
            goToStateBtn.Enabled = moveHistoryListBox.SelectedIndex >= 0;
        }

        private void RefreshMoveHistoryUi() {
            moveHistoryListBox.Items.Clear();
            RefreshPersistenceModeLabel();

            var moves = controller.GetTimelineMoveHistory();
            int cols = controller.GetCols();

            for (int i = 0; i < moves.Count; i++) {
                Move move = moves[i];

                int? index = move.PointIndex ?? move.TargetIndex;
                string targetText = "(n/a)";
                if (index.HasValue) {
                    int row = index.Value / cols;
                    int col = index.Value % cols;
                    targetText = $"({row},{col})";
                }

                string status = move.IsSuccessful ? "ok" : "fail";
                string label = $"{i + 1}. P{move.PlayerId + 1} {move.ActionType} {targetText} [{status}]";
                moveHistoryListBox.Items.Add(label);
            }

            int cursor = controller.GetHistoryCursor();
            if (cursor > 0 && cursor <= moveHistoryListBox.Items.Count) {
                moveHistoryListBox.SelectedIndex = cursor - 1;
            }

            undoBtn.Enabled = controller.CanUndoMove();
            redoBtn.Enabled = controller.CanRedoMove();
            goToStateBtn.Enabled = moveHistoryListBox.SelectedIndex >= 0;
        }

        private void RefreshPersistenceModeLabel() {
            if (controller == null || persistenceModeLabel == null) {
                return;
            }

            persistenceModeLabel.Text = controller.GetPersistenceModeLabel();
            persistenceModeLabel.ForeColor = controller.IsUsingDatabasePersistence()
                ? Color.FromArgb(22, 101, 52)   // Deep green for database mode
                : Color.FromArgb(180, 83, 9);   // Amber for memory fallback mode
        }

        private string PromptForSaveName() {
            using (Form dialog = new Form()) {
                dialog.Text = "Save Game";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new Size(340, 120);

                Label label = new Label {
                    Left = 12,
                    Top = 12,
                    Width = 310,
                    Text = "Save name:"
                };

                TextBox input = new TextBox {
                    Left = 12,
                    Top = 34,
                    Width = 310
                };

                Button okButton = new Button {
                    Text = "OK",
                    Left = 166,
                    Width = 75,
                    Top = 72,
                    DialogResult = DialogResult.OK
                };

                Button cancelButton = new Button {
                    Text = "Cancel",
                    Left = 247,
                    Width = 75,
                    Top = 72,
                    DialogResult = DialogResult.Cancel
                };

                dialog.Controls.Add(label);
                dialog.Controls.Add(input);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                DialogResult result = dialog.ShowDialog(this);
                return result == DialogResult.OK ? input.Text : string.Empty;
            }
        }

        private string PromptForLoadSlot() {
            var saveSlots = controller.GetSaveSlotNames();
            if (saveSlots.Count == 0) {
                MessageBox.Show("No save slots available.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return string.Empty;
            }

            using (Form dialog = new Form()) {
                dialog.Text = "Load Game";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new Size(340, 280);

                Label label = new Label {
                    Left = 12,
                    Top = 12,
                    Width = 310,
                    Text = "Select a save slot:"
                };

                ListBox slotsList = new ListBox {
                    Left = 12,
                    Top = 34,
                    Width = 310,
                    Height = 190
                };

                for (int i = 0; i < saveSlots.Count; i++) {
                    slotsList.Items.Add(saveSlots[i]);
                }

                if (slotsList.Items.Count > 0) {
                    slotsList.SelectedIndex = 0;
                }

                Button okButton = new Button {
                    Text = "Load",
                    Left = 166,
                    Width = 75,
                    Top = 236,
                    DialogResult = DialogResult.OK
                };

                Button cancelButton = new Button {
                    Text = "Cancel",
                    Left = 247,
                    Width = 75,
                    Top = 236,
                    DialogResult = DialogResult.Cancel
                };

                dialog.Controls.Add(label);
                dialog.Controls.Add(slotsList);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                DialogResult result = dialog.ShowDialog(this);
                if (result != DialogResult.OK || slotsList.SelectedItem == null) {
                    return string.Empty;
                }

                return slotsList.SelectedItem.ToString();
            }
        }

    }
}
