#ifdef NUCLEAR_BACKEND_SDL3

#include "SDL3Backend.h"

#include <iostream>

#include "Application.h"
#include "Frame.h"
#include "FontBank.h"
#include "SoundBank.h"
#include "PakFile.h"

#include <SDL3/SDL.h>
#include <SDL3_image/SDL_image.h>
#include <SDL3_ttf/SDL_ttf.h>
#include "stb_vorbis.c"

#ifdef _DEBUG
#include "DebugUI.h"
#include <imgui_impl_sdl3.h>
#endif
SDL_AudioDeviceID SDL3Backend::audio_device = NULL;
Sample samples[1000];
Channel channels[32];
SDL3Backend::SDL3Backend() {
}

SDL3Backend::~SDL3Backend() {
	Deinitialize();
}


void SDL3Backend::Initialize() {
	int windowWidth = Application::Instance().GetAppData()->GetWindowWidth();
	int windowHeight = Application::Instance().GetAppData()->GetWindowHeight();
	std::string windowTitle = Application::Instance().GetAppData()->GetAppName();
	
	
	if (!SDL_Init(SDL_INIT_VIDEO | SDL_INIT_AUDIO | SDL_INIT_GAMEPAD)) {
		std::cerr << "SDL_Init Error: " << SDL_GetError() << std::endl;
		return;
	}

	if (!TTF_Init()) {
		std::cerr << "TTF_Init Error: " << SDL_GetError() << std::endl;
		return;
	}

	// Create the window
	SDL_WindowFlags flags = SDL_WINDOW_RESIZABLE | SDL_WINDOW_HIDDEN;
	window = SDL_CreateWindow(windowTitle.c_str(), windowWidth, windowHeight, flags);
	if (window == nullptr) {
		std::cerr << "SDL_CreateWindow Error: " << SDL_GetError() << std::endl;
		return;
	}
	// Create the Audio Device
	audio_device = SDL_OpenAudioDevice(SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, &spec);
    if (!audio_device) {
        std::cerr << "SDL_OpenAudioDevice Error : " << SDL_GetError() << std::endl;
        return;
    }
	// Create the renderer
	renderer = SDL_CreateRenderer(window, nullptr);
	if (renderer == nullptr) {
		std::cerr << "SDL_CreateRenderer Error: " << SDL_GetError() << std::endl;
		return;
	}

	// Create the render target texture
	renderTarget = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_XRGB8888, SDL_TEXTUREACCESS_TARGET, windowWidth, windowHeight);
	if (renderTarget == nullptr) {
		std::cerr << "SDL_CreateTexture Error: " << SDL_GetError() << std::endl;
		return;
	}

	//load assets
	if (!pakFile.Load(GetAssetsFileName())) {
		std::cerr << "PakFile::Load Error: " << "Failed to load assets file" << std::endl;
		return;
	}

#ifdef _DEBUG
	DEBUG_UI.Initialize(window, renderer);
	
	DEBUG_UI.AddWindow(Application::Instance().GetAppData()->GetAppName(), [this]() {
		ImGui::Text("Platform: %s", GetPlatformName().c_str());
		ImGui::Text("Assets File: %s", GetAssetsFileName().c_str());

		if (ImGui::CollapsingHeader("Window")) {
			ImGui::Checkbox("Fit Inside", &Application::Instance().GetAppData()->GetFitInside());
			ImGui::Checkbox("Resize Display", &Application::Instance().GetAppData()->GetResizeDisplay());
			ImGui::Checkbox("Dont Center Frame", &Application::Instance().GetAppData()->GetDontCenterFrame());
		}
		
		if(ImGui::CollapsingHeader("Global Variables")) {
			if (ImGui::CollapsingHeader("Values")) {
				std::vector<int>& altValues = Application::Instance().GetAppData()->GetGlobalValues();
				for (int i = 0; i < altValues.size(); i++) {
					ImGui::InputInt(("Value " + std::to_string(i)).c_str(), &altValues[i]);
				}
			}
			if (ImGui::CollapsingHeader("Strings")) {
				std::vector<std::string>& altStrings = Application::Instance().GetAppData()->GetGlobalStrings();
				for (int i = 0; i < altStrings.size(); i++) {
					char buffer[256];
					strncpy(buffer, altStrings[i].c_str(), sizeof(buffer) - 1);
					buffer[sizeof(buffer) - 1] = '\0';
					if (ImGui::InputText(("String " + std::to_string(i)).c_str(), buffer, sizeof(buffer))) {
						altStrings[i] = buffer;
					}
				}
			}
		}

		//jump to frame
		static int frameIndex = 0;
		ImGui::InputInt("Frame Index", &frameIndex);
		if (ImGui::Button("Jump to Frame")) {
			Application::Instance().QueueStateChange(GameState::JumpToFrame, frameIndex);
		}

		if (ImGui::CollapsingHeader("Current Frame")) {
			Frame* currentFrame = Application::Instance().GetCurrentFrame().get();
			ImGui::Text("Current Frame: %s", currentFrame->Name.c_str());
			ImGui::Text("Current Frame Index: %d", currentFrame->Index);

			if (ImGui::TreeNode("Object Instances")) {
				int i = 0;
				for (auto& [handle, instance] : currentFrame->ObjectInstances) {					
					if (ImGui::TreeNode(std::string(instance->Name + "##" + std::to_string(handle)).c_str())) {
						ImGui::Text("Handle: %d", handle);
						ImGui::Text("X: %d", instance->X);
						ImGui::Text("Y: %d", instance->Y);

						if (ImGui::TreeNode("OI")) {
							ImGui::Text("Handle: %d", handle);
							ImGui::Text("Type: %d", instance->Type);
							ImGui::Text("RGB Coefficient: %d", instance->RGBCoefficient);
							ImGui::Text("Effect: %d", instance->Effect);
							ImGui::Text("Effect Parameter: %d", instance->GetEffectParameter());
							ImGui::TreePop();
						}

						ImGui::TreePop();
					}

					if (ImGui::IsItemHovered()) {
						DrawRectangle(instance->X, instance->Y, 32, 32, 0xFFFF0000);
					}

					i++;
				}
				ImGui::TreePop();
			}
		}
	});
