#pragma once

#include <memory>
#include "ObjectInfo.h"

class ObjectInstance {
public:
    ObjectInstance(unsigned int handle, std::shared_ptr<ObjectInfo> objectInfo, int x, int y, unsigned int layer, short instanceValue)
        : Handle(handle), OI(objectInfo), X(x), Y(y), Layer(layer), InstanceValue(instanceValue) {}
    virtual ~ObjectInstance() = default;

	unsigned int Handle = 0;

	std::shared_ptr<ObjectInfo> OI;

	int X = 0;
	int Y = 0;

	unsigned int Layer = 0;

	short InstanceValue = 0;
	
	unsigned int GetAngle() const {
		return Angle;
	}

	void SetAngle(int angle) {
		//limit to 0-359 degrees
		while (angle < 0) {
			angle += 360;
		}
		while (angle >= 360) {
			angle -= 360;
		}
		
		Angle = angle;
	}

private:
	unsigned int Angle = 0;
};
