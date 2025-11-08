#include "Application.h"
#include "FrameFactory.h"
#include "Frame.h"
#include <set>

#ifdef NUCLEAR_BACKEND_SDL3
#include "SDL3Backend.h"
#endif

#ifdef NUCLEAR_BACKEND_SDL2
#include "SDL2Backend.h"
#endif

#ifdef PLATFORM_WEB
#include <emscripten.h>
#endif

Application::Application() = default;
Application::~Application() = default;

void Application::Initialize()
{
	appData = std::make_shared<AppData>();
	appData->Initialize();
	std::cout << "Initialized AppData" << std::endl;

	#ifdef NUCLEAR_BACKEND_SDL3
		backend = std::make_shared<SDL3Backend>();
	#elif NUCLEAR_BACKEND_SDL2
		backend = std::make_shared<SDL2Backend>();
	#else
		backend = std::make_shared<Backend>();
	#endif

	backend->Initialize();
	std::cout << "Initialized Backend: " << backend->GetName() << std::endl;

	input = std::make_shared<Input>();
	input->Reset();
	std::cout << "Initialized Input" << std::endl;

	QueueStateChange(GameState::RestartApplication);

	srand(time(0));
}

void Application::Update()
{
	RunState();

	if (currentFrame != nullptr)
	{
		input->Update();
		currentFrame->Update();
	}
	else
	{
		//exit the application
		Shutdown();
	}
}

void Application::Draw()
{
	backend->BeginDrawing();
	currentFrame->Draw();
	backend->EndDrawing();
}

void Application::Shutdown()
{
	backend->Deinitialize();
}

static void Loop()
{
	Application::Instance().Update();
	Application::Instance().Draw();
}

void Application::Run()
{
#ifdef PLATFORM_WEB
	emscripten_set_main_loop(Loop, appData->GetTargetFPS(), 1);
#else
	unsigned int frameStart;
	int frameTime;
	
	while (!backend->ShouldQuit())
	{
		int targetFPS = appData->GetTargetFPS();
		float targetFrameTime = 1000.0f / targetFPS;
		
		frameStart = backend->GetTicks();
		
		Loop();
		
		frameTime = backend->GetTicks() - frameStart;
		
		if (frameTime < targetFrameTime)
		{
			backend->Delay(targetFrameTime - frameTime);
		}
	}
#endif

	Shutdown();
}

void Application::QueueStateChange(GameState newState, int frameIndex)
{
	currentState = newState;
	newFrameIndex = frameIndex;
}

short Application::Random(short max)
{
	return rand() % (max + 1);
}

short Application::RandomRange(short min, short max)
{
	return rand() % (max - min + 1) + min;
}

void Application::LoadFrame(int frameIndex)
{
	if (frameIndex < 0)
	{
		frameIndex = 0;
	}

	std::cout << "Loading frame " << frameIndex << std::endl;
	std::vector<unsigned int> oldImagesUsed;
	std::vector<unsigned int> oldFontsUsed;
	if (currentFrame != nullptr)
	{
		oldImagesUsed = currentFrame->GetImagesUsed();
		oldFontsUsed = currentFrame->GetFontsUsed();

		// merge current frame's global object data with existing data
		std::vector<ObjectGlobalData*> frameData = currentFrame->GetGlobalObjectData();
		MergeGlobalObjectData(frameData);
	}

	currentFrame = FrameFactory::CreateFrame(frameIndex);
	currentFrame->Initialize();
	
	// apply global object data to new frame
	if (!globalObjectData.empty())
	{
		currentFrame->ApplyGlobalObjectData(globalObjectData);
	}
	
	backend->ShowMouseCursor();

	std::vector<unsigned int> newImagesUsed = currentFrame->GetImagesUsed();
	std::vector<unsigned int> newFontsUsed = currentFrame->GetFontsUsed();

	std::vector<unsigned int> imagesToUnload;
	std::vector<unsigned int> fontsToUnload;
	for (unsigned int image : oldImagesUsed)
	{
		if (std::find(newImagesUsed.begin(), newImagesUsed.end(), image) == newImagesUsed.end())
		{
			imagesToUnload.push_back(image);
		}
	}

	for (unsigned int font : oldFontsUsed)
	{
		if (std::find(newFontsUsed.begin(), newFontsUsed.end(), font) == newFontsUsed.end())
		{
			fontsToUnload.push_back(font);
		}
	}

	//unload the images that are no longer used
	for (unsigned int image : imagesToUnload)
	{
		backend->UnloadTexture(image);
	}

	//load the images that are new
	for (unsigned int image : newImagesUsed)
	{
		backend->LoadTexture(image);
	}

	//load the fonts that are new
	for (unsigned int font : newFontsUsed)
	{
		backend->LoadFont(font);
	}

	//unload the fonts that are no longer used
	for (unsigned int font : fontsToUnload)
	{
		backend->UnloadFont(font);
	}

	std::cout << "Loaded frame " << frameIndex << std::endl;
}

