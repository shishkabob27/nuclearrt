#pragma once

#include <vector>
#include <memory>

#include "CounterBase.h"

class Lives : public CounterBase {
public:
	Lives(unsigned int objectInfoHandle, int type, std::string name, int x, int y, unsigned int layer, short instanceValue)
		: CounterBase(objectInfoHandle, type, name, x, y, layer, instanceValue) {}
};