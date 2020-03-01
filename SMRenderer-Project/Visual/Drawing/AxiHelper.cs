﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMRenderer;
using OpenTK.Graphics;

namespace SMRenderer.Drawing
{
    /// <summary>
    /// Creates a AxisHelper
    /// </summary>
    public class AxisHelper : DrawContainer
    {
        /// <summary>
        /// Creates it at a specific position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public AxisHelper(int x, int y) { Create(new Vector2(x, y)); }
        /// <summary>
        /// Creates it at a specific position
        /// </summary>
        /// <param name="Position"></param>
        public AxisHelper(Vector2 Position) { Create(Position); }
        /// <summary>
        /// Creates it with a region
        /// </summary>
        /// <param name="region"></param>
        public AxisHelper(Region region) { Create(region); }
        private void Create(Vector2 Position)
        {
            _RenderOrder = 255;
            purpose = "AxisHelper at " + Position.X + " " + Position.Y;
            items.AddRange(new DrawItem[]
            {
                new DrawItem
                {
                    Position = Position + new Vector2(0,5),
                    Size = new Vector2(5,15),
                    positionAnchor = "lu",
                    connected = this,
                    Rotation = 0,
                    Color = Color4.Blue
                },
                new DrawItem
                {
                    Position = Position + new Vector2(5,0),
                    Size = new Vector2(15,5),
                    positionAnchor = "lu",
                    connected = this,
                    Rotation = 0,
                    Color = Color4.Red
                }
            });
        }
        private void Create(Region region)
        {
            _RenderOrder = 255;
            purpose = "AxisHelper for region";
            items.AddRange(new DrawItem[]
            {
                new DrawItem
                {
                    Region = region,
                    Position = new Vector2(0,5),
                    Size = new Vector2(5,15),
                    positionAnchor = "lu",
                    connected = this,
                    Rotation = 0,
                    Color = Color4.Blue
                },
                new DrawItem
                {
                    Region = region,
                    Position = new Vector2(5,0),
                    Size = new Vector2(15,5),
                    positionAnchor = "lu",
                    connected = this,
                    Rotation = 0,
                    Color = Color4.Red
                }
            });
        }
    }
}