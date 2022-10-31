using System;

namespace Verse
{
	[Flags]
	public enum PawnRenderFlags : uint
	{
		None = 0x0u,
		Portrait = 0x1u,
		HeadStump = 0x2u,
		Invisible = 0x4u,
		DrawNow = 0x8u,
		Cache = 0x10u,
		Headgear = 0x20u,
		Clothes = 0x40u,
		NeverAimWeapon = 0x80u,
		StylingStation = 0x100u
	}
}