void Application::MergeGlobalObjectData(std::vector<ObjectGlobalData*> frameData)
{
	std::map<unsigned int, std::vector<ObjectGlobalData*>> existingByHandle;
	for (auto* data : globalObjectData)
	{
		existingByHandle[data->objectInfoHandle].push_back(data);
	}
	
	std::map<unsigned int, std::vector<ObjectGlobalData*>> frameByHandle;
	for (auto* data : frameData)
	{
		frameByHandle[data->objectInfoHandle].push_back(data);
	}
	
	globalObjectData.clear();
	
	std::set<unsigned int> allHandles;
	for (auto& [handle, dataList] : existingByHandle)
	{
		allHandles.insert(handle);
	}
	for (auto& [handle, dataList] : frameByHandle)
	{
		allHandles.insert(handle);
	}
	
	for (unsigned int handle : allHandles)
	{
		bool hasExisting = existingByHandle.find(handle) != existingByHandle.end();
		bool hasFrame = frameByHandle.find(handle) != frameByHandle.end();
		
		if (hasFrame)
		{
			int frameCount = frameByHandle[handle].size();
			
			if (hasExisting)
			{
				int savedCount = existingByHandle[handle].size();
				
				if (frameCount <= savedCount)
				{
					auto& savedList = existingByHandle[handle];
					auto& frameList = frameByHandle[handle];
					
					for (int i = 0; i < savedCount - frameCount; i++)
					{
						globalObjectData.push_back(savedList[i]);
					}
					for (int i = 0; i < frameCount; i++)
					{
						globalObjectData.push_back(frameList[i]);
					}
				}
				else
				{
					for (auto* data : frameByHandle[handle])
					{
						globalObjectData.push_back(data);
					}
				}
			}
			else
			{
				for (auto* data : frameByHandle[handle])
				{
					globalObjectData.push_back(data);
				}
			}
		}
		else
		{
			for (auto* data : existingByHandle[handle])
			{
				globalObjectData.push_back(data);
			}
		}
	}
}

void Application::RunState()
{
	switch (currentState)
	{
		case GameState::Running:
			break;
		case GameState::RestartApplication:
			globalObjectData.clear();
			LoadFrame(0);
			currentState = GameState::StartOfFrame;
			break;
		case GameState::StartOfFrame:
			currentState = GameState::Running;
			break;
		case GameState::NextFrame:
			LoadFrame(currentFrame->Index + 1);
			currentState = GameState::StartOfFrame;
			break;
		case GameState::PreviousFrame:
			LoadFrame(currentFrame->Index - 1);
			currentState = GameState::StartOfFrame;
			break;
		case GameState::JumpToFrame:
			LoadFrame(newFrameIndex);
			currentState = GameState::StartOfFrame;
			break;
		case GameState::RestartFrame:
			LoadFrame(currentFrame->Index);
			currentState = GameState::StartOfFrame;
			break;
		case GameState::EndApplication:
			Shutdown();
			break;
	}
}
