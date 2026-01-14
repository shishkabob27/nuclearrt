#pragma once

#ifdef NUCLEAR_BACKEND_SDL3

#include "Backend.h"
#include <unordered_map>
#include <set>

#include <SDL3/SDL.h>
#include <SDL3_ttf/SDL_ttf.h>

#ifdef _DEBUG
#include "DebugUI.h"
#endif
typedef struct Channel {
	Uint8 *data = nullptr; // No need to make floats, as SDL_AudioStream does convert samples into a format you give via SDL_AudioSpec
	Uint32 data_len = 0;
	SDL_AudioSpec spec{};
	bool uninterruptable = false;
	SDL_AudioStream *stream = nullptr;
	int position;
	bool finished;
	int curHandle = -1;
	bool loop = false;
	bool pause = false;
	float volume = 1.0f;
	float pan = 0.0f;
	std::string name = "";
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
	void DrawText(FontInfo* fontInfo, int x, int y, int color, const std::string& text, int objectHandle = -1) override;
	// Sample Start
	static void SDLCALL AudioCallback(void* userdata, SDL_AudioStream* stream, int additional_amount, int total_amount);
	bool LoadSample(int id, int channel) override;
	int FindSample(std::string name) override;
	void PlaySample(int id, int channel, int loops, int freq, bool uninterruptable, float volume, float pan) override;
	void UpdateSample() override;
	void PauseSample(int id, bool channel, bool pause) override;
	bool SampleState(int id, bool channel, bool pauseOrStop) override;
	int GetSampleVolume(int id, bool channel) override;
	std::string GetChannelName(int channel) override {return channels[channel].name;}
	void SetSampleVolume(float volume, int id, bool channel) override;
	void LockChannel(int channel, bool unlock) override {if (unlock) SDL_UnlockAudioStream(channels[channel].stream); else SDL_LockAudioStream(channels[channel].stream);}
	void SetSamplePan(float pan, int id, bool channel) override;
	int GetSamplePan(int id, bool channel) override;
	void SetSampleFreq(int freq, int id, bool channel) override;
	int GetSampleFreq(int id, bool channel) override;
	int GetSampleDuration(int id, bool channel) override {
		if (channel && (id > 1 || id < 48)) return static_cast<int>(channels[id].data_len);
		if (!channel && id > -1) {
			for (int i = 1; i < SDL_arraysize(channels); ++i) if (channels[i].curHandle == id) return static_cast<int>(channels[i].data_len);	
		}
		return 0;
	}
	int GetSamplePos(int id, bool channel) override {
		if (channel && (id > 1 || id < 48)) return channels[id].position;
		if (!channel && id > -1) {
			for (int i = 1; i < SDL_arraysize(channels); ++i) if (channels[i].curHandle == id) return channels[i].position;
		}
		return 0;
	}
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
	SDL_GPUDevice* gpuDevice;
	SDL_Texture* renderTarget;
	static SDL_AudioDeviceID audio_device;
	SDL_AudioSpec spec;
	bool renderedFirstFrame = false;
	float mainVol = 100.0f;
	float mainPan = 0.0f;
	SDL_FRect CalculateRenderTargetRect();
	SDL_Color RGBToSDLColor(int color);
	SDL_Color RGBAToSDLColor(int color);

	std::unordered_map<int, SDL_Texture*> mosaics;
	std::unordered_map<int, int> imageToMosaic;
	std::unordered_map<int, std::set<int>> mosaicToImages;

	Channel channels[49]; // 48 will be the last element.
	SDL_AudioStream* masterStream;
	bool windowFocused = true;
	std::unordered_map<int, TTF_Font*> fonts;
	std::unordered_map<std::string, std::shared_ptr<std::vector<uint8_t>>> fontBuffers;

	struct TextCacheKey {
		unsigned int fontHandle;
		std::string text;
		int color;
		int objectHandle;
		
		bool operator==(const TextCacheKey& other) const {
			return fontHandle == other.fontHandle && text == other.text && color == other.color && objectHandle == other.objectHandle;
		}
	};
	
	struct TextCacheKeyHash {
		std::size_t operator()(const TextCacheKey& key) const {
			std::size_t h1 = std::hash<unsigned int>{}(key.fontHandle);
			std::size_t h2 = std::hash<std::string>{}(key.text);
			std::size_t h3 = std::hash<int>{}(key.color);
			std::size_t h4 = std::hash<int>{}(key.objectHandle);
			return h1 ^ (h2 << 1) ^ (h3 << 2) ^ (h4 << 3);
		}
	};
	
	struct CachedText {
		SDL_Texture* texture;
		int width;
		int height;
	};
	
	std::unordered_map<TextCacheKey, CachedText, TextCacheKeyHash> textCache;

	void RemoveOldTextCache();
	void ClearTextCacheForFont(int fontHandle);
	int FusionToSDLKey(short key);
}; 
#endif