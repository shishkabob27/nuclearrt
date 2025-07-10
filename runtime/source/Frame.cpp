#include "Frame.h"
#include "Application.h"
#include "BackdropProperties.h"
#include "CommonProperties.h"
#include "ImageBank.h"
#include "FontBank.h"
#include <math.h>

void Frame::Initialize()
{
}

void Frame::Update()
{
	float deltaTime = Application::Instance().GetBackend()->GetTimeDelta();
	GameTimer.Update(deltaTime);

	//Animation update
	for (auto& instance : ObjectInstances)
	{
		if (instance->OI->Type == 2) // Common object with possible animation
		{
			auto properties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);
			if (properties && properties->oAnimations)
			{
				properties->oAnimations->Update(deltaTime);
			}
		}
	}
}

void Frame::Draw()
{
	//Clear background
	Application::Instance().GetBackend()->Clear(BackgroundColor);

	for (unsigned int i = 0; i < Layers.size(); i++)
	{
		DrawLayer(Layers[i], i);
	}
}

void Frame::SetScroll(int x, int y, int layer)
{
	int windowWidth = Application::Instance().GetAppData()->GetWindowWidth();
	int windowHeight = Application::Instance().GetAppData()->GetWindowHeight();

	x -= windowWidth / 2;
	y -= windowHeight / 2;

	if (layer != -1)
	{
		x = x / Layers[layer].XCoefficient;
		y = y / Layers[layer].YCoefficient;
	}

	x = std::max(0, x);
	y = std::max(0, y);
	x = std::min(Width - windowWidth, x);
	y = std::min(Height - windowHeight, y);

	scrollX = x;
	scrollY = y;
}

void Frame::SetScrollX(int x)
{
	int windowWidth = Application::Instance().GetAppData()->GetWindowWidth();
	x -= windowWidth / 2;
	x = std::max(0, x);
	x = std::min(Width - windowWidth, x);
	scrollX = x;
}

void Frame::SetScrollY(int y)
{
	int windowHeight = Application::Instance().GetAppData()->GetWindowHeight();
	y -= windowHeight / 2;
	y = std::max(0, y);
	y = std::min(Height - windowHeight, y);
	scrollY = y;
}

