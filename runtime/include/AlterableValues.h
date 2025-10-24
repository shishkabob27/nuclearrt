#pragma once

#include <vector>

class AlterableValues
{
public:
	AlterableValues() = default;
	AlterableValues(const std::vector<int>& values) : Values(values) {}

	void SetValue(int index, int value) { 
		// Resize the vector if needed
		if (index >= Values.size())
			Values.resize(index + 1, 0);
		Values[index] = value;
	}

	void AddValue(int index, int value) {
		SetValue(index, GetValue(index) + value);
	}
	
	void SubtractValue(int index, int value) {
		SetValue(index, GetValue(index) - value);
	}

	int GetValue(int index) const {
		if (index < 0 || index >= Values.size())
			return 0;
		return Values[index];
	}
private:
	std::vector<int> Values;
};