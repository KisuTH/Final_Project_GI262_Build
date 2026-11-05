using ColorShiftKnight.Core;
using UnityEngine;

namespace ColorShiftKnight.Gameplay
{
    // Where the painted 6x6 board is inside the board picture, and how that maps onto the canvas.
    public static class BoardLayout
    {
        // The picture is 1980 x 1125 pixels (y goes down).
        public const float ArtWidth = 1980f;
        public const float ArtHeight = 1125f;

        // Y of the 7 horizontal lines of the board, top to bottom.
        private static float[] rowY = { 432f, 500f, 572f, 650f, 735f, 822f, 917f };

        // X of the left and right edge of the board at the top line and at the bottom line.
        private const float TopLeftX = 675f;
        private const float BottomLeftX = 582f;
        private const float TopRightX = 1320f;
        private const float BottomRightX = 1412f;

        // How big the picture is drawn on the canvas, and where its centre is.
        public const float ArtScale = 1.15f;
        public static readonly Vector2 BoardCenterOnCanvas = new Vector2(-380f, 0f);

        // Corner point in the picture. rowLine and colLine are 0..6 (the lines between squares).
        public static Vector2 BoardPoint(int rowLine, int colLine)
        {
            float y = rowY[rowLine];
            float t = (y - rowY[0]) / (rowY[BoardConfig.Size] - rowY[0]);
            float left = Mathf.Lerp(TopLeftX, BottomLeftX, t);
            float right = Mathf.Lerp(TopRightX, BottomRightX, t);
            return new Vector2(Mathf.Lerp(left, right, colLine / (float)BoardConfig.Size), y);
        }

        // Converts a point in the picture to a point on the canvas.
        public static Vector2 ToCanvas(Vector2 art)
        {
            Vector2 fromCenter = new Vector2(art.x - ArtWidth * 0.5f, -(art.y - ArtHeight * 0.5f));
            return BoardCenterOnCanvas + fromCenter * ArtScale;
        }
    }
}
