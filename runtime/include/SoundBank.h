#pragma once

#include <unordered_map>
#include <string>
#include <memory>

struct SoundInfo {
    unsigned int Handle;
    std::string Name;
    std::string Type;
    SoundInfo(unsigned int handle, std::string name, std::string type) : Handle(handle), Name(name), Type(type) {};
};
class SoundBank {
public:
    static SoundBank& Instance() {
        static SoundBank instance;
        return instance;
    }
    SoundInfo* GetSound(unsigned int handle) const {
        auto it = Sounds.find(handle);
        if (it != Sounds.end()) {
            return it->second;
        }
        return nullptr;
    }
    SoundInfo* GetSoundName(std::string name) const {
        for (const auto& pair : Sounds) {
            unsigned int id = pair.first;
            SoundInfo* sound = pair.second;
            if (sound->Name == name) return sound;
        }
        return nullptr;
    }
private:
    SoundBank();

    std::unordered_map<unsigned int, SoundInfo*> Sounds;
};