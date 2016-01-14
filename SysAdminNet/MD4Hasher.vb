'Implements the MD4 message digest algorithm in VB.Net

' Ronald L. Rivest,
' http://www.roxen.com/rfc/rfc1320.html
' The MD4 Message-Digest Algorithm
' IETF RFC-1320 (informational).

'Copyright (c) 2000 Oren Novotny (osn@po.cwru.edu)
'Permission is granted to use this code for anything.
'Derived from the RSA Data Security, Inc. MD4 Message-Digest Algorithm. 
'http://www.rsasecurity.com">RSA Data Security, Inc.</a> requires 
'attribution for any work that is derived from the MD4 Message-Digest 
'Algorithm; for details see <a href="http://www.roxen.com/rfc/rfc1320.html">
'RFC 1320</a>.

'This code is ported from Norbert Hranitzky's 
'(norbert.hranitzky@mchp.siemens.de)
'Java version.

'Copyright (c) 2008 Luca Mauri (http://www.lucamauri.com)
'The orginal version found at http://www.derkeiler.com/Newsgroups/microsoft.public.dotnet.security/2004-08/0004.html
'was not working.
'I corrected and modified it so the current version is now calculating proper MD4 checksum.

Imports System
Imports System.Text

Public Class MD4Hasher

#Region "MD4 specific object variables"
    ' The size in bytes of the input block to the transformation algorithm
    Private Const BLOCK_LENGTH As Integer = 64    ' = 512 / 8
    ' 4 32-bit words (interim result)
    Private context(4 - 1) As UInt32
    ' Number of bytes procesed so far mod. 2 power of 64.
    Private count As Long
    ' 512-bit input buffer = 16 x 32-bit words holds until it reaches 512 bits
    Private buffer(BLOCK_LENGTH - 1) As Byte
    ' 512-bit work buffer = 16 x 32-bit words
    Private X(16 - 1) As UInt32
#End Region

#Region "Constructors"
    Public Sub New()
        engineReset()
    End Sub

    ' This constructor is here to implement the clonability of this class
    Private Sub New(ByVal md As MD4Hasher)
        context = CType(md.context.Clone(), UInt32())
        buffer = CType(md.buffer.Clone(), Byte())
        count = md.count
    End Sub
#End Region

#Region "Clonable method implementation"
    Public Function Clone() As Object
        Return New MD4Hasher(Me)
    End Function
#End Region

