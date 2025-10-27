#pragma once

#include <string>

class Paragraph
{
public:
	unsigned short Font;
	int Color;

	std::string Text;

	Paragraph(unsigned short font, int color, std::string text)
		: Font(font), Color(color), Text(text) {}
};