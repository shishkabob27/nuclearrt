#pragma once

class Value
{
public:
	Value(int defaultValue, int minValue, int maxValue)
		: DefaultValue(defaultValue), MinValue(minValue), MaxValue(maxValue), currentValue(defaultValue) {}
	~Value() = default;

	int DefaultValue;
	int MinValue;
	int MaxValue;

	// Getters and setters
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
	int currentValue;
};