void Frame::DrawLayer(Layer& layer, unsigned int index)
{
	for (auto& instance : ObjectInstances)
	{
		if (instance->Layer != index) continue;
		
		if (instance->OI->Type == 1)
		{
			auto& imageBank = ImageBank::Instance();
			auto backdropProps = static_cast<BackdropProperties*>(instance->OI->Properties.get());
			unsigned int imageId = backdropProps->Image;
			
			Application::Instance().GetBackend()->DrawTexture(
				imageId, instance->X - (scrollX * layer.XCoefficient), instance->Y - (scrollY * layer.YCoefficient),
				0, 0, 0, 1.0f, instance->OI->RGBCoefficient, instance->OI->BlendCoefficient, instance->OI->Effect, instance->OI->EffectParameter);
		}
		else if (instance->OI->Type == 2)
		{
			// Common object with possible animation
			auto properties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);
			if (properties && properties->Visible)
			{
				auto& imageBank = ImageBank::Instance();
				unsigned int imageId = 0;
				
				imageId = properties->oAnimations->GetCurrentImageHandle();

				int scrollXOffset = 0;
				int scrollYOffset = 0;
				if (properties->FollowFrame)
				{
					scrollXOffset = scrollX * layer.XCoefficient;
					scrollYOffset = scrollY * layer.YCoefficient;
				}

				auto imageInfo = imageBank.GetImage(imageId);
				if (imageInfo)
				{
					Application::Instance().GetBackend()->DrawTexture(
						imageId, instance->X - scrollXOffset, instance->Y - scrollYOffset,
						imageInfo->HotspotX, imageInfo->HotspotY, 
						instance->GetAngle(), 1.0f, instance->OI->RGBCoefficient, instance->OI->BlendCoefficient, instance->OI->Effect, instance->OI->EffectParameter);
				}
			}
		}
		else if (instance->OI->Type == 3) // Text
		{
			auto properties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);
			auto paragraphs = properties->oParagraphs;
			if (paragraphs)
			{

				int scrollXOffset = 0;
				int scrollYOffset = 0;
				if (properties->FollowFrame)
				{
					scrollXOffset = scrollX * layer.XCoefficient;
					scrollYOffset = scrollY * layer.YCoefficient;
				}

				std::string text = paragraphs->GetText();
				Application::Instance().GetBackend()->DrawText(FontBank::Instance().GetFont(paragraphs->GetFont()).get(), instance->X - scrollXOffset, instance->Y - scrollYOffset, paragraphs->GetColor(), text);
			}
		}
		else if (instance->OI->Type == 5 || instance->OI->Type == 6 || instance->OI->Type == 7) // Score, Lives, Counter
		{
			auto properties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);
			if (!properties || !properties->Visible) continue;
			
			auto counter = properties->oCounter;

			int scrollXOffset = 0;
			int scrollYOffset = 0;
			if (properties->FollowFrame)
			{
				scrollXOffset = scrollX * layer.XCoefficient;
				scrollYOffset = scrollY * layer.YCoefficient;
			}

			//TODO: Add support for other display types
			if (counter && counter->DisplayType == 1) // Numbers
			{
				auto appdata = Application::Instance().GetAppData();
				
				if (instance->OI->Type == 5) // Score
				{
					DrawCounterNumbers(*properties->oCounter, appdata->GetPlayerScores()[properties->oCounter->Player], instance->X - scrollXOffset, instance->Y - scrollYOffset);
				}
				else if (instance->OI->Type == 6) // Lives
				{
					DrawCounterNumbers(*properties->oCounter, appdata->GetPlayerLives()[properties->oCounter->Player], instance->X - scrollXOffset, instance->Y - scrollYOffset);
				}
				else if (instance->OI->Type == 7) // Counter
				{
					DrawCounterNumbers(*properties->oCounter, properties->oValue->GetValue(), instance->X - scrollXOffset, instance->Y - scrollYOffset);
				}
			}
		}
		else if (instance->OI->Type == 0) // Quick backdrop
		{
			auto properties = std::dynamic_pointer_cast<QuickBackdropProperties>(instance->OI->Properties);
			if (properties)
			{
				int scrollXOffset = scrollX * layer.XCoefficient;
				int scrollYOffset = scrollY * layer.YCoefficient;
				Application::Instance().GetBackend()->DrawQuickBackdrop(instance->X - scrollXOffset, instance->Y - scrollYOffset, properties->Width, properties->Height, properties->oShape);
			}
		}
	}
}

void Frame::DrawCounterNumbers(Counter &counter, int value, int x, int y)
{
	std::string valueString = std::to_string(value);
	int numDigits = valueString.size();

	if (counter.IntDigitCount > 0) numDigits = counter.IntDigitCount;

	bool negative = false;
	if (value < 0)
	{
		negative = true;
		valueString = valueString.substr(1);
	}

	//Fixed Size
	if (counter.IntDigitCount > 0)
	{
		while (valueString.size() < counter.IntDigitCount)
		{
			valueString = "0" + valueString;
		}

		int absCurrent = std::abs(value);

		if (counter.IntDigitCount * 10 < absCurrent)
		{
			valueString = std::to_string(absCurrent);
			int numDigits = valueString.size();
			if (numDigits > counter.IntDigitCount)
			{
				valueString = valueString.substr(valueString.size() - counter.IntDigitCount);
			}
		}
	}

	if (negative)
	{
		valueString = "-" + valueString;
		numDigits++;
	}

	//Counter char map
	std::string charMap = "0123456789-+.e";

	//get the total width of the string to be displayed
	int totalWidth = 0;
	for (int i = 0; i < numDigits; i++)
	{
		if (i >= valueString.size())
		{
			break;
		}

		int imageIndex = charMap.find(valueString[i]);
		totalWidth += ImageBank::Instance().GetImage(counter.Frames[imageIndex])->Width;
	}

	//Get height of the tallest character displayed
	int MaxHeight = 0;
	for (int i = 0; i < numDigits; i++)
	{
		if (i >= valueString.size())
		{
			break;
		}

		int imageIndex = charMap.find(valueString[i]);
		MaxHeight = std::max(MaxHeight, ImageBank::Instance().GetImage(counter.Frames[imageIndex])->Height);
	}

	int currentX = x - totalWidth;

	//Draw
	for (int i = 0; i < numDigits; i++)
	{
		if (i >= valueString.size())
		{
			break;
		}

		int imageIndex = charMap.find(valueString[i]);
		auto imageInfo = ImageBank::Instance().GetImage(counter.Frames[imageIndex]);
		if (imageInfo)
		{
			Application::Instance().GetBackend()->DrawTexture(
				counter.Frames[imageIndex], currentX, y - MaxHeight,
				0, 0, 
				0, 1.0f, 0xFFFFFFFF, 0, 0, 0);
			currentX += imageInfo->Width;
		}
	}
}

