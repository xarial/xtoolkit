Imports System
Imports System.ComponentModel

Namespace Wpf.Docs

    '--- enum-display-name-attribute
    <AttributeUsage(AttributeTargets.Field)>
    Public Class EnumDisplayNameAttribute
        Inherits DisplayNameAttribute

        Public Sub New(ByVal dispName As String)
            MyBase.New(dispName)
        End Sub
    End Class
    '---

    '--- enum
    <Flags>
    Public Enum FlagEnumSample_e
        None = 0
        <EnumDisplayName("Field 1")>
        Field1 = 1
        <Description("Field 2")>
        Field2 = 2
        Field3 = 4
        Field2AndField3 = Field2 Or Field3
        Field4 = 8
    End Enum
    '---

    '--- view-model
    Public Class FlagEnumComboBoxControlVM
        Public Property EnumPrp As FlagEnumSample_e
    End Class
    '---

End Namespace
