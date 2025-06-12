#pragma once

#include <SDL.h>

class Input
{
public:
	Input() = default;
	~Input() = default;

	void Update();
	void Reset();

	bool IsKeyDown(short key);
	bool IsKeyPressed(short key);
	bool IsKeyReleased(short key);
	bool IsAnyKeyPressed();

	bool IsControlsDown(int player, short control);
	bool IsControlsPressed(int player, short control);

	int GetMouseX();
	int GetMouseY();
	int GetMouseWheelMove();
	bool IsMouseButtonDown(int button);
	bool IsMouseButtonPressed(int button, bool doubleClick = false);

private:
	const uint8_t* currentKeyboardState;
	const uint8_t* previousKeyboardState;

	uint32_t currentMouseState;
	uint32_t previousMouseState;
};

