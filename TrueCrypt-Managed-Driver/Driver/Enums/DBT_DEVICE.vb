Namespace Driver
    'Message for WM_DEVICECHANGE
    Public Enum DBT_DEVICE
        ARRIVAL = &H8000
        QUERYREMOVE = &H8001
        QURYREMOVEFAILED = &H8002
        REMOVEPENDING = &H8003
        REMOVECOMPLETE = &H8004
        TYPESPECIFIC = &H8005
    End Enum
End Namespace