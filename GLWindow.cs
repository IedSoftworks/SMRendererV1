﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using SMRenderer.Renderers;

namespace SMRenderer
{
    /// <summary>
    /// The actual window-class. Is child of the OpenTK GameWindow-class
    /// </summary>
    public class GLWindow : GameWindow
    {
        #region | Normal Rendering |
        static bool _preloaded = false;
        private Matrix4 _viewHUD = Matrix4.LookAt(0, 0, 1, 0, 0, 0, 0, 1, 0);
        private Matrix4 _projectionHUD;
        public Matrix4 viewProjectionHUD;

        private Matrix4 _projectionMatrix = new Matrix4();
        private Matrix4 _viewProjectionMatrix = new Matrix4();

        private static GLWindow M_WINDOW;

        private Renderer renderProgram;

        private Object _bloomObj;
        private BloomRenderer _bloomRenderer;
        private Matrix4 _modelMatrixBloom = new Matrix4();

        public Mouse mouse;
        public Camera camera;

        public Matrix4 ViewProjection { get { return _viewProjectionMatrix; } }
        public Matrix4 Projection { get { return _projectionMatrix; } }

        /// <summary>
        /// Represens the renderer to the public
        /// </summary>
        public Renderer Renderer { get { return renderProgram; } }

        /// <summary>
        /// Represens the current window to the public
        /// </summary>
        public static GLWindow Window { get { return M_WINDOW; } }

        /// <summary>
        /// Represens the current Size of the drawing board
        /// </summary>
        public Vector2 pxSize { get; private set; } = new Vector2(0);

        public GLWindow(int width, int height) : base(width, height)
        {
            Title = "GLWíndow Default Title";
            M_WINDOW = this;

            mouse = new Mouse { window = this };
            camera = new Camera { window = this };

            MouseMove += mouse.SaveMousePosition;
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!Focused && Configure.OnlyRenderIfFocused) return;

            Timer.TickChange(e.Time);
            Animations.Animation.Update(e.Time);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferIdMain);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _viewProjectionMatrix = camera.Calculate();

            GL.UseProgram(renderProgram.mProgramId);
            SM.List.ForEach(a => a.Prepare());
            foreach(SMItem item in SM.List.OrderBy(a => a._RenderOrder))
            {
                item.Draw();
            }

            DownsampleFramebuffer();
            ApplyBloom();

