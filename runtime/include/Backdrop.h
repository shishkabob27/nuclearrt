#pragma once

#include "ObjectInstance.h"
#include <vector>
#include <memory>

class Backdrop : public ObjectInstance {
public:
	Backdrop(unsigned int objectInfoHandle, int type, std::string name)
		: ObjectInstance(objectInfoHandle, type, name) {}

	unsigned int ObstacleType;
	unsigned int CollisionType;
	unsigned int Image;	

	std::vector<unsigned int> GetImagesUsed() override {
		return { Image };
	}
};