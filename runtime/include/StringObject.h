#pragma once

#include "Shape.h"
#include <vector>
#include <memory>
#include "Paragraph.h"

class StringObject : public ObjectInstance {
public:
	StringObject(unsigned int objectInfoHandle, int type, std::string name, int x, int y, unsigned int layer, short instanceValue)
		: ObjectInstance(objectInfoHandle, type, name, x, y, layer, instanceValue) {}

	int Width;
	int Height;

	bool Visible = true;
	bool FollowFrame = false;

	std::vector<Paragraph> Paragraphs;

	int CurrentParagraph = 0;
	std::string AlterableText;

	std::string GetText()
	{
		if (CurrentParagraph == -1)
		{
			return AlterableText;
		}
		else
		{
			return Paragraphs[CurrentParagraph].Text;
		}
	}

	unsigned short GetFont()
	{
		if (CurrentParagraph == -1)
		{
			return Paragraphs[0].Font; // TODO: Verify
		}
		else
		{
			return Paragraphs[CurrentParagraph].Font;
		}
	}

	int GetColor()
	{
		if (CurrentParagraph == -1)
		{
			return Paragraphs[0].Color; // TODO: Verify
		}
		else
		{
			return Paragraphs[CurrentParagraph].Color;
		}
	}

	void SetCurrentParagraph(int currentParagraph)
	{
		CurrentParagraph = currentParagraph;
		if (CurrentParagraph >= Paragraphs.size())
		{
			CurrentParagraph = static_cast<int>(Paragraphs.size() - 1);
		}
		else if (CurrentParagraph < 0)
		{
			CurrentParagraph = 0;
		}
	}

	void SetPreviousParagraph()
	{
		if (CurrentParagraph > 0)
		{
			CurrentParagraph--;
		}
	}

	void SetNextParagraph()
	{
		if (CurrentParagraph < Paragraphs.size() - 1)
		{
			CurrentParagraph++;
		}
	}

	void SetAlterableText(std::string alterableText)
	{
		AlterableText = alterableText;
		CurrentParagraph = -1;
	}

	std::vector<unsigned int> GetFontsUsed() override {
		std::vector<unsigned int> fontsUsed;
		for (auto& paragraph : Paragraphs)
		{
			fontsUsed.push_back(paragraph.Font);
		}
		return fontsUsed;
	}
};