#Region "JCE methods"

    ' Resets this object disregarding any temporary data present at the
    ' time of the invocation of this call.
    Private Sub engineReset()
        ' initial values of MD4 i.e. A, B, C, D
        ' as per rfc-1320; they are low-order byte first
        context(0) = 1732584193
        context(1) = 4023233417
        context(2) = 2562383102
        context(3) = 271733878
        count = 0
        Dim i As Integer = 0
        Do While i < BLOCK_LENGTH
            buffer(i) = 0
            i += 1
        Loop
    End Sub

    ' Continues an MD4 message digest using the input byte
    '   byte to input
    Private Sub engineUpdate(ByVal b As Byte)
        ' compute number of bytes still unhashed; ie. present in buffer
        Dim i As Integer = CType(count Mod BLOCK_LENGTH, Integer)
        count += 1 ' update number of bytes
        buffer(i) = b
        If i = (BLOCK_LENGTH - 1) Then
            transform(buffer, 0)
        End If
    End Sub

    ' MD4 block update operation
    ' Continues an MD4 message digest operation by filling the buffer, 
    ' transform(ing) data in 512-bit message block(s), updating the variables
    ' context and count, and leaving (buffering) the remaining bytes in buffer
    ' for the next update or finish.
    '   input  = input block
    '   offset = start of meaningful bytes in input
    '   len    = count of bytes in input blcok to consider
    Private Sub engineUpdate(ByVal input() As Byte, ByVal offset As Integer, ByVal len As Integer)
        ' make sure we don't exceed input's allocated size/length
        If (offset < 0 Or len < 0 Or CType(offset, Integer) + len > input.Length) Then
            Throw New ArgumentOutOfRangeException()
        End If
        ' compute number of bytes still unhashed; ie. present in buffer
        Dim bufferNdx As Integer = CType(count Mod BLOCK_LENGTH, Integer)
        count += len  ' update number of bytes
        Dim partLen As Integer = BLOCK_LENGTH - bufferNdx
        Dim i As Integer = 0

        If len >= partLen Then
            Array.Copy(input, offset + i, buffer, bufferNdx, partLen)

            transform(buffer, 0)

            i = partLen
            Do While (i + BLOCK_LENGTH - 1) < len
                Console.WriteLine("Sono qui")

                transform(input, offset + i)
                i += BLOCK_LENGTH
            Loop
            bufferNdx = 0
        End If

        ' buffer remaining input
        If i < len Then
            Array.Copy(input, offset + i, buffer, bufferNdx, len - i)
        End If
    End Sub

    ' Completes the hash computation by performing final operations such
    ' as padding.  At the return of this engineDigest, the MD engine is
    ' reset.
    '   returns the array of bytes for the resulting hash value.
    Private Function engineDigest() As Byte()
        ' pad output to 56 mod 64; as RFC1320 puts it: congruent to 448 mod 512
        Dim bufferNdx As Integer = CType(count Mod BLOCK_LENGTH, Integer)
        Dim padLen As Integer
        If bufferNdx < 56 Then
            padLen = 56 - bufferNdx
        Else
            padLen = 120 - bufferNdx
        End If
        ' padding is always binary 1 followed by binary 0's
        Dim tail(padLen + 8 - 1) As Byte
        tail(0) = CType(128, Byte)

        ' append length before final transform
        ' save number of bits, casting the long to an array of 8 bytes
        ' save low-order byte first.
        Dim TempArray As Byte()
        TempArray = BitConverter.GetBytes(count * 8)
        TempArray.CopyTo(tail, padLen)

        engineUpdate(tail, 0, tail.Length)

        Dim result(16 - 1) As Byte

        For i As Integer = 0 To 3
            Dim TempStore(4 - 1) As Byte
            TempStore = BitConverter.GetBytes(context(i))
            TempStore.CopyTo(result, i * 4)
        Next

        ' reset the engine
        engineReset()
        Return result
    End Function

    ' Returns a byte hash from a string
    '   s = string to hash
    '   returns byte-array that contains the hash
    Public Function GetByteHashFromString(ByVal s As String) As Byte()
        Dim b() As Byte = Encoding.UTF8.GetBytes(s)
        Dim _md4 As MD4Hasher = New MD4Hasher()

        _md4.engineUpdate(b, 0, b.Length)

        Return _md4.engineDigest()
    End Function

    ' Returns a binary hash from an input byte array
    '   b = byte-array to hash
    '   returns binary hash of input
    Public Function GetByteHashFromBytes(ByVal b() As Byte) As Byte()
        Dim _md4 As MD4Hasher = New MD4Hasher()

        _md4.engineUpdate(b, 0, b.Length)

        Return _md4.engineDigest()
    End Function

    ' Returns a string that contains the hexadecimal hash
    '   b = byte-array to input
    '   returns String that contains the hex of the hash
    Public Function GetHexHashFromBytes(ByVal b() As Byte) As String
        Dim e() As Byte = GetByteHashFromBytes(b)
        Return bytesToHex(e, e.Length)
    End Function

    ' Returns a byte hash from the input byte
    '   b = byte to hash
    '   returns binary hash of the input byte
    Public Function GetByteHashFromByte(ByVal b As Byte) As Byte()
        Dim _md4 As MD4Hasher = New MD4Hasher()

        _md4.engineUpdate(b)

        Return _md4.engineDigest()
    End Function

    ' Returns a string that contains the hexadecimal hash
    '   b = byte to hash
    '   returns String that contains the hex of the hash
    Public Function GetHexHashFromByte(ByVal b As Byte) As String
        Dim e() As Byte = GetByteHashFromByte(b)
        Return bytesToHex(e, e.Length)
    End Function

    ' Returns a string that contains the hexadecimal hash
    '   s = string to hash
    '   returns String that contains the hex of the hash
    Public Function GetHexHashFromString(ByVal s As String) As String
        Dim b() As Byte = GetByteHashFromString(s)
        Return bytesToHex(b, b.Length)
    End Function

    Private Shared Function bytesToHex(ByVal a() As Byte, ByVal len As Integer) As String
        Dim temp As String = BitConverter.ToString(a)
        Dim i As Integer
        ' We need to remove the dashes that come from the BitConverter
        Dim sb As StringBuilder = New StringBuilder(CType((len - 2) / 2, Integer)) ' This should be the final size

        For i = 0 To temp.Length - 1 Step 1
            If temp(i) <> "-" Then
                sb.Append(temp(i))
            End If
        Next
        Return sb.ToString()
    End Function
#End Region

