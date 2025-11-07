#pragma once

#include <unordered_map>
#include <string>
#include <memory>

struct SoundInfo {
    unsigned int Handle;
    SoundInfo(unsigned int handle) : Handle(handle) {};
};
class SoundBank {
public:
    static SoundBank& Instance() {
        static SoundBank instance;
        return instance;
    }
    std::shared_ptr<SoundInfo> GetSound(unsigned int handle) const {
        auto it = Sounds.find(handle);
        if (it != Sounds.end()) {
            return it->second;
        }
        return nullptr;
    }
private:
    SoundBank();

    std::unordered_map<unsigned int, std::shared_ptr<SoundInfo>> Sounds;
};