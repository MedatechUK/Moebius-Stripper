Imports System.Net.Sockets
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Web.Script.Serialization

Module Module1

    Private _dictKey As String = "d0d90723-3851-4c65-8c04-dea85de4051f"
    Dim cout As System.IO.TextWriter = Console.Out
    Dim cin As System.IO.TextReader = Console.In

    Public sock As System.Net.Sockets.Socket
    Dim objIniFile As New IniFile(My.Computer.FileSystem.CurrentDirectory & "/settings.ini")
    Public ircserver As String = objIniFile.GetString("Moebius", "server", "irc.emerge-it.co.uk")
    Public port As Integer = objIniFile.GetInteger("Moebius", "port", 6667)
    Public nick As String = objIniFile.GetString("Moebius", "nick", "Moebius")
    Public channel As String = objIniFile.GetString("Moebius", "channel", "#dev")
    Public identifywithnickserv As Boolean = objIniFile.GetBoolean("Moebius", "ns-identify", False)
    Public nickservpass As String = objIniFile.GetString("Moebius", "ns-pass", "")

    Dim options As String = ""

    Sub Main()
        Dim ipHostInfo As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(ircserver)
        Dim EP As New System.Net.IPEndPoint(ipHostInfo.AddressList(0), port)
        Dim registered As Boolean = False
        sock = New System.Net.Sockets.Socket(EP.Address.AddressFamily, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
        sock.Connect(ircserver, port)

        If sock.Connected Then
            sendConnectCommands()
            While Not registered
                Dim mail As String = recv()
                Debug.WriteLine(mail)
                If mail.Contains("001") And mail.ToLower.Contains("welcome") And mail.ToLower.Contains("network") Then
                    send("JOIN " & channel)
                    registered = True
                End If
            End While
        End If

        While sock.Connected = True
            Dim mail As String = recv()
            Debug.WriteLine(mail)
            If mail.EndsWith(nick & ": shutdown -r now") Then
                Debug.WriteLine("---> Disconnecting and reconnecting.")
                send("QUIT")
                sock.Close()
                Thread.Sleep(50)
                Application.Restart()
            ElseIf mail.Contains(nick & ", --channel") Then
                Dim foo = Split(mail, " ")
                Dim bar = foo(foo.Length - 1)
                send("PART " & channel)
                channel = bar
                send("JOIN " & channel)
            ElseIf mail.Contains(nick & ": --help") Then
                sendhelp(mail)
            ElseIf mail.Contains(nick & ", eval") Then
                Dim foo = Split(mail, " ")
                Dim bar = foo(foo.Length - 1)
                Dim baz = mathEval(bar)
                Debug.WriteLine("---> Evaluating " & bar & " - result: " & baz)
                send("PRIVMSG " & channel & " :" & baz)
            ElseIf mail.Contains(nick & ", -R") Then
                rickroll(mail)
            ElseIf mail.Contains("define:") Or mail.Contains("def:") Then
                Dim v As Boolean = False
                mail = mail.Split(">").Last

                If mail.Contains("def:") Then
                    mail = mail.Replace("def", "").Split(":").Last
                Else
                    v = True
                    mail = mail.Replace("define", "").Split(":").Last
                    'known to contain define if it didn't contain def
                End If
                mail = mail.Trim
                send("PRIVMSG " & channel & " :" & "defining: " & mail)
                DictDef(mail, v)
            Else

            End If


        End While
    End Sub

    Public Sub DictDef(ByVal mail As String, verbose As Boolean)

        Dim ht As New Net.WebClient

        Dim urlbuild = Function(word)
                           Dim url As String = _
                               "http://www.dictionaryapi.com/api/v1/references/collegiate/xml/"
                           url &= word
                           url &= "?key="
                           url &= _dictKey
                           Return url
                       End Function

        Dim response As New Xml.XmlDocument()

        response.LoadXml(ht.DownloadString(urlbuild(mail)))


        Dim j As Integer = 0
        For Each i As Xml.XmlNode In response.SelectNodes("*//dt/text()")
            If Not verbose And j = 3 Then
                Exit Sub
            End If
            j += 1
            send("PRIVMSG " & channel & " : " & j & i.InnerText)
        Next
        If j = 0 Then

            send("PRIVMSG " & channel & " : " & "Nothing found :(")

            urlbuild = Function(word)
                           Dim url As String = _
                               "http://suggestqueries.google.com/complete/search?client=chrome&q="
                           url &= word
                           Return url
                       End Function

            Dim jss As New JavaScriptSerializer

            Dim x = jss.Deserialize(Of List(Of Object))( _
            ht.DownloadString(urlbuild(mail)))

            Debug.Write(x)

            For Each i As String In x(1)
                If j = 3 Then
                    Exit Sub
                End If
                j += 1
                send("PRIVMSG " & channel & " :" & "Did you mean " & i & "?")
            Next
        End If


    End Sub



    Public Sub sendConnectCommands()
        send("NICK " & nick)
        send("USER " & nick & " 0 * :paulandthomas")
        If identifywithnickserv = True Then
            send("PRIVMSG nickserv :identify " & nickservpass)
        End If
        send("MODE " & nick & options)
    End Sub

    Public Sub noticeperson(ByVal mail As String, ByVal texttosend As String)
        Dim foo = Split(mail, " ")
        Dim bar
        If foo(foo.Length - 1) = "" Then
            bar = foo(foo.Length - 2)
        Else
            bar = foo(foo.Length - 1)
        End If
        send("NOTICE " & bar & " :" & texttosend)
        Debug.WriteLine("NOTICE " & bar & " :" & texttosend)
    End Sub

    Public Sub noticepersonwhosentthis(ByVal mail As String, ByVal texttosend As String)
        Dim foo = Split(mail, " ")
        Dim bar = Split(foo(1), ">")
        send("NOTICE " & bar(0) & " :" & texttosend)
        Debug.WriteLine("NOTICE " & bar(0) & " :" & texttosend)
    End Sub

    Sub send(ByVal msg As String)
        msg &= vbCr & vbLf
        Dim data() As Byte = System.Text.ASCIIEncoding.UTF8.GetBytes(msg)
        sock.Send(data, msg.Length, SocketFlags.None)
    End Sub

    Function recv() As String

        Dim data(4096) As Byte
        Try
            sock.Receive(data, 4096, SocketFlags.None)
        Catch
            Return Nothing
        End Try

        Dim mail As String = System.Text.ASCIIEncoding.UTF8.GetString(data)
        If mail.Contains(" ") Then
            If mail.Substring(0, 4) = "PING" Then
                Dim pserv As String = mail.Substring(mail.IndexOf(":"), mail.Length - mail.IndexOf(":"))
                pserv = pserv.TrimEnd(Chr(0))
                mail = "PING from " & pserv & vbNewLine & "PONG to " & pserv
                send("PONG " & pserv)
            ElseIf mail.Substring(mail.IndexOf(" ") + 1, 7) = "PRIVMSG" Then
                Dim tmparr() As String = Nothing
                mail = mail.Remove(0, 1)
                tmparr = mail.Split("!")
                Dim rnick = tmparr(0)
                tmparr = Split(mail, ":", 2)
                Dim rmsg = tmparr(1)
                mail = "msg: " & rnick & ">" & rmsg
            End If
        End If

        Try
            mail = mail.TrimEnd(Chr(0))
            mail = mail.Remove(mail.LastIndexOf(vbLf), 1)
            mail = mail.Remove(mail.LastIndexOf(vbCr), 1)
        Catch ex As Exception

        End Try

        Return mail
    End Function

    Function mathEval(ByVal expression As String)
        Try
            If expression = "everything" Then
                Return 42
                Exit Function
            End If
            Dim updatedExpression As String = Regex.Replace(expression, "(?<func>Sin|Cos|Tan)\((?<arg>.*?)\)", Function(match As Match) _
        If(match.Groups("func").Value = "Sin", Math.Sin(Int32.Parse(match.Groups("arg").Value)).ToString(), _
        If(match.Groups("func").Value = "Cos", Math.Cos(Int32.Parse(match.Groups("arg").Value)).ToString(), _
        Math.Tan(Int32.Parse(match.Groups("arg").Value)).ToString())) _
        )
            Dim result = New DataTable().Compute(updatedExpression, Nothing)
            Return result
        Catch ex As Exception
            Return "EXCEPTION: " & ex.Message
        End Try
    End Function

    Function rickroll(ByVal mail As String)
        Dim foo = Split(mail, " ")
        Dim bar = foo(foo.Length - 1)
        If bar = nick Then
            send("PRIVMSG " & channel & " :I'm not going to rickroll myself, thank you very much.")
            Exit Function
        End If
        Debug.WriteLine("---> Rickrolling " & channel)
        noticeperson(mail, "We're no strangers to love")
        noticeperson(mail, "You know the rules and so do I")
        noticeperson(mail, "A full commitment's what I'm thinking of")
        noticeperson(mail, "You wouldn't get this from any other guy")
        noticeperson(mail, "I just wanna tell you how I'm feeling")
        noticeperson(mail, "Gotta make you understand")
        noticeperson(mail, "Never gonna give you up")
        noticeperson(mail, "Never gonna let you down")
        noticeperson(mail, "Never gonna run around and desert you")
        noticeperson(mail, "Never gonna make you cry")
        noticeperson(mail, "Never gonna say goodbye")
        noticeperson(mail, "Never gonna tell a lie and hurt you")
        Return 0
    End Function
    Function sendhelp(ByVal mail As String)
        noticepersonwhosentthis(mail, "These are the commands I know:-")
        noticepersonwhosentthis(mail, "shutdown -h now (shuts me down)")
        noticepersonwhosentthis(mail, "shutdown -r now (restarts me)")
        noticepersonwhosentthis(mail, "--channel <name> (changes channel)")
        noticepersonwhosentthis(mail, "eval <expression> (evaluates a mathematical expression)")
        noticepersonwhosentthis(mail, "No spaces can be used in the expression, and Sin(), Cos() and Tan() can be used.")
        noticepersonwhosentthis(mail, "-F -A <name> (Annoys <name> with Friday by Rebecca Black)")
        noticepersonwhosentthis(mail, "-R -A <name> (Rickrolls <name>)")
        noticepersonwhosentthis(mail, "And various other conversational responses.")
        Return 0
    End Function

    Function disconnect()
        Debug.WriteLine("---> Disconnecting.")
        send("QUIT")
        sock.Disconnect(False)
    End Function
End Module
