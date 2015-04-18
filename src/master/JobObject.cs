//
// Based on http://stackoverflow.com/questions/6266820/working-example-of-createjobobject-setinformationjobobject-pinvoke-in-net/9164742#9164742
//

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Akka.NUnit
{
	internal sealed class JobObject : IDisposable
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr CreateJobObject(IntPtr a, string lpName);

		[DllImport("kernel32.dll")]
		private static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo,
			UInt32 cbJobObjectInfoLength);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

		private IntPtr _handle;
		private bool _disposed;

		public JobObject()
		{
			_handle = CreateJobObject(IntPtr.Zero, null);

			var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
			{
				LimitFlags = 0x2000
			};

			var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
			{
				BasicLimitInformation = info
			};

			int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
			var extendedInfoPtr = Marshal.AllocHGlobal(length);
			try
			{
				Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

				if (!SetInformationJobObject(_handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
					throw new Exception(string.Format("Unable to set information.  Error: {0}", Marshal.GetLastWin32Error()));
			}
			finally
			{
				Marshal.FreeHGlobal(extendedInfoPtr);
			}
		}

		public void Dispose()
		{
			CloseHandle(_handle);
			_handle = IntPtr.Zero;
		}

		public bool AddProcess(IntPtr processHandle)
		{
			var success = AssignProcessToJobObject(_handle, processHandle);
			if (!success)
			{
				var error = Marshal.GetLastWin32Error();
				Console.WriteLine(
					"Unable to assign process to job object. Error: {0}",
					error == 5 ? "ACCESS DENIED" : error.ToString(CultureInfo.InvariantCulture)
					);
			}
			return success;
		}

		public bool AddProcess(Process process)
		{
			if (process == null) throw new ArgumentNullException("process");
			return AddProcess(process.Handle);
		}

		public bool AddProcess(int processId)
		{
			return AddProcess(Process.GetProcessById(processId));
		}

		#region Helper classes

		[StructLayout(LayoutKind.Sequential)]
		private struct IO_COUNTERS
		{
			public UInt64 ReadOperationCount;
			public UInt64 WriteOperationCount;
			public UInt64 OtherOperationCount;
			public UInt64 ReadTransferCount;
			public UInt64 WriteTransferCount;
			public UInt64 OtherTransferCount;
		}


		[StructLayout(LayoutKind.Sequential)]
		private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
		{
			public Int64 PerProcessUserTimeLimit;
			public Int64 PerJobUserTimeLimit;
			public UInt32 LimitFlags;
			public UIntPtr MinimumWorkingSetSize;
			public UIntPtr MaximumWorkingSetSize;
			public UInt32 ActiveProcessLimit;
			public UIntPtr Affinity;
			public UInt32 PriorityClass;
			public UInt32 SchedulingClass;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SECURITY_ATTRIBUTES
		{
			public UInt32 nLength;
			public IntPtr lpSecurityDescriptor;
			public Int32 bInheritHandle;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
		{
			public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
			public IO_COUNTERS IoInfo;
			public UIntPtr ProcessMemoryLimit;
			public UIntPtr JobMemoryLimit;
			public UIntPtr PeakProcessMemoryUsed;
			public UIntPtr PeakJobMemoryUsed;
		}

		public enum JobObjectInfoType
		{
			AssociateCompletionPortInformation = 7,
			BasicLimitInformation = 2,
			BasicUIRestrictions = 4,
			EndOfJobTimeInformation = 6,
			ExtendedLimitInformation = 9,
			SecurityLimitInformation = 5,
			GroupInformation = 11
		}

		#endregion

	}
}