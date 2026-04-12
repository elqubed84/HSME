using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM
{
	public class AppFlyoutPageFlyoutMenuItem
	{
		public AppFlyoutPageFlyoutMenuItem()
		{
			TargetType = typeof(AppFlyoutPageFlyoutMenuItem);
		}
		public int Id { get; set; }
		public string Title { get; set; }
		public ImageSource Icon { get; set; }
		public bool IsSeparatorVisible { get; set; }
		public string Hed { get; set; }

		public Type TargetType { get; set; }
	}
}
