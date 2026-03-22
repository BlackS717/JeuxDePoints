using System;
using System.Drawing;

namespace JeuxDePoints {
    internal static class CannonGeometry {
        public static Rectangle GetBodyRect(int cannonX, int centerY, int cannonWidth, int cannonHeight) {
            int bodyHeight = cannonHeight / 3;
            return new Rectangle(cannonX, centerY - bodyHeight / 2, cannonWidth, bodyHeight);
        }

        public static Rectangle GetWheelRect(Rectangle bodyRect, int cannonWidth) {
            int wheelDiameter = Math.Max(8, cannonWidth / 2);
            int wheelX = bodyRect.X + (bodyRect.Width - wheelDiameter) / 2;
            int wheelY = bodyRect.Y + bodyRect.Height - wheelDiameter / 2;
            return new Rectangle(wheelX, wheelY, wheelDiameter, wheelDiameter);
        }

        public static ((int x, int y) start, (int x, int y) end) GetBarrelLine(Rectangle bodyRect, bool facingRight, int cannonWidth) {
            int barrelLength = cannonWidth + 14;
            int startX = facingRight ? bodyRect.X + bodyRect.Width - 2 : bodyRect.X + 2;
            int startY = bodyRect.Y + 2;

            float angleDeg = facingRight ? 35f : 145f;
            double angleRad = Math.PI * angleDeg / 180.0;

            int endX = startX + (int)(barrelLength * Math.Cos(angleRad));
            int endY = startY - (int)(barrelLength * Math.Sin(angleRad));

            return ((startX, startY), (endX, endY));
        }

        public static (int x, int y) GetBarrelTip(int cannonX, int centerY, int cannonWidth, int cannonHeight, bool facingRight) {
            Rectangle bodyRect = GetBodyRect(cannonX, centerY, cannonWidth, cannonHeight);
            var barrelLine = GetBarrelLine(bodyRect, facingRight, cannonWidth);
            return barrelLine.end;
        }
    }
}