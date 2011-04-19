servermanagercmd.exe -install SMTP-Server
%systemroot%\system32\inetsrv\appcmd.exe set config /commit:WEBROOT /section:smtp /deliveryMethod:Network /network.port:25 /network.host:localhost
cscript IPGrantRelay.vbs
