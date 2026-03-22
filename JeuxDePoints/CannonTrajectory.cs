using System;
using System.Collections.Generic;

namespace JeuxDePoints {
    internal static class CannonTrajectory {
        public static List<(int x, int y)> GenerateParabolicPath(
            int x0,
            int y0,
            int x1,
            int y1,
            int pointCount,
            int minArcHeight
        ) {
            List<(int x, int y)> path = new List<(int x, int y)>();

            if (pointCount <= 0) {
                path.Add((x1, y1));
                return path;
            }

            double midX = (x0 + x1) * 0.5;
            double distanceX = Math.Abs(x1 - x0);
            double baseArcHeight = Math.Max(minArcHeight, distanceX * 0.25);
            double controlY = Math.Min(y0, y1) - baseArcHeight;

            for (int i = 0; i < pointCount; i++) {
                double t = i / (double)pointCount;
                double oneMinusT = 1.0 - t;

                double x = oneMinusT * oneMinusT * x0
                         + 2.0 * oneMinusT * t * midX
                         + t * t * x1;

                double y = oneMinusT * oneMinusT * y0
                         + 2.0 * oneMinusT * t * controlY
                         + t * t * y1;

                path.Add(((int)Math.Round(x), (int)Math.Round(y)));
            }

            path.Add((x1, y1));
            return path;
        }
    }
}
