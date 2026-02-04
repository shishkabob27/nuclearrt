#pragma once

#include "ObjectInstance.h"
#include "ObjectFactory.h"
#include "ObjectSelector.h"
#include "Timer.h"
#include <vector>
#include <memory>
#include "Layer.h"
#include "CounterBase.h"

class Frame {
public:
	Frame() = default;
    virtual ~Frame() = default;

	int Index = -1;
	std::string Name = "";

	int Width = 0;
	int Height = 0;

	int BackgroundColor = 0;

	std::vector<Layer> Layers;

	std::unordered_map<unsigned int, ObjectInstance*> ObjectInstances;
	unsigned int MaxObjectInstanceHandle = 0;
	
	Timer GameTimer;

	bool IsGroupActive(unsigned int groupId) {
		if (groupId < ActiveGroups.size()) {
			return ActiveGroups[groupId];
		}
		return false;
	}

	void SetGroupActive(unsigned int groupId, bool active) {
		if (groupId >= ActiveGroups.size()) {
			ActiveGroups.resize(groupId + 1, false);
		}
		ActiveGroups[groupId] = active;
	}

	virtual void Initialize();
	void PostInitialize();
	virtual void Update();
	virtual void Draw();

	void SetScroll(int x, int y, int layer = -1);
	void SetScrollX(int x);
	void SetScrollY(int y);

	//mark an instance for deletion
	void MarkForDeletion(ObjectInstance* instance) {
		if (instance) {
			instancesMarkedForDeletion.push_back(instance->Handle);
		}
	}

	void DeleteMarkedInstances() {
		for (auto& handle : instancesMarkedForDeletion) {
			for (auto& layer : Layers) {
				auto it = std::find_if(layer.instances.begin(), layer.instances.end(), [handle](ObjectInstance* instance) {
					return instance->Handle == handle;
				});
				if (it != layer.instances.end()) {
					layer.instances.erase(it);
					break;
				}
			}
			ObjectInstances.erase(handle);
		}
		instancesMarkedForDeletion.clear();
	}

	void DrawLayer(Layer& layer);
	void DrawCounterNumbers(CounterBase *counter, int value, int x, int y);

	std::vector<unsigned int> GetImagesUsed();
	std::vector<unsigned int> GetFontsUsed();

	ObjectInstance* CreateInstance(ObjectInstance* createdInstance, short x, short y, unsigned int layer, short instanceValue, unsigned int objectInfoHandle, short angle, ObjectInstance* parentInstance = nullptr);

	std::vector<ObjectGlobalData*> GetGlobalObjectData();
	void ApplyGlobalObjectData(std::vector<ObjectGlobalData*> globalData);

	void MoveObjectToLayer(ObjectInstance* instance, unsigned int layer);
	void MoveObjectToFront(ObjectInstance* instance);
	void MoveObjectToBack(ObjectInstance* instance);
	void MoveObjectInFrontOf(ObjectInstance* instance, unsigned int oiHandle);
	void MoveObjectBehindOf(ObjectInstance* instance, unsigned int oiHandle);

	int GetMouseX();
	int GetMouseY();

	inline int StringLength(std::string str) {
		return (int)str.length();
	}

	inline std::string StringLeft(std::string str, int length) {
		return str.substr(0, length);
	}

	inline std::string StringRight(std::string str, int length) {
		return str.substr(str.length() - length);
	}

    inline std::string Hex(int v) {
        char buf[16];
        sprintf_s(buf, "%X", v);
        return std::string(buf);
    }

    inline std::string Bin(int v) {
        std::string r;
        for (int i = 31; i >= 0; i--) {
            if (v & (1 << i)) r += '1';
            else if (!r.empty()) r += '0';
        }
        return r.empty() ? "0" : r;
    }

    inline std::string Mid(const std::string& str, int start, int length) {
        if (start < 0) start = 0;
        if (start >= (int)str.length()) return "";
        return str.substr(start, length);
    }

    inline std::string Lower(std::string str) {
        std::transform(str.begin(), str.end(), str.begin(), ::tolower);
        return str;
    }

    inline std::string Upper(std::string str) {
        std::transform(str.begin(), str.end(), str.begin(), ::toupper);
        return str;
    }

    inline int Find(const std::string& str, const std::string& find, int start) {
        if (start < 0) start = 0;
        auto pos = str.find(find, start);
        return (pos == std::string::npos) ? -1 : (int)pos;
    }

    inline int ReverseFind(const std::string& str, const std::string& find, int start) {
        auto pos = str.rfind(find, (start < 0 || start >= str.length()) ? std::string::npos : start);
        return (pos == std::string::npos) ? -1 : (int)pos;
    }

    inline std::string ReplaceString(std::string str, const std::string& find, const std::string& replace) {
        if (find.empty()) return str;
        size_t pos = 0;
        while ((pos = str.find(find, pos)) != std::string::npos) {
            str.replace(pos, find.length(), replace);
            pos += replace.length();
        }
        return str;
    }

    inline std::string NewLine() { return "\n"; }

	int Loopindex(std::string loopName) {
		//TODO: loopindex will return 0 untul the loop system can support expressions
		return 0;
	}

	//Collision detection
	bool IsCollidingWithBackground(ObjectInstance* instance);
	bool IsColliding(ObjectInstance* instance1, ObjectInstance* instance2);
	bool IsColliding(ObjectInstance* instance, int x, int y);
	bool IsPointInPolygon(int x, int y, int polygon[][2], int numPoints);
	bool DoLinesIntersect(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4);

	void RotatePoints(int& x1, int& y1, int& x2, int& y2, int& x3, int& y3, int& x4, int& y4, int offsetX, int offsetY, float angle);
	void RotatePoint(int& x, int& y, float angle);
	bool IsPointOnLine(int x, int y, int x1, int y1, int x2, int y2);

private:
	std::vector<unsigned int> instancesMarkedForDeletion;
	std::vector<bool> ActiveGroups;

	int scrollX = 0;
	int scrollY = 0;
};
