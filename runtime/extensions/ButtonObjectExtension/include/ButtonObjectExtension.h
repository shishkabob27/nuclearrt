#pragma once

#include "Application.h"
#include "Extension.h"
#include "ObjectInstance.h"
#include <string>
	
class ButtonObjectExtension : public Extension {
public:
	ButtonObjectExtension(short width, short height, short type, short flags) : Width(width), Height(height), Type(type), Flags(flags) {}

	void Initialize() override;
	void Update(float deltaTime) override;
	void Draw() override;

	void ButtonDraw();
	void CheckboxDraw();
	void RadioButtonDraw();

	bool IsClicked() const;
	void SetText(const std::string& text);
	void SetEnabled(bool enabled);
	void SetShown(bool shown);
	
private:
	short Width;
	short Height;
	short Type;
	short Flags;
	std::string Text = "";
	bool Enabled = true;
	bool Clicked = false;
	bool HeldDown = false;
	bool Checked = false; // used for checkboxes and radio buttons
	bool Hovered = false;
	bool Shown = true;
};