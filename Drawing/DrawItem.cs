﻿using OpenTK;
using OpenTK.Graphics;
using SMRenderer.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMRenderer.Drawing
{
    /// <summary>
    /// DrawItem saves instructions of the draw
    /// </summary>
    public class DrawItem : SMItem
    {
        // Private
        public Vector2 _centerPoint = new Vector2(0, 0);
        public Vector2 _actualPosition = new Vector2(0, 0);
        public float _actualRotation = 0;
        public Matrix4 modelMatrix;

        /// <summary>
        /// The object that need to be render.
        /// </summary>
        public Object obj = ObjectManager.OB["Quad"];

        /// <summary>
        /// Specifies the position of the object
        /// </summary>
        public Vector2 Position = Vector2.Zero;

        /// <summary>
        /// Specifies the anchor for the position
        /// </summary>
        public string positionAnchor = "cc";

        /// <summary>
        /// Contains the center position of the object; Important when you not use the center-anchor
        /// </summary>
        public Vector2 CenterPoint { get { return _centerPoint; } }

        /// <summary>
        /// Specifies the rotation of the object
        /// </summary>
        public float Rotation = 0;

        /// <summary>
        /// Specifies the scale of the object
        /// </summary>
        public Vector2 Size = new Vector2(50, 50);

        /// <summary>
        /// Specifies the Z-index
        /// </summary>
        public float ZIndex = 0;

        /// <summary>
        /// Specifies where the object renders
        /// </summary>
        public RenderPosition RenderPosition = RenderPosition.Normal;

        /// <summary>
        /// Dictionary for all animations that are possible on this object; Key = identify-string; Value = Animation;
        /// </summary>
        public Dictionary<string, Animation> Animations = new Dictionary<string, Animation>();

        /// <summary>
        /// Specifies the region; Makes Position, Rotation and Z-Index values relative and ignore the RenderPosition
        /// </summary>
        public Region Region = Region.zero;

        /// <summary>
        /// Specifies the used texture
        /// </summary>
        public Texture Texture = Texture.empty;

        /// <summary>
        /// Colorize the texture in that color; Default: White;
        /// </summary>
        public Color4 Color = Color4.White;

        /// <summary>
        /// Contains all arguments for visual effects
        /// </summary>
        public Dictionary<string, object> EffectArgs = VisualEffectArgs.DefaultEffectArgs.ToDictionary(a => a.Key, b => b.Value);

        /// <summary>
        /// Tell the program to actual draw the object
        /// </summary>
        override public void Draw()
        {
            modelMatrix = Matrix4.CreateScale(Size.X, Size.Y, 1) * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(_actualRotation)) * Matrix4.CreateTranslation(_centerPoint.X, _centerPoint.Y, 0);

            Matrix4 view;
            if (RenderPosition == RenderPosition.DynamicBackground || RenderPosition == RenderPosition.HUD) view = GLWindow.Window.viewProjectionHUD;
            else view = GLWindow.Window.ViewProjection;

            Dictionary<string, object> tmp = EffectArgs.Concat(VisualEffectArgs.DefaultEffectArgs.Where(x => !EffectArgs.ContainsKey(x.Key))).ToDictionary(a => a.Key, a => a.Value);

            GLWindow.Window.Renderer.Draw(obj, this, view, modelMatrix, tmp);
        }

        override public void Prepare()
        {
            _actualRotation = Rotation + Region.GetRotation();
            _actualRotation %= 360;

            // Calcuate the position
            _actualPosition = Helper.Rotation.CalculatePositionForRotationAroundPoint(Region.GetPosition(), Position, Region.GetRotation());

            _RenderOrder = ZIndex + Region.GetZIndex();

            if (Region != null) if (Region.renderPosition != RenderPosition.Override) RenderPosition = Region.renderPosition;

            if (RenderPosition == RenderPosition.DynamicBackground || RenderPosition == RenderPosition.StaticBackground) _RenderOrder -= 255;
            if (RenderPosition == RenderPosition.HUD) _RenderOrder += 255;

            calcCenter();
        }

        private void calcCenter()
        {

            float stepX = Size.X / 2;
            float stepY = Size.Y / 2;
            _centerPoint = new Vector2(_actualPosition.X, _actualPosition.Y);

            switch (positionAnchor.First())
            {
                case 'l':
                    _centerPoint.X += stepX;
                    break;
                case 'r':
                    _centerPoint.X -= stepX;
                    break;
            }
            switch (positionAnchor.Last())
            {
                case 'u':
                    _centerPoint.Y += stepY;
                    break;
                case 'l':
                    _centerPoint.Y -= stepY;
                    break;
            }
        }
        override public void Activate()
        {
            SM.List.Add(this);
        }
        override public void Deactivate()
        {
            SM.List.Remove(this);
        }

        public static Vector2 CalculatePositionAnchor(Vector2 position, Vector2 size, string anchor)
        {
            float stepX = size.X / 2;
            float stepY = size.Y / 2;
            switch (anchor.First())
            {
                case 'l':
                    position.X -= stepX;
                    break;
                case 'r':
                    position.X += stepX;
                    break;
            }
            switch (anchor.Last())
            {
                case 'u':
                    position.Y -= stepY;
                    break;
                case 'l':
                    position.Y += stepY;
                    break;
            }
            return position;
        }
    }
    public class DI : DrawItem { }
}