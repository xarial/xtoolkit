Imports Newtonsoft.Json.Linq
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text
Imports Xarial.XToolkit.Services.UserSettings
Imports Xarial.XToolkit.Services.UserSettings.Attributes

Namespace Utils.Docs

    Module V1
        '--- v1
        <UserSettingVersion("1.0.0", GetType(UserSettingsVersionTransformer))>
        Public Class UserSettings
            Public Property Field1 As String
            Public Property Field2 As Double
            Public Property Field3 As Boolean
        End Class
        '---
    End Module

    Module V2
        '--- v2
        <UserSettingVersion("2.0.0", GetType(UserSettingsVersionTransformer))>
        Public Class UserSettings
            Public Property TextField As String
            Public Property Field2 As Double
            Public Property Field3 As Boolean
        End Class
        '---
    End Module

    Module UserSettingsDocs

        '--- v3
        <UserSettingVersion("3.0.0", GetType(UserSettingsVersionTransformer))>
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
                                         Function(t)
                                             Dim field1 = t.Children(Of JProperty)().First(Function(p) p.Name = "Field1")
                                             field1.Replace(New JProperty("TextField", (TryCast(field1, JProperty)).Value))
                                             Return t
                                         End Function),
                    New VersionTransform(New Version("2.0.0"), New Version("3.0.0"),
                                         Function(t)
                                             Dim field2 = t.Children(Of JProperty)().First(Function(p) p.Name = "Field2")
                                             field2.Replace(New JProperty("DoubleField", (TryCast(field2, JProperty)).Value))
                                             Dim field3 = t.Children(Of JProperty)().First(Function(p) p.Name = "Field3")
                                             field3.Replace(New JProperty("BoolField", (TryCast(field3, JProperty)).Value))
                                             Return t
                                         End Function)}
            End Sub

            Public ReadOnly Property Transforms As IReadOnlyList(Of VersionTransform) Implements IVersionsTransformer.Transforms

        End Class
        '---

        Sub StoreSettings()
            '--- store
            Dim svc = New UserSettingsService()
            Dim userSetts = New UserSettings()
            svc.StoreSettings(userSetts,
                              Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                           "my-app-settings.json"))
            '---
        End Sub

        Sub ReadSettings()
            '--- read
            Dim svc = New UserSettingsService()
            Dim userSetts = svc.ReadSettings(Of UserSettings)(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                             "my-app-settings.json"))
            '---
        End Sub
    End Module
End Namespace