std::vector<unsigned int> Frame::GetImagesUsed()
{
	std::vector<unsigned int> imagesUsed;
	
	for (auto& instance : ObjectInstances)
	{
		//Backdrops
		if (instance->OI->Type == 1)
		{
			imagesUsed.push_back(((BackdropProperties*)instance->OI->Properties.get())->Image);
		}
		//Common objects with animations
		else if (instance->OI->Type == 2)
		{
			auto properties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);
			if (properties && properties->oAnimations)
			{
				auto images = properties->oAnimations->GetImagesUsed();
				imagesUsed.insert(imagesUsed.end(), images.begin(), images.end());
			}
		}
		//Score, Lives, Counter
		else if (instance->OI->Type == 5 || instance->OI->Type == 6 || instance->OI->Type == 7)
		{
			auto properties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);
			auto counter = properties->oCounter;
			if (counter)
			{
				for (unsigned int frame : counter->Frames)
				{
					imagesUsed.push_back(frame);
				}
			}
		}
		else if (instance->OI->Type == 0) // Quick backdrop
		{
			auto properties = std::dynamic_pointer_cast<QuickBackdropProperties>(instance->OI->Properties);
			if (properties)
			{
				imagesUsed.push_back(properties->oShape->Image);
			}
		}
	}

	return imagesUsed;
}

std::vector<unsigned int> Frame::GetFontsUsed()
{
	std::vector<unsigned int> fontsUsed;

	for (auto& instance : ObjectInstances)
	{
		if (instance->OI->Type == 3) // Text
		{
			auto properties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);
			if (properties && properties->oParagraphs)
			{
				for (auto& paragraph : properties->oParagraphs->Paragraphs)
				{
					fontsUsed.push_back(paragraph->Font);
				}
			}
		}
	}

	return fontsUsed;
}

void Frame::CreateInstance(short x, short y, unsigned int layer, unsigned int objectInfoHandle, short angle, ObjectInstance *parentInstance)
{
	auto objectInfo = ObjectFactory::Instance().GetObjectInfo(objectInfoHandle);
	if (!objectInfo)
	{
		std::cout << "Unable to create instance: " << objectInfoHandle << std::endl;
		return;
	}

	//get max unique handle
	unsigned int maxHandle = 0;
	for (auto& instance : ObjectInstances)
	{
		maxHandle = std::max(maxHandle, instance->Handle);
	}

	unsigned int handle = maxHandle + 1;

	if (parentInstance) {
		x += parentInstance->X;
		y += parentInstance->Y;
		layer = parentInstance->Layer;
	}

	auto instance = ObjectFactory::Instance().CreateInstance(handle, objectInfoHandle, x, y, layer, 0);
	if (instance) {
		instance->SetAngle(angle);
		ObjectInstances.push_back(instance);
		
		//TODO: move this to a separate function
		// Load any textures needed for this instance
		std::vector<unsigned int> texturesToLoad;
		
		// For backdrops
		if (objectInfo->Type == 1) {
			auto backdropProps = std::dynamic_pointer_cast<BackdropProperties>(objectInfo->Properties);
			if (backdropProps) {
				texturesToLoad.push_back(backdropProps->Image);
			}
		}
		// For common objects with animations
		else if (objectInfo->Type == 2) {
			auto properties = std::dynamic_pointer_cast<CommonProperties>(objectInfo->Properties);
			if (properties && properties->oAnimations) {
				auto images = properties->oAnimations->GetImagesUsed();
				texturesToLoad.insert(texturesToLoad.end(), images.begin(), images.end());
			}
		}
		// For counter objects (score, lives, counter)
		else if (objectInfo->Type == 5 || objectInfo->Type == 6 || objectInfo->Type == 7) {
			auto properties = std::dynamic_pointer_cast<CommonProperties>(objectInfo->Properties);
			if (properties && properties->oCounter) {
				texturesToLoad.insert(texturesToLoad.end(), 
					properties->oCounter->Frames.begin(), 
					properties->oCounter->Frames.end());
			}
		}
		
		// Load all the required textures
		auto backend = Application::Instance().GetBackend();
		for (unsigned int textureId : texturesToLoad) {
			backend->LoadTexture(textureId);
		}
	}
}

