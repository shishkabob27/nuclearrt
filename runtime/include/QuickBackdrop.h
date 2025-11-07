#pragma once

#include <vector>
#include <memory>

#include "ObjectInstance.h"
#include "Shape.h"

class QuickBackdrop : public ObjectInstance {
public:
	QuickBackdrop(unsigned int objectInfoHandle, int type, std::string name)
		: ObjectInstance(objectInfoHandle, type, name) {}

	unsigned int ObstacleType;
	unsigned int CollisionType;
	int Width;
    int Height;
	Shape shape;

	std::vector<unsigned int> GetImagesUsed() override {
		return { shape.Image };
	}
};