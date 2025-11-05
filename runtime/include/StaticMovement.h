#pragma once

#include "Movement.h"

class StaticMovement : public Movement
{
public:
	StaticMovement(unsigned short player, bool movingAtStart, int directionAtStart)
		: Movement(player, movingAtStart, directionAtStart) {}
};