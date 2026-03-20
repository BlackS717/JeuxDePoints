using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JeuxDePoints {
    public class Line {
        public int[] start {get; private set; }
        public int[] end {get; private set; }
        public int playerId { get; private set; }

        public Line(int[] startIndex, int[] endIndex, int playerId) {
            this.start = startIndex;
            this.end = endIndex;
            this.playerId = playerId;
        }

        // Returns true if this line intersects with another line
        public bool Intersects(Line other) {
            return DoLinesIntersect(this.start, this.end, other.start, other.end);
        }

        // Helper using standard line intersection formula
        private bool DoLinesIntersect(int[] p1, int[] p2, int[] q1, int[] q2) {
            int o1 = Orientation(p1, p2, q1);
            int o2 = Orientation(p1, p2, q2);
            int o3 = Orientation(q1, q2, p1);
            int o4 = Orientation(q1, q2, p2);

            if (o1 != o2 && o3 != o4)
                return true; // general case

            // Special cases (collinear points)
            if (o1 == 0 && OnSegment(p1, q1, p2)) return true;
            if (o2 == 0 && OnSegment(p1, q2, p2)) return true;
            if (o3 == 0 && OnSegment(q1, p1, q2)) return true;
            if (o4 == 0 && OnSegment(q1, p2, q2)) return true;

            return false;
        }

        private int Orientation(int[] a, int[] b, int[] c) {
            int val = (b[0] - a[0]) * (c[1] - b[1]) - (b[1] - a[1]) * (c[0] - b[0]);
            if (val == 0) return 0; // collinear
            return (val > 0) ? 1 : 2; // clockwise or counterclockwise
        }

        private bool OnSegment(int[] a, int[] b, int[] c) {
            return b[1] <= Math.Max(a[1], c[1]) && b[1] >= Math.Min(a[1], c[1]) &&
                   b[0] <= Math.Max(a[0], c[0]) && b[0] >= Math.Min(a[0], c[0]);
        }
    }
}
