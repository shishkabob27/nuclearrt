#pragma once

#include <unordered_map>
#include <string>
#include <memory>

struct FontInfo {
    unsigned int Handle;

    std::string FontName;
    std::string FontFileName;

    int Width;
    int Height;

    int Escapment;
    int Orientation;
    int Weight;
    bool Italic;
    bool Underline;
    bool Strikeout;

    FontInfo(unsigned int handle, const std::string& fontName, const std::string& fontFileName, int width, int height, int escapment, int orientation, int weight, bool italic, bool underline, bool strikeout)
        : Handle(handle), FontName(fontName), FontFileName(fontFileName), Width(width), Height(height), Escapment(escapment), Orientation(orientation), Weight(weight), Italic(italic), Underline(underline), Strikeout(strikeout) {}
};

class FontBank {
public:
    static FontBank& Instance() {
        static FontBank instance;
        return instance;
    }
    
    FontInfo* GetFont(unsigned int handle) const {
        auto it = Fonts.find(handle);
        if (it != Fonts.end()) {
            return it->second;
        }
        return nullptr;
    }
    
private:
    FontBank();
    
    std::unordered_map<unsigned int, FontInfo*> Fonts;
}; 