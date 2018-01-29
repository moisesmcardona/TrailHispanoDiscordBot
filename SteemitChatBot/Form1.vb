Imports System.IO
Imports System.Text
Imports DSharpPlus
Imports DSharpPlus.Entities
Imports MySql.Data.MySqlClient

Public Class Form1
    Private WithEvents DiscordClient As DiscordClient
    Private DiscordChannelObject As DiscordChannel
    Private DiscordRunning As Boolean = False
    Private SteemitRunning As Boolean = False
    Private WithEvents DiscordClientLogger As DebugLogger
    Dim MySQLString As String = String.Empty
    Private Async Sub Button1_Click(sender As Object, e As System.EventArgs) Handles Button1.Click
        Dim MySQLFile As StreamReader = New StreamReader("MySQLConfig.txt")
        Dim currentline As String = ""
        Dim MySQLServer As String = ""
        Dim MySQLUser As String = ""
        Dim MySQLPassword As String = ""
        Dim MySQLDatabase As String = ""
        While MySQLFile.EndOfStream = False
            currentline = MySQLFile.ReadLine
            If currentline.Contains("server") Then
                Dim GetServer As String() = currentline.Split("=")
                MySQLServer = GetServer(1)
            ElseIf currentline.Contains("username") Then
                Dim GetUsername As String() = currentline.Split("=")
                MySQLUser = GetUsername(1)
            ElseIf currentline.Contains("password") Then
                Dim GetPassword As String() = currentline.Split("=")
                MySQLPassword = GetPassword(1)
            ElseIf currentline.Contains("database") Then
                Dim GetDatabase As String() = currentline.Split("=")
                MySQLDatabase = GetDatabase(1)
            End If
        End While
        MySQLString = "server=" & MySQLServer & ";user=" & MySQLUser & ";database=" & MySQLDatabase & ";port=3306;password=" & MySQLPassword & ";"
        Button1.Text = "Started Bot"
        Await StartAsync()
    End Sub
    Public Async Function StartAsync() As Task
        Dim dcfg As New DiscordConfiguration
        With dcfg
            .Token = My.Computer.FileSystem.ReadAllText("token.txt")
            .TokenType = TokenType.Bot
            .LogLevel = LogLevel.Debug
            .AutoReconnect = True
        End With
        Me.DiscordClient = New DiscordClient(dcfg)
        Me.DiscordClientLogger = Me.DiscordClient.DebugLogger
        Await Me.DiscordClient.ConnectAsync()
        Await Task.Delay(-1)
    End Function
    Private Function FindUserInFile(user As String) As String
        Dim userInFile As String = String.Empty
        Dim userFile As StreamReader = New StreamReader("users.txt")
        Dim currentUserLine As String = ""
        While userFile.EndOfStream = False
            currentUserLine = userFile.ReadLine
            If currentUserLine.Contains(user) Then
                Dim GetUser As String() = currentUserLine.Split("=")
                userInFile = GetUser(1)
                Exit While
            End If
        End While
        userFile.Close()
        If String.IsNullOrEmpty(userInFile) Then
            userInFile = user
        End If
        Return userInFile
    End Function
    Private Function CheckUserIsInSteemPlace(User As String) As Boolean
        Dim SQLQuery As String = "SELECT DISTINCT * FROM users2 WHERE username='" & User & "' AND approved=1"
        Dim Connection As MySqlConnection = New MySqlConnection(MySQLString)
        Dim Command As New MySqlCommand(SQLQuery, Connection)
        Connection.Open()
        Dim reader As MySqlDataReader = Command.ExecuteReader
        Dim UserFound As Boolean = False
        If reader.HasRows = True Then
            UserFound = True
        Else
            UserFound = False
        End If
        Connection.Close()
        Return UserFound
    End Function

    Private Function CheckUserFollowsTrailhispano(User As String) As Boolean
        Dim SQLQuery As String = "SELECT DISTINCT * FROM followtrail WHERE user='" & User & "' AND account='trailhispano'"
        Dim Connection As MySqlConnection = New MySqlConnection(MySQLString)
        Dim Command As New MySqlCommand(SQLQuery, Connection)
        Connection.Open()
        Dim reader As MySqlDataReader = Command.ExecuteReader
        Dim UserFollowsTrail As Boolean = False
        If reader.HasRows = True Then
            UserFollowsTrail = True
        Else
            UserFollowsTrail = False
        End If
        Connection.Close()
        Return UserFollowsTrail
    End Function

    Private Function CheckUserHasTrailHispanoEnabled(User As String) As Boolean
        Dim SQLQuery As String = "SELECT DISTINCT enabled FROM followtrail WHERE user='" & User & "' AND account='trailhispano'"
        Dim Connection As MySqlConnection = New MySqlConnection(MySQLString)
        Dim Command As New MySqlCommand(SQLQuery, Connection)
        Connection.Open()
        Dim reader As MySqlDataReader = Command.ExecuteReader
        Dim UserHasTrailEnabled As Boolean = False
        If reader.HasRows = True Then
            reader.Read()
            If reader("enabled") = 1 Then
                UserHasTrailEnabled = True
            Else
                UserHasTrailEnabled = False
            End If
        End If
        Connection.Close()
        Return UserHasTrailEnabled
    End Function

    Private Function ActivateTrail(User As String) As Boolean
        Dim SQLQuery As String = "UPDATE followtrail SET enabled=1 WHERE user='" & User & "' AND account='trailhispano'"
        Dim Connection As MySqlConnection = New MySqlConnection(MySQLString)
        Dim Command As New MySqlCommand(SQLQuery, Connection)
        Connection.Open()
        Command.ExecuteNonQuery()
        Connection.Close()
        Return True
    End Function
    Private Function DeactivateTrail(User As String) As Boolean
        Dim SQLQuery As String = "UPDATE followtrail SET enabled=0 WHERE user='" & User & "' AND account='trailhispano'"
        Dim Connection As MySqlConnection = New MySqlConnection(MySQLString)
        Dim Command As New MySqlCommand(SQLQuery, Connection)
        Connection.Open()
        Command.ExecuteNonQuery()
        Connection.Close()
        Return True
    End Function

    Private Function UpdatePercent(User As String, Percent As Double) As Boolean
        Dim SQLQuery As String = "UPDATE followtrail SET percent='" & Percent & "' WHERE user='" & User & "' AND account='trailhispano'"
        Dim Connection As MySqlConnection = New MySqlConnection(MySQLString)
        Dim Command As New MySqlCommand(SQLQuery, Connection)
        Connection.Open()
        Command.ExecuteNonQuery()
        Connection.Close()
        Return True
    End Function
    Private Function GetUserDrupalKey(User As String) As Integer
        Dim SQLQuery As String = "SELECT DISTINCT drupalkey FROM users2 WHERE username='" & User & "' AND approved=1"
        Dim Connection As MySqlConnection = New MySqlConnection(MySQLString)
        Dim Command As New MySqlCommand(SQLQuery, Connection)
        Connection.Open()
        Dim reader As MySqlDataReader = Command.ExecuteReader
        Dim DrupalKey As Integer = 0
        reader.Read()
        DrupalKey = reader("drupalkey")
        Connection.Close()
        Return DrupalKey
    End Function
    Private Function InsertAndActivateTrail(User As String) As Boolean
        Dim DrupalKey = GetUserDrupalKey(User)
        Dim SQLQuery As String = "INSERT INTO followtrail (drupalkey, user, account, percent, enabled) VALUES (" & DrupalKey & ", '" & User & "', 'trailhispano', '10', 1)"
        Dim Connection As MySqlConnection = New MySqlConnection(MySQLString)
        Dim Command As New MySqlCommand(SQLQuery, Connection)
        Connection.Open()
        Command.ExecuteNonQuery()
        Connection.Close()
        Return True
    End Function

    Private Function InsertAndDeactivateTrail(User As String) As Boolean
        Dim DrupalKey = GetUserDrupalKey(User)
        Dim SQLQuery As String = "INSERT INTO followtrail (drupalkey, user, account, percent, enabled) VALUES (" & DrupalKey & ", '" & User & "', 'trailhispano', '10', 0)"
        Dim Connection As MySqlConnection = New MySqlConnection(MySQLString)
        Dim Command As New MySqlCommand(SQLQuery, Connection)
        Connection.Open()
        Command.ExecuteNonQuery()
        Connection.Close()
        Return True
    End Function

    Private Function CheckIfUserSubmittedPostToday(User As String) As Boolean
        Dim SQLQuery As String = "SELECT DISTINCT * FROM trailhispanopostcount WHERE user='" & User & "' AND date='" & Now.ToString("MM/dd/yyyy") & "'"
        Dim Connection As MySqlConnection = New MySqlConnection(MySQLString)
        Dim Command As New MySqlCommand(SQLQuery, Connection)
        Connection.Open()
        Dim Reader As MySqlDataReader = Command.ExecuteReader
        Dim UserAlreadyPostedToday As Boolean = False
        If Reader.HasRows Then
            UserAlreadyPostedToday = True
        Else
            UserAlreadyPostedToday = False
        End If
        Return UserAlreadyPostedToday
    End Function

    Private Function GetPostStatus(PostIdentifier As String) As Integer
        Dim SQLQuery As String = "SELECT DISTINCT * FROM posts WHERE link='" & PostIdentifier & "'"
        Dim Connection As MySqlConnection = New MySqlConnection(MySQLString)
        Dim Command3 As New MySqlCommand(SQLQuery, Connection)
        Connection.Open()
        Dim Reader As MySqlDataReader = Command3.ExecuteReader
        Dim Status As Integer = -1
        If Reader.HasRows Then
            Reader.Read()
            Status = Reader("voted")
        End If
        Return Status
    End Function

    Private Sub AddUserToAlreadyPostedToday(User As String)
        Dim SQLQuery As String = "INSERT INTO trailhispanopostcount (user, postcount, date) VALUES ('" & User & "', 1, '" & Now.ToString("MM/dd/yyyy") & "')"
        Dim Connection As MySqlConnection = New MySqlConnection(MySQLString)
        Dim Command As New MySqlCommand(SQLQuery, Connection)
        Connection.Open()
        Command.ExecuteNonQuery()
        Connection.Close()
    End Sub

    Private Function InsertPostToTrailTable(User As String, PostIdentifier As String, UserDiscordID As ULong)
        Dim SQLQuery2 As String = "INSERT INTO posts (username, link, voted, hastrailtag, userid) VALUES('" & User & "', '" & PostIdentifier & "', 0, 0, '" & UserDiscordID & "')"
        Dim Connection2 As MySqlConnection = New MySqlConnection(MySQLString)
        Dim Command2 As New MySqlCommand(SQLQuery2, Connection2)
        Connection2.Open()
        Command2.ExecuteNonQuery()
        Connection2.Close()
        Return True
    End Function

    Private Function GetPostIdentifier(Message As String) As String
        Dim Split2 As String() = Message.ToLower.Split("/")
        Dim PostIdentifier As String = ""
        Dim i = 0
        For Each Word In Split2
            If Word.Contains("@") Then
                PostIdentifier = Word.Remove(0, 1) & "/" & Split2(i + 1)
                Exit For
            End If
            i = i + 1
        Next
        Return PostIdentifier
    End Function
    Private Function GetPostTags(PostIdentifier As String) As String
        Dim Request As System.Net.WebRequest = System.Net.WebRequest.Create("https://api.steem.place/getPostTags/?p=" & PostIdentifier)
        Dim Response As System.Net.WebResponse = Request.GetResponse()
        Dim ResponseStream As Stream = Response.GetResponseStream()
        Dim Encode As Encoding = System.Text.Encoding.GetEncoding("utf-8")
        Dim ResponseStreamReader As New StreamReader(ResponseStream, Encode)
        Return ResponseStreamReader.ReadLine
    End Function

    Private Function GetWitnessVotes(User As String) As String
        Dim Request As System.Net.WebRequest = System.Net.WebRequest.Create("https://api.steem.place/getWitnessVotes/?a=" & User)
        Dim Response As System.Net.WebResponse = Request.GetResponse()
        Dim ResponseStream As Stream = Response.GetResponseStream()
        Dim Encode As Encoding = System.Text.Encoding.GetEncoding("utf-8")
        Dim ResponseStreamReader As New StreamReader(ResponseStream, Encode)
        Return ResponseStreamReader.ReadLine
    End Function
    Private Async Function OnMessage(ByVal e As EventArgs.MessageCreateEventArgs) As Task Handles DiscordClient.MessageCreated
        Dim User As String = FindUserInFile(e.Message.Author.Username)
        User = User.ToLower
        If e.Message.Author.Username.ToLower = "trailhispano" = False Then
            Try
                '#comandos-trail
                If e.Channel.Id = 368560364222152714 Then
                    If e.Message.Content.ToLower.Contains("!activar") Then
                        If CheckUserIsInSteemPlace(User) Then
                            If CheckUserFollowsTrailhispano(User) Then
                                If ActivateTrail(User) Then
                                    Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", tu participación en el Trail ha sido activada.")
                                End If
                            Else
                                If InsertAndActivateTrail(User) Then
                                    Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", Trail Hispano ha sido activado con un porcentaje de 10%. Utiliza el comando !porciento seguido con el porciento para ajustar el porciento de tu participación. Alternativamente, puedes acceder a tu cuenta de https://steem.place y ajustar el porciento allá.")
                                End If
                            End If
                        Else
                            Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", no se puede activar tu participación en el trail porque no tienes una cuenta en Steem.Place o no la has configurado. Debes registrarte y/o configurar tu cuenta en https://steem.place. Si tienes una cuenta allá, es posible que tu nombre de Discord no sea el mismo que el de Steem. Por favor, notifica en el canal de #ayuda que cambiaste tu nombre en Discord para solucionar este problema.")
                        End If
                    ElseIf e.Message.Content.ToLower.Contains("!desactivar") Then
                        If CheckUserIsInSteemPlace(User) Then
                            If CheckUserFollowsTrailhispano(User) Then
                                If DeactivateTrail(User) Then
                                    Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", tu participación en el Trail ha sido desactivada.")
                                End If
                            Else
                                If InsertAndDeactivateTrail(User) Then
                                    Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", Trail Hispano ha sido añadido a tus trails, desactivado con un porcentaje de 10%. Utiliza el comando !porciento seguido con el porciento para ajustar el porciento de tu participación. Alternativamente, puedes acceder a tu cuenta de https://steem.place y ajustar el porciento allá.")
                                End If
                            End If
                        Else
                            Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", no se puede desactivar tu participación en el trail porque no tienes una cuenta en Steem.Place o no la has configurado. Debes registrarte y/o configurar tu cuenta en https://steem.place. Si tienes una cuenta allá, es posible que tu nombre de Discord no sea el mismo que el de Steem. Por favor, notifica en el canal de #ayuda que cambiaste tu nombre en Discord para solucionar este problema.")
                        End If
                    ElseIf e.Message.Content.ToLower.Contains("!porciento") Then
                        Dim SplitWords As String() = e.Message.Content.Split(" ")
                        If SplitWords.Count >= 2 Then
                            If CheckUserIsInSteemPlace(User) Then
                                If CheckUserFollowsTrailhispano(User) Then
                                    Dim Percent As Double = SplitWords(1)
                                    If UpdatePercent(User, Percent) Then
                                        Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", el porciento de tu participación ha sido actualizado a " & Percent & "%.")
                                    End If
                                Else
                                    Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", no se puede actualizar el porciento de tu participacion en el trail porque no participas en el mismo. Utiliza !activar para activar tu participación y luego usa !porciento (El porciento) para cambiar tu porcentaje.")
                                End If
                            Else
                                Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", no se puede actualizar el porciento de tu participación en el trail porque no tienes una cuenta en Steem.Place o no la has configurado. Debes registrarte y/o configurar tu cuenta en https://steem.place. Si tienes una cuenta allá, es posible que tu nombre de Discord no sea el mismo que el de Steem. Por favor, notifica en el canal de #ayuda que cambiaste tu nombre en Discord para solucionar este problema.")
                            End If
                        Else
                            Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", debes escribir un porciento (sin el símbolo %).")
                        End If
                    End If
                    '#trail
                ElseIf e.Channel.Id = 405907266663874562 Then
                    If e.Message.Content.ToLower.Contains("steemit.com/") Or e.Message.Content.ToLower.Contains("busy.org") Then
                        If e.Message.Content.ToLower.Contains("/@" & User & "/") Then
                            If CheckUserIsInSteemPlace(User) Then
                                If CheckUserFollowsTrailhispano(User) Then
                                    If CheckUserHasTrailHispanoEnabled(User) Then
                                        If CheckIfUserSubmittedPostToday(User) = False Then
                                            Dim PostIdentifier As String = GetPostIdentifier(e.Message.Content.ToLower)
                                            Dim PostTags As String = GetPostTags(PostIdentifier)
                                            Dim PostStatus = GetPostStatus(PostIdentifier)
                                            If PostStatus = -1 Then
                                                If PostTags.Contains("castellano") Then
                                                    If InsertPostToTrailTable(User, PostIdentifier, e.Message.Author.Id) Then
                                                        Dim WitnessVotes As String = GetWitnessVotes(User)
                                                        Dim WitnessMessage As String = ""
                                                        If WitnessVotes.Contains("moisesmcardona") = False Then
                                                            WitnessMessage = "Todavía no has votado a @moisesmcardona como Witness. Considera votándolo como Witness en https://steemit.com/~witnesses o usando el siguiente enlance: https://v2.steemconnect.com/sign/account-witness-vote?witness=castellano&approve=1" & Environment.NewLine
                                                        End If
                                                        If WitnessVotes.Contains("castellano") = False Then
                                                            WitnessMessage = WitnessMessage & " Todavía no has votado a @castellano como Witness. Considera votándolo como Witness en https://steemit.com/~witnesses o usando el siguiente enlance: https://v2.steemconnect.com/sign/account-witness-vote?witness=castellano&approve=1"
                                                        End If
                                                        Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", tu post será considerado para ser votado por el Trail :slight_smile:" & Environment.NewLine)
                                                    End If
                                                Else
                                                    Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", Este post no es elegible para ser votado por el trail. El post debe tener la etiqueta #castellano. NOTA: Si el post está en otro idioma que no es español/castellano y usas la etiqueta #castellano, tu post podría recibir una banderita, también conocido como un ""flag"" o ""downvote"".")
                                                End If
                                            ElseIf PostStatus = 0 Then
                                                Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", Tu post se encuentra en la lista de posts a ser considerados para ser votados por el Trail.")
                                            ElseIf PostStatus = 1 Then
                                                Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", El Trail ya votó en ese post.")
                                            ElseIf PostStatus = 2 Then
                                                Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", Tu post no fue votado por el Trail.")
                                            End If
                                        Else
                                            Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", No se puede considerar tu post para ser votado por el Trail porque ya has puesto un post en este canal en el día de hoy.")
                                        End If
                                    Else
                                        Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", No se puede considerar tu post para ser votado por el Trail porque tienes desactivado la opción de participar en el trail. Utiliza el comando !activar en el canal #comandos-trail")
                                    End If
                                Else
                                    Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", No se puede considerar tu post para ser votado por el Trail porque no perteneces al trail. Utiliza el comando !activar en el canal #comandos-trail")
                                End If
                            Else
                                Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", no se puede considerar tu post en el trail porque no tienes una cuenta en Steem.Place o no la has configurado. Debes registrarte y/o configurar tu cuenta en https://steem.place. Si tienes una cuenta allá, es posible que tu nombre de Discord no sea el mismo que el de Steem. Por favor, notifica en el canal de #ayuda que cambiaste tu nombre en Discord para solucionar este problema.")
                            End If
                        Else
                            Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", El nombre de usuario del post es diferente al del chat. ¿Es esta tu cuenta? Tendrás que cambiar tu nombre de usuario en el chat para que se pueda votar en tu post. ¿Cambiaste tu nombre y todavía sigues viendo este mensaje? Manda un mensaje en el canal #ayuda para notificar que cambiaste tu nombre.")
                        End If
                    Else
                        Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", El mensaje escrito no es un post. En este canal sólamente se aceptan posts de Steemit.com o Busy.org.")
                    End If
                    '#verificar-post
                ElseIf e.Channel.Id = 407348378570194947 Then
                    If e.Message.Content.ToLower.Contains("steemit.com/") Or e.Message.Content.ToLower.Contains("busy.org/") Then
                        If e.Message.Content.ToLower.Contains("/@" & User & "/") Then
                            Dim PostIdentifier As String = GetPostIdentifier(e.Message.Content.ToLower)
                            Dim PostStatus As Integer = GetPostStatus(PostIdentifier)
                            If PostStatus = 0 Then
                                Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", Todavía el Trail no ha votado en tu post. Por favor, intenta más tarde.")
                            ElseIf PostStatus = 1 Then
                                Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", El Trail ya ha votado en tu post.")
                            ElseIf PostStatus = 2 Then
                                Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", Tu post no fue votado por el Trail.")
                            Else
                                Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", Ese post no ha sido considerado por el Trail. ¿Quieres que el trail lo considere? Ponlo en el canal #trail y tu post será considerado para ser votado.")
                            End If
                        Else
                            Await e.Channel.SendMessageAsync(e.Message.Author.Mention & ", El nombre de usuario del post es diferente al del chat. ¿Es esta tu cuenta? Tendrás que cambiar tu nombre de usuario en el chat para que se pueda votar en tu post. ¿Cambiaste tu nombre y todavía sigues viendo este mensaje? Manda un mensaje en el canal #ayuda para notificar que cambiaste tu nombre.")
                        End If
                    End If
                End If
            Catch ex As Exception
            End Try
        End If
    End Function
End Class
