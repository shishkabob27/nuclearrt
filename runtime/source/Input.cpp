#include "Input.h"
#include "Application.h"
#include <cstring>

void Input::Update()
{
	previousKeyboardState = currentKeyboardState;
	currentKeyboardState = Application::Instance().GetBackend()->GetKeyboardState();

	previousMouseState = currentMouseState;
	currentMouseState = Application::Instance().GetBackend()->GetMouseState();
}

void Input::Reset()
{
	previousKeyboardState = new uint8_t[256];
	currentKeyboardState = new uint8_t[256];
	for (int i = 0; i < 256; i++)
	{
		memset((void*)previousKeyboardState, 0, 256);
		memset((void*)currentKeyboardState, 0, 256);
	}

	previousMouseState = 0;
	currentMouseState = 0;
}

bool Input::IsKeyDown(short key)
{
	return currentKeyboardState[key] == 1;
}

bool Input::IsKeyPressed(short key)
{
	return currentKeyboardState[key] == 1 && previousKeyboardState[key] == 0;
}

bool Input::IsKeyReleased(short key)
{
	return currentKeyboardState[key] == 0 && previousKeyboardState[key] == 1;
}

bool Input::IsAnyKeyPressed()
{
	for (int i = 0; i < 256; i++)
	{
		if (IsKeyPressed(i))
			return true;
	}
	return false;
}

bool Input::IsControlsDown(int player, short control)
{
	int controlType = Application::Instance().GetAppData()->GetControlTypes()[player];
	if (controlType != 5) // TODO: other control types besides keyboard
	{
		return false;
	}

	for (int i = 0; i < 8; i++)
	{
		if ((control & (1 << i)) != 0 && !IsKeyDown(Application::Instance().GetAppData()->GetControlKeys()[player][i])) return false;
	}

	return true;
}

bool Input::IsControlsPressed(int player, short control)
{
	int controlType = Application::Instance().GetAppData()->GetControlTypes()[player];
	if (controlType != 5) // TODO: other control types besides keyboard
	{
		return false;
	}

	for (int i = 0; i < 8; i++)
	{
		if ((control & (1 << i)) != 0 && IsKeyPressed(Application::Instance().GetAppData()->GetControlKeys()[player][i])) return true;
	}

	return false;
}

int Input::GetMouseX()
{
	return Application::Instance().GetBackend()->GetMouseX();
}

int Input::GetMouseY()
{
	return Application::Instance().GetBackend()->GetMouseY();
}

int Input::GetMouseWheelMove()
{
	return Application::Instance().GetBackend()->GetMouseWheelMove();
}

bool Input::IsMouseButtonDown(int button)
{
	if (button == 1) button = 0;
	else if (button == 4) button = 1;
	return currentMouseState & (1 << button);
}

bool Input::IsMouseButtonPressed(int button, bool doubleClick)
{
	if (doubleClick) return false; // TODO: implement double click
	
	if (button == 1) button = 0;
	else if (button == 4) button = 1;
	return (currentMouseState & (1 << button)) && !(previousMouseState & (1 << button));
}