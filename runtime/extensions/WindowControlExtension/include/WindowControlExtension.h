#pragma once

#include "Application.h"
#include "Extension.h"
#include "ObjectInstance.h"
#include <string>
class WindowControlExtension : public Extension {
public:
    WindowControlExtension(short flags);
    void Initialize() override;
	void Update(float deltaTime) override;
};