#endif
}

void SDL3Backend::Deinitialize()
{
#ifdef _DEBUG
	DEBUG_UI.Shutdown();
#endif

	// cleanup textures
	for (auto& pair : textures) {
		SDL_DestroyTexture(pair.second);
	}
	textures.clear();

	// cleanup fonts
	for (auto& pair : fonts) {
		TTF_CloseFont(pair.second);
	}
	fonts.clear();
	fontBuffers.clear();
	
	// Close the Audio Device
	SDL_CloseAudioDevice(audio_device);
	// cleanup audio
	for (int i; i <= 32; i++) {
		if (samples[i].stream != nullptr) SDL_DestroyAudioStream(samples[i].stream);
		SDL_free(samples[i].data);
	}
	if (renderTarget != nullptr) {
		SDL_DestroyTexture(renderTarget);
		renderTarget = nullptr;
	}

	// Destroy the renderer
	if (renderer != nullptr) {
		SDL_DestroyRenderer(renderer);
		renderer = nullptr;
	}
	
	// Destroy the window
	if (window != nullptr) {
		SDL_DestroyWindow(window);
		window = nullptr;
	}
	
	TTF_Quit();
	SDL_Quit();
}

bool SDL3Backend::ShouldQuit()
{
	SDL_Event event;
	while (SDL_PollEvent(&event)) {
#ifdef _DEBUG
		// Process ImGui events
		if (DEBUG_UI.IsEnabled()) {
			ImGui_ImplSDL3_ProcessEvent(&event);
		}
		
		// Toggle debug UI with F1 key
		if (event.type == SDL_EVENT_KEY_DOWN && event.key.key == SDLK_F1 && event.key.repeat == 0) {
			DEBUG_UI.ToggleEnabled();
		}
#endif

		if (event.type == SDL_EVENT_QUIT) {
			return true;
		}
	}
	return false;
}

std::string SDL3Backend::GetPlatformName()
{
#if defined(PLATFORM_WINDOWS)
	return "Windows";
#elif defined(PLATFORM_MACOS)
	return "macOS";
#elif defined(PLATFORM_LINUX)
	return "Linux";
#else
	return "Unknown";
#endif
}

std::string SDL3Backend::GetAssetsFileName()
{
	return "assets.pak";
}

void SDL3Backend::BeginDrawing()
{
	if (renderer == nullptr) {
		std::cerr << "BeginDrawing called with null renderer!" << std::endl;
		return;
	}

	//resize render target if needed
	int newWidth = std::min(Application::Instance().GetAppData()->GetWindowWidth(), Application::Instance().GetCurrentFrame()->Width);
	int newHeight = std::min(Application::Instance().GetAppData()->GetWindowHeight(), Application::Instance().GetCurrentFrame()->Height);

	if (newWidth != renderTarget->w || newHeight != renderTarget->h) {
		SDL_DestroyTexture(renderTarget);
		renderTarget = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_XRGB8888, SDL_TEXTUREACCESS_TARGET, newWidth, newHeight);
	}
	
	SDL_SetRenderTarget(renderer, renderTarget);
	
	SDL_Color borderColor = RGBToSDLColor(Application::Instance().GetAppData()->GetBorderColor());
	SDL_SetRenderDrawColor(renderer, borderColor.r, borderColor.g, borderColor.b, SDL_ALPHA_OPAQUE);
	SDL_RenderClear(renderer);

#ifdef _DEBUG
	DEBUG_UI.BeginFrame();
#endif
}

void SDL3Backend::EndDrawing()
{
	if (renderer == nullptr) {
		std::cerr << "EndDrawing called with null renderer!" << std::endl;
		return;
	}

	SDL_SetRenderTarget(renderer, nullptr);
	
	SDL_Color borderColor = RGBToSDLColor(Application::Instance().GetAppData()->GetBorderColor());
	SDL_SetRenderDrawColor(renderer, borderColor.r, borderColor.g, borderColor.b, SDL_ALPHA_OPAQUE);
	SDL_RenderClear(renderer);
	
	SDL_FRect rect = CalculateRenderTargetRect();
	SDL_RenderTexture(renderer, renderTarget, nullptr, &rect);

#ifdef _DEBUG
	DEBUG_UI.EndFrame();
#endif

	SDL_RenderPresent(renderer);

	if (!renderedFirstFrame) {
		renderedFirstFrame = true;
		SDL_ShowWindow(window);
	}
}

void SDL3Backend::Clear(int color)
{
	SDL_SetRenderDrawColor(renderer, (color >> 16) & 0xFF, (color >> 8) & 0xFF, color & 0xFF, SDL_ALPHA_OPAQUE);
	SDL_RenderClear(renderer);
}

