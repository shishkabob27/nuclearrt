#pragma once

#include "ObjectInfoProperties.h"
#include <vector>
#include <memory>

class ObjectInstance;

class Extension : public ObjectInfoProperties {
public:
	Extension() = default;
	virtual ~Extension() = default;

	void SetInstance(ObjectInstance* inst) { instance = inst; }
	ObjectInstance* GetInstance() const { return instance; }

	virtual void Initialize() {}
	virtual void Update(float deltaTime) {}
	virtual void Draw() {}

protected:
	ObjectInstance* instance = nullptr;
};

 