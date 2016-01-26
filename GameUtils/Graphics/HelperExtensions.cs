using System;
using GameUtils.Math;
using SharpDX;
using DXGI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace GameUtils.Graphics
{
    static class HelperExtensions
    {
        public static SharpDX.Color4 ToSharpDXColor(this Color4 color)
        {
            return new SharpDX.Color4(color.R, color.G, color.B, color.A);
        }

        public static Matrix ToSharpDXMatrix(this Matrix2x3 matrix)
        {
            var m = Matrix.Identity;
            m.ScaleVector = new Vector3(matrix[0, 0], matrix[1, 1], 1f);
            m.TranslationVector = new Vector3(matrix.OffsetX, matrix.OffsetY, 0f);
            m.M12 = matrix[1, 0];
            m.M21 = matrix[0, 1];
            m.Transpose();
            return m;
        }

        public static SharpDX.Vector2 ToSharpDXVector(this Math.Vector2 vector)
        {
            return new SharpDX.Vector2(vector.X, vector.Y);
        }

        public static D3D.RasterizerState CreateRasterizerState(this D3D.Device device)
        {
            var rasterizerDesc = new D3D.RasterizerStateDescription()
            {
                CullMode = D3D.CullMode.None,
                IsAntialiasedLineEnabled = true,
                IsMultisampleEnabled = true,
                FillMode = D3D.FillMode.Solid,
                IsDepthClipEnabled = true,
            };
            return new D3D.RasterizerState(device, rasterizerDesc);
        }

        public static DXGI.SampleDescription GetMultisamplingLevel(this D3D.Device device, AntiAliasingMode antiAliasingMode)
        {
            int desiredSampleCount = (int)((uint)antiAliasingMode & 0x0000FFFFu);
            int desiredQuality = (int)((uint)antiAliasingMode >> 16);

            int sampleCount = desiredSampleCount;
            int quality = desiredQuality;
            bool csaaModeFound = false;

            if (desiredQuality > 0) //CSAA is queried
            {
                for (; sampleCount > 0x4; sampleCount >>= 1)
                {
                    quality = device.CheckMultisampleQualityLevels(DXGI.Format.R8G8B8A8_UNorm, sampleCount) - 1;
                    if (quality >= desiredQuality)
                    {
                        csaaModeFound = true;
                        break;
                    }
                    if ((quality >>= 1) >= desiredQuality)
                    {
                        csaaModeFound = true;
                        break;
                    }
                    quality = desiredQuality;
                }
            }

            if (!csaaModeFound) //no CSAA available or not queried
            {
                sampleCount = desiredSampleCount;
                if (desiredQuality > 0) sampleCount <<= 1; //double queried sample count for equal quality

                for (; sampleCount > 0x1; sampleCount >>= 1)
                {
                    quality = device.CheckMultisampleQualityLevels(DXGI.Format.R8G8B8A8_UNorm, sampleCount) - 1;
                    if (quality >= 0) break;
                }
            }

            return new DXGI.SampleDescription(desiredSampleCount, quality);
        }

        private static bool IsWindows8OrNewer()
        {
            Version osVersion = Environment.OSVersion.Version;
            return osVersion.Major >= 6 && osVersion.Minor >= 2;
        }

        public static DXGI.SwapChainDescription1 CreateSwapChainDescription(this D3D.Device device)
        {
            return new DXGI.SwapChainDescription1()
            {
                BufferCount = 3,
                Flags = DXGI.SwapChainFlags.AllowModeSwitch,
                Format = DXGI.Format.R8G8B8A8_UNorm,
                Width = 0,
                Height = 0,
                SampleDescription = new DXGI.SampleDescription(1, 0),
                Scaling = IsWindows8OrNewer() ? DXGI.Scaling.None : DXGI.Scaling.Stretch,
                SwapEffect = DXGI.SwapEffect.FlipSequential,
                Usage = DXGI.Usage.RenderTargetOutput,
                Stereo = false,
            };
        }

        public static D3D.Texture2D CreateSurface(this D3D.Device device, int width, int height, DXGI.SampleDescription sampleDescription, out D3D.RenderTargetView renderTarget)
        {
            var desc = new D3D.Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = DXGI.Format.R8G8B8A8_UNorm,
                SampleDescription = sampleDescription,
                Usage = D3D.ResourceUsage.Default,
                BindFlags = D3D.BindFlags.RenderTarget | D3D.BindFlags.ShaderResource,
                CpuAccessFlags = D3D.CpuAccessFlags.None,
                OptionFlags = D3D.ResourceOptionFlags.Shared,
            };
            var surface = new D3D.Texture2D(device, desc);

            var targetDesc = new D3D.RenderTargetViewDescription
            {
                Format = desc.Format,
                Dimension = desc.SampleDescription.Count == 1
                        ? D3D.RenderTargetViewDimension.Texture2D
                        : D3D.RenderTargetViewDimension.Texture2DMultisampled,
            };
            renderTarget = new D3D.RenderTargetView(device, surface, targetDesc);

            return surface;
        }

        public static D3D.Texture2D CreateDepthStencilBuffer(this D3D.Device device, int width, int height, DXGI.SampleDescription sampleDescription, out D3D.DepthStencilView view)
        {
            var desc = new D3D.Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = DXGI.Format.D24_UNorm_S8_UInt,
                SampleDescription = sampleDescription,
                Usage = D3D.ResourceUsage.Default,
                BindFlags = D3D.BindFlags.DepthStencil,
                CpuAccessFlags = D3D.CpuAccessFlags.None,
                OptionFlags = D3D.ResourceOptionFlags.None,
            };
            var depthStencilBuffer = new D3D.Texture2D(device, desc);

            view = new D3D.DepthStencilView(device, depthStencilBuffer);
            return depthStencilBuffer;
        }

        public static D3D.Buffer CreateConstantBuffer(this D3D.Device device, int sizeInBytes)
        {
            var desc = new D3D.BufferDescription((int)System.Math.Ceiling(sizeInBytes / 16f) * 16,
                D3D.ResourceUsage.Default,
                D3D.BindFlags.ConstantBuffer,
                D3D.CpuAccessFlags.None,
                D3D.ResourceOptionFlags.None,
                0);

            return new D3D.Buffer(device, desc);
        }

        public static D3D.Buffer CreateDynamicBuffer(this D3D.Device device, int sizeInBytes, D3D.BindFlags bindFlags)
        {
            var desc = new D3D.BufferDescription((int)System.Math.Ceiling(sizeInBytes / 16f) * 16,
                D3D.ResourceUsage.Dynamic,
                bindFlags,
                D3D.CpuAccessFlags.Write,
                D3D.ResourceOptionFlags.None,
                0);

            return new D3D.Buffer(device, desc);
        }

        public static D3D.DepthStencilState CreateDepthStencilState(this D3D.Device device, D3D.Comparison comparison, D3D.StencilOperation failOperation, D3D.StencilOperation passOperation)
        {
            var operationDesc = new D3D.DepthStencilOperationDescription()
            {
                Comparison = comparison,
                DepthFailOperation = D3D.StencilOperation.Keep,
                FailOperation = failOperation,
                PassOperation = passOperation,
            };
            var depthStencilDesc = new D3D.DepthStencilStateDescription()
            {
                IsDepthEnabled = false,
                IsStencilEnabled = true,
                StencilReadMask = byte.MaxValue,
                StencilWriteMask = byte.MaxValue,
                FrontFace = operationDesc,
                BackFace = operationDesc,
            };
            return new D3D.DepthStencilState(device, depthStencilDesc);
        }

        public static D3D.SamplerState CreateSamplerState(this D3D.Device device, WrapMode wrapMode, InterpolationMode interpolationMode)
        {
            var desc = new D3D.SamplerStateDescription();

            if (wrapMode.HasFlag(WrapMode.Tile))
            {
                desc.AddressU = D3D.TextureAddressMode.Wrap;
                desc.AddressV = D3D.TextureAddressMode.Wrap;
            }
            else
            {
                desc.AddressU = D3D.TextureAddressMode.Clamp;
                desc.AddressV = D3D.TextureAddressMode.Clamp;
            }
            if (wrapMode.HasFlag(WrapMode.MirrorX)) desc.AddressU = D3D.TextureAddressMode.Mirror;
            if (wrapMode.HasFlag(WrapMode.MirrorY)) desc.AddressV = D3D.TextureAddressMode.Mirror;
            desc.AddressW = D3D.TextureAddressMode.Clamp;

            switch (interpolationMode)
            {
                case InterpolationMode.Linear:
                    desc.Filter = D3D.Filter.MinMagMipLinear;
                    break;
                case InterpolationMode.Anisotropic:
                    desc.Filter = D3D.Filter.Anisotropic;
                    break;
                default:
                    desc.Filter = D3D.Filter.MinLinearMagMipPoint;
                    break;
            }

            desc.ComparisonFunction = D3D.Comparison.Never;
            desc.MaximumAnisotropy = 16;
            desc.MinimumLod = 0;
            desc.MaximumLod = float.MaxValue;

            var samplerState = new D3D.SamplerState(device, desc);
            return samplerState;
        }

        public static D3D.BlendState CreateBlendState(this D3D.Device device)
        {
            var blendDesc = new D3D.BlendStateDescription();
            blendDesc.RenderTarget[0].IsBlendEnabled = true;
            blendDesc.RenderTarget[0].SourceBlend = D3D.BlendOption.SourceAlpha;
            blendDesc.RenderTarget[0].DestinationBlend = D3D.BlendOption.InverseSourceAlpha;
            blendDesc.RenderTarget[0].BlendOperation = D3D.BlendOperation.Add;
            blendDesc.RenderTarget[0].SourceAlphaBlend = D3D.BlendOption.One;
            blendDesc.RenderTarget[0].DestinationAlphaBlend = D3D.BlendOption.Zero;
            blendDesc.RenderTarget[0].AlphaBlendOperation = D3D.BlendOperation.Add;
            blendDesc.RenderTarget[0].RenderTargetWriteMask = D3D.ColorWriteMaskFlags.All;
            return new D3D.BlendState(device, blendDesc);
        }
    }
}
