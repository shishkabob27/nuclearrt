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

	void NextMovement() {
		if (items.empty() || items.find(currentMovementIndex) == items.end()) return;

		if (currentMovementIndex < items.size() - 1) {
			currentMovementIndex++;
		} else {
			return;
		}

		items.at(currentMovementIndex - 1)->OnDisabled();
		items.at(currentMovementIndex)->OnEnabled();
	}

	void PreviousMovement() {
		if (items.empty() || items.find(currentMovementIndex) == items.end()) return;

		if (currentMovementIndex > 0) {
			currentMovementIndex--;
		} else {
			return;
		}

		items.at(currentMovementIndex + 1)->OnDisabled();
		items.at(currentMovementIndex)->OnEnabled();
	}

	Movement* GetCurrentMovement() {
		if (!items.empty() && items.find(currentMovementIndex) != items.end()) return items.find(currentMovementIndex)->second;
		return nullptr;
	}
};