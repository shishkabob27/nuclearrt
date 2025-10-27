#include "ButtonObjectExtension.h"
#include "Application.h"

void ButtonObjectExtension::Initialize() {
	if (Flags & 1) {
		Shown = false;
	}

	if (Flags & 2) {
		Enabled = false;
	}
}

void ButtonObjectExtension::Update(float deltaTime) {
	auto input = Application::Instance().GetInput();
	int mouseX = input->GetMouseX();
	int mouseY = input->GetMouseY();
	Hovered = (mouseX >= X && mouseX < X + Width &&
				mouseY >= Y && mouseY < Y + Height) && Shown;
	Clicked = Hovered && input->IsMouseButtonPressed(1, false) && Enabled;
	HeldDown = Hovered && input->IsMouseButtonDown(1) && Enabled;

	if (!Clicked) {
		return;
	}

	if (ButtonType == 1) { // checkbox
		Checked = !Checked;
	}
	else if (ButtonType == 2) { // radio button
		Checked = true;
	}
}

void ButtonObjectExtension::Draw() {
	if (!Shown) return;

	Application::Instance().GetBackend()->DrawRectangle(X, Y, Width, Height, 0xFFFFFFFF);

	switch (ButtonType) {
		case 0: // button
		case 3: // Bitmap button
		case 4: // Bitmap with text button
			ButtonDraw();
			break;
		case 1: // checkbox
			CheckboxDraw();
			break;
		case 2: // radio button
			RadioButtonDraw();
			break;
	}
}

void ButtonObjectExtension::ButtonDraw() {
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

	Application::Instance().GetBackend()->DrawRectangle(X + 1, Y + 1, Width - 2, Height - 2, fillColor);
	Application::Instance().GetBackend()->DrawRectangleLines(X + 1, Y + 1, Width - 2, Height - 2, borderColor);
}

void ButtonObjectExtension::CheckboxDraw() {
	int boxX = X;
	int boxY = Y;

	int borderColor = 0x626262;
	int fillColor = 0xFFFFFF;

	if (!Enabled) {
		fillColor = 0xF9F9F9;
		borderColor = 0xC3C3C3;
	}
	else if (Checked) {
		fillColor = 0x005FB8;
		borderColor = 0x005FB8;
	}

	if (Flags & 4) {
		boxX = X + Width - 12;
	}

	Application::Instance().GetBackend()->DrawRectangle(boxX, boxY + Height - 18, 12, 12, fillColor);
	Application::Instance().GetBackend()->DrawRectangleLines(boxX, boxY + Height - 18, 12, 12, borderColor);
}

void ButtonObjectExtension::RadioButtonDraw() {
	int radioX = X;
	int radioY = Y;

	int borderColor = 0x626262;
	int fillColor = 0xFFFFFF;

	if (!Enabled) {
		fillColor = 0xF9F9F9;
		borderColor = 0xC3C3C3;
	}
	else if (Checked) {
		fillColor = 0x005FB8;
		borderColor = 0x005FB8;
	}

	if (Flags & 4) {
		radioX = X + Width - 12;
	}

	Application::Instance().GetBackend()->DrawRectangle(radioX, radioY + Height - 18, 12, 12, fillColor);
	Application::Instance().GetBackend()->DrawRectangleLines(radioX, radioY + Height - 18, 12, 12, borderColor);
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

void ButtonObjectExtension::SetShown(bool shown) {
	Shown = shown;
}