#Region "own methods"
    ' MD4 basic transformation
    ' Transforms context based on 512 bits from input block starting
    ' from the offset'th byte.
    '   block  = input sub-array
    '   offset = starting position of sub-array
    Private Sub transform(ByRef block() As Byte, ByVal offset As Integer)
        ' decodes 64 bytes from input block into an array of 16 32-bit
        ' entities. Use A as a temp var.
        Dim i As Integer
        For i = 0 To 15 Step 1
            If offset >= block.Length Then
                Exit For
            End If
            X(i) = (CType(block(offset + 0), UInt32) And 255) Or _
           (CType(block(offset + 1), UInt32) And 255) << 8 Or _
          (CType(block(offset + 2), UInt32) And 255) << 16 Or _
          (CType(block(offset + 3), UInt32) And 255) << 24
            offset += 4
        Next

        Dim A As UInt32 = context(0)
        Dim B As UInt32 = context(1)
        Dim C As UInt32 = context(2)
        Dim D As UInt32 = context(3)

        A = FF(A, B, C, D, X(0), 3)
        D = FF(D, A, B, C, X(1), 7)
        C = FF(C, D, A, B, X(2), 11)
        B = FF(B, C, D, A, X(3), 19)
        A = FF(A, B, C, D, X(4), 3)
        D = FF(D, A, B, C, X(5), 7)
        C = FF(C, D, A, B, X(6), 11)
        B = FF(B, C, D, A, X(7), 19)
        A = FF(A, B, C, D, X(8), 3)
        D = FF(D, A, B, C, X(9), 7)
        C = FF(C, D, A, B, X(10), 11)
        B = FF(B, C, D, A, X(11), 19)
        A = FF(A, B, C, D, X(12), 3)
        D = FF(D, A, B, C, X(13), 7)
        C = FF(C, D, A, B, X(14), 11)
        B = FF(B, C, D, A, X(15), 19)

        A = GG(A, B, C, D, X(0), 3)
        D = GG(D, A, B, C, X(4), 5)
        C = GG(C, D, A, B, X(8), 9)
        B = GG(B, C, D, A, X(12), 13)
        A = GG(A, B, C, D, X(1), 3)
        D = GG(D, A, B, C, X(5), 5)
        C = GG(C, D, A, B, X(9), 9)
        B = GG(B, C, D, A, X(13), 13)
        A = GG(A, B, C, D, X(2), 3)
        D = GG(D, A, B, C, X(6), 5)
        C = GG(C, D, A, B, X(10), 9)
        B = GG(B, C, D, A, X(14), 13)
        A = GG(A, B, C, D, X(3), 3)
        D = GG(D, A, B, C, X(7), 5)
        C = GG(C, D, A, B, X(11), 9)
        B = GG(B, C, D, A, X(15), 13)

        A = HH(A, B, C, D, X(0), 3)
        D = HH(D, A, B, C, X(8), 9)
        C = HH(C, D, A, B, X(4), 11)
        B = HH(B, C, D, A, X(12), 15)
        A = HH(A, B, C, D, X(2), 3)
        D = HH(D, A, B, C, X(10), 9)
        C = HH(C, D, A, B, X(6), 11)
        B = HH(B, C, D, A, X(14), 15)
        A = HH(A, B, C, D, X(1), 3)
        D = HH(D, A, B, C, X(9), 9)
        C = HH(C, D, A, B, X(5), 11)
        B = HH(B, C, D, A, X(13), 15)
        A = HH(A, B, C, D, X(3), 3)
        D = HH(D, A, B, C, X(11), 9)
        C = HH(C, D, A, B, X(7), 11)
        B = HH(B, C, D, A, X(15), 15)


        context(0) = TruncateHex(context(0) + Convert.ToInt64(A))
        context(1) = TruncateHex(context(1) + Convert.ToInt64(B))
        context(2) = TruncateHex(context(2) + Convert.ToInt64(C))
        context(3) = TruncateHex(context(3) + Convert.ToInt64(D))
    End Sub
#End Region

#Region "The basic MD4 atomic functions."

    Private Function FF(ByVal a As UInt32, ByVal b As UInt32, ByVal c As UInt32, ByVal d As UInt32, ByVal x As UInt32, ByVal s As Integer) As UInt32

        Dim t As UInt32

        Try
            t = TruncateHex(TruncateHex(Convert.ToInt64(a) + ((b And c) Or ((Not b) And d))) + Convert.ToInt64(x))
            Return (t << s) Or (t >> (32 - s))
        Catch ex As Exception
            Return (t << s) Or (t >> (32 - s))
        End Try
    End Function
    Private Function GG(ByVal a As UInt32, ByVal b As UInt32, ByVal c As UInt32, ByVal d As UInt32, ByVal x As UInt32, ByVal s As Integer) As UInt32
        Dim t As UInt32

        Try
            t = TruncateHex(TruncateHex(Convert.ToInt64(a) + ((b And (c Or d)) Or (c And d))) + Convert.ToInt64(x) + 1518500249)      '&H5A827999
            Return t << s Or t >> (32 - s)
        Catch
            Return t << s Or t >> (32 - s)
        End Try
    End Function
    Private Function HH(ByVal a As UInt32, ByVal b As UInt32, ByVal c As UInt32, ByVal d As UInt32, ByVal x As UInt32, ByVal s As Integer) As UInt32

        Dim t As UInt32

        Try
            t = TruncateHex(TruncateHex(Convert.ToInt64(a) + (b Xor c Xor d)) + Convert.ToInt64(x) + 1859775393)       '&H6ED9EBA1
            Return t << s Or t >> (32 - s)
        Catch
            Return t << s Or t >> (32 - s)
        End Try
    End Function
#End Region

#Region "Support functions"
    Private Function TruncateHex(ByVal Number64 As UInt64) As UInt32
        Dim HexString As String
        Dim HexStringLimited As String

        HexString = Number64.ToString("x")

        If HexString.Length < 8 Then
            HexStringLimited = HexString.PadLeft(8, "0")
        Else
            HexStringLimited = HexString.Substring(HexString.Length - 8)
        End If

        Return UInt32.Parse(HexStringLimited, Globalization.NumberStyles.HexNumber)
    End Function

#End Region

End Class