Dim objSMTP,objRelayIpList, IPList(0)
Set objSMTP = GetObject("IIS://localhost/smtpsvc/1")
Set objRelayIpList = objSMTP.Get("RelayIpList")

IPList(0) = "127.0.0.1"

objRelayIpList.GrantByDefault = false
objRelayIpList.IPGrant = IPList
objSMTP.Put "RelayIpList",objRelayIpList
objSMTP.SetInfo
