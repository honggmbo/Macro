using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MoonLight
{
	internal struct WINDOWPLACEMENT
	{
		public int length;
		public int flags;
		public SHOW_WINDOW_COMMANDS showc_cmd;
		public Point min_position;
		public Point max_position;
		public Rectangle normal_position;
	}

	public struct POINT
	{
		public POINT(Int32 _x, Int32 _y)
		{
			x = _x;
			y = _y;
		}

		public Int32 x;
		public Int32 y;
	}

	public struct PointData
	{
		public string actionName;
		public Point pt;
		public int delay;
	}

	public struct ActionData
	{
		public string name;
		public Point pt;
	}

	public enum MacroType : int
	{
		eItemBreak = 0,
		eMining,
		eQuest,
		eReturn,
		eCapture,
		eHunting,
		eSQuest,
		eClick,
		eCancle,
		eReQuest,
		eConfirm,
		eShop,
		eSafeMode,
		eMax,
	}

	internal enum SHOW_WINDOW_COMMANDS : int
	{
		HIDE = 0,
		NORMAL = 1,
		MINIMIZED = 2,
		MAXIMIZED = 3,
	}

	internal enum WNDSTATE : int
	{
		SW_HIDE = 0,
		SW_SHOWNORMAL = 1,
		SW_NORMAL = 1,
		SW_SHOWMINIMIZED = 2,
		SW_MAXIMIZE = 3,
		SW_SHOWNOACTIVATE = 4,
		SW_SHOW = 5,
		SW_MINIMIZE = 6,
		SW_SHOWMINNOACTIVE = 7,
		SW_SHOWNA = 8,
		SW_RESTORE = 9,
		SW_SHOWDEFAULT = 10,
		SW_MAX = 10
	}
}
