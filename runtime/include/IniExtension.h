#pragma once

#include "Application.h"
#include "Extension.h"
#include "ObjectInstance.h"
#include <string>
#include <filesystem>
#include "mini/ini.h"
	
class IniExtension : public Extension {
public:
	IniExtension(int flags, const std::string& name) : Flags(flags), Name(name) {}

	void Initialize() override;

	void SetCurrentGroup(const std::string& group);
	void SetCurrentItem(const std::string& item);

	void SetValue(int value);
	void SetValue(const std::string& item, int value);
	void SetValue(const std::string& group, const std::string& item, int value);

	void SetString(const std::string& value);
	void SetString(const std::string& item, const std::string& value);
	void SetString(const std::string& group, const std::string& item, const std::string& value);

	int GetValue();
	int GetValue(const std::string& item);
	int GetValue(const std::string& group, const std::string& item);

	std::string GetString();
	std::string GetString(const std::string& item);
	std::string GetString(const std::string& group, const std::string& item);
private:
	int Flags;
	std::string Name;

	std::string CurrentGroup;
	std::string CurrentItem;

	std::unique_ptr<mINI::INIFile> iniFile;
	mINI::INIStructure ini;
};