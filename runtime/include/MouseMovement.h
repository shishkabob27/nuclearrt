#pragma once

#include "Movement.h"
#include <iostream>

class MouseMovement : public Movement
{
public:
	MouseMovement(unsigned short player, bool movingAtStart, int directionAtStart, int minX, int maxX, int minY, int maxY)
		: Movement(player, movingAtStart, directionAtStart), MinX(minX), MaxX(maxX), MinY(minY), MaxY(maxY) {}

	int MinX;
	int MaxX;
	int MinY;
	int MaxY;

	int disabledCursorX = 0;
	int disabledCursorY = 0;

	int initialX = 0;
	int initialY = 0;

	void Initialize() override;
	void OnEnabled() override;
	void OnDisabled() override;
	void Update(float deltaTime) override;
};