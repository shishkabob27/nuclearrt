#pragma once

#include <vector>
#include <memory>

#include "CounterBase.h"

class Score : public CounterBase {
public:
	Score(unsigned int objectInfoHandle, int type, std::string name)
		: CounterBase(objectInfoHandle, type, name) {}
};