#include "EightDirectionsMovement.h"
#include "Application.h"

#include "Active.h"

void EightDirectionsMovement::Update(float deltaTime) {
	bool moved = false;
	int wishX = 0;
	int wishY = 0;

	deltaTime *= 10;

	if (Application::Instance().GetInput()->IsControlsDown(Player, 1)) // up
	{
		wishY -= 1;
		moved = true;
	}
	if (Application::Instance().GetInput()->IsControlsDown(Player, 2)) // down
	{
		wishY += 1;
		moved = true;
	}
	if (Application::Instance().GetInput()->IsControlsDown(Player, 4)) // left
	{
		wishX -= 1;
		moved = true;
	}
	if (Application::Instance().GetInput()->IsControlsDown(Player, 8)) // right
	{
		wishX += 1;
		moved = true;
	}

	if (moved)
	{
		if (wishX == 0 && wishY < 0) // up
			movementDirection = 8;
		else if (wishX > 0 && wishY < 0) // up-right
			movementDirection = 4;
		else if (wishX > 0 && wishY == 0) // right
			movementDirection = 0;
		else if (wishX > 0 && wishY > 0) // down-right
			movementDirection = 28;
		else if (wishX == 0 && wishY > 0) // down
			movementDirection = 24;
		else if (wishX < 0 && wishY > 0) // down-left
			movementDirection = 20;
		else if (wishX < 0 && wishY == 0) // left
			movementDirection = 16;
		else if (wishX < 0 && wishY < 0) // up-left
			movementDirection = 12;

		//accelerate
		realSpeed += Acceleration * deltaTime;
		if (realSpeed > Speed)
			realSpeed = Speed;
	}
	else
	{
		//decelerate
		realSpeed -= Deceleration * deltaTime;
		if (realSpeed < 0)
			realSpeed = 0;
	}
	
	float xDifference = realSpeed * deltaTime * cos(movementDirection * 3.14159265358979323846f / 16);
	float yDifference = -realSpeed * deltaTime * sin(movementDirection * 3.14159265358979323846f / 16);

	Instance->X += xDifference;
	Instance->Y += yDifference;

	if (!((Active*)Instance)->AutomaticRotation ) {
		((Active*)Instance)->Animations.SetCurrentDirection(movementDirection);
	}
}