void SDL3Backend::LoadTexture(int id) {

	//Check if texture is already loaded
	if (textures.find(id) != textures.end()) {
		return;
	}

	//load texture from pak file
	std::vector<uint8_t> data = pakFile.GetData("images/" + std::to_string(id) + ".png");
	if (data.empty()) {
		std::cerr << "PakFile::GetData Error: " << "Image with id " << id << " not found" << std::endl;
		return;
	}

	//create surface from data
	SDL_IOStream* stream = SDL_IOFromMem(data.data(), data.size());
	SDL_Texture* texture = IMG_LoadTexture_IO(renderer, stream, true);
	if (texture == nullptr) {
		std::cerr << "IMG_LoadTexture_IO Error: " << SDL_GetError() << std::endl;
		return;
	}

	textures[id] = texture;
}

void SDL3Backend::UnloadTexture(int id) {
	auto it = textures.find(id);
	if (it != textures.end()) {
		SDL_DestroyTexture(it->second);
		textures.erase(it);
	}
}

void SDL3Backend::DrawTexture(int id, int x, int y, int offsetX, int offsetY, int angle, float scale, int color, int effect, unsigned char effectParameter)
{
	SDL_Texture* texture = textures[id];
	if (texture == nullptr) {
		return;
	}
	
	// Save original texture properties
	Uint8 origR, origG, origB, origA;
	SDL_BlendMode origBlendMode;
	SDL_GetTextureColorMod(texture, &origR, &origG, &origB);
	SDL_GetTextureAlphaMod(texture, &origA);
	SDL_GetTextureBlendMode(texture, &origBlendMode);
	
	// Apply new color
	Uint8 r = (color >> 16) & 0xFF;
	Uint8 g = (color >> 8) & 0xFF;
	Uint8 b = color & 0xFF;
	SDL_SetTextureColorMod(texture, r, g, b);
	
	//get texture dimensions
	int width, height;
	GetTextureDimensions(id, width, height);
	SDL_FRect rect = { static_cast<float>(x - offsetX), static_cast<float>(y - offsetY), static_cast<float>(width), static_cast<float>(height) };
	
	//Effects
	switch (effect) {
		case 4096:
		case 0:
			SDL_SetTextureAlphaMod(texture, 255 - effectParameter);
			break;
		case 1: // Semi-Transparent:
			SDL_SetTextureColorMod(texture, 255, 255, 255);
			SDL_SetTextureAlphaMod(texture, 255 - effectParameter);
			break;
		case 9: // Additive
			SDL_SetTextureBlendMode(texture, SDL_BLENDMODE_ADD);
			SDL_SetTextureAlphaMod(texture, 255 - effectParameter);
			break;
	}

	SDL_FPoint center{ static_cast<float>(offsetX), static_cast<float>(offsetY) };
	SDL_RenderTextureRotated(renderer, texture, nullptr, &rect, 360 - angle, &center, SDL_FLIP_NONE);
	
	// Restore original texture properties
	SDL_SetTextureColorMod(texture, origR, origG, origB);
	SDL_SetTextureAlphaMod(texture, origA);
	SDL_SetTextureBlendMode(texture, origBlendMode);
}

void SDL3Backend::DrawQuickBackdrop(int x, int y, int width, int height, Shape* shape)
{
	//TODO: Borders
	//TODO: Ellipse masks
	if (shape->ShapeType == 1) { // Line
		SDL_SetRenderDrawColor(renderer, (shape->BorderColor >> 16) & 0xFF, (shape->BorderColor >> 8) & 0xFF, shape->BorderColor & 0xFF, SDL_ALPHA_OPAQUE);

		int x1 = shape->FlipX ? x + width : x;
		int y1 = shape->FlipY ? y + height : y;
		int x2 = shape->FlipX ? x : x + width;
		int y2 = shape->FlipY ? y : y + height;

		//TODO: BorderSize
		SDL_RenderLine(renderer, x1, y1, x2, y2);
	}
	else {
		if (shape->FillType == 1) { // Solid Color
			SDL_SetRenderDrawColor(renderer, (shape->Color1 >> 16) & 0xFF, (shape->Color1 >> 8) & 0xFF, shape->Color1 & 0xFF, SDL_ALPHA_OPAQUE);
			SDL_FRect rect = { static_cast<float>(x), static_cast<float>(y), static_cast<float>(width), static_cast<float>(height) };
			SDL_RenderFillRect(renderer, &rect);
		}
		else if (shape->FillType == 2) { // Gradient
			Uint8 r1 = (shape->Color1 >> 16) & 0xFF;
			Uint8 g1 = (shape->Color1 >> 8) & 0xFF;
			Uint8 b1 = shape->Color1 & 0xFF;
			
			Uint8 r2 = (shape->Color2 >> 16) & 0xFF;
			Uint8 g2 = (shape->Color2 >> 8) & 0xFF;
			Uint8 b2 = shape->Color2 & 0xFF;
			
			if (shape->VerticalGradient) {
				// Vertical gradient (top to bottom)
				for (int i = 0; i < height; i++) {
					float ratio = static_cast<float>(i) / static_cast<float>(height);
					
					Uint8 r = static_cast<Uint8>(r1 + (r2 - r1) * ratio);
					Uint8 g = static_cast<Uint8>(g1 + (g2 - g1) * ratio);
					Uint8 b = static_cast<Uint8>(b1 + (b2 - b1) * ratio);
					
					SDL_SetRenderDrawColor(renderer, r, g, b, SDL_ALPHA_OPAQUE);
					SDL_RenderLine(renderer, x, y + i, x + width - 1, y + i);
				}
			} else {
				// Horizontal gradient (left to right)
				for (int i = 0; i < width; i++) {
					float ratio = static_cast<float>(i) / static_cast<float>(width);
					
					Uint8 r = static_cast<Uint8>(r1 + (r2 - r1) * ratio);
					Uint8 g = static_cast<Uint8>(g1 + (g2 - g1) * ratio);
					Uint8 b = static_cast<Uint8>(b1 + (b2 - b1) * ratio);
					
					SDL_SetRenderDrawColor(renderer, r, g, b, SDL_ALPHA_OPAQUE);
					SDL_RenderLine(renderer, x + i, y, x + i, y + height - 1);
				}
			}
		}
		else if (shape->FillType == 3) { // Motif
			SDL_Texture* texture = textures[shape->Image];
			if (texture == nullptr) {
				return;
			}
			
			int textureWidth, textureHeight;
			GetTextureDimensions(shape->Image, textureWidth, textureHeight);
			
			// Tile the texture across the entire area
			for (int tileY = y; tileY < y + height; tileY += textureHeight) {
				for (int tileX = x; tileX < x + width; tileX += textureWidth) {
					// Calculate the width and height of this tile (might be smaller at edges)
					int tileW = std::min(textureWidth, x + width - tileX);
					int tileH = std::min(textureHeight, y + height - tileY);
					
					SDL_FRect destRect = { static_cast<float>(tileX), static_cast<float>(tileY), static_cast<float>(tileW), static_cast<float>(tileH) };
					SDL_FRect srcRect = { 0.0f, 0.0f, static_cast<float>(tileW), static_cast<float>(tileH) };
					SDL_RenderTexture(renderer, texture, &srcRect, &destRect);
				}
			}
		}
	}
}

