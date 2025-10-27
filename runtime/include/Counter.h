#pragma once

#include <vector>
#include <memory>

#include "CounterBase.h"

class Counter : public CounterBase {
public:
	Counter(unsigned int objectInfoHandle, int type, std::string name, int x, int y, unsigned int layer, short instanceValue)
		: CounterBase(objectInfoHandle, type, name, x, y, layer, instanceValue) {}
	
	int DefaultValue = 0;
	int MinValue = 0;
	int MaxValue = 0;

	int GetValue() const { return currentValue; }
	void SetValue(int value)
	{
		if (value < MinValue)
		{
			currentValue = MinValue;
		}
		else if (value > MaxValue)
		{
			currentValue = MaxValue;
		}
		else
		{
			currentValue = value;
		}
	}

	void AddValue(int value)
	{
		SetValue(currentValue + value);
	}

	void SubtractValue(int value)
	{
		SetValue(currentValue - value);
	}

private:
	int currentValue = 0;
};