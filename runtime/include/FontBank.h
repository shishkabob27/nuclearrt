#pragma once

#include <unordered_map>
#include <string>
#include <memory>

struct FontInfo {
    unsigned int Handle;

    std::string Name;

    int Width;
    int Height;

    int Escapment;
    int Orientation;
    int Weight;
    bool Italic;
    bool Underline;
    bool Strikeout;

    FontInfo(unsigned int handle, const std::string& name, int width, int height, int escapment, int orientation, int weight, bool italic, bool underline, bool strikeout)
        : Handle(handle), Name(name), Width(width), Height(height), Escapment(escapment), Orientation(orientation), Weight(weight), Italic(italic), Underline(underline), Strikeout(strikeout) {}
};

class FontBank {
public:
    static FontBank& Instance() {
        static FontBank instance;
        return instance;
    }
    
    std::shared_ptr<FontInfo> GetFont(unsigned int handle) const {
        auto it = Fonts.find(handle);
        if (it != Fonts.end()) {
            return it->second;
        }
        return nullptr;
    }
    
private:
    FontBank();
    
    std::unordered_map<unsigned int, std::shared_ptr<FontInfo>> Fonts;
}; 