void SDL3Backend::DrawRectangle(int x, int y, int width, int height, int color)
{
	SDL_SetRenderDrawColor(renderer, (color >> 16) & 0xFF, (color >> 8) & 0xFF, color & 0xFF, (color >> 24) & 0xFF);
	SDL_FRect rect = { static_cast<float>(x), static_cast<float>(y), static_cast<float>(width), static_cast<float>(height) };
	SDL_RenderFillRect(renderer, &rect);
}

void SDL3Backend::DrawRectangleLines(int x, int y, int width, int height, int color)
{
	SDL_SetRenderDrawColor(renderer, (color >> 16) & 0xFF, (color >> 8) & 0xFF, color & 0xFF, (color >> 24) & 0xFF);
	SDL_FRect rect = { static_cast<float>(x), static_cast<float>(y), static_cast<float>(width), static_cast<float>(height) };
	SDL_RenderRect(renderer, &rect);
}

void SDL3Backend::DrawLine(int x1, int y1, int x2, int y2, int color)
{
	SDL_SetRenderDrawColor(renderer, (color >> 16) & 0xFF, (color >> 8) & 0xFF, color & 0xFF, (color >> 24) & 0xFF);
	SDL_RenderLine(renderer, x1, y1, x2, y2);
}

void SDL3Backend::DrawPixel(int x, int y, int color)
{
	SDL_SetRenderDrawColor(renderer, (color >> 16) & 0xFF, (color >> 8) & 0xFF, color & 0xFF, (color >> 24) & 0xFF);
	SDL_RenderPoint(renderer, x, y);
}

void SDL3Backend::LoadFont(int id)
{
	//check if font already exists
	if (fonts.find(id) != fonts.end()) {
		return;
	}

	//get font info
	FontInfo* fontInfo = FontBank::Instance().GetFont(id);
	if (fontInfo == nullptr) {
		std::cerr << "FontBank::GetFont Error: " << "Font with id " << id << " not found" << std::endl;
		return;
	}

	SDL_IOStream* stream;

	//if buffer is already loaded, use it
	if (fontBuffers.find(fontInfo->FontFileName) != fontBuffers.end()) {
		stream = SDL_IOFromMem(fontBuffers[fontInfo->FontFileName]->data(), fontBuffers[fontInfo->FontFileName]->size());
	}
	else {
		//load buffer from pak file
		std::shared_ptr<std::vector<uint8_t>> buffer = std::make_shared<std::vector<uint8_t>>(pakFile.GetData("fonts/" + fontInfo->FontFileName));
		if (buffer->empty()) {
			std::cerr << "PakFile::GetData Error: " << "Font with file name " << fontInfo->FontFileName << " not found" << std::endl;
			return;
		}
		stream = SDL_IOFromMem(buffer->data(), buffer->size());
		fontBuffers[fontInfo->FontFileName] = buffer;
	}

	TTF_Font* font = TTF_OpenFontIO(stream, true, static_cast<float>(fontInfo->Height));
	if (!font) {
		std::cerr << "TTF_OpenFontIO Error: " << SDL_GetError() << std::endl;
		return;
	}
	
	//render flags
	int renderFlags = TTF_STYLE_NORMAL;
	if (fontInfo->Weight > 400) {
		renderFlags |= TTF_STYLE_BOLD;
	}
	if (fontInfo->Italic) {
		renderFlags |= TTF_STYLE_ITALIC;
	}
	if (fontInfo->Underline) {
		renderFlags |= TTF_STYLE_UNDERLINE;
	}
	if (fontInfo->Strikeout) {
		renderFlags |= TTF_STYLE_STRIKETHROUGH;
	}	

	TTF_SetFontStyle(font, renderFlags);

	fonts[id] = font;
}

