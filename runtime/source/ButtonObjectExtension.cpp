#include "ButtonObjectExtension.h"
#include "Application.h"

void ButtonObjectExtension::Update(float deltaTime) {
	auto input = Application::Instance().GetInput();
	int mouseX = input->GetMouseX();
	int mouseY = input->GetMouseY();
	Hovered = (mouseX >= instance->X && mouseX < instance->X + Width &&
				mouseY >= instance->Y && mouseY < instance->Y + Height);
	Clicked = Hovered && input->IsMouseButtonPressed(1, false) && Enabled;
	HeldDown = Hovered && input->IsMouseButtonDown(1) && Enabled;
}

void ButtonObjectExtension::Draw() {
	int borderColor = 0xADADAD;
	int fillColor = 0xE1E1E1;

	if (!Enabled) {
		fillColor = 0xCCCCCC;
		borderColor = 0xBFBFBF;
	}
	else if (HeldDown) {
		fillColor = 0xCCE4F7;
		borderColor = 0x005499;
	} else if (Hovered) {
		fillColor = 0xE5F1FB;
		borderColor = 0x0078D7;
	}

	Application::Instance().GetBackend()->DrawRectangle(instance->X, instance->Y, Width, Height, 0xFFFFFFFF);
	Application::Instance().GetBackend()->DrawRectangle(instance->X + 1, instance->Y + 1, Width - 2, Height - 2, fillColor);
	Application::Instance().GetBackend()->DrawRectangleLines(instance->X + 1, instance->Y + 1, Width - 2, Height - 2, borderColor);
}

bool ButtonObjectExtension::IsClicked() const {
	return Clicked;
}

void ButtonObjectExtension::SetText(const std::string& text) {
	Text = text;
}

void ButtonObjectExtension::SetEnabled(bool enabled) {
	Enabled = enabled;
}