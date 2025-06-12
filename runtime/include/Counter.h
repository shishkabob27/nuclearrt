#pragma once

#include "Shape.h"
#include <vector>
#include <memory>

class Counter
{
public:
	int Width;
	int Height;

	short Player;

	unsigned int DisplayType;

	bool IntDigitPadding;
	bool FloatWholePadding;
	bool FloatDecimalPadding;
	bool FloatPadding;
	bool BarDirection;

	char IntDigitCount;
	char FloatWholeCount;
	char FloatDecimalCount;

	unsigned int Font;

	std::vector<unsigned int> Frames;

	std::shared_ptr<Shape> oShape;

	Counter(
		int width = 0,
		int height = 0,
		short player = 0,
		unsigned int displayType = 0,
		bool intDigitPadding = false,
		bool floatWholePadding = false,
		bool floatDecimalPadding = false,
		bool floatPadding = false,
		bool barDirection = false,
		char intDigitCount = 0,
		char floatWholeCount = 0,
		char floatDecimalCount = 0,
		unsigned int font = 0,
		std::vector<unsigned int> frames = {},
		std::shared_ptr<Shape> shape = nullptr
	)
		: Width(width), Height(height), Player(player), DisplayType(displayType), IntDigitPadding(intDigitPadding),
		FloatWholePadding(floatWholePadding), FloatDecimalPadding(floatDecimalPadding), FloatPadding(floatPadding),
		BarDirection(barDirection), IntDigitCount(intDigitCount), FloatWholeCount(floatWholeCount),
		FloatDecimalCount(floatDecimalCount), Font(font), Frames(frames), oShape(shape) {}
};