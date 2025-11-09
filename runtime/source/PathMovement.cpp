#include "PathMovement.h"

#include <cmath>

void PathMovement::Initialize() {
	originX = Instance->X;
	originY = Instance->Y;
	currentNodeIndex = 0;
}

void PathMovement::OnEnabled() {
	originX = Instance->X;
	originY = Instance->Y;
	currentNodeIndex = 0;
}

void PathMovement::Start() {
	stopped = false;
}

void PathMovement::Stop() {
	stopped = true;
}

void PathMovement::Update(float deltaTime) {
	if (Nodes.empty()) {
		return;
	}

	if (stopped) {
		return;
	}

	if (currentNodeIndex >= static_cast<int>(Nodes.size())) {
		if (Loop) {
			bool wasMovingForward = movingForward;
			
			if (RepositionAtEnd) {
				Instance->X = static_cast<float>(originX);
				Instance->Y = static_cast<float>(originY);
			} else {
				if (!ReverseAtEnd) {
					originX = static_cast<int>(Instance->X);
					originY = static_cast<int>(Instance->Y);
				}
			}

			if (ReverseAtEnd) {
				movingForward = !movingForward;
				if (wasMovingForward && !movingForward) {
					int endX = originX;
					int endY = originY;
					for (size_t i = 0; i < Nodes.size(); ++i) {
						endX += Nodes[i].DestinationX;
						endY += Nodes[i].DestinationY;
					}
					Instance->X = static_cast<float>(endX);
					Instance->Y = static_cast<float>(endY);
				}
			}

			if (movingForward) {
				currentNodeIndex = 0;
			} else {
				currentNodeIndex = static_cast<int>(Nodes.size()) - 1;
			}

		} else {
			if (RepositionAtEnd) {
				Instance->X = static_cast<float>(originX);
				Instance->Y = static_cast<float>(originY);
				stopped = true;
			}
			return;
		}
	}
	
	if (currentNodeIndex < 0 && !movingForward) {
		float dx = static_cast<float>(originX) - Instance->X;
		float dy = static_cast<float>(originY) - Instance->Y;
		float distanceToOrigin = sqrtf(dx * dx + dy * dy);
		
		if (distanceToOrigin <= 0.0001f) {
			Instance->X = static_cast<float>(originX);
			Instance->Y = static_cast<float>(originY);
			
			if (Loop && ReverseAtEnd) {
				movingForward = !movingForward;
				currentNodeIndex = 0;
			} else {
				return;
			}
		} else {
			float scaledDelta = deltaTime * 10.0f;
			int nodeSpeed = Nodes.empty() ? MinimumSpeed : static_cast<int>(Nodes[0].Speed);
			if (nodeSpeed < MinimumSpeed) nodeSpeed = MinimumSpeed;
			if (nodeSpeed > MaximumSpeed) nodeSpeed = MaximumSpeed;
			
			float step = nodeSpeed * scaledDelta;
			if (step >= distanceToOrigin) {
				Instance->X = static_cast<float>(originX);
				Instance->Y = static_cast<float>(originY);
				
				if (Loop && ReverseAtEnd) {
					movingForward = !movingForward;
					currentNodeIndex = 0;
				} else {
					return;
				}
			} else {
				float nx = dx / distanceToOrigin;
				float ny = dy / distanceToOrigin;
				Instance->X += nx * step;
				Instance->Y += ny * step;
			}
		}
		return;
	}

	int targetX = originX;
	int targetY = originY;
	if (movingForward) {
		for (int i = 0; i <= currentNodeIndex; ++i) {
			targetX += Nodes[i].DestinationX;
			targetY += Nodes[i].DestinationY;
		}
	} else {
		for (int i = 0; i <= currentNodeIndex; ++i) {
			targetX += Nodes[i].DestinationX;
			targetY += Nodes[i].DestinationY;
		}
	}

	deltaTime *= 10.0f;

	float dx = static_cast<float>(targetX) - Instance->X;
	float dy = static_cast<float>(targetY) - Instance->Y;
	float distanceToTarget = sqrtf(dx * dx + dy * dy);

	if (distanceToTarget <= 0.0001f) {
		Instance->X = static_cast<float>(targetX);
		Instance->Y = static_cast<float>(targetY);
		currentNodeIndex += movingForward ? 1 : -1;
		return;
	}

	int nodeSpeed = static_cast<int>(Nodes[currentNodeIndex].Speed);
	if (nodeSpeed < MinimumSpeed) nodeSpeed = MinimumSpeed;
	if (nodeSpeed > MaximumSpeed) nodeSpeed = MaximumSpeed;

	float step = nodeSpeed * deltaTime;
	if (step >= distanceToTarget) {
		Instance->X = static_cast<float>(targetX);
		Instance->Y = static_cast<float>(targetY);
		currentNodeIndex += movingForward ? 1 : -1;
	} else {
		float nx = dx / distanceToTarget;
		float ny = dy / distanceToTarget;
		Instance->X += nx * step;
		Instance->Y += ny * step;
	}
}

int PathMovement::GetRealSpeed() {
	if (Nodes.empty()) {
		return 0;
	}

	if (stopped) {
		return 0;
	}

	if (currentNodeIndex >= static_cast<int>(Nodes.size()) || currentNodeIndex < 0) {
		return 0;
	}

	return static_cast<int>(Nodes[currentNodeIndex].Speed);
}

int PathMovement::GetMovementDirection() {
	if (Nodes.empty()) {
		return 0;
	}

	if (currentNodeIndex >= static_cast<int>(Nodes.size()) || currentNodeIndex < 0) {
		return 0;
	}

	return static_cast<int>(Nodes[currentNodeIndex].Direction);
}