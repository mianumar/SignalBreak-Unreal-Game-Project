// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;

public class SignalBreak : ModuleRules
{
	public SignalBreak(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[] {
			"Core",
			"CoreUObject",
			"Engine",
			"InputCore",
			"EnhancedInput",
			"AIModule",
			"StateTreeModule",
			"GameplayStateTreeModule",
			"UMG",
			"Slate"
		});

		PrivateDependencyModuleNames.AddRange(new string[] { });

		PublicIncludePaths.AddRange(new string[] {
			"SignalBreak",
			"SignalBreak/Variant_Platforming",
			"SignalBreak/Variant_Platforming/Animation",
			"SignalBreak/Variant_Combat",
			"SignalBreak/Variant_Combat/AI",
			"SignalBreak/Variant_Combat/Animation",
			"SignalBreak/Variant_Combat/Gameplay",
			"SignalBreak/Variant_Combat/Interfaces",
			"SignalBreak/Variant_Combat/UI",
			"SignalBreak/Variant_SideScrolling",
			"SignalBreak/Variant_SideScrolling/AI",
			"SignalBreak/Variant_SideScrolling/Gameplay",
			"SignalBreak/Variant_SideScrolling/Interfaces",
			"SignalBreak/Variant_SideScrolling/UI"
		});

		// Uncomment if you are using Slate UI
		// PrivateDependencyModuleNames.AddRange(new string[] { "Slate", "SlateCore" });

		// Uncomment if you are using online features
		// PrivateDependencyModuleNames.Add("OnlineSubsystem");

		// To include OnlineSubsystemSteam, add it to the plugins section in your uproject file with the Enabled attribute set to true
	}
}
