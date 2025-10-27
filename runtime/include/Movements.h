#pragma once

#include <unordered_map>
#include <memory>

#include "Movement.h"

class Movements
{
public:
	Movements() = default;
	Movements(const std::unordered_map<int, Movement*> movementItems) : items(movementItems) {
		currentMovementIndex = 0;

		if (!items.empty() && items.find(currentMovementIndex) != items.end()) {
			items.find(currentMovementIndex)->second->OnEnabled();
		}
	}

	std::unordered_map<int, Movement*> items;
	unsigned int currentMovementIndex = 0;

	void Update(float deltaTime) {
		if (items.empty() || items.find(currentMovementIndex) == items.end()) return;
		items.at(currentMovementIndex)->Update(deltaTime);
	}

	void SetMovement(int index) {
		if (items.find(currentMovementIndex) != items.end()) {
			items.at(currentMovementIndex)->OnDisabled();
		}

		currentMovementIndex = index;

		if (items.find(currentMovementIndex) != items.end()) {
			items.at(currentMovementIndex)->OnEnabled();
		}	
	}

	Movement* GetCurrentMovement() {
		if (!items.empty() && items.find(currentMovementIndex) != items.end()) return items.find(currentMovementIndex)->second;
		return nullptr;
	}
};