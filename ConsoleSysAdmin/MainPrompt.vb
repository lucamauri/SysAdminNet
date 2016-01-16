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

        Console.WriteLine("Welcome to " & ApplicationTitle & " console frontend")
        Console.WriteLine("This package is version " & GetType(SysAdminNet.Main).Assembly.GetName().Version.ToString)
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
                Case "uptime"
                    Dim Worker As New SysAdminNet.SysUpTime

                    Worker.QuerySysUpTime()
                    If Worker.ErrorInResult Then
                        Result = "ERROR - " & Worker.ErrorDetails
                    Else
                        Result = "Last boot was on: " & Worker.LastStartup & Environment.NewLine & "Uptime is: " & Worker.UpTimeString
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
