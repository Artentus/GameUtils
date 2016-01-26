using System;
using System.Runtime.InteropServices;

namespace GameUtils.Graphics
{
    static class Dwm
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct UNSIGNED_RATIO
		{
			public uint uiNumerator;
			public uint uiDenominator;
		};
 
 
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct DwmTimingInfo
		{
			public uint cbSize;
 
			// Data on DWM composition overall
 
			// Monitor refresh rate
			public UNSIGNED_RATIO rateRefresh;
 
			// Actual period
			public ulong qpcRefreshPeriod;
 
			// composition rate     
			public UNSIGNED_RATIO rateCompose;
 
			// QPC time at a VSync interupt
			public ulong qpcVBlank;
 
			// DWM refresh count of the last vsync
			// DWM refresh count is a 64bit number where zero is
			// the first refresh the DWM woke up to process
			public ulong cRefresh;
 
			// DX refresh count at the last Vsync Interupt
			// DX refresh count is a 32bit number with zero 
			// being the first refresh after the card was initialized
			// DX increments a counter when ever a VSync ISR is processed
			// It is possible for DX to miss VSyncs
			//
			// There is not a fixed mapping between DX and DWM refresh counts
			// because the DX will rollover and may miss VSync interupts
			public uint cDXRefresh;
 
			// QPC time at a compose time.  
			public ulong qpcCompose;
 
			// Frame number that was composed at qpcCompose
			public ulong cFrame;
 
			// The present number DX uses to identify renderer frames
			public uint cDXPresent;
 
			// Refresh count of the frame that was composed at qpcCompose
			public ulong cRefreshFrame;
 
			// DWM frame number that was last submitted
			public ulong cFrameSubmitted;
 
			// DX Present number that was last submitted
			public uint cDXPresentSubmitted;
 
			// DWM frame number that was last confirmed presented
			public ulong cFrameConfirmed;
 
			// DX Present number that was last confirmed presented
			public uint cDXPresentConfirmed;
 
			// The target refresh count of the last
			// frame confirmed completed by the GPU
			public ulong cRefreshConfirmed;
 
			// DX refresh count when the frame was confirmed presented
			public uint cDXRefreshConfirmed;
 
			// Number of frames the DWM presented late
			// AKA Glitches
			public ulong cFramesLate;
 
			// the number of composition frames that 
			// have been issued but not confirmed completed
			public uint cFramesOutstanding;
 
			// Following fields are only relavent when an HWND is specified
			// Display frame
 
			// Last frame displayed
			public ulong cFrameDisplayed;
 
			// QPC time of the composition pass when the frame was displayed
			public ulong qpcFrameDisplayed;
 
			// Count of the VSync when the frame should have become visible
			public ulong cRefreshFrameDisplayed;
 
			// Complete frames: DX has notified the DWM that the frame is done rendering
 
			// ID of the the last frame marked complete (starts at 0)
			public ulong cFrameComplete;
 
			// QPC time when the last frame was marked complete
			public ulong qpcFrameComplete;
 
			// Pending frames:
			// The application has been submitted to DX but not completed by the GPU
 
			// ID of the the last frame marked pending (starts at 0)
			public ulong cFramePending;
 
			// QPC time when the last frame was marked pending
			public ulong qpcFramePending;
 
			// number of unique frames displayed
			public ulong cFramesDisplayed;
 
			// number of new completed frames that have been received
			public ulong cFramesComplete;
 
			// number of new frames submitted to DX but not yet complete
			public ulong cFramesPending;
 
			// number of frames available but not displayed, used or dropped
			public ulong cFramesAvailable;
 
			// number of rendered frames that were never
			// displayed because composition occured too late
			public ulong cFramesDropped;
 
			// number of times an old frame was composed 
			// when a new frame should have been used
			// but was not available
			public ulong cFramesMissed;
 
			// the refresh at which the next frame is
			// scheduled to be displayed
			public ulong cRefreshNextDisplayed;
 
			// the refresh at which the next DX present is 
			// scheduled to be displayed
			public ulong cRefreshNextPresented;
 
			// The total number of refreshes worth of content
			// for this HWND that have been displayed by the DWM
			// since DwmSetPresentParameters was called
			public ulong cRefreshesDisplayed;
 
			// The total number of refreshes worth of content
			// that have been presented by the application
			// since DwmSetPresentParameters was called
			public ulong cRefreshesPresented;
 
			// The actual refresh # when content for this
			// window started to be displayed
			// it may be different than that requested
			// DwmSetPresentParameters
			public ulong cRefreshStarted;
 
			// Total number of pixels DX redirected
			// to the DWM.
			// If Queueing is used the full buffer
			// is transfered on each present.
			// If not queuing it is possible only 
			// a dirty region is updated
			public ulong cPixelsReceived;
 
			// Total number of pixels drawn.
			// Does not take into account if
			// if the window is only partial drawn
			// do to clipping or dirty rect management 
			public ulong cPixelsDrawn;
 
			// The number of buffers in the flipchain
			// that are empty.   An application can 
			// present that number of times and guarantee 
			// it won't be blocked waiting for a buffer to 
			// become empty to present to
			public ulong cBuffersEmpty;
		};
 
		[DllImport("Dwmapi.dll")]
		static extern int DwmGetCompositionTimingInfo(IntPtr hwnd, ref DwmTimingInfo pTimingInfo);

        public static double DisplayRefreshRate
        {
            get
            {
                var dwmTimingInfo = default(DwmTimingInfo);
                dwmTimingInfo.cbSize = 73 * 4;

                int error = DwmGetCompositionTimingInfo(IntPtr.Zero, ref dwmTimingInfo);
                if (error != 0) throw new COMException("Cannot retrieve display refesh rate.", error);

                return (double)dwmTimingInfo.rateRefresh.uiNumerator / (double)dwmTimingInfo.rateRefresh.uiDenominator;
            }
        }
    }
}
