#pragma once

#include <string>
#include <unordered_map>
#include <vector>
#include <memory>
#include <fstream>

struct PakEntry {
    std::string filename;
    unsigned int offset;
    unsigned int size;
};

class PakFile {
public:
    bool Load(const std::string& filename);
    std::vector<uint8_t> GetData(const std::string& filename);
private:
    std::ifstream pakStream;
    std::unordered_map<std::string, PakEntry> entries;
    bool ReadDirectory();
};
