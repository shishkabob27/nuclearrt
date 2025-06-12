#pragma once

#include <string>
#include <memory>
#include "ObjectInfoProperties.h"

class ObjectInfo
{
public:
	ObjectInfo(unsigned int handle, int type, const std::string& name, int rgbCoefficient, int effect, char blendCoefficient, unsigned int effectParameter, std::shared_ptr<ObjectInfoProperties> properties)
		: Handle(handle), Type(type), Name(name), RGBCoefficient(rgbCoefficient), Effect(effect), BlendCoefficient(blendCoefficient), EffectParameter(effectParameter), Properties(properties) {}

	unsigned int Handle;
	int Type;
	std::string Name;
	int RGBCoefficient;
	int Effect = 0;
	char BlendCoefficient = 0; // Alpha
	unsigned int EffectParameter = 0; 
	std::shared_ptr<ObjectInfoProperties> Properties;
};

