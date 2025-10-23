#pragma once

#include <unordered_map>
#include <memory>

#include "ObjectInstance.h"

class Movement
{
public:
	Movement() = default;
	Movement(unsigned short player, bool movingAtStart, int directionAtStart)
		: Player(player), MovingAtStart(movingAtStart), DirectionAtStart(directionAtStart) {}

	unsigned short Player;
	bool MovingAtStart;
	int DirectionAtStart;

	ObjectInstance* Instance;

	virtual ~Movement() = default;
	virtual void Initialize() {}
	virtual int GetRealSpeed() { return 0; }
	virtual int GetMovementDirection() { return 0; } // 0-31 with 0 being right and going counter-clockwise
	virtual void Update(float deltaTime) {}
};