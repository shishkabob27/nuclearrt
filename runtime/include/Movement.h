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

	bool stopped = false; //set by events

	virtual ~Movement() = default;
	virtual void Initialize() {} // called for each movement at the start of the frame
	virtual void OnEnabled() {} // called when the movement is switched to
	virtual void OnDisabled() {} // called when the movement is switched to another movement
	virtual int GetRealSpeed() { return 0; }
	virtual bool IsStopped() { return GetRealSpeed() == 0; }
	virtual int GetMovementDirection() { return 0; } // 0-31 with 0 being right and going counter-clockwise
	virtual void Start() {}
	virtual void Stop() {}
	virtual void Update(float deltaTime) {}
};