﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsGraphicsDevice;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Color = Microsoft.Xna.Framework.Color;
using Microsoft.Xna.Framework.Content;
using Graph;
using System.Runtime.InteropServices;

namespace EvoNet.Controls
{
    struct GraphCache
    {
        public VertexBuffer VertexAreaBuffer;
        public IndexBuffer IndexAreaBuffer;
        public VertexBuffer VertexLineBuffer;
        public int NumElements;
    }

    struct VertexPosition2 : IVertexType
    {
        private static readonly VertexDeclaration InternalVertexDeclaration;

        static VertexPosition2()
        {
            InternalVertexDeclaration = new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0)
            );
        }

        public VertexPosition2(Vector2 Position)
        {
            this.Position = Position;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return InternalVertexDeclaration;
            }
        }

        public Vector2 Position;
    }

    public class GraphControl : GraphicsDeviceControl
    {
        public GraphControl()
        {

        }

        private Dictionary<string, IGraphValueList> graphs = new Dictionary<string, IGraphValueList>();
        private Dictionary<IGraphValueList, GraphCache> graphCaches = new Dictionary<IGraphValueList, GraphCache>();
        public void Add(string name, IGraphValueList list)
        {
            graphs.Add(name, list);
        }

        VertexPositionColor[] Elements = new VertexPositionColor[6];
        Effect Effect;
        protected override void Initialize()
        {
            base.Initialize();
            ContentManager manager = new ContentManager(Services, "Content");
            Effect = manager.Load<Effect>("Color");
        }

        List<VertexPosition2> vertexAreaCache = new List<VertexPosition2>();
        List<VertexPosition2> vertexLineCache = new List<VertexPosition2>();
        List<int> indexCache = new List<int>();

        private void DrawGraph(IGraphValueList graph)
        {
            if (graph.Count <= 1)
            {
                return;
            }

            GraphCache cache;

            bool needsRedraw = true;
            if (graphCaches.TryGetValue(graph, out cache))
            {
                if (cache.NumElements == graph.Count)
                {
                    needsRedraw = false;
                }
            }


            if (needsRedraw)
            {
                int CurrentVertexIndex = 0;
                Func<IGraphValue, decimal> GetY = (value) => { return value.DisplayValue; };
                Func<IGraphValue, decimal> GetX = (value) => { return value.DisplayPosition; };
                decimal maxY = graph.Max(GetY);
                decimal minY = graph.Min(GetY);
                decimal maxX = graph.Max(GetX);
                decimal minX = graph.Min(GetX);

                Func<decimal, float> GetRelativeY = (decimal alpha) =>
                {
                    if(minY == maxY)
                    {
                        return 0.0f;
                    }
                    return ((float)((alpha - minY) / (maxY - minY)) - 0.5f) * 2.0f;
                };

                Func<decimal, float> GetRelativeX = (decimal alpha) =>
                {
                    return ((float)((alpha - minX) / (maxX - minX)) - 0.5f) * 2.0f;
                };

                vertexAreaCache.Clear();
                vertexLineCache.Clear();
                indexCache.Clear();

                VertexPosition2 upperPoint = new VertexPosition2();
                VertexPosition2 lowerPoint = new VertexPosition2();

                upperPoint.Position = new Vector2(-1, GetRelativeY(graph[0].DisplayValue));
                lowerPoint.Position = new Vector2(-1, -1);

                vertexAreaCache.Add(upperPoint);
                vertexAreaCache.Add(lowerPoint);

                float lineWidth = (5.0f / Height) / 2.0f;
                Vector2 lineWidthOffset = new Vector2(0.0f, lineWidth/2.0f);

                vertexLineCache.Add(new VertexPosition2(upperPoint.Position + lineWidthOffset));
                vertexLineCache.Add(new VertexPosition2(upperPoint.Position - lineWidthOffset));

                CurrentVertexIndex = 2;

                Vector2 lastUpperPoint;

                for (int GraphIndex = 1; GraphIndex < graph.Count; GraphIndex++)
                {
                    float x = GetRelativeX(graph[GraphIndex].DisplayPosition);
                    float y = GetRelativeY(graph[GraphIndex].DisplayValue);

                    lastUpperPoint = upperPoint.Position;

                    upperPoint.Position = new Vector2(x, y);
                    lowerPoint.Position = new Vector2(x, -1);
                    vertexAreaCache.Add(upperPoint);
                    vertexAreaCache.Add(lowerPoint);

                    // Calculate orthogonal vector of direction from last point to this one,
                    // so the line keeps its thickness along slopes
                    // Without this sloped graphs would get very thin
                    Vector2 dirFromLastPoint = upperPoint.Position - lastUpperPoint;
                    dirFromLastPoint.Normalize();
                    Vector2 orthogonalDir = new Vector2(-dirFromLastPoint.Y, dirFromLastPoint.X);

                    vertexLineCache.Add(new VertexPosition2(upperPoint.Position + orthogonalDir * lineWidth));
                    vertexLineCache.Add(new VertexPosition2(upperPoint.Position - orthogonalDir * lineWidth));

                    // Counter Clockwise Tris get culled in our current setting
                    // So draw 2 triangles in clockwise order
                    // -2---+0
                    //  |   /|
                    //  |  / |
                    //  | /  |
                    //  |/   |
                    // -1---+1
                    int LastUpperPoint = CurrentVertexIndex - 2;
                    int LastLowerPoint = CurrentVertexIndex - 1;
                    int CurrentUpperPoint = CurrentVertexIndex;
                    int CurrentLowerPoint = CurrentVertexIndex + 1;
                    indexCache.Add(LastUpperPoint);
                    indexCache.Add(CurrentUpperPoint);
                    indexCache.Add(LastLowerPoint);

                    indexCache.Add(LastLowerPoint);
                    indexCache.Add(CurrentUpperPoint);
                    indexCache.Add(CurrentLowerPoint);

                    CurrentVertexIndex += 2;

                }
                cache.VertexLineBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPosition2), CurrentVertexIndex, BufferUsage.WriteOnly);
                cache.VertexLineBuffer.SetData(vertexLineCache.ToArray());

                cache.VertexAreaBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPosition2), CurrentVertexIndex, BufferUsage.WriteOnly);
                cache.VertexAreaBuffer.SetData(vertexAreaCache.ToArray());
                cache.IndexAreaBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indexCache.Count, BufferUsage.WriteOnly);
                cache.IndexAreaBuffer.SetData(indexCache.ToArray());

                cache.NumElements = graph.Count;
                graphCaches[graph] = cache;
            }
            Effect.Parameters["Color"].SetValue(
                new Vector4(
                    graph.Color.R / 255.0f,
                    graph.Color.G / 255.0f,
                    graph.Color.B / 255.0f,
                    (float)graph.PlaneAlpha));
            GraphicsDevice.Indices = cache.IndexAreaBuffer;
            GraphicsDevice.SetVertexBuffer(cache.VertexAreaBuffer);
            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, cache.VertexAreaBuffer.VertexCount, 0, cache.IndexAreaBuffer.IndexCount / 3);
            }
            Effect.Parameters["Color"].SetValue(
                new Vector4(
                    graph.Color.R / 255.0f,
                    graph.Color.G / 255.0f,
                    graph.Color.B / 255.0f,
                    (float)graph.GraphAlpha));
            GraphicsDevice.SetVertexBuffer(cache.VertexLineBuffer);
            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, cache.VertexLineBuffer.VertexCount, 0, cache.IndexAreaBuffer.IndexCount / 3);
            }
        }

        protected override void Draw(GameTime gameTime)
        {

            //GraphicsDevice.RasterizerState.MultiSampleAntiAlias = true;
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            if (pp.MultiSampleCount < 4)
            {
                pp.MultiSampleCount = 4;
                GraphicsDevice.Reset(pp);
            }
            GraphicsDevice.Clear(Color.White);
            foreach (IGraphValueList graph in graphs.Values)
            {
                DrawGraph(graph);
            }

            base.Draw(gameTime);
        }
    }
}
