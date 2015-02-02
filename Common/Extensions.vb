Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Namespace Common
    Friend Module Extensions

        <Extension()>
        Friend Function PointerToStructure(Of T)(ByVal Pointer As IntPtr) As T
            Dim SizeOne As Integer = Marshal.SizeOf(GetType(T))
            Dim Data(SizeOne) As Byte, MyPointer As GCHandle
            Dim Ret As T

            If Pointer = IntPtr.Zero Then Return Ret

            Marshal.Copy(Pointer, Data, 0, Data.Length)

            MyPointer = GCHandle.Alloc(Data, GCHandleType.Pinned)

            Ret = Marshal.PtrToStructure(MyPointer.AddrOfPinnedObject, GetType(T))

            MyPointer.Free()

            Return Ret
        End Function

        <Extension()>
        Friend Function StructureToPointer(Of T)(ByVal [Structure] As T) As IntPtr
            Dim Ret As IntPtr = Marshal.AllocHGlobal(Marshal.SizeOf([Structure]))

            Marshal.StructureToPtr([Structure], Ret, False)

            Return Ret
        End Function

    End Module
End Namespace