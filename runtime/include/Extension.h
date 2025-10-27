#pragma once

#include "ObjectInstance.h"
#include <vector>
#include <memory>

class Extension : public ObjectInstance {
public:
	Extension(unsigned int objectInfoHandle, int type, std::string name, int x, int y, unsigned int layer, short instanceValue)
		: ObjectInstance(objectInfoHandle, type, name, x, y, layer, instanceValue) {}
	virtual ~Extension() = default;

	virtual void Initialize() {}
	virtual void Update(float deltaTime) {}
	virtual void Draw() {}
};

 