void SDL3Backend::UnloadFont(int id)
{
	auto it = fonts.find(id);
	if (it != fonts.end()) {
		// Find the FontInfo associated with this font id to remove buffer
		FontInfo* fontInfo = FontBank::Instance().GetFont(id);
		if (fontInfo != nullptr) {
			// Check if any other loaded font is using the same buffer
			bool bufferUsedByOtherFont = false;
			for (const auto& pair : fonts) {
				if (pair.first != id) {
					FontInfo* otherFontInfo = FontBank::Instance().GetFont(pair.first);
					if (otherFontInfo && otherFontInfo->FontFileName == fontInfo->FontFileName) {
						bufferUsedByOtherFont = true;
						break;
					}
				}
			}
			if (!bufferUsedByOtherFont) {
				fontBuffers.erase(fontInfo->FontFileName);
			}
		}
		
		TTF_CloseFont(it->second);
		fonts.erase(it);
	}
}

void SDL3Backend::DrawText(FontInfo* fontInfo, int x, int y, int color, const std::string& text)
{
	if (fonts.find(fontInfo->Handle) == fonts.end()) {
		return;
	}

	TTF_Font* font = fonts[fontInfo->Handle];
	if (font == nullptr) {
		return;
	}

	//remove \r from text
	std::string modifiedText = text;
	modifiedText.erase(std::remove(modifiedText.begin(), modifiedText.end(), '\r'), modifiedText.end());

	//make tabs 4 spaces
	for (size_t i = 0; i < modifiedText.size(); i++) {
		if (modifiedText[i] == '\t') {
			modifiedText.replace(i, 1, "    ");
		}
	}

	//Check if text is empty/just whitespace
	if (modifiedText.find_first_not_of(" \n\r\t") == std::string::npos) {
		return;
	}

	SDL_Surface* surface = TTF_RenderText_Blended_Wrapped(font, modifiedText.c_str(), 0, RGBToSDLColor(color), 0);
	if (surface == nullptr) {
		std::cerr << "TTF_RenderText_Blended_Wrapped Error: " << SDL_GetError() << std::endl;
		return;
	}

	SDL_Texture* texture = SDL_CreateTextureFromSurface(renderer, surface);
	if (texture == nullptr) {
		std::cerr << "SDL_CreateTextureFromSurface Error: " << SDL_GetError() << std::endl;
		return;
	}

	SDL_FRect rect = { static_cast<float>(x), static_cast<float>(y), static_cast<float>(surface->w), static_cast<float>(surface->h) };
	SDL_RenderTexture(renderer, texture, nullptr, &rect);
	SDL_DestroySurface(surface);
	SDL_DestroyTexture(texture);
}

void SDL3Backend::LoadSample(int id) {
	if (samples[id].stream) return; // Check if sample is already loaded
	SoundInfo* soundInfo = SoundBank::Instance().GetSound(id);
	if (!soundInfo) {
		std::cerr << "SoundBank Error: Sound ID " << id << " not found!" << std::endl;
		return;
	}
	std::cout << soundInfo->Type << "\n";
	std::vector<uint8_t> data = pakFile.GetData("sounds/" + std::to_string(id) + "." + soundInfo->Type);
	if (data.empty()) {
		std::cerr << "PakFile::GetData Error: Sample with id " << id << " not found" << std::endl;
		return;
	}
	if (soundInfo->Type == "wav") {

		SDL_IOStream* stream = SDL_IOFromMem(data.data(), data.size());
		if (!SDL_LoadWAV_IO(stream, true, &samples[id].spec, &samples[id].data, &samples[id].data_len)) {
			std::cout << "SDL_LoadWAV_IO Error (WAV) : " << SDL_GetError() << std::endl;
			return;
		}
		samples[id].stream = SDL_CreateAudioStream(&samples[id].spec, NULL);
		if (!samples[id].stream) {
			std::cout << "SDL_CreateAudioStream (WAV) Error : " << SDL_GetError() << std::endl;
			return;
		}
		else if (!SDL_BindAudioStream(audio_device, samples[id].stream)) {
			std::cout << "SDL_BindAudioStream (WAV) Error : " << SDL_GetError() << std::endl;
			return;
		}
		std::cout << "Loaded WAV Sample ID : " << id << "\n";
	}
	else if (soundInfo->Type == "ogg") {
		std::cout << "OGG Data Size: " << data.size() << " bytes\n";
		int channels, samplerate;
		short* output = nullptr;
		int numSamples = stb_vorbis_decode_memory(data.data(), data.size(), &channels, &samplerate, &output);
		if (numSamples <= 0 || !output) {
			std::cout << "stb_vorbis_decode_memory failed.\n";
			return;
		}
		std::cout << "Decoded Memory (OGG)\n";
		SDL_AudioSpec devSpec;
		SDL_GetAudioDeviceFormat(audio_device, &devSpec, NULL);
		std::cout << "devSpec freq=" << devSpec.freq << " format=" << devSpec.format << " channels=" << (int)devSpec.channels << std::endl;
		samples[id].spec = {};
		samples[id].spec.freq = samplerate;
		samples[id].spec.format = SDL_AUDIO_S16LE;
		samples[id].spec.channels = static_cast<Uint8>(channels);
		samples[id].stream = SDL_CreateAudioStream(&samples[id].spec, &devSpec);
		if (!samples[id].stream) {
			std::cout << "SDL_CreateAudioStream (OGG) Error : " << SDL_GetError() << "\n";
			return;
		}
		else if (!SDL_BindAudioStream(audio_device, samples[id].stream)) {
			std::cout << "SDL_PutAudioStreamData (OGG) Error: " << SDL_GetError() << "\n";
			free(output);
			return;
		}
		samples[id].data_len = numSamples * channels * sizeof(short);
		samples[id].data = (Uint8*)malloc(samples[id].data_len);
		if (samples[id].data == nullptr) {
			std::cout << "malloc (OGG) Error.\n";
			free(output);
			return;
		}
		memcpy(samples[id].data, output, samples[id].data_len);
		free(output);
		std::cout << "Loaded OGG Sample ID : " << id << "\n";
	}
	else std::cout << "Audio File not supported.\n";
	
}
void SDL3Backend::PlaySample(int id, int channel, int loops, int freq, bool interruptable) {
	if (!samples[id].stream) return;

	if (freq != NULL || freq <= -1) samples[id].spec.freq = freq;
	
	samples[id].loops = loops;

	if (channel <= 0) {
		for (int i = 1; i <= 32; i++) {
			if (!channels[i].containsSample) {
				samples[id].active = true;
				channels[i].containsSample = true;
				channel = i;
				break;
			}
		}
	}
	samples[id].active = true;
	channels[channel].containsSample = true;
	std::cout << "Sample ID " << id << " is now playing at channel " << channel << ".\n";
}
const uint8_t* SDL3Backend::GetKeyboardState()
{
	//return the keyboard state in a new array which matches the Fusion key codes
	const bool* keyboardState = SDL_GetKeyboardState(nullptr);
	uint8_t* fusionKeyboardState = new uint8_t[256];
	for (int i = 0; i < 256; i++)
	{
		fusionKeyboardState[i] = keyboardState[FusionToSDLKey(i)] ? 1 : 0;
	}
	return fusionKeyboardState;
}

