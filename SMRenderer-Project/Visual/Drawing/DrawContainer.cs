﻿
using OpenTK;
using SMRenderer.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMRenderer.Drawing
{
    /// <summary>
    /// Can save multiple SMItems at once
    /// </summary>
    public class DrawContainer : SMItem
    {
        /// <summary>
        /// Contains the items
        /// </summary>
        public List<SMItem> items = new List<SMItem>();
        /// <summary>
        /// Called, when it need to draw
        /// </summary>
        /// <param name="viewMatrix">Current viewMatrix</param>
        /// <param name="renderer">The current renderer</param>
        public override void Draw(Matrix4 viewMatrix, GenericObjectRenderer renderer)
        {
            items.ForEach(a => { a.Draw(viewMatrix, renderer); });
        }
        /// <summary>
        /// Adds the item to the list
        /// </summary>
        /// <param name="i"></param>
        public void Add(SMItem i) { items.Add(i); }
        /// <summary>
        /// Removes item from the list
        /// </summary>
        /// <param name="i"></param>
        public void Remove(SMItem i) { items.Remove(i); }
        /// <summary>
        /// Removes any item from the list
        /// </summary>
        public void RemoveAll() { items.ToList().ForEach(a => items.Remove(a)); }
        /// <summary>
        /// Prepare any item for the drawing
        /// </summary>
        /// <param name="i"></param>
        public override void Prepare(double i)
        {
            items.ForEach(a => a.Prepare(i));
        }
    }
}