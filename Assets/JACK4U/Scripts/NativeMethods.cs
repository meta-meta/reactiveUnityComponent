/*
* JACK4U
* Copyright © 2014 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace monoflow {
	
	public static partial class NativeMethods  {

		#region Window
		[DllImport("user32.dll")]
		public static extern System.IntPtr GetActiveWindow();

		[DllImport("user32.dll")]
		public static extern System.IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr WindowHandle);

		[DllImport("user32.dll")]
		public static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);

		[DllImport("user32.dll")]
		public static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern bool IsWindowVisible(IntPtr hWnd);


		[DllImport("User32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
		
		
		public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
		#endregion

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool wow64Process);

		#region process
		[DllImport("kernel32.dll")]
		public static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(
			ProcessAccessFlags dwDesiredAccess, 
		    [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
		     int dwProcessId
			);

		[DllImport("oleacc.dll", SetLastError = true)]
		public static extern IntPtr GetProcessHandleFromHwnd(IntPtr hwnd);

//		[DllImport("user32.dll")]
//		public static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);

		[DllImport("user32.dll", SetLastError=true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		
		
		[DllImport("Psapi.dll", SetLastError=true)]
		public static extern bool EnumProcesses(
			[MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.U4)] [In][Out] UInt32[] processIds,
			UInt32 arraySizeBytes,
			[MarshalAs(UnmanagedType.U4)] out UInt32 bytesCopied
			);

		[Flags]
		public enum ProcessAccessFlags : uint
		{
			All = 0x001F0FFF,
			Terminate = 0x00000001,
			CreateThread = 0x00000002,
			VMOperation = 0x00000008,
			VMRead = 0x00000010,
			VMWrite = 0x00000020,
			DupHandle = 0x00000040,
			SetInformation = 0x00000200,
			QueryInformation = 0x00000400,
			Synchronize = 0x00100000
		}

		[Flags]
		public enum ModuleFilterFlags
		{
			// List the 32-bit modules.
			LIST_MODULES_32BIT = 0x01,
			
			// List the 64-bit modules.
			LIST_MODULES_64BIT = 0x02,
			
			// List all modules.
			LIST_MODULES_ALL = 0x03,
			
			// Use the default behavior.
			LIST_MODULES_DEFAULT = 0x0
		}

		/// <summary>
		/// Retrieves a handle for each module in the specified process that meets
		/// the specified filter criteria.
		/// </summary>
		/// <param name="hProcess">
		/// A handle to the process.
		/// </param>
		/// <param name="lphModule">
		/// An array that receives the list of module handles.
		/// </param>
		/// <param name="cb">
		/// The size of the lphModule array, in bytes.
		/// </param>
		/// <param name="lpcbNeeded ">
		/// The number of bytes required to store all module handles in the
		/// lphModule array.
		/// </param>
		/// <param name="dwFilterFlag">
		/// The filter criteria. This parameter can be one of the following values.
		/// </param>
		/// <returns>
		/// If the function succeeds, the return value is nonzero.
		/// If the function fails, the return value is zero. To get extended error
		/// information, call GetLastError.
		/// </returns>
		[DllImport("psapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumProcessModulesEx(
			[In] IntPtr hProcess,
			[Out] IntPtr[] lphModule,
			[In] int cb,
			[Out] out int lpcbNeeded,
			[In] ModuleFilterFlags dwFilterFlag);



		/// <summary>
		/// Retrieves the fully-qualified path for the file containing the specified module.
		/// </summary>
		/// <param name="hProcess">
		/// A handle to the process that contains the module. 
		/// </param>
		/// <param name="hModule">
		/// A handle to the module. If this parameter is NULL, GetModuleFileNameEx returns 
		/// the path of the executable file of the process specified in hProcess.
		/// </param>
		/// <param name="lpFilename">
		/// A pointer to a buffer that receives the fully-qualified path to the module.
		/// </param>
		/// <param name="nSize">
		/// The size of the lpFilename buffer, in characters.
		/// </param>
		/// <returns>
		/// If the function succeeds, the return value specifies the length of the string
		/// copied to the buffer. 
		/// If the function fails, the return value is zero. To get extended error 
		/// information, call GetLastError.
		/// </returns>
		[DllImport("psapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern uint GetModuleFileNameEx(
			[In] IntPtr hProcess,
			[In] IntPtr hModule,
			[Out] [MarshalAs(UnmanagedType.LPTStr)] System.Text.StringBuilder lpFilename,
			uint nSize);





	}

	#endregion
}
