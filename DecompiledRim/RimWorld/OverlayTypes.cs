using System;

namespace RimWorld
{
	[Flags]
	public enum OverlayTypes
	{
		None = 0x0,
		NeedsPower = 0x1,
		PowerOff = 0x2,
		BurningWick = 0x4,
		Forbidden = 0x8,
		ForbiddenBig = 0x10,
		QuestionMark = 0x20,
		BrokenDown = 0x40,
		OutOfFuel = 0x80,
		ForbiddenRefuel = 0x100,
		SelfShutdown = 0x200,
		ForbiddenAtomizer = 0x400
	}
}