SDL_FRect SDL3Backend::CalculateRenderTargetRect()
{
	// get actual current window size
	int currentWindowWidth, currentWindowHeight;
	SDL_GetWindowSize(window, &currentWindowWidth, &currentWindowHeight);
	
	// get app size
	int renderTargetWidth = std::min(Application::Instance().GetAppData()->GetWindowWidth(), Application::Instance().GetCurrentFrame()->Width);
	int renderTargetHeight = std::min(Application::Instance().GetAppData()->GetWindowHeight(), Application::Instance().GetCurrentFrame()->Height);

	SDL_FRect rect = { 0.0f, 0.0f, static_cast<float>(renderTargetWidth), static_cast<float>(renderTargetHeight) };

	if (Application::Instance().GetAppData()->GetResizeDisplay()) {
		rect.w = static_cast<float>(currentWindowWidth);
		rect.h = static_cast<float>(currentWindowHeight);

		if (Application::Instance().GetAppData()->GetFitInside()) {
			//keeps the aspect ratio of the application and fits inside the window while staying in the center
			float aspectRatio = static_cast<float>(renderTargetWidth) / static_cast<float>(renderTargetHeight);
			if (rect.w / rect.h > aspectRatio) {
				rect.w = rect.h * aspectRatio;
			}
			else {
				rect.h = rect.w / aspectRatio;
			}
			rect.x = static_cast<float>((currentWindowWidth - static_cast<int>(rect.w)) / 2);
			rect.y = static_cast<float>((currentWindowHeight - static_cast<int>(rect.h)) / 2);
		}
	}
	else if (!Application::Instance().GetAppData()->GetDontCenterFrame()) {
		rect.x = static_cast<float>((currentWindowWidth - static_cast<int>(rect.w)) / 2);
		rect.y = static_cast<float>((currentWindowHeight - static_cast<int>(rect.h)) / 2);
	}
	
	return rect;
}

int SDL3Backend::GetMouseX()
{
	float x;
#ifndef PLATFORM_WEB
	int windowX;
	SDL_GetWindowPosition(window, &windowX, NULL);
	SDL_GetGlobalMouseState(&x, NULL);
	float mouseX = x - windowX;
#else
	float mouseX;
	SDL_GetMouseState(&mouseX, NULL);
#endif
	
	//get mouse position relative to render target
	SDL_FRect rect = CalculateRenderTargetRect();
	int windowWidth = std::min(Application::Instance().GetAppData()->GetWindowWidth(), Application::Instance().GetCurrentFrame()->Width);
	float relativeX = (mouseX - rect.x) * (windowWidth / rect.w);
	return static_cast<int>(relativeX);
}

int SDL3Backend::GetMouseY()
{
	float y;
#ifndef PLATFORM_WEB
	int windowY;
	SDL_GetWindowPosition(window, NULL, &windowY);
	SDL_GetGlobalMouseState(NULL, &y);
	float mouseY = y - windowY;
#else
	float mouseY;
	SDL_GetMouseState(NULL, &mouseY);
#endif

	//get mouse position relative to render target
	SDL_FRect rect = CalculateRenderTargetRect();
	int windowHeight = std::min(Application::Instance().GetAppData()->GetWindowHeight(), Application::Instance().GetCurrentFrame()->Height);
	float relativeY = (mouseY - rect.y) * (windowHeight / rect.h);
	return static_cast<int>(relativeY);
}

void SDL3Backend::SetMouseX(int x)
{
	SDL_FRect rect = CalculateRenderTargetRect();
	int renderTargetWidth = std::min(Application::Instance().GetAppData()->GetWindowWidth(), Application::Instance().GetCurrentFrame()->Width);
	
	float windowX = rect.x + (x * rect.w / renderTargetWidth);
	float windowY;
	
	SDL_GetMouseState(NULL, &windowY);
	SDL_WarpMouseInWindow(window, windowX, windowY);
}

