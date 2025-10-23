#include "MouseMovement.h"
#include "Application.h"
#include <algorithm>

void MouseMovement::Initialize() {
	initialX = Instance->X;
	initialY = Instance->Y;
}

void MouseMovement::OnEnabled() {
	Application::Instance().GetBackend()->HideMouseCursor();

	disabledCursorX = Application::Instance().GetBackend()->GetMouseX();
	disabledCursorY = Application::Instance().GetBackend()->GetMouseY();
}

void MouseMovement::OnDisabled() {
	Application::Instance().GetBackend()->ShowMouseCursor();
}

void MouseMovement::Update(float deltaTime) {
	int mouseX = Application::Instance().GetInput()->GetMouseX();
	int mouseY = Application::Instance().GetInput()->GetMouseY();

	int xDifference = mouseX - disabledCursorX;
	int yDifference = mouseY - disabledCursorY;
	
	Application::Instance().GetBackend()->SetMouseX(disabledCursorX);
	Application::Instance().GetBackend()->SetMouseY(disabledCursorY);

	Instance->X += xDifference;
	Instance->Y += yDifference;

	Instance->X = std::clamp(Instance->X - initialX, MinX, MaxX) + initialX;
	Instance->Y = std::clamp(Instance->Y - initialY, MinY, MaxY) + initialY;
}