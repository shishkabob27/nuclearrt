#pragma once

#include <string>
#include <vector>

class AppData {
public:
	AppData() = default;
	~AppData() = default;

	void Initialize();

	// Getters and setters
	std::string GetAppName() const { return m_appName; }
	void SetAppName(const std::string& name) { m_appName = name; }

	std::string GetAboutBox() const { return m_aboutBox; }
	void SetAboutBox(const std::string& about) { m_aboutBox = about; }

	int GetWindowWidth() const { return m_windowWidth; }
	void SetWindowWidth(int width) { m_windowWidth = width; }

	int GetWindowHeight() const { return m_windowHeight; }
	void SetWindowHeight(int height) { m_windowHeight = height; }

	int GetTargetFPS() const { return m_targetFPS; }
	void SetTargetFPS(int fps) { m_targetFPS = fps; }

	int GetBorderColor() const { return m_borderColor; }
	void SetBorderColor(int color) { m_borderColor = color; }

	bool GetFitInside() const { return m_fitInside; }
	void SetFitInside(bool fit) { m_fitInside = fit; }

	bool GetResizeDisplay() const { return m_resizeDisplay; }
	void SetResizeDisplay(bool resize) { m_resizeDisplay = resize; }

	bool GetDontCenterFrame() const { return m_dontCenterFrame; }
	void SetDontCenterFrame(bool dontCenter) { m_dontCenterFrame = dontCenter; }

	std::vector<int>& GetGlobalValues() { return m_globalValues; }
	void SetGlobalValues(const std::vector<int>& values) { m_globalValues = values; }

	std::vector<std::string>& GetGlobalStrings() { return m_globalStrings; }
	void SetGlobalStrings(const std::vector<std::string>& strings) { m_globalStrings = strings; }

	std::vector<int>& GetControlTypes() { return m_controlTypes; }
	void SetControlTypes(const std::vector<int>& types) { m_controlTypes = types; }

	std::vector<std::vector<int>>& GetControlKeys() { return m_controlKeys; }
	void SetControlKeys(const std::vector<std::vector<int>>& keys) { m_controlKeys = keys; }

	std::vector<int>& GetPlayerScores() { return m_playerScores; }
	void SetPlayerScores(const std::vector<int>& scores) { m_playerScores = scores; }
	void SetScore(int playerIndex, int score) { m_playerScores[playerIndex] = score; }
	void AddScore(int playerIndex, int score) { m_playerScores[playerIndex] += score; }
	void SubtractScore(int playerIndex, int score) { m_playerScores[playerIndex] -= score; }

	std::vector<int>& GetPlayerLives() { return m_playerLives; }
	void SetPlayerLives(const std::vector<int>& lives) { m_playerLives = lives; }
	void SetLives(int playerIndex, int lives) { m_playerLives[playerIndex] = lives; }
	void AddLives(int playerIndex, int lives) { m_playerLives[playerIndex] += lives; }
	void SubtractLives(int playerIndex, int lives) { m_playerLives[playerIndex] -= lives; }

private:
	// Default values
	std::string m_appName = "NuclearRT";
	std::string m_aboutBox = "";
	int m_windowWidth = 640;
	int m_windowHeight = 480;
	int m_targetFPS = 60;
	int m_borderColor = 0;
	bool m_fitInside = false;
	bool m_resizeDisplay = false;
	bool m_dontCenterFrame = false;
	std::vector<int> m_globalValues;
	std::vector<std::string> m_globalStrings;

	std::vector<int> m_controlTypes = { 5, 5, 5, 5 };
	std::vector<std::vector<int>> m_controlKeys = { 
		{ 38, 40, 37, 39, 16, 17, 32, 13 },
		{ 38, 40, 37, 39, 16, 17, 32, 13 },
		{ 38, 40, 37, 39, 16, 17, 32, 13 },
		{ 38, 40, 37, 39, 16, 17, 32, 13 }
	};

	std::vector<int> m_playerScores = { 0, 0, 0, 0 };
	std::vector<int> m_playerLives = { 3, 3, 3, 3 };
};	