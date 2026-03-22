namespace JeuxDePoints {
    public class LineState {
        public int StartRow { get; }
        public int StartCol { get; }
        public int EndRow { get; }
        public int EndCol { get; }
        public int PlayerId { get; }

        public LineState(int startRow, int startCol, int endRow, int endCol, int playerId) {
            StartRow = startRow;
            StartCol = startCol;
            EndRow = endRow;
            EndCol = endCol;
            PlayerId = playerId;
        }

        public static LineState FromLine(Line line) {
            return new LineState(line.start[0], line.start[1], line.end[0], line.end[1], line.playerId);
        }

        public Line ToLine() {
            return new Line(new int[] { StartRow, StartCol }, new int[] { EndRow, EndCol }, PlayerId);
        }
    }
}
