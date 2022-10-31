using System;

namespace Verse
{
	[Flags]
	public enum NameFilter
	{
		None = 0x0,
		First = 0x1,
		Nick = 0x2,
		Last = 0x4,
		Title = 0x8
	}
}
