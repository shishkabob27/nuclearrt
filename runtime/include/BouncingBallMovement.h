#pragma once

#include "Movement.h"

class BouncingBallMovement : public Movement
{
public:
	BouncingBallMovement(unsigned short player, bool movingAtStart, int directionAtStart, short speed, short randomizer, short angles, short security, short deceleration)
		: Movement(player, movingAtStart, directionAtStart), speed(speed), randomizer(randomizer), angles(angles), security(security), deceleration(deceleration) {}

	short speed;
	short randomizer;
	short angles;
	short security;
	short deceleration;

	float realSpeed = 0;

	int GetRealSpeed() override { return static_cast<int>(realSpeed); }
	int GetMovementDirection() override { return movementDirection; }
	
	void OnEnabled() override;
	void Update(float deltaTime) override;
};