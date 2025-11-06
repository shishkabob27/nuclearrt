#pragma once

#include "ObjectInstance.h"
#include <vector>
#include <memory>

#include "Animations.h"
#include "AlterableValues.h"
#include "AlterableStrings.h"
#include "AlterableFlags.h"
#include "Movements.h"

class Active : public ObjectInstance {
public:
	Active(unsigned int objectInfoHandle, int type, std::string name)
		: ObjectInstance(objectInfoHandle, type, name) {}

	Animations animations;
	AlterableValues Values;
	AlterableStrings Strings;
	AlterableFlags Flags;
	Movements movements;

	bool Visible = true;
	bool FollowFrame = false;
	bool AutomaticRotation = false;
	bool FineDetection = false;

	std::vector<unsigned int> GetImagesUsed() override {
		return animations.GetImagesUsed();
	}
};

 