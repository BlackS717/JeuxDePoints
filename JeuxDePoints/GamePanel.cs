using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace JeuxDePoints {
    internal class GamePanel : Panel {
        private Controller controller;

        private const int CELL_SIZE = 25;
        private int ROWS;
        private int COLS;
        private const int DRAWING_OFFSET_X = 20;
        private const int DRAWING_OFFSET_Y = 20;

        private const int POINT_RADIUS = 8;
        private const int LINE_THICKNESS = 2;
        private const bool SHOW_LINE_POINT_RING = true;
        private const int LINE_POINT_RING_PADDING = 3;
        private const float LINE_POINT_RING_THICKNESS = 1.5f;

        private const int CANNON_WIDTH = 20;
        private const int CANNON_HEIGHT = 60;
        private const int CANNON_OUTSIDE_MARGIN = 75;

        private int last_interracted_point_index = -1;

        private Brush player1Brush = Brushes.Red;
        private Brush player2Brush = Brushes.Green;

        private Color gridColor = Color.FromArgb(128, Color.Gray);

        private Color player1Color = Color.Red;
        private Color player2Color = Color.LightBlue;

        private Color lastPlacedPointHighlightColor = Color.Yellow;
        private Color highlightColor = Color.GhostWhite;

        private bool PLACE_POINT_AT_INTERSECTION = true;

        private bool disablePaint = false;

        private BulletAnimator bulletAnimator;

        public GamePanel(Controller controller) {
            this.controller = controller;

            this.ROWS = controller.GetRows() - 1;
            this.COLS = controller.GetCols() - 1;

            this.Dock = DockStyle.Fill;
            this.BackColor = Color.Black;
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;

            player1Color = ((SolidBrush)player1Brush).Color;
            player2Color = ((SolidBrush)player2Brush).Color;

            bulletAnimator = new BulletAnimator(this);
            bulletAnimator.AnimationCompleted += BulletAnimator_AnimationCompleted;

            this.Paint += GamePanel_Paint;
            this.MouseClick += GamePanel_MouseClick;
            this.KeyDown += GamePanel_KeyDown;
            this.controller.StartNewGameEvent += StartNewGame;
            this.controller.ActionPerformedEvent += OnActionPerformed;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                this.Paint -= GamePanel_Paint;
                this.MouseClick -= GamePanel_MouseClick;
                this.KeyDown -= GamePanel_KeyDown;

                if (bulletAnimator != null) {
                    bulletAnimator.AnimationCompleted -= BulletAnimator_AnimationCompleted;
                    bulletAnimator.Dispose();
                }

                if (this.controller != null) {
                    this.controller.StartNewGameEvent -= StartNewGame;
                    this.controller.ActionPerformedEvent -= OnActionPerformed;
                }
            }

            base.Dispose(disposing);
        }

        private void BulletAnimator_AnimationCompleted(int pointIndex) {
            last_interracted_point_index = pointIndex;

            //bool hit = controller.GetPointValue(pointIndex) != 0;

            (int targetedRow, int targetedCol) = controller.GetPointCoordinates(pointIndex);

            bool hit = controller.HandleAction(ActionType.ShootCannon, targetedRow, targetedCol);

            this.Invalidate();
        }

        private int GetGridDrawCols() {
            return !PLACE_POINT_AT_INTERSECTION ? COLS + 1 : COLS;
        }

        private int GetGridDrawRows() {
            return !PLACE_POINT_AT_INTERSECTION ? ROWS + 1 : ROWS;
        }

        private int GetBoardPixelWidth() {
            return GetGridDrawCols() * CELL_SIZE;
        }

        private int GetBoardPixelHeight() {
            return GetGridDrawRows() * CELL_SIZE;
        }

        private int GetBoardOffsetX() {
            return DRAWING_OFFSET_X + (this.ClientSize.Width - 2 * DRAWING_OFFSET_X - GetBoardPixelWidth()) / 2;
        }

        private int GetBoardOffsetY() {
            return DRAWING_OFFSET_Y + (this.ClientSize.Height - 2 * DRAWING_OFFSET_Y - GetBoardPixelHeight()) / 2;
        }

        private int GetSnappedCannonCenterY(int cannonGridY, int offsetY) {
            int gridRows = GetGridDrawRows();

            if (cannonGridY < 0) {
                cannonGridY = gridRows / 2;
            }

            cannonGridY = Math.Max(0, Math.Min(gridRows, cannonGridY));

            int y = offsetY + cannonGridY * CELL_SIZE;

            if (!PLACE_POINT_AT_INTERSECTION) {
                y += CELL_SIZE / 2;
            }

            return y;
        }

        private void Form_ResizeBegin(object sender, EventArgs e) {
            disablePaint = true;
        }

        private void Form_ResizeEnd(object sender, EventArgs e) {
            disablePaint = false;
            this.Invalidate();
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e) {
            if (disablePaint) return;

            SyncGridDimensions();
            Graphics g = e.Graphics;

            int offsetX = GetBoardOffsetX();
            int offsetY = GetBoardOffsetY();

            DrawGrid(g, offsetX, offsetY);
            DrawPoints(g, offsetX, offsetY);
            DrawLines(g, offsetX, offsetY);
            DrawCannons(g, offsetX, offsetY);
            DrawLastPlacedPointHighlight(g, offsetX, offsetY);
            HighlightCurrentPlayerCannon(g, offsetX, offsetY);

            if (bulletAnimator.HasCurrentBullet) {
                var pos = bulletAnimator.CurrentPosition;
                g.FillEllipse(bulletAnimator.CurrentBrush, pos.x - POINT_RADIUS / 2, pos.y - POINT_RADIUS / 2, POINT_RADIUS, POINT_RADIUS);
            }
        }

        private void OnActionPerformed() {
            SyncGridDimensions();
            this.Invalidate();
        }

        private void SyncGridDimensions() {
            // Keep visual grid in sync with current controller state (load/new game can change dimensions).
            int rows = controller.GetRows();
            int cols = controller.GetCols();

            ROWS = Math.Max(0, rows - 1);
            COLS = Math.Max(0, cols - 1);
        }

        private void DrawGrid(Graphics g, int offsetX, int offsetY) {
            int col = GetGridDrawCols();
            int row = GetGridDrawRows();

            for (int i = 0; i <= col; i++) {
                int x = offsetX + i * CELL_SIZE;
                using (Pen pen = new Pen(gridColor, 1)) {
                    g.DrawLine(pen, x, offsetY, x, offsetY + row * CELL_SIZE);
                }
            }

            for (int i = 0; i <= row; i++) {
                int y = offsetY + i * CELL_SIZE;
                using (Pen pen = new Pen(gridColor, 1)) {
                    g.DrawLine(pen, offsetX, y, offsetX + col * CELL_SIZE, y);
                }
            }
        }

        private void DrawCannons(Graphics g, int offsetX, int offsetY) {
            int col = GetGridDrawCols();

            int cannonLeftX = offsetX - CANNON_WIDTH - CANNON_OUTSIDE_MARGIN;
            int cannonRightX = offsetX + col * CELL_SIZE + CANNON_OUTSIDE_MARGIN;

            int player1CenterY = GetSnappedCannonCenterY(controller.GetCannonY(0), offsetY);
            int player2CenterY = GetSnappedCannonCenterY(controller.GetCannonY(1), offsetY);

            DrawSingleMortar(g, player1Color, cannonLeftX, player1CenterY, true);
            DrawSingleMortar(g, player2Color, cannonRightX, player2CenterY, false);
        }

        private void DrawSingleMortar(Graphics g, Color color, int x, int centerY, bool facingRight) {
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle bodyRect = CannonGeometry.GetBodyRect(x, centerY, CANNON_WIDTH, CANNON_HEIGHT);
            Rectangle wheelRect = CannonGeometry.GetWheelRect(bodyRect, CANNON_WIDTH);
            var barrelLine = CannonGeometry.GetBarrelLine(bodyRect, facingRight, CANNON_WIDTH);

            int barrelThickness = 6;

            int startX = barrelLine.start.x;
            int startY = barrelLine.start.y;
            int endX = barrelLine.end.x;
            int endY = barrelLine.end.y;

            Color dark = Color.FromArgb(
                color.A,
                (int)(color.R * 0.55),
                (int)(color.G * 0.55),
                (int)(color.B * 0.55)
            );

            Color light = Color.FromArgb(
                color.A,
                Math.Min(255, (int)(color.R * 1.2)),
                Math.Min(255, (int)(color.G * 1.2)),
                Math.Min(255, (int)(color.B * 1.2))
            );

            using (Brush bodyBrush = new SolidBrush(color))
            using (Brush wheelBrush = new SolidBrush(dark))
            using (Pen outlinePen = new Pen(Color.FromArgb(180, Color.Black), 1.5f))
            using (Pen barrelPen = new Pen(dark, barrelThickness))
            using (Pen barrelHighlightPen = new Pen(light, 2f)) {
                barrelPen.StartCap = LineCap.Round;
                barrelPen.EndCap = LineCap.Round;

                g.FillRectangle(bodyBrush, bodyRect);
                g.DrawRectangle(outlinePen, bodyRect);

                g.FillEllipse(wheelBrush, wheelRect);
                g.DrawEllipse(outlinePen, wheelRect);

                g.DrawLine(barrelPen, startX, startY, endX, endY);
                g.DrawLine(barrelHighlightPen, startX, startY - 1, endX, endY - 1);
            }

            g.SmoothingMode = previous;

            DrawMortarAmmo(g, color, x, centerY, facingRight);
        }

        private void DrawMortarAmmo(Graphics g, Color color, int x, int centerY, bool facingRight) {
            int playerId = facingRight ? 0 : 1;
            int currentAmmo = controller.GetCannonCurrentAmmo(playerId);
            int maxAmmo = controller.GetCannonMaxAmmo(playerId);

            if (maxAmmo <= 0) {
                return;
            }

            int ammoRadius = Math.Max(3, CANNON_WIDTH / 5);
            int ammoDiameter = ammoRadius * 2;
            int ammoGap = 4;

            int totalWidth = maxAmmo * ammoDiameter + (maxAmmo - 1) * ammoGap;
            int startX = x + (CANNON_WIDTH - totalWidth) / 2;
            int y = centerY - CANNON_HEIGHT / 2 - ammoDiameter - 6;

            Color shellFillColor = Color.FromArgb(
                color.A,
                Math.Min(255, (int)(color.R * 1.15)),
                Math.Min(255, (int)(color.G * 1.15)),
                Math.Min(255, (int)(color.B * 1.15))
            );

            using (Pen outlinePen = new Pen(Color.FromArgb(180, Color.Black), 1.2f))
            using (Brush fillBrush = new SolidBrush(shellFillColor))
            using (Brush emptyBrush = new SolidBrush(Color.FromArgb(60, Color.White))) {
                for (int i = 0; i < maxAmmo; i++) {
                    int ammoX = startX + i * (ammoDiameter + ammoGap);
                    Rectangle ammoRect = new Rectangle(ammoX, y, ammoDiameter, ammoDiameter);

                    if (i < currentAmmo) {
                        g.FillEllipse(fillBrush, ammoRect);
                    } else {
                        g.FillEllipse(emptyBrush, ammoRect);
                    }

                    g.DrawEllipse(outlinePen, ammoRect);
                }
            }
        }

        private void DrawPoints(Graphics g, int offsetX, int offsetY) {
            using (Pen linePointRingPen = new Pen(Color.WhiteSmoke, LINE_POINT_RING_THICKNESS)) {
                for (int row = 0; row <= ROWS; row++) {
                    for (int col = 0; col <= COLS; col++) {
                        int pointValue = controller.GetPointValue(row, col);
                        if (pointValue == 0) continue;

                        Brush brush = Brushes.Gray;

                        if (pointValue == (int)CellState.Player1) {
                            brush = player1Brush;
                        } else if (pointValue == (int)CellState.Player2) {
                            brush = player2Brush;
                        } else {
                            continue;
                        }

                        /*
                        int currentPointIndex = row * (COLS + 1) + col;
                        
                        if (bulletAnimator.IsAnimating && currentPointIndex == bulletAnimator.AnimatingPointIndex) {
                            continue;
                        }

                        */

                        int x = offsetX + col * CELL_SIZE;
                        int y = offsetY + row * CELL_SIZE;

                        if (!PLACE_POINT_AT_INTERSECTION) {
                            x += CELL_SIZE / 2;
                            y += CELL_SIZE / 2;
                        }

                        x -= POINT_RADIUS / 2;
                        y -= POINT_RADIUS / 2;

                        g.FillEllipse(brush, x, y, POINT_RADIUS, POINT_RADIUS);

                        if (SHOW_LINE_POINT_RING && controller.IsLinePoint(row, col)) {
                            int ringX = x - LINE_POINT_RING_PADDING;
                            int ringY = y - LINE_POINT_RING_PADDING;
                            int ringSize = POINT_RADIUS + LINE_POINT_RING_PADDING * 2;
                            g.DrawEllipse(linePointRingPen, ringX, ringY, ringSize, ringSize);
                        }
                    }
                }
            }
        }

        private void DrawLines(Graphics g, int offsetX, int offsetY) {
            List<Line> lines = controller.GetLines();

            foreach (Line line in lines) {
                int startRow = line.start[0];
                int startCol = line.start[1];
                int endRow = line.end[0];
                int endCol = line.end[1];
                int x1 = offsetX + startCol * CELL_SIZE;
                int y1 = offsetY + startRow * CELL_SIZE;
                int x2 = offsetX + endCol * CELL_SIZE;
                int y2 = offsetY + endRow * CELL_SIZE;

                if (!PLACE_POINT_AT_INTERSECTION) {
                    x1 += CELL_SIZE / 2;
                    y1 += CELL_SIZE / 2;
                    x2 += CELL_SIZE / 2;
                    y2 += CELL_SIZE / 2;
                }

                Color penColor = Color.Gray;
                int playerId = line.playerId;
                if (playerId == 0) {
                    penColor = player1Color;
                } else if (playerId == 1) {
                    penColor = player2Color;
                } else {
                    continue;
                }

                using (Pen pen = new Pen(penColor, LINE_THICKNESS)) {
                    g.DrawLine(pen, x1, y1, x2, y2);
                }
            }
        }

        private void DrawLastPlacedPointHighlight(Graphics g, int offsetX, int offsetY) {
            if (last_interracted_point_index == -1) return;

            int row = last_interracted_point_index / (COLS + 1);
            int col = last_interracted_point_index % (COLS + 1);
            int x = offsetX + col * CELL_SIZE;
            int y = offsetY + row * CELL_SIZE;

            if (!PLACE_POINT_AT_INTERSECTION) {
                x += CELL_SIZE / 2;
                y += CELL_SIZE / 2;
            }

            x -= POINT_RADIUS;
            y -= POINT_RADIUS;

            using (Pen pen = new Pen(lastPlacedPointHighlightColor, 2)) {
                g.DrawEllipse(pen, x, y, POINT_RADIUS * 2, POINT_RADIUS * 2);
            }
        }

        private void HighlightCurrentPlayerCannon(Graphics g, int offsetX, int offsetY) {
            int currentPlayerId = controller.GetCurrentPlayerId();
            int col = GetGridDrawCols();

            int cannonX = currentPlayerId == 0
                ? offsetX - CANNON_WIDTH - CANNON_OUTSIDE_MARGIN
                : offsetX + col * CELL_SIZE + CANNON_OUTSIDE_MARGIN;

            int cannonCenterY = GetSnappedCannonCenterY(controller.GetCannonY(currentPlayerId), offsetY);
            int markerPadding = 5;
            int markerHeight = CANNON_HEIGHT / 2 + markerPadding * 2;
            int markerY = cannonCenterY - markerHeight / 2;

            using (Pen selPen = new Pen(highlightColor, 2)) {
                g.DrawRectangle(selPen, cannonX - markerPadding, markerY, CANNON_WIDTH + markerPadding * 2, markerHeight);
            }
        }

        private void StartNewGame() {
            SyncGridDimensions();
            
            last_interracted_point_index = -1;
            bulletAnimator.Stop();
            this.Invalidate();
        }

        private void GamePanel_KeyDown(object sender, KeyEventArgs e) {
            if (controller.IsGameOver() || bulletAnimator.IsAnimating) {
                return;
            }

            HandleCannonMovement(e);
            HandleCannonShooting(e);

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void HandleCannonMovement(KeyEventArgs e) {
            int delta = 0;
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Z) {
                delta = -1;
            } else if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S) {
                delta = 1;
            } else {
                return;
            }

            int playerId = controller.GetCurrentPlayerId();
            int currentRow = controller.GetCannonY(playerId);
            if (currentRow < 0) {
                currentRow = ROWS / 2;
            }

            if (controller.MoveCurrentPlayerCannonToRow(currentRow + delta)) {
                this.Invalidate();
            }
        }

        private void HandleCannonShooting(KeyEventArgs e) {
            int power = 0;
            // only shoot if ctrl is held down with a number key, to avoid conflicts with normal movement keys (Z/S or Up/Down)
            // Check for number keys (1-9) on both main keyboard and numpad

            if (controller.IsGameOver()) {
                return;
            }

            if (!e.Control) {
                return;
            }

            if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9) {
                power = e.KeyCode - Keys.D0;
            } else if (e.KeyCode >= Keys.NumPad1 && e.KeyCode <= Keys.NumPad9) {
                power = e.KeyCode - Keys.NumPad0;
            } else {
                return;
            }

            // Prevent shooting again while animating
            if (bulletAnimator.IsAnimating) {
                return;
            }

            int playerId = controller.GetCurrentPlayerId();
            int cannonRow = controller.GetCannonY(playerId);

            if (cannonRow < 0) {
                cannonRow = ROWS / 2;
            }


            //Console.WriteLine($"Player {playerId + 1} shoots with power {power}");


            int targetedCol = controller.GetShotTargetCol(power);
            int targetedRow = cannonRow;

            bool isPlayer1 = playerId == 0;

            int drawOffsetX = GetBoardOffsetX();
            int drawOffsetY = GetBoardOffsetY();

            int drawCols = GetGridDrawCols();
            int cannonLeftX = drawOffsetX - CANNON_WIDTH - CANNON_OUTSIDE_MARGIN;
            int cannonRightX = drawOffsetX + drawCols * CELL_SIZE + CANNON_OUTSIDE_MARGIN;
            int cannonX = isPlayer1 ? cannonLeftX : cannonRightX;
            int cannonCenterY = GetSnappedCannonCenterY(controller.GetCannonY(playerId), drawOffsetY);

            var barrelTip = CannonGeometry.GetBarrelTip(cannonX, cannonCenterY, CANNON_WIDTH, CANNON_HEIGHT, isPlayer1);
            int startX = barrelTip.x;
            int startY = barrelTip.y;

            int targetX = drawOffsetX + targetedCol * CELL_SIZE;
            int targetY = drawOffsetY + targetedRow * CELL_SIZE;
            
            // Snap target to grid point (cell center or intersection)
            if (!PLACE_POINT_AT_INTERSECTION) {
                targetX += CELL_SIZE / 2;
                targetY += CELL_SIZE / 2;
            }

            List<(int, int)> bulletPath = GenerateBulletPath(
                x0: startX,
                y0: startY,
                x1: targetX,
                y1: targetY,
                power: power
            );

            /*
            Console.WriteLine("Bullet path:");
            foreach (var point in bulletPath) {
                Console.WriteLine($"({point.Item1}, {point.Item2})");
            }
            */

            // Check if the shot hits a point
            bool canShoot = controller.CanShootCannon(playerId);

            //bool hit = controller.HandleAction(ActionType.ShootCannon, (int)controller.GetCurrentPlayerId(), targetedRow, targetedCol);

            if (canShoot) {

                if (GameRule.SHOT_ANIMATION_ENABLED) {
                    last_interracted_point_index = -1;
                    bulletAnimator.Start(
                        bulletPath,
                        targetedCol + targetedRow * (COLS + 1),
                        isPlayer1 ? player1Brush : player2Brush
                    );
                } else {
                    bulletAnimator.Stop();
                    last_interracted_point_index = targetedCol + targetedRow * (COLS + 1);
                }
            }

            
        }

        private List<(int, int)> GenerateBulletPath(
            int x0, int y0,         // starting position
            int x1, int y1,         // target position
            double power,           // controls speed
            double angleDeg = 35,   // launch angle in degrees
            double g = 500          // gravity in pixels/sec^2
        ) {
            int miniArcHeight = CELL_SIZE;
            return CannonTrajectory.GenerateParabolicPath(x0, y0, x1, y1, 25, miniArcHeight);
        }

        private void GamePanel_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left || controller.IsGameOver() || bulletAnimator.IsAnimating) {
                return;
            }

            this.Focus();

            if (!IsWithinGrid(e.X, e.Y)) {
                return;
            }

            int index = getClosestPointIndex(e.X, e.Y);
            int targetCol = index % (COLS + 1);
            int targetRow = index / (COLS + 1);

            bool success = controller.HandleAction(
                ActionType.PlacePoint,
                targetRow,
                targetCol
            );

            if (success) {
                last_interracted_point_index = index;
            }
        }

        private int getClosestPointIndex(int mouseX, int mouseY) {
            int offsetX = GetBoardOffsetX();
            int offsetY = GetBoardOffsetY();

            int closestCol;
            int closestRow;

            if (PLACE_POINT_AT_INTERSECTION) {
                // nearest intersection
                closestCol = (int)Math.Round((mouseX - offsetX) / (double)CELL_SIZE);
                closestRow = (int)Math.Round((mouseY - offsetY) / (double)CELL_SIZE);
            } else {
                // cell mode: pick the cell containing the click
                closestCol = (mouseX - offsetX) / CELL_SIZE;
                closestRow = (mouseY - offsetY) / CELL_SIZE;
            }

            closestCol = Math.Max(0, Math.Min(COLS, closestCol));
            closestRow = Math.Max(0, Math.Min(ROWS, closestRow));

            return closestRow * (COLS + 1) + closestCol;
        }

        private bool IsWithinGrid(int x, int y) {
            int offsetX = GetBoardOffsetX();
            int offsetY = GetBoardOffsetY();

            int gridWidth = GetBoardPixelWidth();
            int gridHeight = GetBoardPixelHeight();

            return x >= offsetX && x <= offsetX + gridWidth && y >= offsetY && y <= offsetY + gridHeight;
        }

        public void setPlacePointAtIntersection(bool value) {
            PLACE_POINT_AT_INTERSECTION = value;
            this.Invalidate();
        }
    }
}
