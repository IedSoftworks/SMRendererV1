﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SMRenderer;
using SMRenderer.Renderers;

namespace SMRenderer.Drawing
{
    public class Text : SMItem
    {
        public static FontCharCollection defaultFont;
        public DrawItem draw;
        public DrawContainer container;
        public Region region;
        public string content;
        public TextArguments args;
        public int Width = 0;
        public int Height = 0;
        public Text(string text, Region region, TextArguments arguments)
        {
            content = text;
            this.region = region;
            args = arguments;
            Create();
        }
        private void Create()
        {
            int pos = 0;
            Width = 0;
            List<FontChar> maps = new List<FontChar>();
            foreach (char ch in content.ToCharArray())
            {
                FontChar c = args.fontCollection.chars.FirstOrDefault(a => a.Character == ch.ToString());
                if (c != null)
                {
                    maps.Add(c);
                    Width += c.bitmap.Width + args.characterSpace;
                }
            }
            Height = maps.OrderBy(a => a.bitmap.Height).Last().bitmap.Height;

            if (args.combined)
            {
                if (draw == null) draw = new DrawItem { obj = OM.OB["Quad"], Rotation = 0, Region = region, connected = this, positionAnchor = args.positionAnchor  };
                else draw.Texture.Dispose();
                Bitmap txt = new Bitmap(Width, Height);
                using (Graphics g = Graphics.FromImage(txt))
                {
                    g.Clear(Color.Transparent);
                    maps.ForEach(a =>
                    {
                        g.DrawImage(a.bitmap, pos, Height - a.bitmap.Height);
                        pos += a.bitmap.Width + args.characterSpace;
                    });
                }
                draw.Texture = new Texture(txt);
                draw.Color = args.color;
                draw.Size = new OpenTK.Vector2(Width, Height);
            } else
            {
                if (container.items.Count != 0)
                {
                    container = new DrawContainer();
                }

                maps.ForEach(a =>
                {
                    DrawItem item = new DrawItem { Position = new OpenTK.Vector2(pos, Height - a.bitmap.Height), Size = new OpenTK.Vector2(a.bitmap.Width, a.bitmap.Height), Rotation = 0, Texture = a.texture,  Color = args.color, Region = region, connected = this, positionAnchor = args.positionAnchor };
                    container.items.Add(item);
                });
            }
        }
        public override void Draw(Matrix4 viewMatrix, GenericObjectRenderer renderer)
        {
            if (args.combined) draw.Draw(viewMatrix, renderer);
            else container.Draw(viewMatrix, renderer);
        }
        public override void Prepare(double i)
        {
            if (args.combined) draw.Prepare(i);
            else container.Prepare(i);
        }
    }
    public class FontChar : TextureItem
    {
        /// <summary>
        /// The character
        /// </summary>
        public string Character;
        public FontChar(Bitmap b) : base(b) { }
    }
    public class FontCharCollection
    {
        public List<FontChar> chars = new List<FontChar>();
        /// <summary>
        /// Create a new FontChar and add it to the "chars"-list
        /// </summary>
        /// <param name="ch">The character</param>
        /// <param name="bitmap">The bitmap</param>
        public void Add(string ch, Bitmap bitmap)
        {
            chars.Add(new FontChar(bitmap) { Character = ch });
        }
        /// <summary>
        /// Adds the fontChar to the "chars"-list
        /// </summary>
        /// <param name="fontChar"></param>
        public void Add(FontChar fontChar) { chars.Add(fontChar); }
    }
    public class TextArguments
    {
        /// <summary>
        /// Defines the used fontCollection
        /// </summary>
        public FontCharCollection fontCollection = Text.defaultFont;

        /// <summary>
        /// Defines how much space between chars have to be
        /// </summary>
        public int characterSpace = 5;

        /// <summary>
        /// Defines if the text is combined to one object instead of multiple.
        /// <para>Disables this, when you make large amounts of text.</para>
        /// </summary>
        public bool combined = true;

        /// <summary>
        /// Defines the color for the text
        /// </summary>
        public Color color = Color.Black;

        /// <summary>
        /// Defines the positionAnchor
        /// </summary>
        public string positionAnchor = "lc";
        
    }
}
