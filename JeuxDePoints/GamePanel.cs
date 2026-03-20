using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace JeuxDePoints {
    internal class GamePanel: Panel {
        private Controller controller;

        private const int CELL_SIZE = 50;
        private int ROWS = 10;
        private int COLS = 10;
        private const int DRAWING_OFFSET_X = 20;
        private const int DRAWING_OFFSET_Y = 20;

        private const int POINT_RADIUS = 17;
        private const int LINE_THICKNESS = 3;

        private Brush player1Brush = Brushes.Red;
        private Brush player2Brush = Brushes.Green;

        private Color gridColor = Color.FromArgb(128, Color.Gray);

        private Color player1Color = Color.Red; // default, will be set to match player1Brush in constructor
        private Color player2Color = Color.LightBlue; // default, will be set to match player2Brush in constructor


        private bool PLACE_POINT_AT_INTERSECTION = true; // if true, points are drawn at grid intersections, otherwise in cell centers

        private bool disablePaint = false;

        public GamePanel(Controller controller) {
            this.controller = controller;

            this.ROWS = controller.GetRows() - 1;
            this.COLS = controller.GetCols() - 1;

            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.DoubleBuffered = true;

            this.BackColor = Color.Black;

            player1Color = ((SolidBrush)player1Brush).Color;
            player2Color = ((SolidBrush)player2Brush).Color;


            this.ResizeRedraw = true;

            this.Paint += GamePanel_Paint;
            this.MouseClick += GamePanel_MouseClick;
            this.controller.GameUpdated += () => this.Invalidate();
        }
        private void Form_ResizeBegin(object sender, EventArgs e) {
            disablePaint = true;
        }

        private void Form_ResizeEnd(object sender, EventArgs e) {
            disablePaint = false;
            this.Invalidate(); // Force repaint after resize ends
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e) {
            if (disablePaint) return;
            Graphics g = e.Graphics;

            int offsetX = DRAWING_OFFSET_X + (this.ClientSize.Width - 2 * DRAWING_OFFSET_X - COLS * CELL_SIZE) / 2;
            int offsetY = DRAWING_OFFSET_Y + (this.ClientSize.Height - 2 * DRAWING_OFFSET_Y - ROWS * CELL_SIZE) / 2;

            drawGrid(g, offsetX, offsetY);

            drawPoints(g, offsetX, offsetY);

            drawLines(g, offsetX, offsetY);

            drawCannons(g, offsetX, offsetY);
        }

        private void drawGrid(Graphics g, int offsetX, int offsetY) {

            // draw vertical lines
            int col = !PLACE_POINT_AT_INTERSECTION ? COLS + 1 : COLS;
            int row = !PLACE_POINT_AT_INTERSECTION ? ROWS + 1 : ROWS;

            for (int i = 0; i <= col; i++) {
                int x = offsetX + i * CELL_SIZE;

                using (Pen pen = new Pen(gridColor, 1)) {
                    g.DrawLine(pen, x, offsetY, x, offsetY + row * CELL_SIZE);
                    
                }
            }

            // draw horizontal lines
            for (int i = 0; i <= row; i++) {
                int y = offsetY + i * CELL_SIZE;

                using (Pen pen = new Pen(gridColor, 1)) {
                    g.DrawLine(pen, offsetX, y, offsetX + col * CELL_SIZE, y);
                }
            }
        }

        private void drawCannons(Graphics g, int offsetX, int offsetY) {
            int col = !PLACE_POINT_AT_INTERSECTION ? COLS + 1 : COLS;
            int row = !PLACE_POINT_AT_INTERSECTION ? ROWS + 1 : ROWS;

            // left cannon
            int cannonWidth = 20;
            int cannonHeight = 60;
            int cannonLeftX = offsetX - cannonWidth - 10;
            int cannonY = offsetY + (row * CELL_SIZE - cannonHeight) / 2;
            g.FillRectangle(player1Brush, cannonLeftX, cannonY, cannonWidth, cannonHeight);

            // right cannon
            int cannonRightX = offsetX + col * CELL_SIZE + 10;
            g.FillRectangle(player2Brush, cannonRightX, cannonY, cannonWidth, cannonHeight);
        }

        private void drawPoints(Graphics g, int offsetX, int offsetY) {
            for (int row = 0; row <= ROWS; row++) {          
                for (int col = 0; col <= COLS; col++) {      
                    int pointValue = controller.GetPointValue(row, col);
                    if (pointValue != 0) {

                        Brush brush = Brushes.Gray;

                        if(pointValue == (int)CellState.Player1Point || pointValue == (int)CellState.Player1Line) {
                            brush = player1Brush;
                        } else if(pointValue == (int)CellState.Player2Point || pointValue == (int)CellState.Player2Line) {
                            brush = player2Brush;
                        } else {
                            continue; // skip non-point cells
                        }

                        int x = offsetX + col * CELL_SIZE;   // intersection x
                        int y = offsetY + row * CELL_SIZE;   // intersection y

                        if (!PLACE_POINT_AT_INTERSECTION) {
                            x += CELL_SIZE / 2; // center in cell
                            y += CELL_SIZE / 2; // center in cell
                        }

                        x -= POINT_RADIUS / 2; // adjust to center the ellipse
                        y -= POINT_RADIUS / 2; // adjust to center the ellipse

                        g.FillEllipse(brush, x, y, POINT_RADIUS, POINT_RADIUS);  // center on intersection
                    }
                }
            }
        }

        private void drawLines(Graphics g, int offsetX, int offsetY) {
            // optional: draw lines between points if needed
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

        private void GamePanel_MouseClick(object sender, MouseEventArgs e) {
            if(!IsWithinGrid(e.X, e.Y)) {
                return; // ignore clicks outside the grid
            } 

            int index = getClosestPointIndex(e.X, e.Y);
            int value = controller.GetPointValue(index);


            // send the action to the controller
            // a click is always a place point action, the controller will determine if it's valid or not
            bool success = controller.HandleAction(ActionType.PlacePoint, controller.GetCurrentPlayerId(), index / (COLS + 1), index % (COLS + 1));

            if (success) {
                this.Invalidate(); // redraw to show the new point
            }

        }

        private int getClosestPointIndex(int mouseX, int mouseY) {
            int offsetX = DRAWING_OFFSET_X + (this.ClientSize.Width - 2 * DRAWING_OFFSET_X - COLS * CELL_SIZE) / 2;
            int offsetY = DRAWING_OFFSET_Y + (this.ClientSize.Height - 2 * DRAWING_OFFSET_Y - ROWS * CELL_SIZE) / 2;
            int closestCol = (mouseX - offsetX + CELL_SIZE / 2) / CELL_SIZE;
            int closestRow = (mouseY - offsetY + CELL_SIZE / 2) / CELL_SIZE;
            closestCol = Math.Max(0, Math.Min(COLS, closestCol));
            closestRow = Math.Max(0, Math.Min(ROWS, closestRow));
            return closestRow * (COLS + 1) + closestCol; // convert to cell index
        }

        private bool IsWithinGrid(int x, int y) {
            int offsetX = DRAWING_OFFSET_X + (this.ClientSize.Width - 2 * DRAWING_OFFSET_X - COLS * CELL_SIZE) / 2;
            int offsetY = DRAWING_OFFSET_Y + (this.ClientSize.Height - 2 * DRAWING_OFFSET_Y - ROWS * CELL_SIZE) / 2;
            return x >= offsetX && x <= offsetX + COLS * CELL_SIZE && y >= offsetY && y <= offsetY + ROWS * CELL_SIZE;
        }

        public void setPlacePointAtIntersection(bool value) {
            PLACE_POINT_AT_INTERSECTION = !value;
            this.Invalidate(); // redraw to reflect change
        }
    }
}
