Imports System.Security.Cryptography
Imports System.IO

Public Class TextCollection
    Inherits System.Collections.CollectionBase

    Public ReadOnly Property Item(ByVal Index As Integer) As String
        Get
            Return List.Item(Index)
        End Get
    End Property

    Public Sub Add(ByVal FilePath As String)
        List.Add(FilePath)
    End Sub
End Class

Public Class ArrayCollection
    Inherits System.Collections.CollectionBase

    Public ReadOnly Property Item(ByVal Index As Integer) As String()
        Get
            Return List.Item(Index)
        End Get
    End Property

    Public Sub Add(ByVal Item As String())
        List.Add(Item)
    End Sub
End Class

Public Class MD5Checker

    Dim Files As TextCollection
    Dim OriginStream As Stream
    Dim mWorkerActive As Boolean

    Public ReadOnly Property CurrStream() As Stream
        Get
            Return OriginStream
        End Get
    End Property
    Public WriteOnly Property TextString() As String
        Set(ByVal Value As String)

        End Set
    End Property

    Public WriteOnly Property FilePaths() As TextCollection
        Set(ByVal Value As TextCollection)
            Files = Value
        End Set
    End Property

    Public Property WorkerActive() As Boolean
        Get
            Return mWorkerActive
        End Get
        Set(ByVal Value As Boolean)
            mWorkerActive = Value
        End Set
    End Property

    Private Function BytesTOString(ByVal RawHash() As Byte) As String

        Dim B As Byte
        Dim Result As New System.Text.StringBuilder

        For Each B In RawHash
            Dim StrTemp As String
            StrTemp = Convert.ToString(B, 16)
            If StrTemp.Length = 1 Then
                StrTemp = "0" & StrTemp
            End If
            Result.Append(StrTemp)
        Next

        Return Result.ToString

    End Function
    Public Overloads Function HashFiles() As String(,)
        Dim CurrFile As String
        Dim Infos(,) As String
        Dim i As Integer

        ReDim Infos(1, Files.Count - 1)

        i = 0

        For Each CurrFile In Files
            Try
                OriginStream = File.Open(CurrFile, FileMode.Open)
                Infos(0, i) = HashSingleFile(OriginStream)
                Infos(1, i) = CurrFile
                'Result.Add(CurrFile & " " & HashSingleFile(OriginStream))
                'Result.Add(Infos)

            Catch ex As Exception
                'Result.Add(CurrFile & " " & ex.ToString)
                Infos(0, i) = ex.ToString
                Infos(1, i) = CurrFile
                'Result.Add(Infos)

            End Try

            'Console.WriteLine(Infos(1))
            'Result.Add(Infos)
            OriginStream.Close()

            i += 1

        Next

        Return Infos

    End Function
    Public Overloads Function HashFiles(ByVal SingleFile As String) As String
        Dim Result As String
        'Dim i As Integer

        Try
            OriginStream = File.Open(SingleFile, FileMode.Open)
            Result = HashSingleFile(OriginStream)

        Catch ex As Exception
            Result = ex.ToString

        Finally
            OriginStream.Close()

        End Try

        Return Result

    End Function

    Private Function HashSingleFile(ByVal DataStream As Stream) As String
        Dim MD5Class As New MD5CryptoServiceProvider
        Dim Result As String
        'Dim Digit As Byte

        Result = BytesTOString(MD5Class.ComputeHash(DataStream))

        DataStream.Close()

        Return Result
    End Function

    Public Function HasString(ByVal Text As String) As String

        Dim InputArray() As Byte
        Dim MD5 As New MD5CryptoServiceProvider
        Dim Result As String

        InputArray = System.Text.Encoding.ASCII.GetBytes(Text)

        Result = BytesTOString(MD5.ComputeHash(InputArray))

        Return Result

    End Function

    Public Sub New()

        Files = New TextCollection

    End Sub
End Class