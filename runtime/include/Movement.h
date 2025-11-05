#pragma once

#include <unordered_map>
#include <memory>
#include <vector>

#include "ObjectInstance.h"
#include "Application.h"

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

	bool stopped = false; //set by events

	int movementDirection = 0;

	virtual ~Movement() = default;
	virtual void Initialize() {} // called for each movement at the start of the frame
	virtual void OnEnabled() {} // called when the movement is switched to
	virtual void OnDisabled() {} // called when the movement is switched to another movement
	virtual int GetRealSpeed() { return 0; }
	virtual bool IsStopped() { return GetRealSpeed() == 0; }
	virtual void SetMovementDirection(int directionMask) { 
		if (directionMask == 0) return;
		
		std::vector<int> directions;
		for (int i = 0; i < 32; i++) {
			if (directionMask & (1 << i)) {
				directions.push_back(i);
			}
		}
		//todo: only set valid directions
		movementDirection = directions[Application::Instance().RandomRange(0, static_cast<short>(directions.size() - 1))];
	}
	virtual int GetMovementDirection() { return movementDirection; } // 0-31 with 0 being right and going counter-clockwise
	virtual void Start() {}
	virtual void Stop() {}
	virtual void Update(float deltaTime) {}
};