Imports System.Globalization
Public Class IPv4Handler
    Public ReadOnly Property BroadcastAddress As String
    Public ReadOnly Property CalcError As String
    Public ReadOnly Property SubnetAddress As String
    Public ReadOnly Property SubnetMask As String
    Public ReadOnly Property UsableRange As String
    Public ReadOnly Property UsableIPs As List(Of String)
    Public ReadOnly Property AllUsableIPs As List(Of String)

    ''' <summary>
    ''' Start the subnet calculation
    ''' </summary>
    ''' <param name="IPAddress">The IP address in dotted decimal notation</param>
    ''' <param name="Bits">Lenght of the subnet mask in bits</param>
    Function Calculate(IPAddress As String, Bits As Integer) As Boolean
        Dim BinarySubnetMask As String
        Dim BinaryInvertedSubnetMask As String

        Dim BinaryFullAddress As String
        Dim BinaryFullSubnet As String
        Dim BinaryFullBroadcast As String

        Try
            If Bits < 1 OrElse Bits > 31 Then
                _CalcError = "Subnet mask lenght is invalid. Must be between 1 and 31 bits."
                Return False
            End If

            BinaryFullAddress = DottedDecimalToBinary(IPAddress)

            BinarySubnetMask = ""
            BinaryInvertedSubnetMask = ""
            For i As Integer = 1 To Bits
                BinarySubnetMask &= "1"
                BinaryInvertedSubnetMask &= "0"
            Next
            BinarySubnetMask = BinarySubnetMask.PadRight(32, "0")
            BinaryInvertedSubnetMask = BinaryInvertedSubnetMask.PadRight(32, "1")

            BinaryFullSubnet = AndOperation(BinaryFullAddress, BinarySubnetMask)
            BinaryFullBroadcast = OrOperation(BinaryFullSubnet, BinaryInvertedSubnetMask)

            _SubnetAddress = FormatDottedDecimal(BinaryToDottedDecimal(BinaryFullSubnet))
            _BroadcastAddress = FormatDottedDecimal(BinaryToDottedDecimal(BinaryFullBroadcast))
            _SubnetMask = FormatDottedDecimal(BinaryToDottedDecimal(BinarySubnetMask))
            _UsableRange = UsableRangeCalc(BinaryFullSubnet, BinaryFullBroadcast)
            _UsableIPs = UsableIPsCalc(BinaryFullSubnet, BinaryFullBroadcast)
            _AllUsableIPs = AllUsableIPsCalc(BinaryFullSubnet, BinaryFullBroadcast)

            _CalcError = "None"
            Return True
        Catch ex As Exception
            _CalcError = ex.ToString
            Return False
        End Try
    End Function
    Function AndOperation(Address As String, SubnetMask As String) As String
        Dim FinalResult As String

        FinalResult = ""
        For Bit As Integer = 1 To 32
            FinalResult &= Convert.ToInt32(Convert.ToBoolean(Char.GetNumericValue(Address(Bit - 1))) And Convert.ToBoolean(Char.GetNumericValue(SubnetMask(Bit - 1))))
        Next

        Return FinalResult

    End Function
    Function OrOperation(Address As String, InvertedSubnetMask As String) As String
        Dim FinalResult As String

        FinalResult = ""
        For Bit As Integer = 1 To 32
            FinalResult &= Convert.ToInt32(Convert.ToBoolean(Char.GetNumericValue(Address(Bit - 1))) Or Convert.ToBoolean(Char.GetNumericValue(InvertedSubnetMask(Bit - 1))))
        Next
        Return FinalResult
    End Function
    Function BinaryToDottedDecimal(FullBinary As String) As List(Of Integer)
        Dim DottedDecimal As List(Of Integer)
        Dim PartBinary As String

        DottedDecimal = New List(Of Integer)
        For i As Integer = 0 To 31 Step 8
            PartBinary = FullBinary.Substring(i, 8)
            DottedDecimal.Add(Convert.ToInt16(PartBinary, 2))
        Next
        Return DottedDecimal
    End Function
    Function FormatDottedDecimal(Octets As List(Of Integer)) As String
        Dim FinalIP As Text.StringBuilder
        Dim IsFirst As Boolean

        FinalIP = New System.Text.StringBuilder
        IsFirst = True

        For Each Octet As Integer In Octets
            If IsFirst Then
                IsFirst = False
            Else
                FinalIP.Append(".")
            End If
            FinalIP.Append(Octet.ToString)
        Next
        Return FinalIP.ToString
    End Function
    Function FormattedDottedDecimal(FullBinary As String) As String
        Return FormatDottedDecimal(BinaryToDottedDecimal(FullBinary))
    End Function
    Function UsableRangeCalc(BinarySubnetAddress As String, BinaryBroadcastAddress As String) As String
        Dim BinaryFirstUsable As String
        Dim BinaryLastUsable As String

        BinaryFirstUsable = BinarySubnetAddress.Substring(0, 31) & "1"
        BinaryLastUsable = BinaryBroadcastAddress.Substring(0, 31) & "0"

        Return FormatDottedDecimal(BinaryToDottedDecimal(BinaryFirstUsable)) & " … " & FormatDottedDecimal(BinaryToDottedDecimal(BinaryLastUsable))
    End Function
    Function UsableIPsCalc(BinarySubnetAddress As String, BinaryBroadcastAddress As String) As List(Of String)
        Dim BinaryFirstUsable As String
        Dim BinaryLastUsable As String
        Dim IPList As New List(Of String)

        BinaryFirstUsable = BinarySubnetAddress.Substring(0, 31) & "1"
        BinaryLastUsable = BinaryBroadcastAddress.Substring(0, 31) & "0"

        IPList.Add(FormatDottedDecimal(BinaryToDottedDecimal(BinaryFirstUsable)))
        IPList.Add(FormatDottedDecimal(BinaryToDottedDecimal(BinaryLastUsable)))

        Return IPList
    End Function
    Function AllUsableIPsCalc(BinarySubnetAddress As String, BinaryBroadcastAddress As String) As List(Of String)
        Dim BinaryFirstUsable As String
        Dim BinaryLastUsable As String
        Dim IntegerFirstUsable As Integer
        Dim IntegerLastUsable As Integer
        Dim IPList As New List(Of String)

        BinaryFirstUsable = BinarySubnetAddress.Substring(0, 31) & "1"
        BinaryLastUsable = BinaryBroadcastAddress.Substring(0, 31) & "0"

        IntegerFirstUsable = Convert.ToInt32(BinaryFirstUsable, 2)
        IntegerLastUsable = Convert.ToInt32(BinaryLastUsable, 2)

        For i As Integer = IntegerFirstUsable To IntegerLastUsable
            IPList.Add(FormatDottedDecimal(BinaryToDottedDecimal(Convert.ToString(i, 2).PadLeft(32, "0"))))
        Next

        Return IPList
    End Function
    Function DottedDecimalToBinary(DottedDecimalAddress As String) As String
        Dim TextOctets As String()
        Dim DecimalOctets As List(Of Integer)
        Dim BinaryOctets As List(Of String)
        Dim BinaryFullAddress As String

        TextOctets = DottedDecimalAddress.Split(".")
        If TextOctets.Length <> 4 Then
            Throw (New AddressFormatException("IP Address is in wrong format: 4 decimal values separated by dot (.) must be provided."))
        End If

        DecimalOctets = New List(Of Integer)
        BinaryOctets = New List(Of String)
        BinaryFullAddress = ""

        For Each TextOctet As String In TextOctets
            Dim DecimalOctet As Integer
            Dim BinaryOctet As String
            If Integer.TryParse(TextOctet, DecimalOctet) Then
                If DecimalOctet < 0 OrElse DecimalOctet > 255 Then
                    Throw (New AddressFormatException("IP Address is in wrong format, each octet's value must be between 0 and 255."))
                Else
                    DecimalOctets.Add(DecimalOctet)

                    BinaryOctet = Convert.ToString(DecimalOctet, 2).PadLeft(8, "0")
                    BinaryOctets.Add(BinaryOctet)
                    BinaryFullAddress &= BinaryOctet
                End If
            Else
                Throw (New AddressFormatException("IP Address is in wrong format, octet is not a valid integer value."))
            End If
        Next
        Return BinaryFullAddress
    End Function
End Class
Class AddressFormatException : Inherits ApplicationException
        Public Sub New(ByVal Description As String)
            MyBase.New(Description)
        End Sub
End Class