#include "IniExtension.h"
#include "Application.h"
#include <memory>
#include <string>

void IniExtension::Initialize()
{
	CurrentGroup = "";
	CurrentItem = "";

	SetFileName(Name);
}	

void IniExtension::SetFileName(const std::string& name)
{
	Name = name;
	std::filesystem::path path = Name;
	iniFile = std::make_unique<mINI::INIFile>(path);
	iniFile->read(ini);
}

void IniExtension::SetCurrentGroup(const std::string& group)
{
	CurrentGroup = group;
}

void IniExtension::SetCurrentItem(const std::string& item)
{
	CurrentItem = item;
}

void IniExtension::SetValue(int value)
{
	ini[CurrentGroup][CurrentItem] = std::to_string(value);
	iniFile->write(ini);
}

void IniExtension::SetValue(const std::string& item, int value)
{
	ini[CurrentGroup][item] = std::to_string(value);
	iniFile->write(ini);
}

void IniExtension::SetValue(const std::string& group, const std::string& item, int value)
{
	ini[group][item] = std::to_string(value);
	iniFile->write(ini);
}

void IniExtension::SetString(const std::string& value)
{
	ini[CurrentGroup][CurrentItem] = value;
	iniFile->write(ini);
}

void IniExtension::SetString(const std::string& item, const std::string& value)
{
	ini[CurrentGroup][item] = value;
	iniFile->write(ini);
}

void IniExtension::SetString(const std::string& group, const std::string& item, const std::string& value)
{
	ini[group][item] = value;
	iniFile->write(ini);
}

void IniExtension::SavePosition(ObjectInstance* object)
{
	std::string item = "pos." + object->Name;
	ini[CurrentGroup].set(item, std::to_string(object->X) + "," + std::to_string(object->Y));
	iniFile->write(ini);
}

void IniExtension::LoadPosition(ObjectInstance* object)
{
	std::string item = "pos." + object->Name;
	std::string value = ini[CurrentGroup][item];
	if (value.empty())
	{
		return;
	}

	std::string xValue = value.substr(0, value.find(','));
	std::string yValue = value.substr(value.find(',') + 1);

	object->X = std::stoi(xValue);
	object->Y = std::stoi(yValue);
}

int IniExtension::GetValue()
{
	std::string value = ini[CurrentGroup][CurrentItem];
	return value.empty() ? 0 : std::stoi(value);
}

int IniExtension::GetValue(const std::string& item)
{
	std::string value = ini[CurrentGroup][item];
	return value.empty() ? 0 : std::stoi(value);
}

int IniExtension::GetValue(const std::string& group, const std::string& item)
{
	std::string value = ini[group][item];
	return value.empty() ? 0 : std::stoi(value);
}

std::string IniExtension::GetString()
{
	return ini[CurrentGroup][CurrentItem];
}

std::string IniExtension::GetString(const std::string& item)
{
	return ini[CurrentGroup][item];
}

std::string IniExtension::GetString(const std::string& group, const std::string& item)
{
	return ini[group][item];
}

void IniExtension::DeleteGroup(const std::string& group)
{
	ini.remove(group);
	iniFile->write(ini);
}

void IniExtension::DeleteItem(const std::string& item)
{
	ini[CurrentGroup].remove(item);
	iniFile->write(ini);
}

void IniExtension::DeleteItem(const std::string& group, const std::string& item)
{
	ini[group].remove(item);
	iniFile->write(ini);
}