;------------------------------------------------------------------------------;
;   Installer Script for Batch Processor                                       ;
;                                                                              ;
;   Author John Karvounis                                                      ;
;                                                                              ;                                        ;
;------------------------------------------------------------------------------;

;------------------------------------------------------------------------------;
; General Configuration                                                        ;
;------------------------------------------------------------------------------;

!define PRODUCT_NAME_DISPLAY        "Batch Processor"
!define PRODUCT_NAME                "Batch Processor"
!define PRODUCT_PUBLISHER_DISPLAY   "John Karvounis"
!define PRODUCT_PUBLISHER           "Karvounis"
!define PRODUCT_WEB_SITE            "https://github.com/jkarvounis/BatchProcessor"
!define PRODUCT_DISPLAY_VERSION		"1.0.1.0"

!define PRODUCT_UNINST_ROOT_KEY     "HKLM"
!define PRODUCT_UNINST_KEY          "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"

!define HIGH_DPI_ROOT_KEY           "HKLM"

!define PRODUCT_EXE                 "BatchProcessorUI.exe"

;------------------------------------------------------------------------------;
; Compression                                                                  ;
;------------------------------------------------------------------------------;

SetCompressor /SOLID lzma

;------------------------------------------------------------------------------;
; Interface Settings                                                           ;
;------------------------------------------------------------------------------;

!include "MUI2.nsh"
!include "WinVer.nsh"

!define MUI_ABORTWARNING
!define MUI_ICON                                    "resources\logo.ico"
!define MUI_UNICON                                  "resources\logo.ico"
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP                      "resources\logo.bmp"
!define MUI_HEADERIMAGE_BITMAP_NOSTRETCH
!define MUI_HEADERIMAGE_LEFT
!define MUI_HEADER_TRANSPARENT_TEXT
!define MUI_WELCOMEFINISHPAGE_BITMAP                "resources\logo.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP_NOSTRETCH

;------------------------------------------------------------------------------;
; Pages                                                                        ;
;------------------------------------------------------------------------------;

!insertmacro MUI_PAGE_INSTFILES

!define MUI_FINISHPAGE_RUN                          "$INSTDIR\${PRODUCT_EXE}"
!define MUI_FINISHPAGE_RUN_TEXT                     "Start ${PRODUCT_NAME}"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE                           "English"

;------------------------------------------------------------------------------;
; Installer Settings                                                           ;
;------------------------------------------------------------------------------;

Name                        "${PRODUCT_NAME_DISPLAY}"
OutFile                     "${PRODUCT_NAME} Setup.exe"
InstallDir                  "$PROGRAMFILES\${PRODUCT_PUBLISHER}\${PRODUCT_NAME}"
ShowInstDetails             nevershow
ShowUnInstDetails           nevershow
BrandingText                "${PRODUCT_PUBLISHER_DISPLAY}"
RequestExecutionLevel       admin                               ; Request application privileges for Windows Vista, 7, 8

;------------------------------------------------------------------------------;
; Macro - Check Program Installed                                              ;
;------------------------------------------------------------------------------;

!macro MacroCheckProgramInstalled

    Push $R0

    ReadRegStr $R0 ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString"
    StrCmp $R0 "" done

    MessageBox MB_OKCANCEL|MB_ICONEXCLAMATION "${PRODUCT_NAME_DISPLAY} is already installed. $\n$\nClick 'OK' to remove the previous version or 'Cancel' to cancel this upgrade." IDOK uninst
    Abort

    uninst:

    ClearErrors
    ExecWait '$R0 _?=$INSTDIR' ;Do not copy the uninstaller to a temp file

    done:

    Pop $R0

!macroend

;------------------------------------------------------------------------------;
; Installer Functions                                                          ;
;------------------------------------------------------------------------------;

Function .onInit

    ${IfNot} ${AtLeastWin7}

        MessageBox MB_ICONSTOP "${PRODUCT_NAME_DISPLAY} requires Windows 7 or later."
        Abort

    ${Endif}

    !insertmacro MacroCheckProgramInstalled

