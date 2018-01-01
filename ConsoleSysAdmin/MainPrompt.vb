Module MainPrompt
    Public Const PromptSymbol As String = "SAT > "
    Public Const ApplicationTitle As String = ".net System Administrator Toolkit"

    Sub Main()
        Dim Statement As String
        Dim BrokenDownStatement As String()
        Dim Command As String
        Dim Args As String()
        Dim Result As String

        Console.ForegroundColor = ConsoleColor.Cyan
        Console.Title = ApplicationTitle & " command line console"
        Console.OutputEncoding = Text.Encoding.UTF8


        Console.WriteLine("Welcome to " & ApplicationTitle & " console frontend")
        Console.WriteLine("This package is version " & GetType(SysAdminNet.Main).Assembly.GetName().Version.ToString)
        Console.WriteLine("Current date and time is " & DateTime.Now.ToString("u"))
        Console.WriteLine()
        Console.Write(PromptSymbol)

        Do While True
            Statement = Console.ReadLine()
            BrokenDownStatement = Statement.Split(" ")
            ReDim Args(BrokenDownStatement.Length - 1)
            Command = BrokenDownStatement(0)

            For i = 1 To BrokenDownStatement.Length - 1
                Args(i - 1) = BrokenDownStatement(i)
            Next

            Select Case Command.ToLower
                Case "tail"
                    Console.WriteLine("Interrupt with Ctrl+C")
                    'TODO Review input handling
                    'Dim Worker As New SysAdminNet.Tail(System.IO.Path.GetFullPath(Args(0))

                Case "subnetcalc", "subnet"
                    Dim Worker As New SysAdminNet.SubnetCalc

                    If Worker.Calculate(Args(0), Convert.ToInt16(Args(1))) Then
                        Console.WriteLine("Subnet address:    {0}", Worker.SubnetAddress)
                        Console.WriteLine("Broadcast address: {0}", Worker.BroadcastAddress)
                        Console.WriteLine("Subnet mask:       {0}", Worker.SubnetMask)
                        Console.WriteLine("Usable range:      {0}", Worker.UsableRange)
                        Console.WriteLine("OK")
                    Else
                        Console.ForegroundColor = ConsoleColor.Red
                        Console.WriteLine("ERROR")
                        Console.ForegroundColor = ConsoleColor.Cyan
                        Console.WriteLine(Worker.CalcError)
                    End If
                Case "uptime"
                    Dim Worker As New SysAdminNet.SysUpTime

                    Worker.QuerySysUpTime()
                    If Worker.ErrorInResult Then
                        Result = "ERROR - " & Worker.ErrorDetails
                    Else
                        Result = "Last boot was on: " & Worker.LastStartup & Environment.NewLine & "Uptime is: " & Worker.UpTimeString
                    End If
                Case "wol", "wakeonlan"
                    Dim Worker As New SysAdminNet.WakeOnLAN

                    Worker.macAddress = Args(0)
                    Worker.wakeIt()
                    If Worker.bytesSent = 0 Then
                        Result = "ERROR - No data sent, review input and check MAC address"
                    Else
                        Result = Worker.bytesSent & "bytes sent to the remote machine to wake it up"
                    End If
                Case "exit", "quit"
                    Exit Do
                Case "ver"
                    Result = "This package is version " & GetType(SysAdminNet.Main).Assembly.GetName().Version.ToString
                Case Else
                    Result = "Command not acknowldged: -" & Command & "-"
            End Select
            Console.WriteLine(Result)
            Console.Write(PromptSymbol)
        Loop

        Console.WriteLine("I am exiting, time is " & DateTime.Now.ToString("u"))
        Console.WriteLine("Goodbye")
        Environment.Exit(0)
    End Sub
End Module
