﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Windows.Forms

Namespace Microsoft.VisualStudio.Editors.PropertyPages

    ''' <summary>
    ''' The exception will be thrown when validation failed...
    ''' </summary>
    ''' <remarks></remarks>
    Friend Class ValidationException
        Inherits ApplicationException

        Private _validationResult As ValidationResult
        Private _control As Control

        Public Sub New(result As ValidationResult, message As String, Optional control As Control = Nothing, Optional InnerException As Exception = Nothing)
            MyBase.New(message, InnerException)
            _validationResult = result
            _control = control
        End Sub

        Public ReadOnly Property Result() As ValidationResult
            Get
                Return _validationResult
            End Get
        End Property

        Public Sub RestoreFocus()
            If _control Is Nothing Then Exit Sub
            _control.Focus()
            TryCast(_control, TextBox)?.SelectAll()
        End Sub
    End Class
End Namespace
