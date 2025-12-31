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
	SDL_AudioSpec spec;
	std::string name;
} Sample;
typedef struct Channel {
	bool uninterruptable;
	SDL_AudioStream *stream;
	int curHandle;
	bool loop;
	bool pause;
	float volume = 1.0f;
} Channel;
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
	void DrawTexture(int id, int x, int y, int offsetX, int offsetY, int angle, float scale, int color, int effect, unsigned char effectParameter) override;
	void DrawQuickBackdrop(int x, int y, int width, int height, Shape* shape) override;
	
	void DrawRectangle(int x, int y, int width, int height, int color) override;
	void DrawRectangleLines(int x, int y, int width, int height, int color) override;
	void DrawLine(int x1, int y1, int x2, int y2, int color) override;
	void DrawPixel(int x, int y, int color) override;

	void LoadFont(int id) override;
	void UnloadFont(int id) override;
	void DrawText(FontInfo* fontInfo, int x, int y, int color, const std::string& text) override;
	// Sample Start
	bool LoadSample(int id) override;
	void PlaySample(int id, int channel, int loops, int freq, bool uninterruptable) override;
	void UpdateSample() override;
	void PauseSample(int id, bool channel, bool pause) override;
	bool SampleState(int id, bool channel, bool pauseOrStop) override;
	int GetSampleVolume(int id, bool channel) override;
	void SetSampleVolume(float volume, int id, bool channel) override;
	void SetSamplePan(float pan, int id, bool channel) override;
	void StopSample(int id, bool channel) override;
	// Sample End
	const uint8_t* GetKeyboardState() override;

	int GetMouseX() override;
	int GetMouseY() override;
	void SetMouseX(int x) override;
	void SetMouseY(int y) override;
	int GetMouseWheelMove() override;
	uint32_t GetMouseState() override;
	void HideMouseCursor() override;
	void ShowMouseCursor() override;

	void SetTitle(const char* name) override {}
	void HideWindow() override {}
	void ShowWindow() override {}
	void ChangeWindowPosX(int x) override {}
	void ChangeWindowPosY(int y) override {}
	void Windowed() override {}
	void Fullscreen(bool fullscreenDesktop) override {}
	
	unsigned int GetTicks() override { return SDL_GetTicks(); }
	float GetTimeDelta() override;
	void Delay(unsigned int ms) override;

	bool IsPixelTransparent(int textureId, int x, int y) override;
	void GetTextureDimensions(int textureId, int& width, int& height) override;

#ifdef _DEBUG
	void ToggleDebugUI() { DEBUG_UI.ToggleEnabled(); }
	bool IsDebugUIEnabled() { return DEBUG_UI.IsEnabled(); }
#endif

private:
	SDL_Window* window;
	SDL_Renderer* renderer;
	SDL_Texture* renderTarget;
	static SDL_AudioDeviceID audio_device;
	SDL_AudioSpec spec;
	bool renderedFirstFrame = false;
	float mainVol = 100.0f;
	SDL_FRect CalculateRenderTargetRect();
	SDL_Color RGBToSDLColor(int color);
	SDL_Color RGBAToSDLColor(int color);

	std::unordered_map<int, SDL_Texture*> textures;
	std::unordered_map<int, Sample> samples;
	Channel channels[49]; // 48 will be the last element.
	std::unordered_map<int, TTF_Font*> fonts;
	std::unordered_map<std::string, std::shared_ptr<std::vector<uint8_t>>> fontBuffers;

	int FusionToSDLKey(short key);
}; 
#endif