void SDL3Backend::SetMouseY(int y)
{
	SDL_FRect rect = CalculateRenderTargetRect();
	int renderTargetHeight = std::min(Application::Instance().GetAppData()->GetWindowHeight(), Application::Instance().GetCurrentFrame()->Height);
	
	float windowX;
	float windowY = rect.y + (y * rect.h / renderTargetHeight);
	
	SDL_GetMouseState(&windowX, NULL);
	SDL_WarpMouseInWindow(window, windowX, windowY);
}

int SDL3Backend::GetMouseWheelMove()
{
	SDL_Event event;
	while (SDL_PollEvent(&event)) {
		if (event.type == SDL_EVENT_MOUSE_WHEEL) {
			return event.wheel.y;
		}
	}
	return 0;
}

uint32_t SDL3Backend::GetMouseState()
{
	return SDL_GetMouseState(nullptr, nullptr);
}

void SDL3Backend::HideMouseCursor()
{
	SDL_HideCursor();
}

void SDL3Backend::ShowMouseCursor()
{
	SDL_ShowCursor();
}

SDL_Color SDL3Backend::RGBToSDLColor(int color)
{
	return SDL_Color{
		static_cast<Uint8>((color >> 16) & 0xFF),
		static_cast<Uint8>((color >> 8) & 0xFF),
		static_cast<Uint8>(color & 0xFF),
		255
	};
}

SDL_Color SDL3Backend::RGBAToSDLColor(int color)
{
	return SDL_Color{
		static_cast<Uint8>((color >> 16) & 0xFF),
		static_cast<Uint8>((color >> 8) & 0xFF),
		static_cast<Uint8>(color & 0xFF),
		static_cast<Uint8>((color >> 24) & 0xFF)
	};
}

int SDL3Backend::FusionToSDLKey(short key)
{
	switch (key)
	{
		default:
			return SDL_SCANCODE_UNKNOWN;
		case 0x08:
			return SDL_SCANCODE_BACKSPACE;
		case 0x09:
			return SDL_SCANCODE_TAB;
		case 0x0D:
			return SDL_SCANCODE_RETURN;
		case 0x10:
			return SDL_SCANCODE_LSHIFT;
		case 0x11:
			return SDL_SCANCODE_LCTRL;
		case 0x13:
			return SDL_SCANCODE_PAUSE;
		case 0x14:
			return SDL_SCANCODE_CAPSLOCK;
		case 0x1B:
			return SDL_SCANCODE_ESCAPE;
		case 0x20:
			return SDL_SCANCODE_SPACE;
		case 0x21:
			return SDL_SCANCODE_PAGEUP;
		case 0x22:
			return SDL_SCANCODE_PAGEDOWN;
		case 0x23:
			return SDL_SCANCODE_END;
		case 0x24:
			return SDL_SCANCODE_HOME;
		case 0x25:
			return SDL_SCANCODE_LEFT;
		case 0x26:
			return SDL_SCANCODE_UP;
		case 0x27:
			return SDL_SCANCODE_RIGHT;
		case 0x28:
			return SDL_SCANCODE_DOWN;
		case 0x2D:
			return SDL_SCANCODE_INSERT;
		case 0x2E:
			return SDL_SCANCODE_DELETE;
		case 0x30:
			return SDL_SCANCODE_0;
		case 0x31:
			return SDL_SCANCODE_1;
		case 0x32:
			return SDL_SCANCODE_2;
		case 0x33:
			return SDL_SCANCODE_3;
		case 0x34:
			return SDL_SCANCODE_4;
		case 0x35:
			return SDL_SCANCODE_5;
		case 0x36:
			return SDL_SCANCODE_6;
		case 0x37:
			return SDL_SCANCODE_7;
		case 0x38:
			return SDL_SCANCODE_8;
		case 0x39:
			return SDL_SCANCODE_9;
		case 0x41:
			return SDL_SCANCODE_A;
		case 0x42:
			return SDL_SCANCODE_B;
		case 0x43:
			return SDL_SCANCODE_C;
		case 0x44:
			return SDL_SCANCODE_D;
		case 0x45:
			return SDL_SCANCODE_E;
		case 0x46:
			return SDL_SCANCODE_F;
		case 0x47:
			return SDL_SCANCODE_G;
		case 0x48:
			return SDL_SCANCODE_H;
		case 0x49:
			return SDL_SCANCODE_I;
		case 0x4A:
			return SDL_SCANCODE_J;
		case 0x4B:
			return SDL_SCANCODE_K;
		case 0x4C:
			return SDL_SCANCODE_L;
		case 0x4D:
			return SDL_SCANCODE_M;
		case 0x4E:
			return SDL_SCANCODE_N;
		case 0x4F:
			return SDL_SCANCODE_O;
		case 0x50:
			return SDL_SCANCODE_P;
		case 0x51:
			return SDL_SCANCODE_Q;
		case 0x52:
			return SDL_SCANCODE_R;
		case 0x53:
			return SDL_SCANCODE_S;
		case 0x54:
			return SDL_SCANCODE_T;
		case 0x55:
			return SDL_SCANCODE_U;
		case 0x56:
			return SDL_SCANCODE_V;
		case 0x57:
			return SDL_SCANCODE_W;
		case 0x58:
			return SDL_SCANCODE_X;
		case 0x59:
			return SDL_SCANCODE_Y;
		case 0x5A:
			return SDL_SCANCODE_Z;
		case 0x60:
			return SDL_SCANCODE_KP_0;
		case 0x61:
			return SDL_SCANCODE_KP_1;
		case 0x62:
			return SDL_SCANCODE_KP_2;
		case 0x63:
			return SDL_SCANCODE_KP_3;
		case 0x64:
			return SDL_SCANCODE_KP_4;
		case 0x65:
			return SDL_SCANCODE_KP_5;
		case 0x66:
			return SDL_SCANCODE_KP_6;
		case 0x67:
			return SDL_SCANCODE_KP_7;
		case 0x68:
			return SDL_SCANCODE_KP_8;
		case 0x69:
			return SDL_SCANCODE_KP_9;
		case 0x6A:
			return SDL_SCANCODE_KP_MULTIPLY;
		case 0x6B:
			return SDL_SCANCODE_KP_PLUS;
		case 0x6D:
			return SDL_SCANCODE_KP_MINUS;
		case 0x6E:
			return SDL_SCANCODE_KP_PERIOD;
		case 0x6F:
			return SDL_SCANCODE_KP_DIVIDE;
		case 0x70:
			return SDL_SCANCODE_F1;
		case 0x71:
			return SDL_SCANCODE_F2;
		case 0x72:
			return SDL_SCANCODE_F3;
		case 0x73:
			return SDL_SCANCODE_F4;
		case 0x74:
			return SDL_SCANCODE_F5;
		case 0x75:
			return SDL_SCANCODE_F6;
		case 0x76:
			return SDL_SCANCODE_F7;
		case 0x77:
			return SDL_SCANCODE_F8;
		case 0x78:
			return SDL_SCANCODE_F9;
		case 0x79:
			return SDL_SCANCODE_F10;
		case 0x7A:
			return SDL_SCANCODE_F11;
		case 0x7B:
			return SDL_SCANCODE_F12;
		case 0x90:
			return SDL_SCANCODE_NUMLOCKCLEAR;
		case 0xBA:
			return SDL_SCANCODE_SEMICOLON;
		case 0xBB:
			return SDL_SCANCODE_EQUALS;
		case 0xBC:
			return SDL_SCANCODE_COMMA;
		case 0xBD:
			return SDL_SCANCODE_MINUS;
		case 0xBE:
			return SDL_SCANCODE_PERIOD;
		case 0xBF:
			return SDL_SCANCODE_SLASH;
		case 0xC0:
			return SDL_SCANCODE_GRAVE;
		case 0xDB:
			return SDL_SCANCODE_LEFTBRACKET;
		case 0xDC:
			return SDL_SCANCODE_BACKSLASH;
		case 0xDD:
			return SDL_SCANCODE_RIGHTBRACKET;
		case 0xDE:
			return SDL_SCANCODE_APOSTROPHE;
	}
}

