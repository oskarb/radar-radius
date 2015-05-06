Imports System
Imports System.Text
Imports System.Security.Cryptography
Imports System.Collections
Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.IO.Path
Imports System.IO.Directory

Public Class RADIUSClient

    Private Const UDP_TTL As Integer = 10

    Private pSharedSecret As String

    Private pUsername As String = Nothing
    Private pPassword As String = Nothing
    Private pServer As String = Nothing
    Private pRadiusPort As Integer = 1812
    Private pUDPTimeout As Integer = 5000
    Private pDebug As Boolean = True
    Private pRA As Byte() = New Byte(15) {}
    Private pClientIdentifier As Integer
    Private pEndpoint

    Public Sub New(server As String, port As Integer, sharedSecret As String)
        pSharedSecret = sharedSecret
        pServer = server
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

    Public Function Authenticate(Username As String, Password As String, pAttributes As RADIUSAttributes) As RADIUSPacket
        DebugOutput("AccessRequest: " & " Username: " & Username)

        For Each a In pAttributes
            DebugOutput("Type: " & a.Type.ToString & " Value: " & a.Value.Length)
        Next

        Dim pRandonNumber As New Random()

        Dim pCode As Byte = RadiusPacketCode.AccessRequest
        GenerateRA()
        Dim pIdentifier As Byte = Convert.ToByte(pRandonNumber.[Next](0, 32000) Mod 256)

        Dim pUserAttribute As New RADIUSAttribute(RadiusAttributeType.UserName, Username)
        Dim pPassAttribute As New RADIUSAttribute(RadiusAttributeType.UserPassword, Crypto.GeneratePAP_PW(Password, pSharedSecret, pRA))

        pAttributes.Add(pUserAttribute)
        pAttributes.Add(pPassAttribute)

        Dim request = New RADIUSPacket(pCode, pIdentifier, pAttributes, pRA)
        Dim udpClient As New Sockets.UdpClient()
        udpClient.Client.SendTimeout = pUDPTimeout
        udpClient.Client.ReceiveTimeout = pUDPTimeout

        udpClient.Ttl = UDP_TTL
        udpClient.Connect(pServer, pRadiusPort)
        udpClient.Send(request.Bytes, request.Bytes.Length)

        For i As Integer = 0 To 10
            Threading.Thread.Sleep(20)
            Try
                Dim RemoteIpEndPoint As New IPEndPoint(IPAddress.Any, 0)
                Dim receiveBytes As [Byte]() = udpClient.Receive(RemoteIpEndPoint)
                Dim response = New RADIUSPacket(receiveBytes)
                DebugOutput("Response:" & response.Code.ToString)
                Return response
            Catch e As Exception
                DebugOutput("Exception Error: " + e.ToString())
            Finally
                udpClient.Close()
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
            Dim assemblyPath As String = (New System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath
            Dim logPath As String = GetParent(GetDirectoryName(assemblyPath)).FullName & "\log\radius_client.txt"
            Dim log As New StreamWriter(logPath, True)
            log.WriteLine(Now.ToString & ": " & "[DEBUG] " + Output)
            log.Close()
        End If

    End Sub

End Class



