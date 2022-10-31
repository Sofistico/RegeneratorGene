using System;

namespace Verse
{
	[Flags]
	public enum DevelopmentalStage : uint
	{
		None = 0x0u,
		Newborn = 0x1u,
		Baby = 0x2u,
		Child = 0x4u,
		Adult = 0x8u
	}
}
