#pragma once

#include "ObjectInstance.h"
#include <vector>
#include <memory>

class Backdrop : public ObjectInstance {
public:
	Backdrop(unsigned int objectInfoHandle, int type, std::string name, int x, int y, unsigned int layer, short instanceValue)
		: ObjectInstance(objectInfoHandle, type, name, x, y, layer, instanceValue) {}

	unsigned int ObstacleType;
	unsigned int CollisionType;
	unsigned int Image;	

	std::vector<unsigned int> GetImagesUsed() override {
		return { Image };
	}
};