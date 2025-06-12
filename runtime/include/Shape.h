#pragma once

class Shape
{
public:
	Shape(
		bool flipX = false,
		bool flipY = false,
		int borderSize = 0,
		int borderColor = 0,
		int shapeType = 0,
		int fillType = 0,
		int color1 = 0,
		int color2 = 0,
		bool verticalGradient = false,
		unsigned int image = 0
	)
		: FlipX(flipX), FlipY(flipY), BorderSize(borderSize), BorderColor(borderColor), ShapeType(shapeType),
		FillType(fillType), Color1(color1), Color2(color2), VerticalGradient(verticalGradient), Image(image) {}
	~Shape() = default;

	bool FlipX = false;
	bool FlipY = false;

	int BorderSize = 0;
	int BorderColor = 0;

	int ShapeType = 0;
	
	int FillType = 0;

	int Color1 = 0;
	int Color2 = 0;

	bool VerticalGradient = false;

	unsigned int Image;
};