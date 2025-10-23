#pragma once

#include <unordered_map>
#include <memory>

#include "Movement.h"

class Movements
{
public:
	Movements() = default;
	Movements(const std::vector<std::shared_ptr<Movement>>& movementItems) : items(movementItems) {
		currentMovementIndex = 0;
	}

	std::vector<std::shared_ptr<Movement>> items;
	unsigned int currentMovementIndex = 0;

	void Update(float deltaTime) {
		if (items[currentMovementIndex] == nullptr) return;
		items[currentMovementIndex]->Update(deltaTime);
	}

	void SetMovement(int index) {
		currentMovementIndex = index;
	}

	std::shared_ptr<Movement> GetCurrentMovement() {
		return items[currentMovementIndex];
	}
};