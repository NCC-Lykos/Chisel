; Chisel NSIS Installer
; ---------------------

; Installer Info
Name "Chisel"
OutFile "Chisel.Editor.{version}.exe"
InstallDir "$PROGRAMFILES\Chisel Editor"
InstallDirRegKey HKLM "Software\Chisel\Editor" "InstallDir"
RequestExecutionLevel admin

; Version Info
VIProductVersion "{version}"
VIAddVersionKey "FileVersion" "{version}"
VIAddVersionKey "ProductName" "Chisel Editor"
VIAddVersionKey "FileDescription" "Installer for Chisel Editor"
VIAddVersionKey "LegalCopyright" "http://github.com/NCC-Lykos/Chisel 2018"

; Ensure Admin Rights
!include LogicLib.nsh

Function .onInit
    UserInfo::GetAccountType
    pop $0
    ${If} $0 != "admin" ;Require admin rights on NT4+
        MessageBox mb_iconstop "Administrator rights required!" /SD IDOK
        SetErrorLevel 740 ;ERROR_ELEVATION_REQUIRED
        Quit
    ${EndIf}
FunctionEnd

; Installer Pages

Page components
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

; Installer Sections

Section "Chisel Editor"
    IfSilent 0 +2 ; Silent mode: Chisel has executed the installer for an update
        Sleep 2000 ; Make sure the program has shut down...
    
    SectionIn RO
    SetOutPath $INSTDIR
    File /r "Build\*"
    
    WriteRegStr HKLM "Software\Chisel\Editor" "InstallDir" "$INSTDIR"
    WriteRegStr HKLM "Software\Chisel\Editor" "Version" "{version}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChiselEditor" "DisplayName" "Chisel Editor"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChiselEditor" "UninstallString" '"$INSTDIR\Uninstall.exe"'
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChiselEditor" "NoModify" 1
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChiselEditor" "NoRepair" 1
    WriteUninstaller "Uninstall.exe"
SectionEnd

Section "Start Menu Shortcuts"
    IfSilent 0 +2
        Goto end ; Silent update: Don't redo shortcuts
        
    SetShellVarContext all
    CreateDirectory "$SMPROGRAMS\Chisel Editor"
    CreateShortCut "$SMPROGRAMS\Chisel Editor\Uninstall.lnk" "$INSTDIR\Uninstall.exe" "" "$INSTDIR\Uninstall.exe" 0
    CreateShortCut "$SMPROGRAMS\Chisel Editor\Chisel Editor.lnk" "$INSTDIR\Chisel.Editor.exe" "" "$INSTDIR\Chisel.Editor.exe" 0

    end:
SectionEnd

Section "Desktop Shortcut"
    IfSilent 0 +2
        Goto end ; Silent update: Don't redo shortcuts
    
    SetShellVarContext all
    CreateShortCut "$DESKTOP\Chisel Editor.lnk" "$INSTDIR\Chisel.Editor.exe" "" "$INSTDIR\Chisel.Editor.exe" 0
    
    end:
SectionEnd

Section "Run Chisel After Installation"
    SetAutoClose true
    Exec "$INSTDIR\Chisel.Editor.exe"
SectionEnd

; Uninstall

Section "Uninstall"

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ChiselEditor"
  DeleteRegKey HKLM "Software\Chisel\Editor"

  SetShellVarContext all
  Delete "$SMPROGRAMS\Chisel Editor\*.*"
  Delete "$DESKTOP\Chisel Editor.lnk"

  RMDir /r "$SMPROGRAMS\Chisel Editor"
  RMDir /r "$INSTDIR"

SectionEnd