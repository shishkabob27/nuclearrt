#pragma once

#include <vector>
#include <memory>

#include "CounterBase.h"

class Score : public CounterBase {
public:
	Score(unsigned int objectInfoHandle, int type, std::string name, int x, int y, unsigned int layer, short instanceValue)
		: CounterBase(objectInfoHandle, type, name, x, y, layer, instanceValue) {}
};