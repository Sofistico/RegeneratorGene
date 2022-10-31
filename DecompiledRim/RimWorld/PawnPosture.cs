using System;

namespace RimWorld
{
	[Flags]
	public enum PawnPosture : byte
	{
		Standing = 0x0,
		LayingOnGroundFaceUp = 0x3,
		LayingOnGroundNormal = 0x1,
		LayingInBed = 0x5,
		LayingInBedFaceUp = 0x7,
		LayingMask = 0x1,
		FaceUpMask = 0x2,
		InBedMask = 0x4
	}
}
