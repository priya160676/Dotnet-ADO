' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

'-----------------------------------------------------------------------------
' Refer to vsappid.idl for detail.
'    QueryService(IID_IVsAppId), 
'  srpVsAppId->GetProperty(VSAPROPID_SKUEdition, &varEdition)))
'-----------------------------------------------------------------------------
Imports System.Runtime.InteropServices

Namespace Microsoft.VisualStudio.Editors.AppDesInterop

    <ComVisible(False), _
    ComImport(), _
    Guid("1EAA526A-0898-11d3-B868-00C04F79F802"), _
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Friend Interface IVsAppId

        ' HRESULT SetSite([in] IServiceProvider *pSP);
        Sub SetSite(<MarshalAs(UnmanagedType.Interface)>  pSP As Microsoft.VisualStudio.OLE.Interop.IServiceProvider)

        ' HRESULT GetProperty([in] VSAPROPID propid,
        '                     [out] VARIANT *pvar);
        <PreserveSig()> _
        Function GetProperty( propid As Integer, _
                         <Out(), MarshalAs(UnmanagedType.Struct)> ByRef pvar As Object) As Integer

        ' HRESULT SetProperty([in] VSAPROPID propid,
        '                     [in] VARIANT var);
        Sub SetProperty( propid As Integer, _
                         <[In](), MarshalAs(UnmanagedType.Struct)>  var As Object)

        ' HRESULT GetGuidProperty([in] VSAPROPID propid,
        '                         [out] GUID *pguid);
        Sub GetGuidProperty( propid As Integer, _
                             <Out()> ByRef pguid As Guid)

        ' HRESULT SetGuidProperty([in] VSAPROPID propid,
        '                         [in] REFGUID rguid);
        Sub SetGuidProperty( propid As Integer, <[In]()> ByRef rguid As Guid)

        ' HRESULT Initialize();  ' called after main initialization and before command executing and entering main loop
        Sub Initialize()
    End Interface

End Namespace