int Frame::GetMouseX()
{
	return Application::Instance().GetBackend()->GetMouseX();
}

int Frame::GetMouseY()
{
	return Application::Instance().GetBackend()->GetMouseY();
}

//Check if the object is colliding with any backdrop
bool Frame::IsCollidingWithBackground(ObjectInstance *instance)
{
	if (instance->OI->Type != 2) return false; // Only Active objects
	
	// Check collision with all backdrop objects
	for (auto& backdropInstance : ObjectInstances)
	{
		// Check only against backdrop objects (Type 1)
		if (backdropInstance->OI->Type == 1)
		{
			auto backdropProps = static_cast<BackdropProperties*>(backdropInstance->OI->Properties.get());
			
			// Check if this backdrop is an obstacle
			if (backdropProps && backdropProps->ObstacleType > 0)
			{
				// Check if the backdrop is on the same layer as the instance
				// to ensure scrolling is handled consistently
				if (backdropInstance->Layer == instance->Layer)
				{
					// Use the existing collision detection between objects
					if (IsColliding(instance, backdropInstance.get()))
					{
						return true;
					}
				}
			}
		}
	}
	
	return false; // No collision with any backdrop
}

bool Frame::IsColliding(ObjectInstance *instance1, ObjectInstance *instance2)
{
	// Only check collision for relevant object types
	if ((instance1->OI->Type != 1 && instance1->OI->Type != 2) ||
		(instance2->OI->Type != 1 && instance2->OI->Type != 2))
		return false;

	// Get image IDs for both instances
	unsigned int imageId1, imageId2;
	
	// Calculate draw positions for both instances exactly as they'd be drawn
	int drawX1, drawY1, drawX2, drawY2;
	int hotspotX1 = 0, hotspotY1 = 0;
	int hotspotX2 = 0, hotspotY2 = 0;
	
	// Get information for instance1
	if (instance1->OI->Type == 1) // Backdrop
	{
		auto backdropProps = static_cast<BackdropProperties*>(instance1->OI->Properties.get());
		if (!backdropProps) return false;
		imageId1 = backdropProps->Image;
		
		// Position exactly as in DrawLayer for backdrops
		drawX1 = instance1->X - (scrollX * Layers[instance1->Layer].XCoefficient);
		drawY1 = instance1->Y - (scrollY * Layers[instance1->Layer].YCoefficient);
	}
	else // Common object
	{
		auto properties = std::dynamic_pointer_cast<CommonProperties>(instance1->OI->Properties);
		if (!properties || !properties->oAnimations) return false;
		
		imageId1 = properties->oAnimations->GetCurrentImageHandle();
		
		// Position exactly as in DrawLayer for common objects
		int scrollXOffset = 0;
		int scrollYOffset = 0;
		if (properties->FollowFrame)
		{
			scrollXOffset = scrollX * Layers[instance1->Layer].XCoefficient;
			scrollYOffset = scrollY * Layers[instance1->Layer].YCoefficient;
		}
		
		drawX1 = instance1->X - scrollXOffset;
		drawY1 = instance1->Y - scrollYOffset;
		
		auto imageInfo = ImageBank::Instance().GetImage(imageId1);
		if (imageInfo)
		{
			hotspotX1 = imageInfo->HotspotX;
			hotspotY1 = imageInfo->HotspotY;
		}
	}

	// Get information for instance2
	if (instance2->OI->Type == 1) // Backdrop
	{
		auto backdropProps = static_cast<BackdropProperties*>(instance2->OI->Properties.get());
		if (!backdropProps) return false;
		imageId2 = backdropProps->Image;
		
		// Position exactly as in DrawLayer for backdrops
		drawX2 = instance2->X - (scrollX * Layers[instance2->Layer].XCoefficient);
		drawY2 = instance2->Y - (scrollY * Layers[instance2->Layer].YCoefficient);
	}
	else // Common object
	{
		auto properties = std::dynamic_pointer_cast<CommonProperties>(instance2->OI->Properties);
		if (!properties || !properties->oAnimations) return false;
		
		imageId2 = properties->oAnimations->GetCurrentImageHandle();
		
		// Position exactly as in DrawLayer for common objects
		int scrollXOffset = 0;
		int scrollYOffset = 0;
		if (properties->FollowFrame)
		{
			scrollXOffset = scrollX * Layers[instance2->Layer].XCoefficient;
			scrollYOffset = scrollY * Layers[instance2->Layer].YCoefficient;
		}
		
		drawX2 = instance2->X - scrollXOffset;
		drawY2 = instance2->Y - scrollYOffset;
		
		auto imageInfo = ImageBank::Instance().GetImage(imageId2);
		if (imageInfo)
		{
			hotspotX2 = imageInfo->HotspotX;
			hotspotY2 = imageInfo->HotspotY;
		}
	}

	// Get image info for both instances
	auto imageInfo1 = ImageBank::Instance().GetImage(imageId1);
	auto imageInfo2 = ImageBank::Instance().GetImage(imageId2);
	if (!imageInfo1 || !imageInfo2) return false;

	// Calculate bounding box for instance1
	int x1_1 = drawX1 - hotspotX1;
	int y1_1 = drawY1 - hotspotY1;
	int x2_1 = x1_1 + imageInfo1->Width;
	int y2_1 = y1_1;
	int x3_1 = x1_1;
	int y3_1 = y1_1 + imageInfo1->Height;
	int x4_1 = x2_1;
	int y4_1 = y3_1;

	// Calculate bounding box for instance2
	int x1_2 = drawX2 - hotspotX2;
	int y1_2 = drawY2 - hotspotY2;
	int x2_2 = x1_2 + imageInfo2->Width;
	int y2_2 = y1_2;
	int x3_2 = x1_2;
	int y3_2 = y1_2 + imageInfo2->Height;
	int x4_2 = x2_2;
	int y4_2 = y3_2;
	
	// Apply rotation to bounding boxes if needed
	if (instance1->GetAngle() != 0)
	{
		RotatePoints(x1_1, y1_1, x2_1, y2_1, x3_1, y3_1, x4_1, y4_1, 
			drawX1, drawY1, instance1->GetAngle());
	}
	
	if (instance2->GetAngle() != 0)
	{
		RotatePoints(x1_2, y1_2, x2_2, y2_2, x3_2, y3_2, x4_2, y4_2, 
			drawX2, drawY2, instance2->GetAngle());
	}

	// If either object is rotated, use polygon intersection test
	if (instance1->GetAngle() != 0 || instance2->GetAngle() != 0)
	{
		// Define the polygons
		int poly1[][2] = {{x1_1, y1_1}, {x2_1, y2_1}, {x4_1, y4_1}, {x3_1, y3_1}};
		int poly2[][2] = {{x1_2, y1_2}, {x2_2, y2_2}, {x4_2, y4_2}, {x3_2, y3_2}};
		
		// Check for any edge intersection
		for (int i = 0; i < 4; i++)
		{
			int j = (i + 1) % 4;
			for (int k = 0; k < 4; k++)
			{
				int l = (k + 1) % 4;
				
				// Check if line segments intersect
				if (DoLinesIntersect(
					poly1[i][0], poly1[i][1], poly1[j][0], poly1[j][1],
					poly2[k][0], poly2[k][1], poly2[l][0], poly2[l][1]))
				{
					return true;
				}
			}
		}
		
		// Check if one polygon is inside the other
		// Test if a point from poly1 is inside poly2
		if (IsPointInPolygon(poly1[0][0], poly1[0][1], poly2, 4))
			return true;
		
		// Test if a point from poly2 is inside poly1
		if (IsPointInPolygon(poly2[0][0], poly2[0][1], poly1, 4))
			return true;
		
		return false;
	}
	else
	{
		// Simple AABB collision for non-rotated objects
		// Check if the rectangles overlap - using half-open intervals [x, x+width)
		return (x1_1 < x2_2 && x1_2 < x2_1 && y1_1 < y3_2 && y1_2 < y3_1);
	}
}

