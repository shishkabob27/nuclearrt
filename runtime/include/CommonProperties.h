#pragma once

#include "ObjectInfoProperties.h"
#include "Animations.h"
#include "Value.h"
#include "Counter.h"
#include "AlterableValues.h"
#include "AlterableStrings.h"
#include "AlterableFlags.h"
#include "ObjectParagraphs.h"
#include "Extension.h"

#include <string>
#include <memory>

class CommonProperties : public ObjectInfoProperties {
public:
	CommonProperties(const std::string& identifier, bool visible = true, bool followFrame = false, bool fineDetection = false, std::vector<short> qualifiers = { -1, -1, -1, -1, -1, -1, -1, -1 }, std::shared_ptr<AlterableValues> alterableValues = nullptr,
					std::shared_ptr<AlterableStrings> alterableStrings = nullptr, std::shared_ptr<AlterableFlags> alterableFlags = nullptr, std::shared_ptr<Animations> animations = nullptr, std::shared_ptr<Value> value = nullptr, std::shared_ptr<Counter> counter = nullptr, std::shared_ptr<ObjectParagraphs> paragraphs = nullptr, std::shared_ptr<Extension> extension = nullptr)
		: Identifier(identifier), Visible(visible), FollowFrame(followFrame), FineDetection(fineDetection), Qualifiers(qualifiers), oAlterableValues(alterableValues),
		  oAlterableFlags(alterableFlags), oAlterableStrings(alterableStrings), oAnimations(animations), oValue(value), oCounter(counter), oParagraphs(paragraphs), oExtension(extension) {}

	std::string Identifier;
	bool Visible = true;
	bool FollowFrame = false;
	bool FineDetection = false;

	//qualifiers
	std::vector<short> Qualifiers = { -1, -1, -1, -1, -1, -1, -1, -1 };

	std::shared_ptr<AlterableValues> oAlterableValues;
	std::shared_ptr<AlterableStrings> oAlterableStrings;
	std::shared_ptr<AlterableFlags> oAlterableFlags;
	std::shared_ptr<Animations> oAnimations;
	std::shared_ptr<Value> oValue;
	std::shared_ptr<Counter> oCounter;
	std::shared_ptr<ObjectParagraphs> oParagraphs;
	std::shared_ptr<Extension> oExtension;
}; 