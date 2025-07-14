#pragma once

#include "Application.h"
#include "Extension.h"
#include "ObjectInstance.h"
#include <string>
	
class ButtonObjectExtension : public Extension {
public:
	ButtonObjectExtension(int width, int height) : Width(width), Height(height) {}

	void Update(float deltaTime) override;
	void Draw() override;

	bool IsClicked() const;
	void SetText(const std::string& text);
	void SetEnabled(bool enabled);
	
private:
	int Width;
	int Height;
	std::string Text = "";
	bool Enabled = true;
	bool Clicked = false;
	bool HeldDown = false;
	bool Hovered = false;
};