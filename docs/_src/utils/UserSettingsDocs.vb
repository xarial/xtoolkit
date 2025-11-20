Imports Newtonsoft.Json.Linq
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports Xarial.XToolkit.Services.Data
Imports Xarial.XToolkit.Services.Data.Attributes

Namespace Utils.Docs

    Module V1
        '--- v1
        <DataVersion("1.0.0", GetType(UserSettingsVersionTransformer))>
        Public Class UserSettings
            Public Property Field1 As String
            Public Property Field2 As Double
            Public Property Field3 As Boolean
        End Class
        '---
    End Module

    Module V2
        '--- v2
        <DataVersion("2.0.0", GetType(UserSettingsVersionTransformer))>
        Public Class UserSettings
            Public Property TextField As String
            Public Property Field2 As Double
            Public Property Field3 As Boolean
        End Class
        '---
    End Module

    Module UserSettingsDocs

        '--- v3
        <DataVersion("3.0.0", GetType(UserSettingsVersionTransformer))>
        Public Class UserSettings
            Public Property TextField As String
            Public Property DoubleField As Double
            Public Property BoolField As Boolean
        End Class
        '---

        '--- transformer
        Public Class UserSettingsVersionTransformer
            Implements IVersionsTransformer

            Public Sub New()
                Transforms = New VersionTransform() {
                    New VersionTransform(New Version("1.0.0"), New Version("2.0.0"),
                                         Function(o)
                                             Dim field1 = o.Property("Field1")
                                             field1.Replace(New JProperty("TextField", field1.Value))
                                             Return o
                                         End Function),
                    New VersionTransform(New Version("2.0.0"), New Version("3.0.0"),
                                         Function(o)
                                             Dim field2 = o.Property("Field2")
                                             field2.Replace(New JProperty("DoubleField", field2.Value))
                                             Dim field3 = o.Property("Field3")
                                             field3.Replace(New JProperty("BoolField", field3.Value))
                                             Return o
                                         End Function)}
            End Sub

            Public ReadOnly Property Transforms As IReadOnlyList(Of VersionTransform) Implements IVersionsTransformer.Transforms

        End Class
        '---

        Sub StoreSettings()
            '--- store
            Dim svc = New NsJsonDataSerializer(Of UserSettings)()
            Dim userSetts = New UserSettings()
            svc.Save(userSetts,
                              Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                           "my-app-settings.json"))
            '---
        End Sub

        Sub ReadSettings()
            '--- read
            Dim svc = New NsJsonDataSerializer(Of UserSettings)()
            Dim userSetts = svc.Read(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                             "my-app-settings.json"))
            '---
        End Sub
    End Module
End Namespace