FunctionEnd

;------------------------------------------------------------------------------;
; Installer Section                                                            ;
;------------------------------------------------------------------------------;

Section "MainSection" MainSection

    SetOverwrite on

    SetOutPath "$INSTDIR"

    !define BinDirectory  "..\BatchProcessorUI\bin\Release\"

    File "${BinDirectory}\BatchProcessor.exe"
    File "${BinDirectory}\BatchProcessorAPI.dll"
    File "${BinDirectory}\BatchProcessorUI.exe"
    File "${BinDirectory}\Newtonsoft.Json.Bson.dll"
    File "${BinDirectory}\Newtonsoft.Json.dll"
    File "${BinDirectory}\System.Data.Common.dll"
    File "${BinDirectory}\System.Diagnostics.StackTrace.dll"
    File "${BinDirectory}\System.Diagnostics.Tracing.dll"
    File "${BinDirectory}\System.Globalization.Extensions.dll"
    File "${BinDirectory}\System.IO.Compression.dll"
    File "${BinDirectory}\System.Net.Http.dll"
    File "${BinDirectory}\System.Net.Sockets.dll"
	File "${BinDirectory}\System.Runtime.Serialization.Primitives.dll"
    File "${BinDirectory}\System.Security.Cryptography.Algorithms.dll"
    File "${BinDirectory}\System.Security.SecureString.dll"
    File "${BinDirectory}\System.Threading.Overlapped.dll"
	File "${BinDirectory}\System.Xml.XPath.XDocument.dll"
    File "${BinDirectory}\Topshelf.dll"	

    ; Create Uninstaller
    WriteUninstaller "$INSTDIR\Uninstall ${PRODUCT_NAME}.exe"
	
	; Add firewall rule
	nsExec::Exec '"netsh" advfirewall firewall add rule name="${PRODUCT_NAME}" dir=in action=allow program="$INSTDIR\BatchProcessor.exe"'        

    WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString"    "$INSTDIR\Uninstall ${PRODUCT_NAME}.exe"
    WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName"        "${PRODUCT_NAME_DISPLAY}"
    WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon"        "$INSTDIR\Uninstall ${PRODUCT_NAME}.exe"
    WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion"     "${PRODUCT_DISPLAY_VERSION}"
    WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout"       "${PRODUCT_WEB_SITE}"
    WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher"          "${PRODUCT_PUBLISHER_DISPLAY}"

    ;Add Program Files Group
    CreateDirectory "$STARTMENU\Programs\${PRODUCT_PUBLISHER}\${PRODUCT_NAME}"
    CreateShortCut  "$STARTMENU\Programs\${PRODUCT_PUBLISHER}\${PRODUCT_NAME}\${PRODUCT_NAME}.lnk"              		  "$INSTDIR\${PRODUCT_EXE}"
	CreateShortCut  "$STARTMENU\Programs\${PRODUCT_PUBLISHER}\${PRODUCT_NAME}\Uninstall ${PRODUCT_NAME}.lnk"              "$INSTDIR\Uninstall ${PRODUCT_NAME}.exe"

SectionEnd

;------------------------------------------------------------------------------;
; Uninstaller Functions                                                        ;
;------------------------------------------------------------------------------;

Function un.onInit

FunctionEnd

;------------------------------------------------------------------------------;
; Uninstaller Section                                                          ;
;------------------------------------------------------------------------------;

Section "Uninstall"

	; Remove the service if possible
	nsExec::Exec "$INSTDIR\BatchProcessor.exe stop"
	nsExec::Exec "$INSTDIR\BatchProcessor.exe uninstall"
	
	; Remove firewall rule
	nsExec::Exec '"netsh" advfirewall firewall delete  rule name="${PRODUCT_NAME}"'  

    ; Remove Program Files Group
    RMDir /R "$STARTMENU\Programs\${PRODUCT_PUBLISHER}\${PRODUCT_NAME}"

    ; Remove Installation Directory
    RMDir /R "$INSTDIR"

    ; Remove from 'Installed Programs List'
    DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"

SectionEnd
