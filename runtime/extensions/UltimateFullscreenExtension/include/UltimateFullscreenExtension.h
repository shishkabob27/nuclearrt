#pragma once
#include "Application.h"
#include "Extension.h"
#include "ObjectInstance.h"
#include <string>

class UltimateFullscreenExtension : public Extension {
public:
    bool isFullscreen;
    void GoFullscreen();
    void GoWindowed();
    UltimateFullscreenExtension(short flags);
    void Initialize() override;
	void Update(float deltaTime) override;
};