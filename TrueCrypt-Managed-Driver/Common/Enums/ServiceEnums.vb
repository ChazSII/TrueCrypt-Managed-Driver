Namespace Common
    ' Service Start Type
    Friend Enum SERVICE_START_TYPE As Integer
        SYSTEM_START = &H1
        AUTO_START = &H2
        DEMAND_START = &H3
    End Enum

    ' Service Control Codes
    Friend Enum SERVICE_CONTROL As Integer
        [STOP] = &H1
        PAUSE = &H2
        [CONTINUE] = &H3
        INTERROGATE = &H4
        SHUTDOWN = &H5
        PARAMCHANGE = &H6
        NETBINDADD = &H7
        NETBINDREMOVE = &H8
        NETBINDENABLE = &H9
        NETBINDDISABLE = &HA
        DEVICEEVENT = &HB
        HARDWAREPROFILECHANGE = &HC
        POWEREVENT = &HD
        SESSIONCHANGE = &HE
    End Enum

    ' Service States
    Friend Enum SERVICE_STATE As Integer
        SERVICE_STOPPED = &H1
        SERVICE_START_PENDING = &H2
        SERVICE_STOP_PENDING = &H3
        SERVICE_RUNNING = &H4
        SERVICE_CONTINUE_PENDING = &H5
        SERVICE_PAUSE_PENDING = &H6
        SERVICE_PAUSED = &H7
    End Enum

    Friend Enum SERVICE_ACCEPT As Integer
        [STOP] = &H1
        PAUSE_CONTINUE = &H2
        SHUTDOWN = &H4
        PARAMCHANGE = &H8
        NETBINDCHANGE = &H10
        HARDWAREPROFILECHANGE = &H20
        POWEREVENT = &H40
        SESSIONCHANGE = &H80
    End Enum
End Namespace