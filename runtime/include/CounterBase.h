#pragma once

#include <vector>
#include <memory>

#include "ObjectInstance.h"
#include "AlterableValues.h"
#include "AlterableStrings.h"
#include "AlterableFlags.h"
#include "Shape.h"


class CounterBase : public ObjectInstance {
public:
	CounterBase(unsigned int objectInfoHandle, int type, std::string name)
		: ObjectInstance(objectInfoHandle, type, name) {}

	bool Visible = true;
	bool FollowFrame = false;

	AlterableValues Values;
	AlterableStrings Strings;
	AlterableFlags Flags;
	
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

	Shape oShape;

	std::vector<unsigned int> Frames;

	unsigned int Font = 0;

	std::vector<unsigned int> GetImagesUsed() override {
		return Frames;
	}
	
	std::vector<unsigned int> GetFontsUsed() override {
		return { Font };
	}
};