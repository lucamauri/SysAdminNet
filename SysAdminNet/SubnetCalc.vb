Public Class SubnetCalc
    Public ReadOnly Property CalcError As String
    ''' <summary>
    ''' Start the subnet calculation
    ''' </summary>
    ''' <param name="IPAddress">The IP address in dotted decimal notation</param>
    ''' <param name="Bits">Lenght of the subnet mask in bits</param>
    Function Calculate(IPAddress As String, Bits As Integer) As Boolean
        Dim TextOctets() As String
        Dim DecimalOctets As List(Of Integer)
        Dim BinaryOctets As List(Of String)
        Dim BinarySubnetMask As String
        Dim BinaryFullAddress As String
        Dim BinaryFullSubnet As String

        Try
            If Bits < 1 OrElse Bits > 31 Then
                _CalcError = "Subnet mask lenght is invalid. Must be between 1 and 31 bits."
                Return False
            End If

            TextOctets = IPAddress.Split(".")
            If TextOctets.Count < 4 Then
                _CalcError = "IP Address is in wrong format, 4 decimal values separated by dot (.) must be provided."
                Return False
            End If

            DecimalOctets = New List(Of Integer)
            BinaryOctets = New List(Of String)
            BinaryFullAddress = ""

            For Each TextOctet In TextOctets
                Dim DecimalOctet As Integer
                Dim BinaryOctet As String
                If Integer.TryParse(TextOctet, DecimalOctet) Then
                    If DecimalOctet < 0 OrElse DecimalOctet > 255 Then
                        _CalcError = "IP Address is in wrong format, each octet's value must be between 0 and 255."
                        Return False
                    Else
                        DecimalOctets.Add(DecimalOctet)

                        BinaryOctet = Convert.ToString(DecimalOctet, 2).PadLeft(8, "0")
                        BinaryOctets.Add(BinaryOctet)
                        BinaryFullAddress &= BinaryOctet
                    End If
                Else
                    _CalcError = "IP Address is in wrong format, octet is not a valid integer value."
                    Return False
                End If
            Next

            BinarySubnetMask = ""
            For i = 1 To Bits
                BinarySubnetMask &= "1"
            Next
            BinarySubnetMask.PadRight(32, "0")

            BinaryFullSubnet = ""
            For Bit = 1 To 32
                BinaryFullSubnet &= Convert.ToString(Convert.ToBoolean(BinaryFullAddress(Bit - 1)) And Convert.ToBoolean(BinarySubnetMask(Bit - 1)))
            Next

            Return True
        Catch ex As Exception
            _CalcError = ex.ToString
            Return False
        End Try
    End Function
    Function BinaryToDottedDecimal(FullBinary As String) As List(Of Integer)
        Dim DottedBinary As List(Of Integer)

        DottedBinary = New List(Of Integer)
        For i = 0 To 31 Step 8
            DottedBinary.Add(Convert.ToInt16(FullBinary.Substring(i, i + 8), 2))
        Next




    End Function
End Class
