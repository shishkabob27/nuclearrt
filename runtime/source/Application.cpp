#include "Application.h"
#include "FrameFactory.h"
#include "Frame.h"

#ifdef NUCLEAR_BACKEND_SDL3
#include "SDL3Backend.h"
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

void Application::Run()
{
	unsigned int frameStart;
	int frameTime;
	
	while (!backend->ShouldQuit())
	{
		int targetFPS = appData->GetTargetFPS();
		float targetFrameTime = 1000.0f / targetFPS;
		
		frameStart = backend->GetTicks();
		
		Update();
		Draw();
		
		frameTime = backend->GetTicks() - frameStart;
		
		if (frameTime < targetFrameTime)
		{
			backend->Delay(targetFrameTime - frameTime);
		}
	}

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
	}

	currentFrame = FrameFactory::CreateFrame(frameIndex);
	currentFrame->Initialize();

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

void Application::RunState()
{
	switch (currentState)
	{
		case GameState::RestartApplication:
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
