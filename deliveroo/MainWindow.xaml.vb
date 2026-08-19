Imports System.Globalization

Class MainWindow
    Private _db As deliveroo.DatabaseManager
    Private _editingSession As deliveroo.WorkSessionModel = Nothing
    Private Const FUEL_PRICE As Decimal = 2D
    Private Const PROFITABILITY_COEFFICIENT As Decimal = 0.67D
    Private Const INPS_RATE As Decimal = 0.2607D

    Public Sub New()
        InitializeComponent()
        _db = New deliveroo.DatabaseManager()
        dpDate.SelectedDate = DateTime.Today

        ' Inizializza tema (default Dark Mode o preferenza salvata)
        deliveroo.ThemeManager.InitializeTheme()
        UpdateThemeButtonText()
        AddHandler btnToggleTheme.Click, AddressOf BtnToggleTheme_Click

        AddHandler btnSave.Click, AddressOf BtnSave_Click
        AddHandler btnRefresh.Click, AddressOf BtnRefresh_Click
        AddHandler btnEdit.Click, AddressOf BtnEdit_Click
        AddHandler btnDelete.Click, AddressOf BtnDelete_Click
        AddHandler dgSessions.SelectionChanged, AddressOf DgSessions_SelectionChanged
        AddHandler cbMonthYear.SelectionChanged, AddressOf CbMonthYear_SelectionChanged
        AddHandler dpStatDay.SelectedDateChanged, AddressOf DpStatDay_SelectedDateChanged
        AddHandler cbYear.SelectionChanged, AddressOf CbYear_SelectionChanged
        AddHandler cbTaxRate.SelectionChanged, AddressOf CbTaxRate_SelectionChanged

        cbTaxRate.SelectedIndex = 0

        LoadSessions()
        LoadMonthYearComboBox()
        LoadYearComboBox()

        ' inizializza selettori specifici
        dpStatDay.SelectedDate = DateTime.Today
        If cbYear.Items.Count > 0 Then
            cbYear.SelectedIndex = 0
        End If

        ' caricamento iniziale statistiche
        If dpStatDay.SelectedDate.HasValue Then LoadDailyStatistics(dpStatDay.SelectedDate.Value)
        If cbYear.SelectedItem IsNot Nothing Then
            Dim it = CType(cbYear.SelectedItem, ComboBoxItem)
            LoadAnnualStatistics(CInt(it.Tag))
        End If
    End Sub

    Private Sub BtnToggleTheme_Click(sender As Object, e As RoutedEventArgs)
        deliveroo.ThemeManager.IsDarkMode = Not deliveroo.ThemeManager.IsDarkMode
        UpdateThemeButtonText()
    End Sub

    Private Sub UpdateThemeButtonText()
        If deliveroo.ThemeManager.IsDarkMode Then
            btnToggleTheme.Content = "☀️ Modalità Chiara"
        Else
            btnToggleTheme.Content = "🌙 Modalità Scura"
        End If
    End Sub

    Private Sub CbTaxRate_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If cbYear.SelectedItem IsNot Nothing AndAlso cbTaxRate.SelectedItem IsNot Nothing Then
            Dim selectedYear = CInt(CType(cbYear.SelectedItem, ComboBoxItem).Tag)
            Dim taxRateStr = CType(cbTaxRate.SelectedItem, ComboBoxItem).Tag.ToString()
            Dim taxRate = Decimal.Parse(taxRateStr, CultureInfo.InvariantCulture)
            _db.SetTaxRateForYear(selectedYear, taxRate)
            UpdateStatistics()
            If dpStatDay.SelectedDate.HasValue Then LoadDailyStatistics(dpStatDay.SelectedDate.Value)
            LoadAnnualStatistics(selectedYear)
        End If
    End Sub

    Private Sub LoadMonthYearComboBox()
        Try
            Dim currentSelectedTag As (Integer, Integer)? = Nothing
            If cbMonthYear.SelectedItem IsNot Nothing Then
                Dim curItem = CType(cbMonthYear.SelectedItem, ComboBoxItem)
                If TypeOf curItem.Tag Is ValueTuple(Of Integer, Integer) Then
                    currentSelectedTag = CType(curItem.Tag, ValueTuple(Of Integer, Integer))
                End If
            End If

            cbMonthYear.Items.Clear()
            Dim allSessions = _db.GetAll()

            Dim monthDates As New SortedSet(Of DateTime)()
            Dim today = DateTime.Today
            monthDates.Add(New DateTime(today.Year, today.Month, 1))

            If allSessions IsNot Nothing Then
                For Each session In allSessions
                    monthDates.Add(New DateTime(session.Date.Year, session.Date.Month, 1))
                Next
            End If

            Dim itCulture = CultureInfo.GetCultureInfo("it-IT")
            Dim indexToSelect As Integer = 0
            Dim currentIndex As Integer = 0

            For Each d In monthDates.Reverse()
                Dim item As New ComboBoxItem()
                item.Content = d.ToString("MMMM yyyy", itCulture)
                item.Tag = (d.Year, d.Month)
                cbMonthYear.Items.Add(item)

                If currentSelectedTag.HasValue AndAlso currentSelectedTag.Value.Item1 = d.Year AndAlso currentSelectedTag.Value.Item2 = d.Month Then
                    indexToSelect = currentIndex
                End If
                currentIndex += 1
            Next

            If cbMonthYear.Items.Count > 0 Then
                cbMonthYear.SelectedIndex = indexToSelect
            End If
        Catch ex As Exception
            MessageBox.Show("Errore LoadMonthYearComboBox: " & ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub CbMonthYear_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If cbMonthYear.SelectedItem IsNot Nothing Then
            UpdateStatistics()
        End If
    End Sub

    Private Sub UpdateStatistics()
        If cbMonthYear.SelectedItem Is Nothing Then Return

        Dim selectedItem = CType(cbMonthYear.SelectedItem, ComboBoxItem)
        Dim tag = CType(selectedItem.Tag, ValueTuple(Of Integer, Integer))
        Dim year = tag.Item1
        Dim month = tag.Item2

        Try
            Dim monthlySessions = _db.GetMonthlyData(year, month)

            Dim lordoMensile As Decimal = monthlySessions.Sum(Function(s) s.TotalEarnings)
            Dim nettoBonificato As Decimal = lordoMensile
            Dim imponibile As Decimal = lordoMensile * PROFITABILITY_COEFFICIENT
            Dim inps As Decimal = imponibile * INPS_RATE
            Dim rate = _db.GetTaxRateForYear(year)
            Dim imposta As Decimal = (imponibile - inps) * rate
            Dim tasseTotali As Decimal = inps + imposta
            Dim speseCarburante As Decimal = CDec(monthlySessions.Sum(Function(s) If(s.Consumption > 0, (s.Km / s.Consumption) * CDbl(FUEL_PRICE), 0)))
            Dim nettoRealeTasca As Decimal = lordoMensile - speseCarburante - tasseTotali
            Dim accumulo730 As Decimal = tasseTotali

            If lordoMensile > 0 Then
                Dim efficienzaIncidenza As Decimal = (speseCarburante / lordoMensile) * 100
                Dim efficienzaVoto As Decimal = 10D - ((efficienzaIncidenza - 10D) * 0.2D)
                If efficienzaVoto > 10D Then efficienzaVoto = 10D
                If efficienzaVoto < 1D Then efficienzaVoto = 1D
                tbEfficienzaVotoMensile.Text = $"{efficienzaVoto:F1}"
                lblIncidenzaMensile.Content = $"Incidenza carburante: {efficienzaIncidenza:F1}%"
            Else
                tbEfficienzaVotoMensile.Text = "-"
                lblIncidenzaMensile.Content = "Incidenza carburante: - %"
            End If

            tbLordoMensile.Text = $"€ {lordoMensile:F2}"
            tbNettoBonificato.Text = $"€ {nettoBonificato:F2}"
            tbSpeseCarburante.Text = $"€ {speseCarburante:F2}"
            tbNettoRealeTasca.Text = $"€ {nettoRealeTasca:F2}"
            tbAccumulo730.Text = $"€ {accumulo730:F2}"

            Dim totKm As Double = monthlySessions.Sum(Function(s) s.Km)
            Dim totDeliveries As Integer = monthlySessions.Sum(Function(s) s.Deliveries)
            Dim totLiters As Double = monthlySessions.Sum(Function(s) If(s.Consumption > 0, s.Km / s.Consumption, 0))
            Dim avgCons As Double = If(totLiters > 0, totKm / totLiters, monthlySessions.Where(Function(s) s.Consumption > 0).Select(Function(s) s.Consumption).DefaultIfEmpty(0).Average())

            lblKmMensile.Content = $"Km percorsi: {totKm:F1} km"
            lblConsegneMensile.Content = $"Numero consegne: {totDeliveries}"
            lblConsumoMensile.Content = If(avgCons > 0, $"Consumo registrato: {avgCons:F1} km/litro", "Consumo registrato: - km/litro")
        Catch ex As Exception
            MessageBox.Show("Errore calcolo statistiche: " & ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub DpStatDay_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
        If dpStatDay.SelectedDate.HasValue Then
            LoadDailyStatistics(dpStatDay.SelectedDate.Value)
        End If
    End Sub

    Private Sub CbYear_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If cbYear.SelectedItem IsNot Nothing Then
            Dim year = CInt(CType(cbYear.SelectedItem, ComboBoxItem).Tag)
            
            Dim rate = _db.GetTaxRateForYear(year)
            For Each item As ComboBoxItem In cbTaxRate.Items
                Dim itemRate = Decimal.Parse(item.Tag.ToString(), CultureInfo.InvariantCulture)
                If itemRate = rate Then
                    cbTaxRate.SelectedItem = item
                    Exit For
                End If
            Next

            LoadAnnualStatistics(year)
        End If
    End Sub

    Private Sub LoadYearComboBox()
        Try
            Dim currentSelectedYear As Integer? = Nothing
            If cbYear.SelectedItem IsNot Nothing Then
                Dim curItem = CType(cbYear.SelectedItem, ComboBoxItem)
                If curItem.Tag IsNot Nothing Then
                    currentSelectedYear = CInt(curItem.Tag)
                End If
            End If

            cbYear.Items.Clear()
            Dim all = _db.GetAll()
            Dim years As New SortedSet(Of Integer)()
            Dim today = DateTime.Today
            years.Add(If(today.Month = 12, today.Year + 1, today.Year))
            If all IsNot Nothing Then
                For Each s In all
                    years.Add(If(s.Date.Month = 12, s.Date.Year + 1, s.Date.Year))
                Next
            End If

            Dim indexToSelect As Integer = 0
            Dim currentIndex As Integer = 0

            For Each y In years.Reverse()
                Dim item As New ComboBoxItem()
                item.Content = y.ToString()
                item.Tag = y
                cbYear.Items.Add(item)

                If currentSelectedYear.HasValue AndAlso currentSelectedYear.Value = y Then
                    indexToSelect = currentIndex
                End If
                currentIndex += 1
            Next

            If cbYear.Items.Count > 0 Then
                cbYear.SelectedIndex = indexToSelect
            End If
        Catch ex As Exception
            MessageBox.Show("Errore LoadYearComboBox: " & ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub LoadDailyStatistics(day As DateTime)
        Try
            Dim all = _db.GetAll()
            Dim daySessions = all.Where(Function(s) s.Date.Date = day.Date).ToList()

            Dim lordo As Decimal = daySessions.Sum(Function(s) s.TotalEarnings)
            Dim nettoBonif As Decimal = lordo
            Dim imponibile As Decimal = lordo * PROFITABILITY_COEFFICIENT
            Dim inps As Decimal = imponibile * INPS_RATE
            Dim rate = _db.GetTaxRateForYear(day.Year)
            Dim imposta As Decimal = (imponibile - inps) * rate
            Dim tasseTotali As Decimal = inps + imposta
            Dim spese As Decimal = CDec(daySessions.Sum(Function(s) If(s.Consumption > 0, (s.Km / s.Consumption) * CDbl(FUEL_PRICE), 0)))
            Dim nettoTasca As Decimal = lordo - spese - tasseTotali
            Dim accum As Decimal = tasseTotali

            If lordo > 0 Then
                Dim efficienzaIncidenza As Decimal = (spese / lordo) * 100
                Dim efficienzaVoto As Decimal = 10D - ((efficienzaIncidenza - 10D) * 0.2D)
                If efficienzaVoto > 10D Then efficienzaVoto = 10D
                If efficienzaVoto < 1D Then efficienzaVoto = 1D
                tbEfficienzaVotoGiorno.Text = $"{efficienzaVoto:F1}"
                lblIncidenzaGiorno.Content = $"Incidenza carburante: {efficienzaIncidenza:F1}%"
            Else
                tbEfficienzaVotoGiorno.Text = "-"
                lblIncidenzaGiorno.Content = "Incidenza carburante: - %"
            End If

            tbLordoGiorno.Text = $"€ {lordo:F2}"
            tbNettoBonificatoGiorno.Text = $"€ {nettoBonif:F2}"
            tbSpeseCarburanteGiorno.Text = $"€ {spese:F2}"
            tbNettoRealeTascaGiorno.Text = $"€ {nettoTasca:F2}"
            tbAccumulo730Giorno.Text = $"€ {accum:F2}"

            Dim totKm As Double = daySessions.Sum(Function(s) s.Km)
            Dim totDeliveries As Integer = daySessions.Sum(Function(s) s.Deliveries)
            Dim totLiters As Double = daySessions.Sum(Function(s) If(s.Consumption > 0, s.Km / s.Consumption, 0))
            Dim avgCons As Double = If(totLiters > 0, totKm / totLiters, daySessions.Where(Function(s) s.Consumption > 0).Select(Function(s) s.Consumption).DefaultIfEmpty(0).Average())

            lblKmGiorno.Content = $"Km percorsi: {totKm:F1} km"
            lblConsegneGiorno.Content = $"Numero consegne: {totDeliveries}"
            lblConsumoGiorno.Content = If(avgCons > 0, $"Consumo registrato: {avgCons:F1} km/litro", "Consumo registrato: - km/litro")
        Catch ex As Exception
            MessageBox.Show("Errore LoadDailyStatistics: " & ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub LoadAnnualStatistics(year As Integer)
        Try
            Dim all = _db.GetAll()
            Dim yearSessions = all.Where(Function(s) If(s.Date.Month = 12, s.Date.Year + 1, s.Date.Year) = year).ToList()
            Dim previousYearSessions = all.Where(Function(s) If(s.Date.Month = 12, s.Date.Year + 1, s.Date.Year) = year - 1).ToList()

            Dim lordo As Decimal = yearSessions.Sum(Function(s) s.TotalEarnings)
            Dim nettoBonif As Decimal = lordo
            Dim imponibile As Decimal = lordo * PROFITABILITY_COEFFICIENT
            Dim inps As Decimal = imponibile * INPS_RATE
            Dim rate = _db.GetTaxRateForYear(year)
            Dim imposta As Decimal = (imponibile - inps) * rate
            Dim tasseTotali As Decimal = inps + imposta
            Dim spese As Decimal = CDec(yearSessions.Sum(Function(s) If(s.Consumption > 0, (s.Km / s.Consumption) * CDbl(FUEL_PRICE), 0)))
            Dim nettoTasca As Decimal = lordo - spese - tasseTotali
            Dim accum As Decimal = tasseTotali

            Dim totaleConRimborso As Decimal = imponibile

            If lordo > 0 Then
                Dim efficienzaIncidenza As Decimal = (spese / lordo) * 100
                Dim efficienzaVoto As Decimal = 10D - ((efficienzaIncidenza - 10D) * 0.2D)
                If efficienzaVoto > 10D Then efficienzaVoto = 10D
                If efficienzaVoto < 1D Then efficienzaVoto = 1D
                tbEfficienzaVotoAnno.Text = $"{efficienzaVoto:F1}"
                lblIncidenzaAnno.Content = $"Incidenza carburante: {efficienzaIncidenza:F1}%"
            Else
                tbEfficienzaVotoAnno.Text = "-"
                lblIncidenzaAnno.Content = "Incidenza carburante: - %"
            End If

            tbLordoAnno.Text = $"€ {lordo:F2}"
            tbNettoBonificatoAnno.Text = $"€ {nettoBonif:F2}"
            tbSpeseCarburanteAnno.Text = $"€ {spese:F2}"
            tbNettoRealeTascaAnno.Text = $"€ {nettoTasca:F2}"
            tbAccumulo730Anno.Text = $"€ {accum:F2}"
            tbNettoTotale730Anno.Text = $"€ {totaleConRimborso:F2}"
            lblDescNettoTotale730Anno.Text = $"Base su cui si pagano tasse e INPS"

            Dim totKm As Double = yearSessions.Sum(Function(s) s.Km)
            Dim totDeliveries As Integer = yearSessions.Sum(Function(s) s.Deliveries)
            Dim totLiters As Double = yearSessions.Sum(Function(s) If(s.Consumption > 0, s.Km / s.Consumption, 0))
            Dim avgCons As Double = If(totLiters > 0, totKm / totLiters, yearSessions.Where(Function(s) s.Consumption > 0).Select(Function(s) s.Consumption).DefaultIfEmpty(0).Average())

            lblKmAnno.Content = $"Km percorsi: {totKm:F1} km"
            lblConsegneAnno.Content = $"Numero consegne: {totDeliveries}"
            lblConsumoAnno.Content = If(avgCons > 0, $"Consumo registrato: {avgCons:F1} km/litro", "Consumo registrato: - km/litro")
        Catch ex As Exception
            MessageBox.Show("Errore LoadAnnualStatistics: " & ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub RefreshAllDataAndStats()
        LoadSessions()
        LoadMonthYearComboBox()
        LoadYearComboBox()

        ' Aggiorna statistiche giorno se selezionato
        If dpStatDay.SelectedDate.HasValue Then
            LoadDailyStatistics(dpStatDay.SelectedDate.Value)
        End If

        ' Aggiorna statistiche mese
        UpdateStatistics()

        ' Aggiorna statistiche anno se selezionato
        If cbYear.SelectedItem IsNot Nothing Then
            Dim it = CType(cbYear.SelectedItem, ComboBoxItem)
            If it.Tag IsNot Nothing Then
                LoadAnnualStatistics(CInt(it.Tag))
            End If
        End If
    End Sub

    ' --- Restante logica CRUD (inserimento/modifica/eliminazione) ---
    Private Sub BtnSave_Click(sender As Object, e As RoutedEventArgs)
        If Not dpDate.SelectedDate.HasValue Then
            MessageBox.Show("Seleziona una data.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Dim parsedDecimal As Decimal
        Dim parsedInt As Integer
        Dim parsedDouble As Double

        Dim cleanEarnings = If(tbEarnings.Text, "").Trim().Replace(","c, "."c)
        If Not Decimal.TryParse(cleanEarnings, NumberStyles.AllowDecimalPoint Or NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, parsedDecimal) Then
            MessageBox.Show("Inserisci un guadagno valido (es. 50.00 o 50,00).", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        If Not Integer.TryParse(If(tbDeliveries.Text, "").Trim(), parsedInt) Then
            MessageBox.Show("Inserisci un numero di consegne valido.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Dim cleanKm = If(tbKm.Text, "").Trim().Replace(","c, "."c)
        If Not Double.TryParse(cleanKm, NumberStyles.AllowDecimalPoint Or NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, parsedDouble) Then
            MessageBox.Show("Inserisci un valore Km valido (es. 120.5 o 120,5).", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Dim parsedConsumption As Double = 0
        If Not String.IsNullOrWhiteSpace(tbConsumption.Text) Then
            Dim cleanConsumption = tbConsumption.Text.Trim().Replace(","c, "."c)
            If Not Double.TryParse(cleanConsumption, NumberStyles.AllowDecimalPoint Or NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, parsedConsumption) Then
                MessageBox.Show("Inserisci un valore di consumo valido (es. 18.5 o 18,5).", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
        End If

        Try
            Dim wasEditing As Boolean = (_editingSession IsNot Nothing)

            If wasEditing Then
                _editingSession.Date = dpDate.SelectedDate.Value.Date
                _editingSession.TotalEarnings = parsedDecimal
                _editingSession.Deliveries = parsedInt
                _editingSession.Km = parsedDouble
                _editingSession.Consumption = parsedConsumption
                _db.Update(_editingSession)
                btnSave.Content = "Salva"
                _editingSession = Nothing
            Else
                Dim session As New deliveroo.WorkSessionModel With {
                    .Date = dpDate.SelectedDate.Value.Date,
                    .TotalEarnings = parsedDecimal,
                    .Deliveries = parsedInt,
                    .Km = parsedDouble,
                    .Consumption = parsedConsumption
                }
                _db.Insert(session)
                MessageBox.Show("Nuova sessione registrata con successo!", "Conferma", MessageBoxButton.OK, MessageBoxImage.Information)
            End If
            ClearForm()
            RefreshAllDataAndStats()

            If wasEditing Then
                MainTabControl.SelectedIndex = 1
            End If
        Catch ex As Exception
            MessageBox.Show("Errore salvataggio: " & ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub BtnClear_Click(sender As Object, e As RoutedEventArgs)
        _editingSession = Nothing
        btnSave.Content = "Salva"
        ClearForm()
    End Sub

    Private Sub ClearForm()
        dpDate.SelectedDate = DateTime.Today
        tbEarnings.Text = String.Empty
        tbDeliveries.Text = String.Empty
        tbKm.Text = String.Empty
        tbConsumption.Text = String.Empty
    End Sub

    Private Sub LoadSessions()
        Try
            Dim list = _db.GetAll()
            dgSessions.ItemsSource = list
        Catch ex As Exception
            MessageBox.Show("Errore caricamento dati: " & ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

    Private Sub BtnRefresh_Click(sender As Object, e As RoutedEventArgs)
        RefreshAllDataAndStats()
    End Sub

    Private Sub DgSessions_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        btnEdit.IsEnabled = (dgSessions.SelectedItem IsNot Nothing)
    End Sub

    Private Sub BtnEdit_Click(sender As Object, e As RoutedEventArgs)
        If dgSessions.SelectedItem Is Nothing Then
            MessageBox.Show("Seleziona una sessione da modificare.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        _editingSession = CType(dgSessions.SelectedItem, deliveroo.WorkSessionModel)
        dpDate.SelectedDate = _editingSession.Date
        tbEarnings.Text = _editingSession.TotalEarnings.ToString("0.##", CultureInfo.InvariantCulture)
        tbDeliveries.Text = _editingSession.Deliveries.ToString()
        tbKm.Text = _editingSession.Km.ToString("0.##", CultureInfo.InvariantCulture)
        tbConsumption.Text = If(_editingSession.Consumption > 0, _editingSession.Consumption.ToString("0.##", CultureInfo.InvariantCulture), "")
        btnSave.Content = "Aggiorna"
        MainTabControl.SelectedIndex = 0
    End Sub

    Private Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
        If dgSessions.SelectedItem Is Nothing Then
            MessageBox.Show("Seleziona una sessione da eliminare.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Dim selectedSession = CType(dgSessions.SelectedItem, deliveroo.WorkSessionModel)
        Dim result = MessageBox.Show("Sei sicuro di voler eliminare questa sessione?", "Conferma", MessageBoxButton.YesNo, MessageBoxImage.Question)
        If result = MessageBoxResult.Yes Then
            Try
                _db.Delete(selectedSession.Id)
                RefreshAllDataAndStats()
            Catch ex As Exception
                MessageBox.Show("Errore eliminazione: " & ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End If
    End Sub

    Private Sub dgSessions_SelectionChanged_1(sender As Object, e As SelectionChangedEventArgs) Handles dgSessions.SelectionChanged

    End Sub

    ' --- Posizionamento Libero Immagine Deliveroo col Mouse ---
    Private _isDraggingImage As Boolean = False
    Private _startMousePosition As Point
    Private _startImageLeft As Double
    Private _startImageTop As Double

    Private Sub ImgDeliveroo_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        _isDraggingImage = True
        _startMousePosition = e.GetPosition(canvasSessione)

        Dim curLeft = Canvas.GetLeft(imgDeliveroo)
        Dim curTop = Canvas.GetTop(imgDeliveroo)
        _startImageLeft = If(Double.IsNaN(curLeft), 0, curLeft)
        _startImageTop = If(Double.IsNaN(curTop), 0, curTop)

        imgDeliveroo.CaptureMouse()
        e.Handled = True
    End Sub

    Private Sub ImgDeliveroo_MouseMove(sender As Object, e As MouseEventArgs)
        If _isDraggingImage Then
            Dim currentPosition = e.GetPosition(canvasSessione)
            Dim deltaX = currentPosition.X - _startMousePosition.X
            Dim deltaY = currentPosition.Y - _startMousePosition.Y
            Canvas.SetLeft(imgDeliveroo, _startImageLeft + deltaX)
            Canvas.SetTop(imgDeliveroo, _startImageTop + deltaY)
        End If
    End Sub

    Private Sub ImgDeliveroo_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        If _isDraggingImage Then
            _isDraggingImage = False
            imgDeliveroo.ReleaseMouseCapture()
            e.Handled = True
        End If
    End Sub

    Private Sub ImgDeliveroo_LostMouseCapture(sender As Object, e As MouseEventArgs)
        _isDraggingImage = False
    End Sub
End Class