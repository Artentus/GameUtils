typedef float4 Vector4;
typedef float2 Vector2;
typedef float4 Color;


/*
	Vertex Shader
*/
cbuffer MatrixBuffer : register(b0)
{
	matrix World;
	matrix View;
	matrix Text;
};

struct VS_Output
{
	Vector4 Position : SV_POSITION;
	Vector2 UntransformedPosition : TEXTCOORD0;
	Vector2 TextureCoordinates : TEXTCOORD1;
	Vector2 BezierCoordinates : TEXTCOORD2;
	int Mode : MODE;
};

VS_Output VS(Vector4 position : POSITION, Vector2 textureCoordinates : TEXTCOORD0, Vector2 bezierCoordinates : TEXTCOORD1, int mode : MODE)
{
	VS_Output output;

	position = mul(position, Text);

	output.UntransformedPosition.x = position.x;
	output.UntransformedPosition.y = position.y;

	output.Position = mul(position, World);
	output.Position = mul(output.Position, View);

	output.TextureCoordinates = textureCoordinates;

	output.BezierCoordinates = bezierCoordinates;

	output.Mode = mode;

	return output;
}




/*
	Pixel Shader
*/
Texture2D CurrentTexture : register(t0);
SamplerState CurrentState : register(s0);

cbuffer BrushBuffer : register(b1)
{
	int Type;
	float Opacity;
	Vector2 Point1;
	Vector2 Point2;
	float Matrix[6];
	int ColorCount;
	Color GradientColors[16];
	float GradientPositions[16];
};

struct PS_Input
{
	Vector4 Position : SV_POSITION;
	Vector2 UntransformedPosition : TEXTCOORD0;
	Vector2 TextureCoordinates : TEXTCOORD1;
	Vector2 BezierCoordinates : TEXTCOORD2;
	int Mode : MODE;
};

Color BlendColors(Color color1, Color color2, float alpha)
{
	return (color1 * alpha) + (color2 * (1 - alpha));
}

Color InterpolateGradientColor(float gradientDistance)
{
	if (gradientDistance < GradientPositions[0])
		return GradientColors[0];
	else if (gradientDistance > GradientPositions[ColorCount - 1])
		return GradientColors[ColorCount - 1];
	else
	{
		Color color1, color2;
		float alpha;
		for (int i = ColorCount - 2; i >= 0; i--)
		{
			if (GradientPositions[i] <= gradientDistance)
			{
				color1 = GradientColors[i];
				color2 = GradientColors[i + 1];
				alpha = 1 - (gradientDistance - GradientPositions[i]) / (GradientPositions[i + 1] - GradientPositions[i]);
				break;
			}
		}
		return BlendColors(color1, color2, alpha);
	}
}

float GetLinearGradientDistance(Vector2 startPoint, Vector2 endPoint, Vector2 input)
{
	Vector2 a = endPoint - startPoint;
	Vector2 b = input - startPoint;
	float l = length(a);
	a = a / l;

	return dot(a, b) / l;
}

Color GetLinearGradientPixel(Vector2 input)
{
	Vector2 startPoint = Point1;
	Vector2 endPoint = Point2;

	float gradientDistance = GetLinearGradientDistance(startPoint, endPoint, input);
	return InterpolateGradientColor(gradientDistance);
}

Vector2 ApplyMatrix(Vector2 p)
{
	Vector2 result;
	result.x = p.x * Matrix[0] + p.y * Matrix[1] + Matrix[2];
	result.y = p.x * Matrix[3] + p.y * Matrix[4] + Matrix[5];
	return result;
}

float GetRadialGradientDistance(Vector2 input)
{
	if (Point2.x == 1)
	{
		Vector2 p = Point1;
		input = ApplyMatrix(input);
		Vector2 v = input - p;

		if (length(input) >= 1) return 1;

		float m = v.y / v.x;
		float t = p.y - p.x * m;

		float a = m * m + 1;
		float b = 2 * m * t;
		float c = t * t - 1;

		float d = b * b - 4 * a * c;
		if (d < 0) return 1;

		d = sqrt(d);
		float x1 = (-b - d) / (2 * a);
		float x2 = (-b + d) / (2 * a);

		Vector2 p1;
		p1.x = x1;
		p1.y = m * x1 + t;
		Vector2 v1 = p1 - p;

		Vector2 p2;
		p2.x = x2;
		p2.y = m * x2 + t;
		Vector2 v2 = p2 - p;

		if (v1.x / v.x < 0)
		{
			return distance(input, p) / distance(p, p2);
		}
		else
		{
			if (v2.x / v.x > 0)
			{
				if (distance(p, p1) > distance(p, p2))
					return distance(input, p) / distance(p, p1);
				else
					return distance(input, p) / distance(p, p2);
			}
			else
			{
				return distance(input, p) / distance(p, p1);
			}
		}
	}
	else
	{
		Vector2 p = ApplyMatrix(input);
		return length(p);
	}
}

Color GetRadialGradientPixel(Vector2 input)
{
	float gradientDistance = GetRadialGradientDistance(input);
	return InterpolateGradientColor(gradientDistance);
}

Color GetTextureBrushPixel(Vector2 coordinates)
{
	return CurrentTexture.Sample(CurrentState, coordinates);
}

Color GetPixel(PS_Input input)
{
	Color pixelColor = 0;

	switch (Type)
	{
	case 1:
		pixelColor = GradientColors[0];
		break;
	case 2:
		pixelColor = GetLinearGradientPixel(input.UntransformedPosition);
		break;
	case 3:
		pixelColor = GetRadialGradientPixel(input.UntransformedPosition);
		break;
	case 4:
		pixelColor = GetTextureBrushPixel(input.TextureCoordinates);
		break;
	default:
		discard;
		break;
	}

	if (input.Mode > 0)
	{
		Vector2 p = input.BezierCoordinates;

		float2 px = ddx(p);
			float2 py = ddy(p);

			float fx = (2 * p.x)*px.x - px.y;
		float fy = (2 * p.x)*py.x - py.y;

		float sd = (p.x*p.x - p.y) / sqrt(fx*fx + fy*fy);

		float alpha = 0.5 - sd;
		if (input.Mode == 2) alpha = 1 - alpha;

		if (alpha < 0)
			discard;
		else if (alpha < 1)
			pixelColor.a *= alpha;
	}
	

	pixelColor.a *= Opacity;
	return pixelColor;
}

Color PS(PS_Input input) : SV_TARGET
{
	Color pixel = GetPixel(input);
	return pixel;
}