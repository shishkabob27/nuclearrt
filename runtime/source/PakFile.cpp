#include "PakFile.h"
#include <string>
#include <iostream>
#include <cstring>

bool PakFile::Load(const std::string& filename) {
	pakStream.open(filename, std::ios::binary);
	if (!pakStream) {
		std::cerr << "Failed to open pak file: " << filename << std::endl;
		return false;
	}

	// read header
	char magic[4];
	pakStream.read(magic, 4);
	if (memcmp(magic, "PACK", 4) != 0) {
		std::cerr << "Invalid pak file magic: " << std::string(magic, 4) << std::endl;
		return false;
	}

	unsigned int dirOffset, dirSize;
	pakStream.read(reinterpret_cast<char*>(&dirOffset), 4);
	pakStream.read(reinterpret_cast<char*>(&dirSize), 4);

	// read directory
	pakStream.seekg(dirOffset);
	return ReadDirectory();
}

bool PakFile::ReadDirectory() {
	unsigned int entryCount = 0;
	
	while (pakStream.good() && !pakStream.eof()) {
		char filename[56];
		pakStream.read(filename, 56);
		
		// check if we've reached the end
		if (pakStream.eof()) break;
		
		// check if filename is all zeros (end of directory)
		bool allZeros = true;
		for (int i = 0; i < 56; i++) {
			if (filename[i] != 0) {
				allZeros = false;
				break;
			}
		}
		if (allZeros) break;
		
		// null-terminate the filename
		filename[55] = '\0';
		std::string name = std::string(filename);
		
		// trim null bytes
		size_t nullPos = name.find('\0');
		if (nullPos != std::string::npos) {
			name = name.substr(0, nullPos);
		}
		
		if (name.empty()) break;

		PakEntry entry;
		entry.filename = name;
		pakStream.read(reinterpret_cast<char*>(&entry.offset), 4);
		pakStream.read(reinterpret_cast<char*>(&entry.size), 4);
		
		entries[name] = entry;
		entryCount++;
	}
	
	return true;
}

std::vector<uint8_t> PakFile::GetData(const std::string& filename) {
	auto it = entries.find(filename);
	if (it == entries.end()) {
		std::cerr << "File not found in PAK: " << filename << std::endl;
		return {};
	}

	const PakEntry& entry = it->second;
	
	pakStream.clear();
	pakStream.seekg(entry.offset);
	
	std::vector<uint8_t> data(entry.size);
	pakStream.read(reinterpret_cast<char*>(data.data()), entry.size);
	
	return data;
}