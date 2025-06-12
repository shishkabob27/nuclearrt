#pragma once

#include <vector>
#include <memory>
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

class ObjectParagraphs
{
public:
	ObjectParagraphs(int width, int height, std::vector<std::shared_ptr<Paragraph>> paragraphs)
		: Width(width), Height(height), Paragraphs(paragraphs) {}

	int Width;
	int Height;

	std::vector<std::shared_ptr<Paragraph>> Paragraphs;

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
			return Paragraphs[CurrentParagraph]->Text;
		}
	}

	unsigned short GetFont()
	{
		if (CurrentParagraph == -1)
		{
			return Paragraphs[0]->Font; // TODO: Verify
		}
		else
		{
			return Paragraphs[CurrentParagraph]->Font;
		}
	}

	int GetColor()
	{
		if (CurrentParagraph == -1)
		{
			return Paragraphs[0]->Color; // TODO: Verify
		}
		else
		{
			return Paragraphs[CurrentParagraph]->Color;
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
};

