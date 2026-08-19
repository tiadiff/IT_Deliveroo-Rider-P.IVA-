Imports System.Data.SQLite
Imports System.IO

Namespace deliveroo
    Public Class DatabaseManager
        Private ReadOnly _dbPath As String
        Private ReadOnly _connectionString As String

        Public Sub New()
            Dim projectRoot = AppDomain.CurrentDomain.BaseDirectory
            _dbPath = Path.Combine(projectRoot, "deliveroo.db")
            _connectionString = $"Data Source={_dbPath};Version=3;"
            InitializeDatabase()
        End Sub

        Private Sub InitializeDatabase()
            If Not File.Exists(_dbPath) Then
                SQLiteConnection.CreateFile(_dbPath)
            End If

            Using conn = New SQLiteConnection(_connectionString)
                conn.Open()
                Dim cmd = conn.CreateCommand()
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS WorkSessions (Id INTEGER PRIMARY KEY AUTOINCREMENT, Date TEXT NOT NULL, TotalEarnings REAL, Deliveries INTEGER, Km REAL, Consumption REAL);"
                cmd.ExecuteNonQuery()

                Dim cmd2 = conn.CreateCommand()
                cmd2.CommandText = "CREATE TABLE IF NOT EXISTS YearlySettings (Year INTEGER PRIMARY KEY, TaxRate REAL);"
                cmd2.ExecuteNonQuery()
            End Using
        End Sub

        Public Sub Insert(session As WorkSessionModel)
            Using conn = New SQLiteConnection(_connectionString)
                conn.Open()
                Dim cmd = conn.CreateCommand()
                cmd.CommandText = "INSERT INTO WorkSessions (Date, TotalEarnings, Deliveries, Km, Consumption) VALUES (@Date, @TotalEarnings, @Deliveries, @Km, @Consumption);"
                cmd.Parameters.AddWithValue("@Date", session.Date.ToString("yyyy-MM-dd"))
                cmd.Parameters.AddWithValue("@TotalEarnings", session.TotalEarnings)
                cmd.Parameters.AddWithValue("@Deliveries", session.Deliveries)
                cmd.Parameters.AddWithValue("@Km", session.Km)
                cmd.Parameters.AddWithValue("@Consumption", session.Consumption)
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Public Function GetAll() As List(Of WorkSessionModel)
            Dim list As New List(Of WorkSessionModel)()
            Dim formats = New String() {"yyyy-MM-dd", "MM/dd/yyyy", "M/d/yyyy", "dd/MM/yyyy", "d/M/yyyy"}

            Using conn = New SQLiteConnection(_connectionString)
                conn.Open()
                Dim cmd = conn.CreateCommand()
                cmd.CommandText = "SELECT Id, Date, TotalEarnings, Deliveries, Km, Consumption FROM WorkSessions ORDER BY Date DESC;"
                Using reader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim rawDateStr = reader("Date").ToString()
                        Dim parsedDate As DateTime
                        Dim dateValue As DateTime

                        ' Proviamo prima formati espliciti poi il TryParse generico
                        If DateTime.TryParseExact(rawDateStr, formats, Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, parsedDate) Then
                            dateValue = parsedDate
                        ElseIf DateTime.TryParse(rawDateStr, Globalization.CultureInfo.GetCultureInfo("it-IT"), Globalization.DateTimeStyles.None, parsedDate) Then
                            dateValue = parsedDate
                        ElseIf DateTime.TryParse(rawDateStr, Globalization.CultureInfo.InvariantCulture, Globalization.DateTimeStyles.None, parsedDate) Then
                            dateValue = parsedDate
                        Else
                            ' fallback: ignora la riga se non si riesce a parsare
                            Continue While
                        End If

                        Dim s As New WorkSessionModel() With {
                    .Id = Convert.ToInt32(reader("Id")),
                    .Date = dateValue,
                    .TotalEarnings = Convert.ToDecimal(reader("TotalEarnings")),
                    .Deliveries = Convert.ToInt32(reader("Deliveries")),
                    .Km = Convert.ToDouble(reader("Km")),
                    .Consumption = Convert.ToDouble(reader("Consumption"))
                }
                        list.Add(s)
                    End While
                End Using
            End Using
            Return list
        End Function

        Public Sub Update(session As WorkSessionModel)
            Using conn = New SQLiteConnection(_connectionString)
                conn.Open()
                Dim cmd = conn.CreateCommand()
                cmd.CommandText = "UPDATE WorkSessions SET Date=@Date, TotalEarnings=@TotalEarnings, Deliveries=@Deliveries, Km=@Km, Consumption=@Consumption WHERE Id=@Id;"
                cmd.Parameters.AddWithValue("@Id", session.Id)
                cmd.Parameters.AddWithValue("@Date", session.Date.ToString("yyyy-MM-dd"))
                cmd.Parameters.AddWithValue("@TotalEarnings", session.TotalEarnings)
                cmd.Parameters.AddWithValue("@Deliveries", session.Deliveries)
                cmd.Parameters.AddWithValue("@Km", session.Km)
                cmd.Parameters.AddWithValue("@Consumption", session.Consumption)
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Public Sub Delete(id As Integer)
            Using conn = New SQLiteConnection(_connectionString)
                conn.Open()
                Dim cmd = conn.CreateCommand()
                cmd.CommandText = "DELETE FROM WorkSessions WHERE Id=@Id;"
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Public Function GetMonthlyData(year As Integer, month As Integer) As List(Of WorkSessionModel)
            ' Prendiamo tutte le sessioni già parse correttamente e filtri per anno/mese
            Dim all = GetAll()
            Return all.Where(Function(s) s.Date.Year = year AndAlso s.Date.Month = month).ToList()
        End Function

        Public Function GetTaxRateForYear(year As Integer) As Decimal
            Dim rate As Decimal = 0.05D ' Default 5%
            Using conn = New SQLiteConnection(_connectionString)
                conn.Open()
                Dim cmd = conn.CreateCommand()
                cmd.CommandText = "SELECT TaxRate FROM YearlySettings WHERE Year=@Year;"
                cmd.Parameters.AddWithValue("@Year", year)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso Not DBNull.Value.Equals(result) Then
                    rate = Convert.ToDecimal(result)
                End If
            End Using
            Return rate
        End Function

        Public Sub SetTaxRateForYear(year As Integer, taxRate As Decimal)
            Using conn = New SQLiteConnection(_connectionString)
                conn.Open()
                Dim cmd = conn.CreateCommand()
                cmd.CommandText = "INSERT OR REPLACE INTO YearlySettings (Year, TaxRate) VALUES (@Year, @TaxRate);"
                cmd.Parameters.AddWithValue("@Year", year)
                cmd.Parameters.AddWithValue("@TaxRate", taxRate)
                cmd.ExecuteNonQuery()
            End Using
        End Sub

    End Class
End Namespace
