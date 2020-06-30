Imports System

Namespace Wpf.Docs

    '--- enum
    <Flags>
    Public Enum EnumSample_e
        None = 0
        Field1 = 1
        Field2 = 2
        Field3 = 4
        Field2AndField3 = Field2 Or Field3
        Field4 = 8
    End Enum
    '---

    '--- view-model
    Public Class EnumComboBoxControlVM
        Public Property EnumPrp As EnumSample_e
    End Class
    '---

End Namespace
