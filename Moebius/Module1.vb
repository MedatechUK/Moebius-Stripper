Imports System.Net.Sockets
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading

Module Module1
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
    Sub Main()
        Dim ipHostInfo As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(ircserver)
        Dim EP As New System.Net.IPEndPoint(ipHostInfo.AddressList(0), port)
        sock = New System.Net.Sockets.Socket(EP.Address.AddressFamily, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
        sock.Connect(ircserver, port)
        sendConnectCommands()
        While sock.Connected = True
            Dim mail As String = recv()
            Debug.WriteLine(mail)
            If mail.EndsWith("Zola'u niprrte' ma " & nick & ", oel ngati kameie. Ngaru lu fpom srak?") Then
                Debug.WriteLine("---> Got greeting, responding.")
                send("PRIVMSG " & channel & " :Irayo ma Neytiri, oel ngati kameie. Oeru leiu fpom, ngari tut?")
            ElseIf mail.EndsWith(nick & ": shutdown -h now") Then
                Debug.WriteLine("---> Closing socket, shutting down.")
                send("QUIT")
                sock.Close()
            ElseIf mail.EndsWith(nick & ", shutdown -h now") Then
                send("QUIT")
                Debug.WriteLine("---> Closing socket, shutting down.")
                sock.Close()
            ElseIf mail.EndsWith(nick & ": shutdown -r now") Then
                Debug.WriteLine("---> Disconnecting and reconnecting.")
                send("QUIT")
                sock.Close()
                Thread.Sleep(50)
                Application.Restart()
            ElseIf mail.EndsWith(nick & ", shutdown -r now") Then
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
            ElseIf mail.Contains(nick & ": --channel") Then
                Dim foo = Split(mail, " ")
                Dim bar = foo(foo.Length - 1)
                send("PART " & channel)
                channel = bar
                send("JOIN " & channel)
            ElseIf mail.Contains(nick & ": --help") Then
                sendhelp(mail)
            ElseIf mail.Contains(nick & ", --help") Then
                sendhelp(mail)
            ElseIf mail.Contains(nick & ", eval") Then
                Dim foo = Split(mail, " ")
                Dim bar = foo(foo.Length - 1)
                Dim baz = mathEval(bar)
                Debug.WriteLine("---> Evaluating " & bar & " - result: " & baz)
                send("PRIVMSG " & channel & " :" & baz)
            ElseIf mail.Contains(nick & ": eval") Then
                Dim foo = Split(mail, " ")
                Dim bar = foo(foo.Length - 1)
                Dim baz = mathEval(bar)
                Debug.WriteLine("---> Evaluating " & bar & " - result: " & baz)
                send("PRIVMSG " & channel & " :" & baz)
            ElseIf mail.Contains(nick & ": -F -A") Then
                annoywithfriday(mail)
            ElseIf mail.Contains(nick & ", -F -A") Then
                annoywithfriday(mail)
            ElseIf mail.Contains(nick & ": -R -A") Then
                rickroll(mail)
            ElseIf mail.Contains(nick & ", -R -A") Then
                rickroll(mail)
            ElseIf mail.Contains("gm") Then
                send("PRIVMSG " & channel & " :Good morning, " & Split(Split(mail, " ")(1), ">")(0))
            ElseIf mail.Contains("Ninat") Or mail.Contains("ninat") Then
                send("PRIVMSG " & channel & " :" & Chr(1) & "ACTION slaps " & Split(Split(mail, " ")(1), ">")(0) & " around a bit with Ninat's queue" & Chr(1))
            ElseIf mail.Contains("Moebius reactor") Or mail.Contains("moebius reactor") Or mail.Contains("Moebius Reactor") Then
                send("PRIVMSG " & channel & " :" & Chr(1) & "ACTION adds 25 to the Ghost's starting energy" & Chr(1))
            ElseIf mail.EndsWith("!portal 1") Then
                send("PRIVMSG " & channel & " :You think you're doing some damage? Two plus two is... Ten. IN BASE FOUR! I'M FINE!")
            ElseIf mail.EndsWith("!portal 2") Then
                send("PRIVMSG " & channel & " :Maybe you should marry that thing since you love it so much. Do you want to marry it? WELL I WON'T LET YOU! How does that feel? ")
            ElseIf mail.EndsWith("!portal 3") Then
                send("PRIVMSG " & channel & " :There was even going to be a party for you. A big party that all your friends were invited to. I invited your best friend, the Companion Cube. Of course, he couldn't come because you murdered him. All your other friends couldn't come, either, because you don't have any other friends because of how unlikable you are. It says so right here in your personnel file: ")
                send("PRIVMSG " & channel & " :'Unlikable. Liked by no one. A bitter, unlikable loner, whose passing shall not be mourned. Shall NOT be mourned.' That's exactly what it says. Very formal. Very official. It also says you were adopted, so that's funny, too.")
            ElseIf mail.EndsWith("!portal 4") Then
                send("PRIVMSG " & channel & " :When I said 'deadly neurotoxin,' the 'deadly' was in massive sarcasm quotes. I could take a bath in this stuff. Put in on cereal, rub it right into my eyes. Honestly, it's not deadly at all... to *me.* You, on the other hand, are going to find its deadliness... a lot less funny. ")
            ElseIf mail.EndsWith("!portal 5") Then
                send("PRIVMSG " & channel & " :Oh. Hi. So. How are you holding up? BECAUSE I'M A POTATO! ")
            ElseIf mail.EndsWith("!portal 6") Then
                send("PRIVMSG " & channel & " :Well done. Here are the test results: You are a horrible person. I'm serious, that's what it says: A horrible person. We weren't even testing for that. Don't let that 'horrible person' thing discourage you. It's just a data point. If it makes you feel any better, science has now validated your birth mother's decision to abandon you on a doorstep.  ")
            ElseIf mail.EndsWith("!portal 7") Then
                send("PRIVMSG " & channel & " :I hope you brought something stronger than a portal gun this time. Otherwise, I'm afraid you're about to become the immediate past president of the Being Alive club. Ha ha. ")
            ElseIf mail.EndsWith("!portal 8") Then
                send("PRIVMSG " & channel & " :Look at you, soaring through the air like an eagle... piloting a blimp. ")
            ElseIf mail.EndsWith("!portal 9") Then
                send("PRIVMSG " & channel & " :You look ugly in that jumpsuit. That's not my opinion, it's right here on your fact sheet. They said on everyone else it looked fine but on you it looked hideous. But still what does an old engineer know about fashion. Oh, wait it's a she. Still, what does she know about, oh wait. She has a medical degree. In fashion. From France. ")
            ElseIf mail.EndsWith("!portal 10") Then
                send("PRIVMSG " & channel & " :Crushing's too good for him. First he'll spend a year in the incinerator. Year two: Cryogenic refrigeration wing. Then TEN years in the chamber I built where all the robots scream at you. THEN I'll kill him. ")
            ElseIf mail.EndsWith("!portal 11") Then
                send("PRIVMSG " & channel & " :They told me, if I ever turned this flashlight on, I would die. They told me that about EVERYTHING! I don't know why they even bothered to give me this stuff when they don't want me using it. It's pointless. Mad.")
            ElseIf mail.Contains("portal") Or mail.Contains("Portal") Then
                send("PRIVMSG " & channel & " :Oh, it's you. How have you been? I've been really busy being dead, you know, after you MURDERED me.")
            ElseIf mail.Contains(":akiwiguy") And mail.Contains("JOIN :" & channel) Then
                send("PRIVMSG " & channel & " :wb akiwiguy")
            ElseIf mail.Contains(":Adventus") And mail.Contains("JOIN :" & channel) Then
                send("PRIVMSG " & channel & " :" & Chr(1) & "ACTION slaps Adventus around a bit with some annoying intelligence-dampening sphere" & Chr(1))
            End If

        End While
    End Sub

    Public Sub sendConnectCommands()
        send("NICK " & nick)
        send("USER " & nick & " 0 * :akiwiguy")
        If identifywithnickserv = True Then
            send("PRIVMSG nickserv :identify " & nickservpass)
        End If
        send("MODE " & nick & " +B")
        send("JOIN " & channel)
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
        sock.Receive(data, 4096, SocketFlags.None)
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
        mail = mail.TrimEnd(Chr(0))
        mail = mail.Remove(mail.LastIndexOf(vbLf), 1)
        mail = mail.Remove(mail.LastIndexOf(vbCr), 1)
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

    Function annoywithfriday(ByVal mail As String)
        Dim foo = Split(mail, " ")
        Dim bar = foo(foo.Length - 1)
        If bar = nick Then
            send("PRIVMSG " & channel & " :I'm not going to kill myself, thank you very much.")
            Exit Function
        End If
        Debug.WriteLine("---> Annoying " & bar & " with Friday by Rebecca Black.")
        noticeperson(mail, "It's Friday, Friday")
        noticeperson(mail, "Gotta get down on Friday")
        noticeperson(mail, "Everybody's lookin' forward to the weekend, weekend")
        noticeperson(mail, "Friday, Friday")
        noticeperson(mail, "Gettin' down on Friday")
        noticeperson(mail, "Everybody's lookin' forward to the weekend")
        noticeperson(mail, "Partyin', partyin' Yeah!")
        noticeperson(mail, "Partyin', partyin' Yeah!")
        noticeperson(mail, "Fun, fun, fun, fun")
        noticeperson(mail, "Lookin' forward to the weekend")
        Return 0
    End Function
    Function rickroll(ByVal mail As String)
        Dim foo = Split(mail, " ")
        Dim bar = foo(foo.Length - 1)
        If bar = nick Then
            send("PRIVMSG " & channel & " :I'm not going to rickroll myself, thank you very much.")
            Exit Function
        End If
        Debug.WriteLine("---> Rickrolling " & bar)
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
