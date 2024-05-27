@echo on
SC QUERY NFIMPACTO-ENVIO > NUL
IF ERRORLEVEL 1060 GOTO MISSING
	ECHO SERVICE ALREADY EXISTS
	GOTO END

:MISSING
	ECHO SERVICE IS MISSING
    sc create "NFIMPACTO-ENVIO" binPath= "D:\Servicos\NfImpacto.Envio\NfImpacto.Envio.exe" displayname= "Servico NF IMPACTO ENVIO" start= auto
    sc description "NFIMPACTO-ENVIO" "Servico NF IMPACTO ENVIO"
:END