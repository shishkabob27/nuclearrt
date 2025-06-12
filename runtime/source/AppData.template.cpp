#include "AppData.h"

void AppData::Initialize()
{
	m_appName = "{{ app_name }}";
	m_aboutBox = "{{ about_box }}";
	m_windowWidth = {{ window_width }};
	m_windowHeight = {{ window_height }};
	m_targetFPS = {{ target_fps }};
	m_borderColor = {{ border_color }};
	m_fitInside = {{ fit_inside }};
	m_resizeDisplay = {{ resize_display }};
	m_dontCenterFrame = {{ dont_center_frame }};

	m_globalValues = {{ global_values }};
	m_globalStrings = {{ global_strings }};

	m_controlTypes = {{ control_types }};
	m_controlKeys = {{ control_keys }};

	m_playerScores = {{ player_scores }};
	m_playerLives = {{ player_lives }};
}