#pragma once

#include <vector>
#include <memory>

#include "ObjectInstance.h"
#include "Shape.h"

class QuickBackdrop : public ObjectInstance {
public:
	QuickBackdrop(unsigned int objectInfoHandle, int type, std::string name, int x, int y, unsigned int layer, short instanceValue)
		: ObjectInstance(objectInfoHandle, type, name, x, y, layer, instanceValue) {}

	unsigned int ObstacleType;
	unsigned int CollisionType;
	int Width;
    int Height;
	Shape Shape;

	std::vector<unsigned int> GetImagesUsed() override {
		return { Shape.Image };
	}
};