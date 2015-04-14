Imports System
Imports System.Text
Imports System.Security.Cryptography
Imports System.Collections
Imports System.IO
Imports System.Net
Imports System.Reflection



Public Class RADIUSClient
    Private Const UDP_TTL As Integer = 10
    Private pSSecret As String = Nothing
    Private pUsername As String = Nothing
    Private pPassword As String = Nothing
    Private pServer As String = Nothing
    Private pRadiusPort As Integer = 1812
    Private pUDPTimeout As Integer = 5000
    Private pDebug As Boolean = False
    Private pRA As Byte() = New Byte(15) {}
    Private pClientIdentifier As Integer
    Private pEndpoint

    Public Sub New(server As String, port As Integer)
        pServer = Server
        pRadiusPort = port
    End Sub

    Public Property UDPTimeout() As Integer
        Get
            Return pUDPTimeout
        End Get
        Set(value As Integer)
            pUDPTimeout = value
        End Set
    End Property

    Public Property Debug() As Boolean
        Get
            Return pDebug
        End Get
        Set(value As Boolean)
            pDebug = value
        End Set
    End Property

    Public Function Authenticate(SharedSecret As String, Username As String, Password As String, pAttributes As RADIUSAttributes) As RADIUSPacket
        DebugOutput("Authenticate: SharedSecret: " & SharedSecret & " Username: " & Username & " Password: " & Password)

        Dim pRandonNumber As New Random()

        Dim pCode As Byte = RadiusPacketCode.AccessRequest
        GenerateRA()
        Dim pIdentifier As Byte = Convert.ToByte(pRandonNumber.[Next](0, 32000) Mod 256)

        Dim pUserAttribute As New RADIUSAttribute(RadiusAttributeType.UserName, Username)
        Dim pPassAttribute As New RADIUSAttribute(RadiusAttributeType.UserPassword, Crypto.GeneratePAP_PW(Password, SharedSecret, pRA))

        pAttributes.Add(pUserAttribute)
        pAttributes.Add(pPassAttribute)

        Dim request = New RADIUSPacket(pCode, pIdentifier, pAttributes, pRA)

        For i As Integer = 0 To 10
            Try
                Dim myRadiusClient As New Sockets.UdpClient()
                myRadiusClient.Client.SendTimeout = pUDPTimeout
                myRadiusClient.Client.ReceiveTimeout = pUDPTimeout

                myRadiusClient.Ttl = UDP_TTL
                myRadiusClient.Connect(pServer, pRadiusPort)

                Threading.Thread.Sleep(200)

                DebugOutput("Sending data to radius server" & Encoding.Default.GetString(request.Bytes))

                myRadiusClient.Send(request.Bytes, request.Bytes.Length)

                Threading.Thread.Sleep(200)

                Dim RemoteIpEndPoint As New IPEndPoint(IPAddress.Any, 0)

                Dim receiveBytes As [Byte]() = myRadiusClient.Receive(RemoteIpEndPoint)
                DebugOutput("Receive data to radius server" & Encoding.Default.GetString(receiveBytes))
                myRadiusClient.Close()

                Dim response = New RADIUSPacket(receiveBytes)
                Return response
            Catch e As Exception
                DebugOutput("Exception Error: " + e.ToString())
            End Try
        Next
        Return Nothing
    End Function

    Private Sub GenerateRA()
        Dim pRandonNumber As New Random()
        For i As Integer = 0 To 14
            pRA(i) = Convert.ToByte(1 + pRandonNumber.[Next]() Mod 255)
            pRandonNumber.[Next]()
        Next

    End Sub


    Private Sub DebugOutput(Output As String)
        If pDebug Then

            Dim path As String = (New System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath

            path = Replace(Replace(path.ToLower, "/bin/radiusclient.dll", "/log/"), "/", "\")
            '  Console.WriteLine("[DEBUG]  " + Output)
            Dim myDebugWriter As New StreamWriter(path & "Radius_Debug.txt", True)
            myDebugWriter.WriteLine(Now.ToString & ": " & "[DEBUG] " + Output)
            myDebugWriter.Close()
        End If

    End Sub




End Class



