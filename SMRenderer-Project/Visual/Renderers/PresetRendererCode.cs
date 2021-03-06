﻿using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using SMRenderer.Visual.Drawing;

namespace SMRenderer.Visual.Renderers
{
    internal class PresetRendererCode
    {
        public static void AddRangeIfNotExist(List<string> list, string[] addArray)
        {
            foreach (string a in addArray)
                if (!list.Contains(a))
                    list.Add(a);
        }

        public static void UniformEssencal(GenericRenderer renderer)
        {
            AddRangeIfNotExist(renderer.RequestedAttrib, new[] {"aPosition", "aTexture"});
            AddRangeIfNotExist(renderer.RequestedFragData, new[] {"color"});
            AddRangeIfNotExist(renderer.RequestedUniforms, new[] {"uMVP", "uObjectSize"});
        }

        public static void DrawEssencal(GenericRenderer r, Matrix4 mvp, Vector2 size)
        {
            GL.UniformMatrix4(r.Uniforms["uMVP"], false, ref mvp);
            GL.Uniform2(r.Uniforms["uObjectSize"], size);
        }


        public static void UniformLighting(GenericRenderer renderer)
        {
            AddRangeIfNotExist(renderer.RequestedAttrib, new[] {"aNormal", "aPosition"});
            AddRangeIfNotExist(renderer.RequestedFragData, new[] {"color", "bloom"});
            AddRangeIfNotExist(renderer.RequestedUniforms, new[]
            {
                "uN",
                "uM",

                "uLightAllowed",
                "uLightPositions",
                "uLightColors",
                "uLightDirections",
                "uLightCount",

                "uAmbientLight"
            });
        }

        public static void DrawLighting(GenericRenderer r, Matrix4 modelm, Matrix4 normalm, VisualEffectArgs args)
        {
            GL.UniformMatrix4(r.Uniforms["uM"], false, ref modelm);
            GL.UniformMatrix4(r.Uniforms["uN"], false, ref normalm);

            GL.Uniform1(r.Uniforms["uLightAllowed"], args.AffectedByLight ? 1 : 0);
            GL.Uniform4(r.Uniforms["uLightPositions"], Scene.Current.lights.Count,
                Scene.Current.lights.ShaderArgsPositions);
            GL.Uniform4(r.Uniforms["uLightColors"], Scene.Current.lights.Count, Scene.Current.lights.ShaderArgsColors);
            GL.Uniform3(r.Uniforms["uLightDirections"], Scene.Current.lights.Count,
                Scene.Current.lights.ShaderArgsDirections);
            GL.Uniform1(r.Uniforms["uLightCount"], Scene.Current.lights.Count);

            GL.Uniform4(r.Uniforms["uAmbientLight"], Scene.Current.ambientLight);
        }

        public static void UniformDepth(GenericRenderer r)
        {
            AddRangeIfNotExist(r.RequestedAttrib, new[] { "aPosition"});
            AddRangeIfNotExist(r.RequestedFragData, new[] { "color" });
            AddRangeIfNotExist(r.RequestedUniforms, new [] {"uDepthSettings"});
        }

        public static void DrawDepth(GenericRenderer r)
        {
            GL.Uniform3(r.Uniforms["uDepthSettings"], Scene.Current.depthSettings.ShaderArgument());
        }

        public static void UniformTexturing(GenericRenderer renderer)
        {
            AddRangeIfNotExist(renderer.RequestedAttrib, new[] {"aPosition", "aTexture"});
            AddRangeIfNotExist(renderer.RequestedFragData, new[] {"color"});
            AddRangeIfNotExist(renderer.RequestedUniforms, new[]
            {
                "uColor",

                "uTexture",
                "uTexSize",
                "uTexPosition",

                "uActiveForm",
                "uForm",
            });
        }

        public static void DrawTexturing(GenericRenderer r, Color4 color, int mainTex, TextureHandler handler)
        {
            GL.Uniform4(r.Uniforms["uColor"], color);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, mainTex);
            GL.Uniform1(r.Uniforms["uTexture"], 0);

            GL.Uniform2(r.Uniforms["uTexSize"], handler.TexSize);
            GL.Uniform2(r.Uniforms["uTexPosition"], handler.TexPosition);
        }

        public static void DrawTexturing(GenericRenderer r, Color4 color, int mainTex, TextureHandler handler, int formTex)
        {
            DrawTexturing(r, color, mainTex, handler);

            GL.Uniform1(r.Uniforms["uActiveForm"], 1);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, mainTex);
            GL.Uniform1(r.Uniforms["uForm"], 1);
        }


        public static void UniformBloom(GenericRenderer r)
        {
            AddRangeIfNotExist(r.RequestedFragData, new[] {"color", "bloom"});
            AddRangeIfNotExist(r.RequestedUniforms, new[] {"uBloomUsage", "uBloomColor", "uColor"});
        }

        public static void DrawBloom(GenericRenderer r, Color4 objectColor, VisualEffectArgs effectArgs)
        {
            GL.Uniform1(r.Uniforms["uBloomUsage"], (int) effectArgs.BloomUsage);
            GL.Uniform4(r.Uniforms["uBloomColor"], effectArgs.BloomColor);
        }
    }
}