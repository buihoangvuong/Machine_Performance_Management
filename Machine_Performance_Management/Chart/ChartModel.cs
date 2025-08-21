using Machine_Performance_Management.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machine_Performance_Management.Chart
{
	public class ChartModel
	{
        protected LocalConfigTable config;

        public ChartModel()
        {
            IniService ini = new IniService();
            config = ini.GetLocalConfig();
        }
    }
}