// Helper method to determine if a point is inside a polygon
bool Frame::IsPointInPolygon(int x, int y, int polygon[][2], int numPoints)
{
	bool inside = false;
	for (int i = 0, j = numPoints - 1; i < numPoints; j = i++)
	{
		// Check if point is exactly on an edge (more precise edge detection)
		if (IsPointOnLine(x, y, polygon[i][0], polygon[i][1], polygon[j][0], polygon[j][1]))
			return true;
			
		// Standard ray-casting algorithm
		if (((polygon[i][1] > y) != (polygon[j][1] > y)) &&
			(x < (polygon[j][0] - polygon[i][0]) * (y - polygon[i][1]) / (float)(polygon[j][1] - polygon[i][1]) + polygon[i][0]))
		{
			inside = !inside;
		}
	}
	return inside;
}

// Helper method to determine if two line segments intersect
bool Frame::DoLinesIntersect(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
{
	// Calculate the direction of the lines
	float uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / 
			(float)((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
	float uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / 
			(float)((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));

	// If uA and uB are between 0-1, lines are colliding
	// Adding a small epsilon for floating point precision
	const float epsilon = 0.0001f;
	return (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1) &&
	       !(fabs(uA) < epsilon || fabs(uA - 1.0f) < epsilon || 
	         fabs(uB) < epsilon || fabs(uB - 1.0f) < epsilon);
}

bool Frame::IsColliding(ObjectInstance *instance, int x, int y)
{
	if (instance->OI->Type != 1 && instance->OI->Type != 2) return false;

	unsigned int imageId;
	bool fineDetection = false;
	
	// Calculate the draw position exactly as it would be drawn in DrawLayer
	int drawX, drawY;
	int hotspotX = 0, hotspotY = 0;
	
	if (instance->OI->Type == 1) // Backdrop
	{
		auto backdropProps = static_cast<BackdropProperties*>(instance->OI->Properties.get());
		if (!backdropProps) return false;
		imageId = backdropProps->Image;
		
		// Position exactly as in DrawLayer for backdrops
		drawX = instance->X - (scrollX * Layers[instance->Layer].XCoefficient);
		drawY = instance->Y - (scrollY * Layers[instance->Layer].YCoefficient);
	}
	else // Common object
	{
		auto properties = std::dynamic_pointer_cast<CommonProperties>(instance->OI->Properties);
		if (!properties) return false;

		fineDetection = properties->FineDetection;
		
		if (properties->oAnimations)
		{
			imageId = properties->oAnimations->GetCurrentImageHandle();
		}
		else
		{
			imageId = 0;
		}
		
		// Position exactly as in DrawLayer for common objects
		int scrollXOffset = 0;
		int scrollYOffset = 0;
		if (properties->FollowFrame)
		{
			scrollXOffset = scrollX * Layers[instance->Layer].XCoefficient;
			scrollYOffset = scrollY * Layers[instance->Layer].YCoefficient;
		}
		
		drawX = instance->X - scrollXOffset;
		drawY = instance->Y - scrollYOffset;
		
		auto imageInfo = ImageBank::Instance().GetImage(imageId);
		if (imageInfo)
		{
			hotspotX = imageInfo->HotspotX;
			hotspotY = imageInfo->HotspotY;
		}
	}

	auto imageInfo = ImageBank::Instance().GetImage(imageId);
	if (!imageInfo) return false;

	// Mouse coordinates don't need adjusting
	int targetX = x;
	int targetY = y;

	if (!fineDetection)
	{
		// Calculate bounding box points
		int x1 = drawX - hotspotX;
		int y1 = drawY - hotspotY;
		int x2 = x1 + imageInfo->Width;
		int y2 = y1;
		int x3 = x1;
		int y3 = y1 + imageInfo->Height;
		int x4 = x2;
		int y4 = y3;

		if (instance->GetAngle() != 0)
		{
			RotatePoints(x1, y1, x2, y2, x3, y3, x4, y4, drawX, drawY, instance->GetAngle());
			
			// Check edges
			if (IsPointOnLine(targetX, targetY, x1, y1, x2, y2) || IsPointOnLine(targetX, targetY, x1, y1, x3, y3) ||
				IsPointOnLine(targetX, targetY, x3, y3, x4, y4) || IsPointOnLine(targetX, targetY, x4, y4, x2, y2))
				return true;

			// Ray casting
			bool inside = false;
			int points[][2] = {{x1,y1}, {x2,y2}, {x4,y4}, {x3,y3}};
			
			for (int i = 0, j = 3; i < 4; j = i++)
			{
				if (((points[i][1] > targetY) != (points[j][1] > targetY)) &&
					(targetX < (points[j][0] - points[i][0]) * (targetY - points[i][1]) / (float)(points[j][1] - points[i][1]) + points[i][0]))
					inside = !inside;
			}
			return inside;
		}
		return (targetX >= x1 && targetX < x4 && targetY >= y1 && targetY < y4);
	}
	else
	{
		auto backend = Application::Instance().GetBackend();

		// Get texture dimensions
		int texWidth, texHeight;
		backend->GetTextureDimensions(imageId, texWidth, texHeight);

		// Calculate position on texture
		int relX, relY;
		
		// For non-rotated objects, directly calculate position
		if (instance->GetAngle() == 0)
		{
			// Texture coordinates relative to object's top-left
			relX = targetX - (drawX - hotspotX);
			relY = targetY - (drawY - hotspotY);
		}
		else
		{
			// For rotated objects, we need to calculate the position differently
			// First, get position relative to the object's center
			float pointX = targetX - drawX;
			float pointY = targetY - drawY;
			
			// Rotate the point in the opposite direction
			float radians = instance->GetAngle() * (SDL_PI_F / 180.0f);
			float cosA = cos(radians);
			float sinA = sin(radians);
			
			float newX = pointX * cosA - pointY * sinA;
			float newY = pointX * sinA + pointY * cosA;
			
			// Adjust for hotspot
			relX = static_cast<int>(newX + hotspotX);
			relY = static_cast<int>(newY + hotspotY);
		}
		
		// Check bounds
		if (relX < 0 || relX >= texWidth || relY < 0 || relY >= texHeight)
			return false;
			
		// Check transparency
		return !backend->IsPixelTransparent(imageId, relX, relY);
	}
}

bool Frame::IsPointOnLine(int x, int y, int x1, int y1, int x2, int y2)
{
	// Calculate the distance from point to line segment
	float A = x - x1;
	float B = y - y1;
	float C = x2 - x1;
	float D = y2 - y1;

	float dot = A * C + B * D;
	float len_sq = C * C + D * D;
	float param = -1;

	if (len_sq != 0)
		param = dot / len_sq;

	float xx, yy;

	if (param < 0)
	{
		xx = x1;
		yy = y1;
	}
	else if (param > 1)
	{
		xx = x2;
		yy = y2;
	}
	else
	{
		xx = x1 + param * C;
		yy = y1 + param * D;
	}

	float dx = x - xx;
	float dy = y - yy;
	float distance = sqrt(dx * dx + dy * dy);

	// Return true if point is exactly on the line or very close
	return distance < 0.5f;
}

void Frame::RotatePoints(int& x1, int& y1, int& x2, int& y2, int& x3, int& y3, int& x4, int& y4, int offsetX, int offsetY, float angle)
{
	//invert angle
	angle = 360 - angle;

	// Move points relative to offset point
	x1 -= offsetX;
	y1 -= offsetY;
	x2 -= offsetX;
	y2 -= offsetY;
	x3 -= offsetX;
	y3 -= offsetY;
	x4 -= offsetX;
	y4 -= offsetY;
	
	// Rotate points
	RotatePoint(x1, y1, angle);
	RotatePoint(x2, y2, angle);
	RotatePoint(x3, y3, angle);
	RotatePoint(x4, y4, angle);

	// Move points back
	x1 += offsetX;
	y1 += offsetY;
	x2 += offsetX;
	y2 += offsetY;
	x3 += offsetX;
	y3 += offsetY;
	x4 += offsetX;
	y4 += offsetY;
	
}

void Frame::RotatePoint(int& x, int& y, float angle)
{
	// Convert to radians
	float radians = angle * (SDL_PI_F / 180.0f);

	// Rotate point around origin
	float xNew = x * cos(radians) - y * sin(radians);
	float yNew = x * sin(radians) + y * cos(radians);

	x = static_cast<int>(xNew);
	y = static_cast<int>(yNew);
}
