#pragma once

#include "Movement.h"
#include <iostream>

class EightDirectionsMovement : public Movement
{
public:
	EightDirectionsMovement(unsigned short player, bool movingAtStart, int directionAtStart, short speed, short acceleration, short deceleration, short bounceFactor, int directions)
		: Movement(player, movingAtStart, directionAtStart), Speed(speed), Acceleration(acceleration), Deceleration(deceleration), BounceFactor(bounceFactor), Directions(directions) {}

	short Speed;
	short Acceleration;
	short Deceleration;
	short BounceFactor;
	int Directions;

	float realSpeed = 0;

	int GetRealSpeed() override { return static_cast<int>(realSpeed); }
	int GetMovementDirection() override { return movementDirection; }
	
	void Update(float deltaTime) override;
};