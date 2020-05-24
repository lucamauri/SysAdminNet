Imports System.IO

Public Class Tail
    Sub New(SourceFile As Path)
        Dim SourceReader As StreamReader
        Dim SourceStream As FileStream

        Dim PreviousMaxOffset As Long

        Dim SourceLine As String

        SourceStream = New FileStream(SourceFile.ToString, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        SourceReader = New StreamReader(SourceStream)

        PreviousMaxOffset = SourceReader.BaseStream.Length

        Do While True
            Threading.Thread.Sleep(1000)
            If SourceReader.BaseStream.Length = PreviousMaxOffset Then
                Continue Do
            Else
                SourceReader.BaseStream.Seek(PreviousMaxOffset, SeekOrigin.Begin)
                SourceLine = ""

                While (SourceLine = SourceReader.ReadLine)
                    Console.WriteLine(SourceLine)
                    PreviousMaxOffset = SourceReader.BaseStream.Position
                End While
            End If
        Loop
    End Sub
End Class
