#pragma once

#include <string>
#include <cstdint>

#include "FontBank.h"
#include "Shape.h"
#include "PakFile.h"

class Backend {
public:
	Backend() = default;
	virtual ~Backend() = default;

	virtual std::string GetName() const { return ""; }

	virtual void Initialize() {}
	virtual void Deinitialize() {}

	virtual bool ShouldQuit() { return false; }

	virtual std::string GetPlatformName() { return "Unknown"; }
	virtual std::string GetAssetsFileName() { return ""; }

	virtual unsigned int GetTicks() { return 0; }
	virtual float GetTimeDelta() { return 0.0f; }
	virtual void Delay(unsigned int ms) {}

	virtual void BeginDrawing() {}
	virtual void EndDrawing() {}
	virtual void Clear(int color) {}

	virtual void LoadTexture(int id) {}
	virtual void UnloadTexture(int id) {}
	virtual void DrawTexture(int id, int x, int y, int offsetX, int offsetY, int angle, float scale, int color, unsigned char blendCoefficient, int effect, unsigned int effectParam) {}
	virtual void DrawQuickBackdrop(int x, int y, int width, int height, Shape* shape) {}

	virtual void DrawRectangle(int x, int y, int width, int height, int color) {}
	virtual void DrawRectangleLines(int x, int y, int width, int height, int color) {}
	virtual void DrawLine(int x1, int y1, int x2, int y2, int color) {}
	virtual void DrawPixel(int x, int y, int color) {}

	virtual void LoadFont(int id) {}
	virtual void UnloadFont(int id) {}
	virtual void DrawText(FontInfo* fontInfo, int x, int y, int color, const std::string& text) {}

	virtual const uint8_t* GetKeyboardState() { return nullptr; }

	virtual int GetMouseX() { return 0; }
	virtual int GetMouseY() { return 0; }
	virtual void SetMouseX(int x) {}
	virtual void SetMouseY(int y) {}
	virtual int GetMouseWheelMove() { return 0; }
	virtual uint32_t GetMouseState() { return 0; }
	virtual void HideMouseCursor() {}
	virtual void ShowMouseCursor() {}

	virtual bool IsPixelTransparent(int textureId, int x, int y) { return true; }
	virtual void GetTextureDimensions(int textureId, int& width, int& height) { width = 0; height = 0; }

protected:
	PakFile pakFile;
}; 