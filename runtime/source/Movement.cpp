#include "Movement.h"

#include <cmath>

#include "Active.h"

void Movement::LookAtObject(ObjectInstance* targetObject, int xOffset, int yOffset) {
	int targetX = targetObject->X + xOffset;
	int targetY = targetObject->Y + yOffset;
	int currentX = Instance->X;
	int currentY = Instance->Y;

	double dx = static_cast<double>(targetX - currentX);
	double dy = static_cast<double>(targetY - currentY);
	double angleDeg = std::atan2(-dy, dx) * 180.0 / 3.14159265358979323846;
	if (angleDeg < 0.0) angleDeg += 360.0;

	movementDirection = static_cast<int>((angleDeg / 360.0) * 32.0 + 0.5) % 32;

	static_cast<Active*>(Instance)->animations.SetCurrentDirection(movementDirection);
}

void Movement::LookAtPoint(int x, int y) {
	double dx = static_cast<double>(x - Instance->X);
	double dy = static_cast<double>(y - Instance->Y);
	double angleDeg = std::atan2(-dy, dx) * 180.0 / 3.14159265358979323846;
	if (angleDeg < 0.0) angleDeg += 360.0;

	movementDirection = static_cast<int>((angleDeg / 360.0) * 32.0 + 0.5) % 32;

	static_cast<Active*>(Instance)->animations.SetCurrentDirection(movementDirection);
}
