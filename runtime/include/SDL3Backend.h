#pragma once

#ifdef NUCLEAR_BACKEND_SDL3

#include "Backend.h"
#include <unordered_map>

#include <SDL3/SDL.h>
#include <SDL3_ttf/SDL_ttf.h>

#ifdef _DEBUG
#include "DebugUI.h"
#endif
typedef struct Sample {
	Uint8 *data;
	Uint32 data_len;
	SDL_AudioStream *stream;
	SDL_AudioSpec spec;
	bool active;
	int loops;
} Sample;
extern Sample samples[1000];
typedef struct Channel {
	bool containsSample;
} Channel;
extern Channel channels[32];
class SDL3Backend : public Backend {
public:
	SDL3Backend();
	~SDL3Backend() override;

	std::string GetName() const override { return "SDL3"; }

	void Initialize() override;
	void Deinitialize() override;

	bool ShouldQuit() override;

	std::string GetPlatformName() override;
	std::string GetAssetsFileName() override;

	void BeginDrawing() override;
	void EndDrawing() override;
	void Clear(int color) override;

	void LoadTexture(int id) override;
	void UnloadTexture(int id) override;
	void DrawTexture(int id, int x, int y, int offsetX, int offsetY, int angle, float scale, int color, unsigned char blendCoefficient, int effect, unsigned int effectParam) override;
	void DrawQuickBackdrop(int x, int y, int width, int height, Shape* shape) override;
	
	void DrawRectangle(int x, int y, int width, int height, int color) override;
	void DrawRectangleLines(int x, int y, int width, int height, int color) override;
	void DrawLine(int x1, int y1, int x2, int y2, int color) override;
	void DrawPixel(int x, int y, int color) override;

	void LoadFont(int id) override;
	void UnloadFont(int id) override;
	void DrawText(FontInfo* fontInfo, int x, int y, int color, const std::string& text) override;

	void LoadSample(int id) override;
	void PlaySample(int id, int channel, int loops, int freq, bool interruptable);

	const uint8_t* GetKeyboardState() override;

	int GetMouseX() override;
	int GetMouseY() override;
	void SetMouseX(int x) override;
	void SetMouseY(int y) override;
	int GetMouseWheelMove() override;
	uint32_t GetMouseState() override;
	void HideMouseCursor() override;
	void ShowMouseCursor() override;

	unsigned int GetTicks() override { return SDL_GetTicks(); }
	float GetTimeDelta() override;
	void Delay(unsigned int ms) override;

	bool IsPixelTransparent(int textureId, int x, int y) override;
	void GetTextureDimensions(int textureId, int& width, int& height) override;
	void SetTitle(const char* name) override {SDL_SetWindowTitle(window, name);};
	void HideWindow() override {SDL_HideWindow(window);};
	void ShowWindow() override {SDL_ShowWindow(window);};
	void ChangeWindowPosX(int x) override {SDL_SetWindowPosition(window, x, SDL_WINDOWPOS_UNDEFINED);};
	void ChangeWindowPosY(int y) override {SDL_SetWindowPosition(window, SDL_WINDOWPOS_UNDEFINED, y);};
	void Fullscreen(bool fullscreenDesktop) override {
		if (fullscreenDesktop) SDL_SetWindowFullscreen(window, true);
		else SDL_SetWindowFullscreen(window, false);
	};
	void Windowed() override {SDL_SetWindowFullscreen(window, false);};
#ifdef _DEBUG
	void ToggleDebugUI() { DEBUG_UI.ToggleEnabled(); }
	bool IsDebugUIEnabled() { return DEBUG_UI.IsEnabled(); }
#endif

private:
	SDL_Window* window;
	SDL_Renderer* renderer;
	SDL_Texture* renderTarget;
	SDL_AudioSpec spec;
	static SDL_AudioDeviceID audio_device;
	SDL_FRect CalculateRenderTargetRect();
	SDL_Color RGBToSDLColor(int color);
	SDL_Color RGBAToSDLColor(int color);

	std::unordered_map<int, SDL_Texture*> textures;

	std::unordered_map<int, TTF_Font*> fonts;
	std::unordered_map<std::string, std::shared_ptr<std::vector<uint8_t>>> fontBuffers;
	
	int FusionToSDLKey(short key);
}; 
#endif