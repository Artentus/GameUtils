using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using TK = SharpDX.Toolkit.Graphics;

namespace GameUtils.Graphics
{
    public sealed partial class Renderer : IEngineComponent, IDisposable
    {
        struct MatrixBuffer
        {
            public Matrix World;
            public Matrix View;
            public Matrix Text;
        }

        readonly ISurface surface;
        D3D11.Texture2D surfaceTexture;
        D3D11.RenderTargetView surfaceTarget;
        D3D11.ShaderResourceView surfaceView;

        D3D11.Device device;
        TK.GraphicsDevice tkDevice;
        DXGI.SwapChain1 swapChain;
        D3D11.DeviceContext deviceContext;
        D3D11.BlendState blendState;

        D3D11.DepthStencilState defaultDepthStencilState;
        D3D11.DepthStencilState clipDepthStencilState;
        D3D11.DepthStencilState clippingDepthStencilState;
        D3D11.Texture2D depthStencilBuffer;
        D3D11.DepthStencilView depthStencilView;

        D3D11.VertexShader vertexShader;
        D3D11.PixelShader pixelShader;
        DXGI.SampleDescription sampleDescription;

        const int BufferCount = 3;
        const int IndexBufferStartSize = 0x06;
        const int VertexBufferStartSize = 0x04;
        const int BufferSizeStep = 6;
        D3D11.Buffer[] indexBuffers;
        D3D11.Buffer[] vertexBuffers;
        int currentBufferIndex;
        D3D11.Buffer indexBuffer;
        D3D11.Buffer vertexBuffer;

        Matrix viewMatrix;
        Matrix worldMatrix;
        Matrix textMatrix;
        D3D11.Buffer matrixBuffer;

        D3D11.ShaderResourceView currentTexture;
        WrapMode currentWrapMode;
        InterpolationMode currentInterpolationMode;
        D3D11.SamplerState samplerState;
        D3D11.Buffer brushBuffer;

        bool resized;

        Stopwatch sw;
        double refreshRate;

