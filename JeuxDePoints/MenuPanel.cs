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
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;

            Font sectionFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            Font itemFont = new Font("Segoe UI", 8.5F, FontStyle.Regular);

            Label AddSection(string text) {
                Label section = new Label {
                    AutoSize = false,
                    Width = flowLayoutPanel1.ClientSize.Width - 24,
                    Height = 22,
                    Margin = new Padding(0, 8, 0, 2),
                    Font = sectionFont,
                    ForeColor = Color.FromArgb(31, 41, 55),
                    Text = text
                };
                flowLayoutPanel1.Controls.Add(section);
                return section;
            }

            Label AddRule(string text) {
                Label rule = new Label {
                    AutoSize = false,
                    Width = flowLayoutPanel1.ClientSize.Width - 24,
                    Height = 20,
                    Margin = new Padding(0, 1, 0, 1),
                    Font = itemFont,
                    ForeColor = Color.FromArgb(55, 65, 81),
                    Text = "- " + text
                };
                flowLayoutPanel1.Controls.Add(rule);
                return rule;
            }

            // --- Game Configuration ---
            AddSection("Game Configuration");
            AddRule($"Total points to form line: {GameRule.TOTAL_POINTS_IN_LINE}");
            AddRule($"Total points gained per line: {GameRule.TOTAL_POINTS_PER_LINE}");
            AddRule($"Number of players: {GameRule.NUMBER_OF_PLAYERS}");

            // --- Gameplay Behavior ---
            AddSection("Gameplay Behavior");
            AddRule($"Turn-based: {(GameRule.IS_TURN_BASE ? "Yes" : "No")}");

            // --- Line Behavior ---
            AddSection("Line Behavior");
            AddRule($"Can use points already in lines: {(GameRule.CAN_USE_POINTS_IN_LINES ? "Yes" : "No")}");
            AddRule($"Can cut through opponent lines: {(GameRule.CAN_CUT_THROUGH_OPPONENT_LINE ? "Yes" : "No")}");
            AddRule($"Can cut through own lines: {(GameRule.CAN_CUT_THROUGH_OWN_LINE ? "Yes" : "No")}");
            AddRule($"Connect to longest chain: {(GameRule.CONNECT_TO_LONGEST_CHAIN ? "Yes" : "No")}");

            // --- Cannon Behavior ---
            AddSection("Cannon Behavior");
            AddRule($"Line points immune to opponent cannon: {(GameRule.LINE_POINTS_ARE_IMMUNE_TO_OPPONENT_CANNON ? "Yes" : "No")}");
            AddRule($"Line points immune to own cannon: {(GameRule.LINE_POINTS_ARE_IMMUNE_TO_OWN_CANNON ? "Yes" : "No")}");
            AddRule($"Successful shot uses turn: {(GameRule.SUCCESSFUL_SHOT_CONSUME_TURN ? "Yes" : "No")}");
            AddRule($"Missed shot uses turn: {(GameRule.MISSED_SHOT_CONSUME_TURN ? "Yes" : "No")}");
            AddRule($"Can shoot own points: {(GameRule.CAN_SHOOT_OWN_POINTS ? "Yes" : "No")}");
            AddRule($"Can shoot opponent points: {(GameRule.CAN_SHOOT_OPPONENT_POINTS ? "Yes" : "No")}");
            AddRule($"Successful shot refunds ammo: {(GameRule.SUCCESSFUL_SHOT_REFUND_AMMO ? "Yes" : "No")}");
            AddRule($"Shot animation enabled: {(GameRule.SHOT_ANIMATION_ENABLED ? "Yes" : "No")}");

            // --- Cannon Power ---
            AddSection("Cannon Power");
            AddRule($"Minimum cannon power: {GameRule.MIN_CANNON_POWER}");
            AddRule($"Maximum cannon power: {GameRule.MAX_CANNON_POWER}");

            // --- Ammo Rules ---
            AddSection("Ammo Rules");
            AddRule($"Infinite ammo: {(GameRule.INFINITE_AMMO ? "Yes" : "No")}");
            AddRule($"Can reload: {(GameRule.CAN_RELOAD_AMMO ? "Yes" : "No")}");
            AddRule($"Reloading consumes turn: {(GameRule.RELOADING_USE_TURN ? "Yes" : "No")}");
            AddRule($"Max ammo: {GameRule.MAX_AMMO}");
            AddRule($"Ammo reload amount: {GameRule.AMMO_RELOAD_AMOUNT}");
        }

        private void newGameBtn_Click(object sender, EventArgs e) {
            controller.StartNewGame();
        }

        private void saveBtn_Click(object sender, EventArgs e) {
            string slotName = PromptForSaveSlot();
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

        private string PromptForSaveSlot() {
            var saveSlots = controller.GetSaveSlotNames();
            string selectedSlotName = string.Empty;

            using (Form dialog = new Form()) {
                dialog.Text = "Save Game";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ClientSize = new Size(360, 320);

                Label existingLabel = new Label {
                    Left = 12,
                    Top = 12,
                    Width = 336,
                    Text = "Existing save slots (select one to update):"
                };

                ListBox slotsList = new ListBox {
                    Left = 12,
                    Top = 34,
                    Width = 336,
                    Height = 180
                };

                for (int i = 0; i < saveSlots.Count; i++) {
                    slotsList.Items.Add(saveSlots[i]);
                }

                Label newNameLabel = new Label {
                    Left = 12,
                    Top = 224,
                    Width = 336,
                    Text = "New name (or selected slot name):"
                };

                TextBox input = new TextBox {
                    Left = 12,
                    Top = 246,
                    Width = 249
                };

                Button saveButton = new Button {
                    Text = "Save",
                    Left = 273,
                    Width = 75,
                    Top = 244
                };

                Button cancelButton = new Button {
                    Text = "Cancel",
                    Left = 247,
                    Width = 75,
                    Top = 282,
                    DialogResult = DialogResult.Cancel
                };

                slotsList.SelectedIndexChanged += (s, e) => {
                    if (slotsList.SelectedItem != null) {
                        input.Text = slotsList.SelectedItem.ToString();
                    }
                };

                saveButton.Click += (s, e) => {
                    string targetName = input.Text.Trim();
                    if (string.IsNullOrWhiteSpace(targetName) && slotsList.SelectedItem != null) {
                        targetName = slotsList.SelectedItem.ToString();
                    }

                    if (string.IsNullOrWhiteSpace(targetName)) {
                        MessageBox.Show(dialog, "Please enter a save name or select an existing slot.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    selectedSlotName = targetName;
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                };

                slotsList.DoubleClick += (s, e) => {
                    if (slotsList.SelectedItem == null) {
                        return;
                    }

                    selectedSlotName = slotsList.SelectedItem.ToString();
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                };

                dialog.Controls.Add(existingLabel);
                dialog.Controls.Add(slotsList);
                dialog.Controls.Add(newNameLabel);
                dialog.Controls.Add(input);
                dialog.Controls.Add(saveButton);
                dialog.Controls.Add(cancelButton);
                dialog.AcceptButton = saveButton;
                dialog.CancelButton = cancelButton;

                DialogResult result = dialog.ShowDialog(this);
                return result == DialogResult.OK ? selectedSlotName : string.Empty;
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

                Action refreshSlots = () => {
                    int previousIndex = slotsList.SelectedIndex;
                    slotsList.Items.Clear();

                    var updatedSlots = controller.GetSaveSlotNames();
                    for (int i = 0; i < updatedSlots.Count; i++) {
                        slotsList.Items.Add(updatedSlots[i]);
                    }

                    if (slotsList.Items.Count == 0) {
                        return;
                    }

                    if (previousIndex >= 0 && previousIndex < slotsList.Items.Count) {
                        slotsList.SelectedIndex = previousIndex;
                    } else {
                        slotsList.SelectedIndex = 0;
                    }
                };

                refreshSlots();

                Button okButton = new Button {
                    Text = "Load",
                    Left = 85,
                    Width = 75,
                    Top = 236,
                    DialogResult = DialogResult.OK
                };

                Button deleteButton = new Button {
                    Text = "Delete",
                    Left = 166,
                    Width = 75,
                    Top = 236
                };

                Button cancelButton = new Button {
                    Text = "Cancel",
                    Left = 247,
                    Width = 75,
                    Top = 236,
                    DialogResult = DialogResult.Cancel
                };

                deleteButton.Click += (s, e) => {
                    if (slotsList.SelectedItem == null) {
                        MessageBox.Show(dialog, "Select a save slot to delete.", "Delete Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    string slotName = slotsList.SelectedItem.ToString();
                    DialogResult confirm = MessageBox.Show(
                        dialog,
                        $"Delete save slot '{slotName}'?",
                        "Delete Save",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (confirm != DialogResult.Yes) {
                        return;
                    }

                    bool deleted = controller.DeleteSaveSlot(slotName);
                    if (!deleted) {
                        MessageBox.Show(dialog, "Unable to delete selected save.", "Delete Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    refreshSlots();
                    if (slotsList.Items.Count == 0) {
                        MessageBox.Show(dialog, "All save slots deleted.", "Delete Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dialog.DialogResult = DialogResult.Cancel;
                        dialog.Close();
                    }
                };

                dialog.Controls.Add(label);
                dialog.Controls.Add(slotsList);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(deleteButton);
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
