#pragma once

#include "Frame.h"
#include "ObjectInstance.h"
#include "ObjectSelector.h"

class GeneratedFrame{{ FRAME_INDEX }} : public Frame {
public:
    GeneratedFrame{{ FRAME_INDEX }}() = default;
    ~GeneratedFrame{{ FRAME_INDEX }}() = default;

	void Initialize() override;
	void Update() override;
	void Draw() override;

private:
	// Event functions
	{{ EVENT_INCLUDES }}

	{{ OBJECT_SELECTORS }}

	{{ LOOP_INCLUDES }}

	{{ RUN_ONCE_CONDITION }}

	{{ ONLY_ONE_ACTION_WHEN_LOOP_CONDITION }}
};