        void InitializeShaders(ShaderBytecode vertexShaderBytecode, ShaderBytecode pixelShaderBytecode)
        {
            vertexShader = new D3D11.VertexShader(device, vertexShaderBytecode);
            deviceContext.VertexShader.Set(vertexShader);
            pixelShader = new D3D11.PixelShader(device, pixelShaderBytecode);
            deviceContext.PixelShader.Set(pixelShader);

            D3D11.InputElement[] inputElements = new[]
            {
                new D3D11.InputElement("POSITION", 0, DXGI.Format.R32G32B32A32_Float, 0),
                new D3D11.InputElement("TEXTCOORD", 0, DXGI.Format.R32G32_Float, 0),
                new D3D11.InputElement("TEXTCOORD", 1, DXGI.Format.R32G32_Float, 0),
                new D3D11.InputElement("MODE", 0, DXGI.Format.R32_UInt, 0), 
            };
            deviceContext.InputAssembler.InputLayout = new D3D11.InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderBytecode), inputElements);
        }

        unsafe void InitializeInner(AntiAliasingMode antiAliasingMode)
        {
            worldMatrix = Matrix.Identity;
            textMatrix = Matrix.Identity;

            device = new D3D11.Device(DriverType.Hardware,
                D3D11.DeviceCreationFlags.None,
                new[] { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0 });
            tkDevice = TK.GraphicsDevice.New(device);
            deviceContext = device.ImmediateContext;
            deviceContext.Rasterizer.State = device.CreateRasterizerState();

            //TODO: replace with precompiled bytecode
            const string shaderFile = @"..\..\..\GameUtils\Graphics\shaders.fx";
            ShaderBytecode vertexShaderBytecode = ShaderBytecode.CompileFromFile(shaderFile, "VS", "vs_4_0", ShaderFlags.Debug);
            ShaderBytecode pixelShaderBytecode = ShaderBytecode.CompileFromFile(shaderFile, "PS", "ps_4_0", ShaderFlags.Debug);
            InitializeShaders(vertexShaderBytecode, pixelShaderBytecode);

            indexBuffers = new D3D11.Buffer[BufferCount];
            vertexBuffers = new D3D11.Buffer[BufferCount];
            for (int i = 0, indexBufferSize = IndexBufferStartSize, vertexBufferSize = VertexBufferStartSize;
                i < BufferCount;
                i++, indexBufferSize <<= BufferSizeStep, vertexBufferSize <<= BufferSizeStep)
            {
                indexBuffers[i] = device.CreateDynamicBuffer(sizeof(int) * indexBufferSize, D3D11.BindFlags.IndexBuffer);
                vertexBuffers[i] = device.CreateDynamicBuffer(sizeof(Vertex) * vertexBufferSize, D3D11.BindFlags.VertexBuffer);
            }
            currentBufferIndex = 0;
            indexBuffer = indexBuffers[0];
            vertexBuffer = vertexBuffers[0];

            //indexBuffer = device.CreateDynamicBuffer(sizeof(int) * IndexBufferSize, D3D11.BindFlags.IndexBuffer);
            //vertexBuffer = device.CreateDynamicBuffer(sizeof(Vertex) * VertexBufferSize, D3D11.BindFlags.VertexBuffer);
            matrixBuffer = device.CreateConstantBuffer(sizeof(MatrixBuffer));
            brushBuffer = device.CreateConstantBuffer(sizeof(BrushBuffer));

            deviceContext.InputAssembler.SetIndexBuffer(indexBuffer, DXGI.Format.R32_UInt, 0);
            deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(vertexBuffer, sizeof(Vertex), 0));
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            blendState = device.CreateBlendState();
            deviceContext.OutputMerger.SetBlendState(blendState);

            currentWrapMode = WrapMode.Clamp;
            currentInterpolationMode = InterpolationMode.Linear;
            samplerState = device.CreateSamplerState(WrapMode.Clamp, InterpolationMode.Linear);
            deviceContext.PixelShader.SetSampler(0, samplerState);

            sampleDescription = device.GetMultisamplingLevel(antiAliasingMode);

            defaultDepthStencilState = device.CreateDepthStencilState(D3D11.Comparison.Always,
                D3D11.StencilOperation.Keep,
                D3D11.StencilOperation.Keep);
            clipDepthStencilState = device.CreateDepthStencilState(D3D11.Comparison.Never,
                D3D11.StencilOperation.Replace,
                D3D11.StencilOperation.Keep);
            clippingDepthStencilState = device.CreateDepthStencilState(D3D11.Comparison.Equal,
                D3D11.StencilOperation.Keep,
                D3D11.StencilOperation.Keep);
            deviceContext.OutputMerger.SetDepthStencilState(defaultDepthStencilState);
        }

        unsafe void UpdateMatrixBuffer()
        {
            var mb = new MatrixBuffer() { View = viewMatrix, World = worldMatrix, Text = textMatrix};

            deviceContext.UpdateSubresource(matrixBuffer, 0, null, (IntPtr)(&mb), 1, 0);
            deviceContext.VertexShader.SetConstantBuffer(0, matrixBuffer);
        }

        void Initialize(IWin32Window window, AntiAliasingMode antiAliasingMode)
        {
            InitializeInner(antiAliasingMode);

            var swapChainDesc = device.CreateSwapChainDescription();
            DXGI.Factory2 factory = device.QueryInterface<DXGI.Device1>().Adapter.GetParent<DXGI.Factory2>();
            swapChain = new DXGI.SwapChain1(factory, device, window.Handle, ref swapChainDesc);
        }

        void Initialize(ComObject coreWindow, AntiAliasingMode antiAliasingMode)
        {
            InitializeInner(antiAliasingMode);

            var swapChainDesc = device.CreateSwapChainDescription();
            DXGI.Factory2 factory = device.QueryInterface<DXGI.Device1>().Adapter.GetParent<DXGI.Factory2>();
            swapChain = new DXGI.SwapChain1(factory, device, coreWindow, ref swapChainDesc);
        }

        /// <summary>
        /// Creates a renderer for a surface.
        /// </summary>
        /// <param name="surface">The target surface.</param>
        /// <param name="antiAliasingMode">The desired level of multisampling to be used by the renderer.</param>
        /// <remarks>The specified multisampling level is not guaranteed to be used if it is not supported by the current hardware. Instead the highest possible level is used.</remarks>
        /// <exception cref="System.ArgumentException">The specified surface is not a valid Win32 Window or Core Window.</exception>
        public Renderer(ISurface surface, AntiAliasingMode antiAliasingMode = AntiAliasingMode.None)
        {
            this.surface = surface;
            surface.Resize += (sender, e) => resized = true;
            resized = true;

            IWin32Window win32Window = surface.OutputTarget as IWin32Window;
            if (win32Window != null)
            {
                Initialize(win32Window, antiAliasingMode);
                return;
            }

            ComObject coreWindow = surface.OutputTarget as ComObject;
            if (coreWindow != null)
            {
                Initialize(coreWindow, antiAliasingMode);
                return;
            }

            throw new ArgumentException("Invalid output target, must be either a Win32 Window or a Core Window.");
        }

        void ResizeSurface()
        {
            Math.Rectangle newBounds = surface.Bounds;
            SurfaceSize = newBounds.Size;
            SurfaceBounds = newBounds;

            deviceContext.OutputMerger.ResetTargets();
            swapChain.ResizeBuffers(0, (int)newBounds.Width, (int)newBounds.Height, DXGI.Format.R8G8B8A8_UNorm, DXGI.SwapChainFlags.None);
            deviceContext.Rasterizer.SetViewport(0, 0, newBounds.Width, newBounds.Height);

            viewMatrix = Matrix.Transpose(Matrix.Scaling(2f / newBounds.Width, -2f / newBounds.Height, 1));
            viewMatrix *= Matrix.Transpose(Matrix.Translation(-newBounds.Width / 2f, -newBounds.Height / 2f, 0));
            UpdateMatrixBuffer();

            depthStencilView?.Dispose();
            depthStencilBuffer?.Dispose();
            surfaceTarget?.Dispose();
            surfaceView?.Dispose();
            surfaceTexture?.Dispose();

            depthStencilBuffer = device.CreateDepthStencilBuffer((int)newBounds.Width, (int)newBounds.Height, sampleDescription, out depthStencilView);
            surfaceTexture = device.CreateSurface((int)newBounds.Width, (int)newBounds.Height, sampleDescription, out surfaceTarget);
            deviceContext.OutputMerger.SetTargets(depthStencilView, surfaceTarget);
            surfaceView = new D3D11.ShaderResourceView(device, surfaceTexture);
        }

        internal void BeginRender(Color4 clearingColor)
        {
            if (sw == null)
            {
                sw = new Stopwatch();
                refreshRate = Dwm.DisplayRefreshRate * 2;
                sw.Start();
            }

            if (resized)
            {
                resized = false;
                ResizeSurface();
            }

            deviceContext.OutputMerger.SetDepthStencilState(defaultDepthStencilState);
            deviceContext.ClearDepthStencilView(depthStencilView, D3D11.DepthStencilClearFlags.Depth | D3D11.DepthStencilClearFlags.Stencil, 1, 0);
            deviceContext.ClearRenderTargetView(surfaceTarget, new SharpDX.Color4(clearingColor.R, clearingColor.G, clearingColor.B, 1));
        }

        internal void EndRender()
        {
            if (SyncInterval > 0 || sw.Elapsed.TotalMilliseconds >= 1000.0 / refreshRate)
            {
                D3D11.Texture2D backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0);
                deviceContext.ResolveSubresource(surfaceTexture, 0, backBuffer, 0, DXGI.Format.R8G8B8A8_UNorm);
                backBuffer.Dispose();

                swapChain.Present(SyncInterval, DXGI.PresentFlags.None);
                sw.Restart();
            }
        }

        void SetTextureInner(D3D11.ShaderResourceView texture)
        {
            if (texture != currentTexture)
            {
                currentTexture = texture;
                deviceContext.PixelShader.SetShaderResource(0, texture);
            }
        }

        unsafe void SetTexture(Texture texture, float opacity)
        {
            SetTextureInner(texture.ResourceView);

            var buffer = new BrushBuffer() { Type = 4, Opacity = opacity };
            deviceContext.UpdateSubresource(brushBuffer, 0, null, (IntPtr)(&buffer), 0, 0);
            deviceContext.PixelShader.SetConstantBuffer(1, brushBuffer);
        }

        void SetSamplerState(WrapMode wrapMode, InterpolationMode interpolationMode)
        {
            if (wrapMode != currentWrapMode || interpolationMode != currentInterpolationMode)
            {
                D3D11.SamplerState newSamplerState = device.CreateSamplerState(wrapMode, interpolationMode);
                deviceContext.PixelShader.SetSampler(0, newSamplerState);

                samplerState.Dispose();
                samplerState = newSamplerState;
                currentWrapMode = wrapMode;
                currentInterpolationMode = interpolationMode;
            }
        }

        unsafe void SetBrushBuffer(BrushBuffer buffer)
        {
            deviceContext.UpdateSubresource(brushBuffer, 0, null, (IntPtr)(&buffer), 0, 0);
            deviceContext.PixelShader.SetConstantBuffer(1, brushBuffer);
        }

        void SetBrush(Brush brush)
        {
            SetBrushBuffer(brush.CreateBuffer());

            var textureBrush = brush as TextureBrush;
            if (textureBrush != null)
            {
                SetSamplerState(textureBrush.WrapMode, textureBrush.InterpolationMode == InterpolationMode.Default ? defaultInterpolationMode : textureBrush.InterpolationMode);
                SetTextureInner(textureBrush.Texture.ResourceView);
            }
        }

        unsafe void DrawVertices(int[] indices, Vertex[] vertices, int bufferIndex)
        {
            if (bufferIndex != currentBufferIndex)
            {
                indexBuffer = indexBuffers[bufferIndex];
                vertexBuffer = vertexBuffers[bufferIndex];
                currentBufferIndex = bufferIndex;

                deviceContext.InputAssembler.SetIndexBuffer(indexBuffer, DXGI.Format.R32_UInt, 0);
                deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(vertexBuffer, sizeof(Vertex), 0));
            }

            DataStream stream;
            deviceContext.MapSubresource(indexBuffer, D3D11.MapMode.WriteDiscard, D3D11.MapFlags.None, out stream);
            stream.Position = 0;
            stream.WriteRange(indices);
            deviceContext.UnmapSubresource(indexBuffer, 0);
            stream.Dispose();

            deviceContext.MapSubresource(vertexBuffer, D3D11.MapMode.WriteDiscard, D3D11.MapFlags.None, out stream);
            stream.Position = 0;
            stream.WriteRange(vertices);
            deviceContext.UnmapSubresource(vertexBuffer, 0);
            stream.Dispose();

            deviceContext.DrawIndexed(indices.Length, 0, 0);
        }

        void DrawVertices(int[] indices, Vertex[] vertices)
        {
            for (int i = 0, indexBufferSize = IndexBufferStartSize, vertexBufferSize = VertexBufferStartSize;
                 i < BufferCount;
                 i++, indexBufferSize <<= BufferSizeStep, vertexBufferSize <<= BufferSizeStep)
            {
                if (indices.Length <= indexBufferSize && vertices.Length <= vertexBufferSize)
                {
                    DrawVertices(indices, vertices, i);
                    break;
                }
            }
        }

        internal D3D11.ShaderResourceView CreateTexture(string fileName, out TK.Texture2D texture)
        {
            texture = TK.Texture2D.Load(tkDevice, fileName);
            return new D3D11.ShaderResourceView(device, (D3D11.Texture2D)texture);
        }

        internal D3D11.ShaderResourceView CreateTexture(Stream stream, out TK.Texture2D texture)
        {
            texture = TK.Texture2D.Load(tkDevice, stream);
            return new D3D11.ShaderResourceView(device, (D3D11.Texture2D)texture);
        }

        

















































        object IEngineComponent.Tag
        {
            get { return null; }
        }
        bool IEngineComponent.IsCompatibleTo(IEngineComponent component)
        {
            return !(component is Renderer);
        }

        public void Dispose()
        {
            
        }
    }
}