float SDL3Backend::GetTimeDelta()
{
	static Uint32 previousTicks = SDL_GetTicks();
	Uint32 currentTicks = SDL_GetTicks();
	float delta = (currentTicks - previousTicks) / 1000.0f;
	previousTicks = currentTicks;
	return delta;
}

void SDL3Backend::Delay(unsigned int ms)
{
    for (int i = 0; i < SDL_arraysize(samples); i++) {
        if (SDL_GetAudioStreamQueued(samples[i].stream) < ((int)samples[i].data_len) && samples[i].active)
            SDL_PutAudioStreamData(samples[i].stream, samples[i].data, samples[i].data_len);
        if (SDL_GetAudioStreamQueued(samples[i].stream) == ((int)samples[i].data_len)) {
			samples[i].active = false;
        }
    }
	SDL_Delay(ms);
}

bool SDL3Backend::IsPixelTransparent(int textureId, int x, int y)
{
	auto it = textures.find(textureId);
	if (it == textures.end()) return true;

	SDL_Texture* texture = it->second;
	int width, height;
	GetTextureDimensions(textureId, width, height);

	if (x < 0 || x >= width || y < 0 || y >= height) return true;

	// Create a surface to read the texture data
	SDL_Surface* surface = SDL_CreateSurface(width, height, SDL_GetPixelFormatForMasks(32, 0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000));
	if (!surface) return true;

	// Create a temporary render target
	SDL_Texture* tempTarget = SDL_CreateTexture(renderer, SDL_PIXELFORMAT_ARGB8888, SDL_TEXTUREACCESS_TARGET, width, height);
	if (!tempTarget) {
		SDL_DestroySurface(surface);
		return true;
	}

	// Save the current render target
	SDL_Texture* currentTarget = SDL_GetRenderTarget(renderer);

	// Set the temporary render target
	SDL_SetRenderTarget(renderer, tempTarget);

	// Copy the original texture to the temporary target
	SDL_RenderTexture(renderer, texture, nullptr, nullptr);

	// Read the pixels from the temporary target
	SDL_Rect rect = { 0, 0, width, height };
	SDL_Surface* surface2 = SDL_RenderReadPixels(renderer, &rect);

	// Restore the original render target
	SDL_SetRenderTarget(renderer, currentTarget);

	// Get the pixel data
	Uint32* pixels = static_cast<Uint32*>(surface2->pixels);
	Uint32 pixel = pixels[y * width + x];
	
	// Check alpha channel
	bool isTransparent = (pixel & 0xFF000000) == 0;

	// Clean up
	SDL_DestroyTexture(tempTarget);
	SDL_DestroySurface(surface);
	SDL_DestroySurface(surface2);

	return isTransparent;
}

void SDL3Backend::GetTextureDimensions(int textureId, int& width, int& height)
{
	auto it = textures.find(textureId);
	if (it != textures.end())
	{
		float w, h;
		SDL_GetTextureSize(it->second, &w, &h);
		width = static_cast<int>(w);
		height = static_cast<int>(h);
	}
	else
	{
		width = 0;
		height = 0;
	}
}
#endif