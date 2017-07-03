Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Namespace Common
    Friend Module Extensions

        <Extension()>
        Friend Function ToStructure(Of T As Structure)(pointer As IntPtr) As T
            Dim pointerSize As Integer = Marshal.SizeOf(GetType(T))
            Dim dataArray(pointerSize) As Byte
            Dim gcHandle As GCHandle
            Dim returnObject As T = Nothing

            If pointer = IntPtr.Zero Then Return returnObject

            Marshal.Copy(pointer, dataArray, 0, dataArray.Length)

            gcHandle = GCHandle.Alloc(dataArray, GCHandleType.Pinned)

            returnObject = Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject, GetType(T))

            gcHandle.Free()

            Return returnObject
        End Function

        <Extension()>
        Friend Function ToPointer(Of T As Structure)([Structure] As T) As IntPtr
            Dim returnPointer As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf([Structure]))

            Marshal.StructureToPtr([Structure], returnPointer, False)

            Return returnPointer
        End Function

    End Module
End Namespace