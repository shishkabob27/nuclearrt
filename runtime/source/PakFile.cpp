#include "PakFile.h"
#include <string>
#include <iostream>
#include <cstring>

bool PakFile::Load(const std::string& filename) {
    pakStream.open(filename, std::ios::binary);
    if (!pakStream) return false;

    char magic[4];
    pakStream.read(magic, 4);
    if (memcmp(magic, "NRTP", 4) != 0) return false;

    unsigned int entryCount;
    pakStream.read(reinterpret_cast<char*>(&entryCount), 4);

    for (unsigned int i = 0; i < entryCount; ++i) {
        PakEntry entry;

        pakStream.read(reinterpret_cast<char*>(&entry.type), 1);
        pakStream.read(reinterpret_cast<char*>(&entry.id), 4);
        pakStream.read(reinterpret_cast<char*>(&entry.offset), 4);
        pakStream.read(reinterpret_cast<char*>(&entry.size), 4);

        if (entry.type == PakAssetType::Image) {
            imageEntries[entry.id] = entry;
        }
        else if (entry.type == PakAssetType::Font) {
            fontEntries[entry.id] = entry;
        }
    }

    return true;
}

std::vector<uint8_t> PakFile::GetImageData(unsigned int id) {
    auto it = imageEntries.find(id);
    if (it == imageEntries.end()) return {};

    const PakEntry& entry = it->second;
    pakStream.seekg(entry.offset);

    std::vector<uint8_t> data(entry.size);
    pakStream.read(reinterpret_cast<char*>(data.data()), entry.size);

    return data;
}

std::vector<uint8_t> PakFile::GetFontData(unsigned int id) {
    auto it = fontEntries.find(id);
    if (it == fontEntries.end()) return {};

    const PakEntry& entry = it->second;
    pakStream.seekg(entry.offset);

    std::vector<uint8_t> data(entry.size);
    pakStream.read(reinterpret_cast<char*>(data.data()), entry.size);
    return data;
}