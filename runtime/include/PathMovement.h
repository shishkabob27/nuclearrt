#pragma once

#include <iostream>

#include "Movement.h"
#include "PathNode.h"

class PathMovement : public Movement
{
public:
	PathMovement(unsigned short player, bool movingAtStart, int directionAtStart, short minimumSpeed, short maximumSpeed, bool loop, bool repositionAtEnd, bool reverseAtEnd, std::vector<PathNode> nodes)
		: Movement(player, movingAtStart, directionAtStart), MinimumSpeed(minimumSpeed), MaximumSpeed(maximumSpeed), Loop(loop), RepositionAtEnd(repositionAtEnd), ReverseAtEnd(reverseAtEnd), Nodes(nodes) {}

	short MinimumSpeed;
	short MaximumSpeed;
	bool Loop;
	bool RepositionAtEnd;
	bool ReverseAtEnd;
	std::vector<PathNode> Nodes;

	int originX = 0;
	int originY = 0;

	int currentNodeIndex = 0;
	bool movingForward = true;

	void Initialize() override;
	void Start() override;
	void Stop() override;
	void Update(float deltaTime) override;
	int GetRealSpeed() override;
	int GetMovementDirection() override;
};