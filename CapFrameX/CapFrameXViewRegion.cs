﻿using CapFrameX.View;
using Prism.Modularity;

namespace CapFrameX
{
    public class CapFrameXViewRegion : IModule
    {
        public void Initialize()
        {
			RegionManagerWrapper.Singleton.RegisterViewWithRegion("ColorbarRegion", typeof(ColorbarView));
			RegionManagerWrapper.Singleton.RegisterViewWithRegion("ControlRegion", typeof(ControlView));
			RegionManagerWrapper.Singleton.RegisterViewWithRegion("DataRegion", typeof(DataView));
			RegionManagerWrapper.Singleton.RegisterViewWithRegion("DataRegion", typeof(ComparisonDataView));
			RegionManagerWrapper.Singleton.RegisterViewWithRegion("DataRegion", typeof(ReportView));
			RegionManagerWrapper.Singleton.RegisterViewWithRegion("DataRegion", typeof(SynchronizationView));
			RegionManagerWrapper.Singleton.RegisterViewWithRegion("DataRegion", typeof(AggregationView));
			RegionManagerWrapper.Singleton.RegisterViewWithRegion("StateRegion", typeof(StateView));
			RegionManagerWrapper.Singleton.RegisterViewWithRegion("OverlayRegion", typeof(OverlayView));			
		}
    }
}
