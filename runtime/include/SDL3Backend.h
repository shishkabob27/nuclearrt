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

	bool renderedFirstFrame = false;

	SDL_FRect CalculateRenderTargetRect();
	SDL_Color RGBToSDLColor(int color);
	SDL_Color RGBAToSDLColor(int color);

	std::unordered_map<int, SDL_Texture*> mosaics;
	std::unordered_map<int, int> imageToMosaic;
	std::unordered_map<int, std::set<int>> mosaicToImages;

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