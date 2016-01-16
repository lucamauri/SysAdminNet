Public Class SysUpTime
    Public ReadOnly Property ErrorInResult As Boolean
    Public ReadOnly Property ErrorDetails As String
    Public ReadOnly Property UpTimeSpan As TimeSpan

    Public ReadOnly Property LastStartup As DateTime
        Get
            Return DateTime.Now.Subtract(_UpTimeSpan)
        End Get
    End Property
    Public ReadOnly Property UpTimeString As String
        Get
            Return String.Format("{0:D2} days {1:D2}:{2:D2}:{3:D2}", Convert.ToInt16(Math.Truncate(_UpTimeSpan.TotalDays)), _UpTimeSpan.Hours, _UpTimeSpan.Minutes, _UpTimeSpan.Seconds)
        End Get
    End Property

    Sub New()
        _ErrorInResult = True
        _ErrorDetails = """GetSysUpTime"" never called, class initialized just now"
        _UpTimeSpan = New TimeSpan(0)
    End Sub

    Public Sub QuerySysUpTime()
        Try
            Dim UpTimeCounter = New PerformanceCounter("System", "System Up Time")
            UpTimeCounter.NextValue()

            _UpTimeSpan = TimeSpan.FromSeconds(UpTimeCounter.NextValue)
            _ErrorDetails = "No error"
            _ErrorInResult = False
        Catch ex As Exception
            _ErrorInResult = True
            _ErrorDetails = ex.ToString
            _UpTimeSpan = New TimeSpan(0)
        End Try
    End Sub
End Class