Imports System.IO
Imports System.Text.Json
Imports System.Windows.Media

Namespace deliveroo
    Public Module ThemeManager
        Private ReadOnly _settingsFilePath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme_settings.json")
        Private _isDarkMode As Boolean = True

        Public Property IsDarkMode As Boolean
            Get
                Return _isDarkMode
            End Get
            Set(value As Boolean)
                _isDarkMode = value
                ApplyTheme(value)
                SaveThemePreference(value)
            End Set
        End Property

        Public Sub InitializeTheme()
            _isDarkMode = LoadThemePreference()
            ApplyTheme(_isDarkMode)
        End Sub

        Public Function LoadThemePreference() As Boolean
            Try
                If File.Exists(_settingsFilePath) Then
                    Dim json = File.ReadAllText(_settingsFilePath)
                    Using doc = JsonDocument.Parse(json)
                        If doc.RootElement.TryGetProperty("IsDarkMode", Nothing) Then
                            Return doc.RootElement.GetProperty("IsDarkMode").GetBoolean()
                        End If
                    End Using
                End If
            Catch ex As Exception
                ' Default su errore: Dark Mode
            End Try
            Return True ' Default Dark Mode
        End Function

        Public Sub SaveThemePreference(isDark As Boolean)
            Try
                Dim json = JsonSerializer.Serialize(New With {.IsDarkMode = isDark})
                File.WriteAllText(_settingsFilePath, json)
            Catch ex As Exception
            End Try
        End Sub

        Public Sub ApplyTheme(isDark As Boolean)
            Dim res = Application.Current.Resources

            If isDark Then
                ' --- PALETTE TEMA SCURO ---
                SetBrush(res, "BgWindow", "#18181B")
                SetBrush(res, "BgCard", "#27272A")
                SetBrush(res, "BgInput", "#27272A")
                SetBrush(res, "BorderInput", "#3F3F46")
                SetBrush(res, "FgText", "#F4F4F5")
                SetBrush(res, "FgMuted", "#A1A1AA")

                ' Tabs
                SetBrush(res, "BgTabControl", "#18181B")
                SetBrush(res, "BgTabItem", "#27272A")
                SetBrush(res, "BgTabItemSelected", "#18181B")
                SetBrush(res, "BgTabItemHover", "#323236")
                SetBrush(res, "FgTabItem", "#A1A1AA")
                SetBrush(res, "FgTabItemSelected", "#FFFFFF")

                ' Griglia
                SetBrush(res, "BgDataGridHeader", "#27272A")
                SetBrush(res, "BgDataGridRow", "#18181B")
                SetBrush(res, "BgDataGridRowAlt", "#222225")
                SetBrush(res, "DataGridGridLines", "#3F3F46")

                ' Box Statistiche
                SetBrush(res, "BgCardLordo", "#142918")
                SetBrush(res, "BorderCardLordo", "#4CAF50")
                SetBrush(res, "FgCardLordo", "#81C784")

                SetBrush(res, "BgCardNettoB", "#0C233C")
                SetBrush(res, "BorderCardNettoB", "#2196F3")
                SetBrush(res, "FgCardNettoB", "#64B5F6")

                SetBrush(res, "BgCardSpese", "#331D0C")
                SetBrush(res, "BorderCardSpese", "#FF9800")
                SetBrush(res, "FgCardSpese", "#FFB74D")

                SetBrush(res, "BgCardNettoR", "#0B2644")
                SetBrush(res, "BorderCardNettoR", "#42A5F5")
                SetBrush(res, "FgCardNettoR", "#FFFFFF")
                SetBrush(res, "SubFgCardNettoR", "#90CAF9")

                SetBrush(res, "BgCard730", "#26112C")
                SetBrush(res, "BorderCard730", "#AB47BC")
                SetBrush(res, "FgCard730", "#CE93D8")

                SetBrush(res, "BgCardTotale730", "#0B2B26")
                SetBrush(res, "BorderCardTotale730", "#00BFA5")
                SetBrush(res, "FgCardTotale730", "#FFFFFF")
                SetBrush(res, "SubFgCardTotale730", "#64FFDA")

                SetBrush(res, "BgBadgeVoto", "#381313")
                SetBrush(res, "BorderBadgeVoto", "#EF5350")
                SetBrush(res, "FgBadgeVoto", "#EF5350")

                SetBrush(res, "FgStatsLabels", "#E4E4E7")
                SetBrush(res, "BtnThemeBg", "#27272A")
                SetBrush(res, "BtnThemeFg", "#F4F4F5")
            Else
                ' --- PALETTE TEMA CHIARO ---
                SetBrush(res, "BgWindow", "#FFFFFF")
                SetBrush(res, "BgCard", "#FFFFFF")
                SetBrush(res, "BgInput", "#FFFFFF")
                SetBrush(res, "BorderInput", "#CBD5E1")
                SetBrush(res, "FgText", "#0F172A")
                SetBrush(res, "FgMuted", "#64748B")

                ' Tabs
                SetBrush(res, "BgTabControl", "#FFFFFF")
                SetBrush(res, "BgTabItem", "#E2E8F0")
                SetBrush(res, "BgTabItemSelected", "#FFFFFF")
                SetBrush(res, "BgTabItemHover", "#CBD5E1")
                SetBrush(res, "FgTabItem", "#64748B")
                SetBrush(res, "FgTabItemSelected", "#0F172A")

                ' Griglia
                SetBrush(res, "BgDataGridHeader", "#F1F5F9")
                SetBrush(res, "BgDataGridRow", "#FFFFFF")
                SetBrush(res, "BgDataGridRowAlt", "#F8FAFC")
                SetBrush(res, "DataGridGridLines", "#E2E8F0")

                ' Box Statistiche
                SetBrush(res, "BgCardLordo", "#F1F8E9")
                SetBrush(res, "BorderCardLordo", "#4CAF50")
                SetBrush(res, "FgCardLordo", "#2E7D32")

                SetBrush(res, "BgCardNettoB", "#E3F2FD")
                SetBrush(res, "BorderCardNettoB", "#2196F3")
                SetBrush(res, "FgCardNettoB", "#1565C0")

                SetBrush(res, "BgCardSpese", "#FFF3E0")
                SetBrush(res, "BorderCardSpese", "#FF9800")
                SetBrush(res, "FgCardSpese", "#E65100")

                SetBrush(res, "BgCardNettoR", "#0D47A1")
                SetBrush(res, "BorderCardNettoR", "#1565C0")
                SetBrush(res, "FgCardNettoR", "#FFFFFF")
                SetBrush(res, "SubFgCardNettoR", "#B3E5FC")

                SetBrush(res, "BgCard730", "#F3E5F5")
                SetBrush(res, "BorderCard730", "#9C27B0")
                SetBrush(res, "FgCard730", "#6A1B9A")

                SetBrush(res, "BgCardTotale730", "#004D40")
                SetBrush(res, "BorderCardTotale730", "#00796B")
                SetBrush(res, "FgCardTotale730", "#FFFFFF")
                SetBrush(res, "SubFgCardTotale730", "#E0F2F1")

                SetBrush(res, "BgBadgeVoto", "#FFF5F5")
                SetBrush(res, "BorderBadgeVoto", "#D32F2F")
                SetBrush(res, "FgBadgeVoto", "#D32F2F")

                SetBrush(res, "FgStatsLabels", "#334155")
                SetBrush(res, "BtnThemeBg", "#F1F5F9")
                SetBrush(res, "BtnThemeFg", "#0F172A")
            End If
        End Sub

        Private Sub SetBrush(res As ResourceDictionary, key As String, hex As String)
            Dim brush = New BrushConverter().ConvertFromString(hex)
            If res.Contains(key) Then
                res(key) = brush
            Else
                res.Add(key, brush)
            End If
        End Sub
    End Module
End Namespace