            SwapBuffers();
        }
        protected override void OnLoad(EventArgs e)
        {
            renderProgram = new Renderer(this);
            Preload();

            _bloomRenderer = new BloomRenderer();
            _bloomObj = ObjectManager.OB["Quad"];

            if (Configure.UseScale)
            {
                float aspect = (float)Width / Height;
                pxSize = new Vector2(Configure.Scale, Configure.Scale / aspect);
                _projectionHUD = Matrix4.CreateOrthographicOffCenter(0, pxSize.X, pxSize.Y, 0, 0.1f, 100f);
            }
            else
            {
                pxSize = new Vector2(Width, Height);
                _projectionHUD = Matrix4.CreateOrthographicOffCenter(0, Width, Height, 0, 0.1f, 100f);
            }
            camera.zoomfactor = 1;

            viewProjectionHUD = _viewHUD * _projectionHUD;

            TargetUpdateFrequency = Configure.UpdatePerSecond;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.DepthTest);

            GL.ClearColor(Configure.ClearColor);

            base.OnLoad(e);
        }
        public static void Preload()
        {
            if (!_preloaded)
            {
                ObjectManager.LoadObj();
                Texture.CreateEmpty();
                _preloaded = true;
            }
        }
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle);

            if (Configure.UseScale)
            {
                float aspect = (float)Width / Height;
                pxSize = new Vector2(Configure.Scale, Configure.Scale / aspect);
                _projectionHUD = Matrix4.CreateOrthographicOffCenter(0, pxSize.X, pxSize.Y, 0, 0.1f, 100f);
            }
            else
            {
                pxSize = new Vector2(Width, Height);
                 _projectionHUD = Matrix4.CreateOrthographicOffCenter(0, Width, Height, 0, 0.1f, 100f);
            }
            camera.CalcOrtho();

            viewProjectionHUD = _viewHUD * _projectionHUD;
            _modelMatrixBloom = Matrix4.CreateScale(Width, Height, 1) *
                Matrix4.LookAt(0, 0, 1, 0, 0, 0, 0, 1, 0) *
                Matrix4.CreateOrthographic(Width, Height, 0.1f, 100f);

            InitializeFramebuffers(0);
        }
        #endregion
        private void ApplyBloom()
        {
            GL.UseProgram(_bloomRenderer.mProgramId);

            int loopCount = 4;
            int sourceTex;
            for (int i = 0; i<loopCount; i++)
            {
                if (i%2 == 0)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferIdBloom1);
                    if (i == 0) sourceTex = _framebufferTextureBloomDownsampled;
                    else sourceTex = _framebufferTextureBloom2;
                }
                else
                {
                    sourceTex = _framebufferTextureBloom1;
                    if (i == loopCount - 1) GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    else GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebufferIdBloom2);
                }

                _bloomRenderer.DrawBloom(
                    _bloomObj,
                    ref _modelMatrixBloom,
                    i % 2 == 0,
                    i == loopCount - 1,
                    Width,
                    Height,
                    _framebufferTextureMainDownsampled,
                    sourceTex);
            }
            GL.UseProgram(0);
        }

        #region | Framebuffers |
        private int _framebufferIdMain = -1;
        private int _framebufferIdMainDownsampled = -1;
        private int _framebufferIdBloom1 = -1;
        private int _framebufferIdBloom2 = -1;

        private int _framebufferTextureMain = -1;
        private int _framebufferTextureMainDownsampled = -1;
        private int _framebufferTextureBloom = -1;
        private int _framebufferTextureBloom1 = -1;
        private int _framebufferTextureBloom2 = -1;
        private int _framebufferTextureBloomDownsampled = -1;

        private void InitializeFramebuffers(int fsaa)
        {
            DeleteFramebuffers();

            // Sometimes, frame buffer initialization fails
            // if the window gets resized too often.
            // I found no better way around this:
            Thread.Sleep(50);

            InitFramebufferMain(fsaa);
            InitFramebufferMainDownsampled();
            InitFramebufferBloom();
        }

        private void DeleteFramebuffers()
        {
            GL.DeleteTextures(6, new int[] { _framebufferTextureMain, _framebufferTextureMainDownsampled, _framebufferTextureBloom, _framebufferTextureBloom1, _framebufferTextureBloom2, _framebufferTextureBloomDownsampled });
            GL.DeleteFramebuffers(4, new int[] { _framebufferIdMain, _framebufferIdMainDownsampled, _framebufferIdBloom1, _framebufferIdBloom2 });
        }

        private void InitFramebufferMain(int fsaa)
        {
            int framebufferId = -1;
            int renderedTexture = -1;
            int renderedTextureAttachment = -1;
            int renderbufferFSAA = -1;
            int renderbufferFSAA2 = -1;

            framebufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);

            renderedTexture = GL.GenTexture();
            renderedTextureAttachment = GL.GenTexture();


            GL.DrawBuffers(2, new DrawBuffersEnum[2] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });

            GL.BindTexture(TextureTarget.Texture2D, renderedTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);


            renderbufferFSAA = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbufferFSAA);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, fsaa, RenderbufferStorage.Rgba8, Width, Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, renderbufferFSAA);

            GL.BindTexture(TextureTarget.Texture2D, renderedTextureAttachment);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);

            renderbufferFSAA2 = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbufferFSAA2);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, fsaa, RenderbufferStorage.Rgba8, Width, Height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, RenderbufferTarget.Renderbuffer, renderbufferFSAA2);

            FramebufferErrorCode code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("GL_FRAMEBUFFER_COMPLETE failed. Cannot use FrameBuffer object.");
            }
            else
            {
                _framebufferIdMain = framebufferId;
                _framebufferTextureMain = renderedTexture;
                _framebufferTextureBloom = renderedTextureAttachment;
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void InitFramebufferMainDownsampled()
        {
            int framebufferId = -1;
            int renderedTexture = -1;
            int renderedTextureAttachment = -1;

            framebufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);

            renderedTexture = GL.GenTexture();
            renderedTextureAttachment = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, renderedTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureParameterName.ClampToEdge);


            GL.BindTexture(TextureTarget.Texture2D, renderedTextureAttachment);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureParameterName.ClampToEdge);

            GL.DrawBuffers(2, new DrawBuffersEnum[2] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderedTexture, 0);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, renderedTextureAttachment, 0);

            FramebufferErrorCode code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("GL_FRAMEBUFFER_COMPLETE failed. Cannot use FrameBuffer object.");
            }
            else
            {
                _framebufferIdMainDownsampled = framebufferId;
                _framebufferTextureMainDownsampled = renderedTexture;
                _framebufferTextureBloomDownsampled = renderedTextureAttachment;
            }


        }
        private void InitFramebufferBloom()
        {
            int framebufferTempId = -1;
            int renderedTextureTemp = -1;

            // =========== TEMP #1 ===========
            framebufferTempId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferTempId);

            renderedTextureTemp = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, renderedTextureTemp);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureParameterName.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderedTextureTemp, 0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            FramebufferErrorCode code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("GL_FRAMEBUFFER_COMPLETE failed. Cannot use FrameBuffer object.");
            }
            else
            {
                _framebufferIdBloom1 = framebufferTempId;
                _framebufferTextureBloom1 = renderedTextureTemp;
            }

            // =========== TEMP 2 ===========
            int framebufferId = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);

            int renderedTextureTemp2 = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, renderedTextureTemp2);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureParameterName.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderedTextureTemp2, 0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            code = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (code != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("GL_FRAMEBUFFER_COMPLETE failed. Cannot use FrameBuffer object.");
            }
            else
            {
                _framebufferIdBloom2 = framebufferId;
                _framebufferTextureBloom2 = renderedTextureTemp2;
            }
        }

        private void DownsampleFramebuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _framebufferIdMain);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _framebufferIdMainDownsampled);

            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

            GL.ReadBuffer(ReadBufferMode.ColorAttachment1);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment1);
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
        }
        #endregion
    }
}