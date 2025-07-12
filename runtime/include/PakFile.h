#pragma once

#include <string>
#include <unordered_map>
#include <vector>
#include <memory>
#include <fstream>

enum class PakAssetType : uint8_t {
    Image = 0,
    Sound = 1,
    Font = 2,
    Binary = 3,
};

struct PakEntry {
    PakAssetType type;
    unsigned int id;
    unsigned int offset;
    unsigned int size;
};

class PakFile {
public:
    bool Load(const std::string& filename);
    std::vector<uint8_t> GetImageData(unsigned int id);
    std::vector<uint8_t> GetFontData(unsigned int id);
private:
    std::ifstream pakStream;
    std::unordered_map<unsigned int, PakEntry> imageEntries;
    std::unordered_map<unsigned int, PakEntry> fontEntries;
    std::vector<uint8_t> pakData;
};
