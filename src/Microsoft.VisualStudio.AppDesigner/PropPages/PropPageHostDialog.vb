' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.VisualStudio.Editors.AppDesDesignerFramework
Imports System.Windows.Forms

Namespace Microsoft.VisualStudio.Editors.PropertyPages

    Public NotInheritable Class PropPageHostDialog
        Inherits BaseDialog
        'Inherits Form

        Private _propPage As PropPageUserControlBase
        Public WithEvents Cancel As System.Windows.Forms.Button
        Public WithEvents OK As System.Windows.Forms.Button
        Public WithEvents okCancelTableLayoutPanel As System.Windows.Forms.TableLayoutPanel
        Public WithEvents overArchingTableLayoutPanel As System.Windows.Forms.TableLayoutPanel
        Private _firstFocusHandled As Boolean

        ''' <summary>
        ''' Gets the F1 keyword to push into the user context for this property page
        ''' </summary>
        ''' <value></value>
        ''' <remarks></remarks>
        Protected Overrides Property F1Keyword() As String
            Get
                Dim keyword As String = MyBase.F1Keyword
                If String.IsNullOrEmpty(keyword) AndAlso _propPage IsNot Nothing Then
                    Return DirectCast(_propPage, IPropertyPageInternal).GetHelpContextF1Keyword()
                End If
                Return keyword
            End Get
            Set(ByVal Value As String)
                MyBase.F1Keyword = Value
            End Set
        End Property

        Public Property PropPage() As PropPageUserControlBase
            Get
                Return _propPage
            End Get
            Set(ByVal Value As PropPageUserControlBase)
                SuspendLayout()
                If _propPage IsNot Nothing Then
                    'Remove previous page if any
                    overArchingTableLayoutPanel.Controls.Remove(_propPage)
                End If
                _propPage = Value
                If _propPage IsNot Nothing Then
                    'm_propPage.SuspendLayout()
                    BackColor = Value.BackColor
                    MinimumSize = System.Drawing.Size.Empty
                    AutoSize = True

                    If (_propPage.PageResizable) Then
                        FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
                    Else
                        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
                    End If

                    _propPage.Margin = New System.Windows.Forms.Padding(0, 0, 0, 3)
                    _propPage.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                        Or System.Windows.Forms.AnchorStyles.Left) _
                        Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
                    _propPage.TabIndex = 0
                    'overArchingTableLayoutPanel.SuspendLayout()
                    overArchingTableLayoutPanel.Controls.Add(_propPage, 0, 0)
                    'overArchingTableLayoutPanel.ResumeLayout(False)

                    'm_propPage.ResumeLayout(False)
                End If
                ResumeLayout(False)
                PerformLayout()
                SetFocusToPage()
            End Set
        End Property

#Region " Windows Form Designer generated code "

        'Form overrides dispose to clean up the component list.
        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                If Not (_components Is Nothing) Then
                    _components.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

        'Required by the Windows Form Designer
        Private _components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(PropPageHostDialog))
            OK = New System.Windows.Forms.Button
            Cancel = New System.Windows.Forms.Button
            okCancelTableLayoutPanel = New System.Windows.Forms.TableLayoutPanel
            overArchingTableLayoutPanel = New System.Windows.Forms.TableLayoutPanel
            okCancelTableLayoutPanel.SuspendLayout()
            overArchingTableLayoutPanel.SuspendLayout()
            SuspendLayout()
            '
            'OK
            '
            resources.ApplyResources(OK, "OK")
            OK.DialogResult = System.Windows.Forms.DialogResult.OK
            OK.Margin = New System.Windows.Forms.Padding(0, 0, 3, 0)
            OK.Name = "OK"
            '
            'Cancel
            '
            resources.ApplyResources(Cancel, "Cancel")
            Cancel.CausesValidation = False
            Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
            Cancel.Margin = New System.Windows.Forms.Padding(3, 0, 0, 0)
            Cancel.Name = "Cancel"
            '
            'okCancelTableLayoutPanel
            '
            resources.ApplyResources(okCancelTableLayoutPanel, "okCancelTableLayoutPanel")
            okCancelTableLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            okCancelTableLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
            okCancelTableLayoutPanel.Controls.Add(Cancel, 1, 0)
            okCancelTableLayoutPanel.Controls.Add(OK, 0, 0)
            okCancelTableLayoutPanel.Margin = New System.Windows.Forms.Padding(0, 6, 0, 0)
            okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel"
            okCancelTableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle)
            '
            'overArchingTableLayoutPanel
            '
            resources.ApplyResources(overArchingTableLayoutPanel, "overArchingTableLayoutPanel")
            overArchingTableLayoutPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            overArchingTableLayoutPanel.Controls.Add(okCancelTableLayoutPanel, 0, 1)
            overArchingTableLayoutPanel.Name = "overArchingTableLayoutPanel"
            overArchingTableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            overArchingTableLayoutPanel.RowStyles.Add(New System.Windows.Forms.RowStyle)
            '
            'PropPageHostDialog
            '
            resources.ApplyResources(Me, "$this")
            Controls.Add(overArchingTableLayoutPanel)
            Padding = New System.Windows.Forms.Padding(12, 12, 12, 12)
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
            HelpButton = True
            MaximizeBox = False
            MinimizeBox = False
            Name = "PropPageHostDialog"
            ' Do not scale, the proppage will handle it. If we set AutoScale here, the page will expand twice, and becomes way huge
            'Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            ShowIcon = False
            ShowInTaskbar = False
            okCancelTableLayoutPanel.ResumeLayout(False)
            okCancelTableLayoutPanel.PerformLayout()
            overArchingTableLayoutPanel.ResumeLayout(False)
            overArchingTableLayoutPanel.PerformLayout()
            ResumeLayout(False)
            PerformLayout()

        End Sub

#End Region

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="ServiceProvider"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal ServiceProvider As System.IServiceProvider, ByVal F1Keyword As String)
            MyBase.New(ServiceProvider)

            'This call is required by the Windows Form Designer.
            InitializeComponent()

            'Add any initialization after the InitializeComponent() call
            Me.F1Keyword = F1Keyword

            AcceptButton = OK
            CancelButton = Cancel
        End Sub

        Protected Overrides Sub OnShown(ByVal e As EventArgs)
            MyBase.OnShown(e)

            If MinimumSize.IsEmpty Then
                MinimumSize = Size
                AutoSize = False
            End If
        End Sub

        Private Sub Cancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Cancel.Click
            PropPage.RestoreInitialValues()
            Close()
        End Sub

        Private Sub OK_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles OK.Click
            'Save the changes if current values
            Try
                'No errors in the values, apply & close the dialog
                If PropPage.IsDirty Then
                    PropPage.Apply()
                End If
                Close()
            Catch ex As ValidationException
                _propPage.ShowErrorMessage(ex)
                ex.RestoreFocus()
                Return
            Catch ex As SystemException
                _propPage.ShowErrorMessage(ex)
                Return
            Catch ex As Exception
                Debug.Fail(ex.Message)
                AppDesCommon.RethrowIfUnrecoverable(ex)
                _propPage.ShowErrorMessage(ex)
                Return
            End Try
        End Sub

        Protected Overrides Sub OnFormClosing(ByVal e As FormClosingEventArgs)
            If e.CloseReason = CloseReason.None Then
                ' That happens when the user clicks the OK button, but validation failed
                ' That is how we block the user leave when something wrong.
                e.Cancel = True
            ElseIf DialogResult <> System.Windows.Forms.DialogResult.OK Then
                ' If the user cancelled the edit, we should restore the initial values...
                PropPage.RestoreInitialValues()
            End If
        End Sub

        Public Sub SetFocusToPage()
            If Not _firstFocusHandled AndAlso _propPage IsNot Nothing Then
                _firstFocusHandled = True
                For i As Integer = 0 To _propPage.Controls.Count - 1
                    With _propPage.Controls.Item(i)
                        If .CanFocus() Then
                            .Focus()
                            Return
                        End If
                    End With
                Next i
            End If
        End Sub

        Private Sub PropPageHostDialog_HelpButtonClicked(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.HelpButtonClicked
            e.Cancel = True
            ShowHelp()
        End Sub
    End Class

End Namespace

