#pragma once

#include <string>
#include <vector>

class ObjectInstance;

class Layer {
public:
	Layer(std::string name, float XCoefficient, float YCoefficient)
		: Name(name)
		, XCoefficient(XCoefficient)
		, YCoefficient(YCoefficient)
	{
	}

	std::string Name;

	float XCoefficient;
	float YCoefficient;

	std::vector<ObjectInstance*> instances;
};
